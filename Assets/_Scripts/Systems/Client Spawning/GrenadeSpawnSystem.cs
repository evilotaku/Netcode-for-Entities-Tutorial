using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.TextCore.Text;

public struct FireCounter : IComponentData
{
    public int Value;
}

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct GrenadeSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
       
        state.RequireForUpdate<ThirdPersonPlayerInputs>();
        state.RequireForUpdate<GrenadePrefab>();
        state.EntityManager.CreateSingleton<FireCounter>();
    }   
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick)
            return;

        var config = SystemAPI.GetSingleton<GrenadeConfig>();
        var commandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
        var grenadePrefab = SystemAPI.GetSingleton<GrenadePrefab>().Grenade;
        var fireCounter = SystemAPI.GetSingleton<FireCounter>();
        var fireCounterEntity = SystemAPI.GetSingletonEntity<FireCounter>();
        var localToWorldTransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        var time = state.WorldUnmanaged.Time.ElapsedTime;
        var isClient = state.WorldUnmanaged.IsClient();

        foreach (var (player,input, anchorPoint) in SystemAPI.Query<ThirdPersonCharacterAspect,RefRO<ThirdPersonPlayerInputs>, RefRO<AnchorPoint>>().WithAll<Simulate>())
        {
            if(player.CharacterControl.ValueRO.Fire)
            {
                //Create grenade
                var grenadeEntity = commandBuffer.Instantiate(grenadePrefab);
                // Set position
                var spawnPointEntity = anchorPoint.ValueRO.SpawnPoint;
                var grenadeSpawnPosition = localToWorldTransformLookup[spawnPointEntity].Position;
                var grenadeSpawnRotation = localToWorldTransformLookup[spawnPointEntity].Rotation;
                commandBuffer.SetComponent(grenadeEntity, LocalTransform.FromPositionRotation(grenadeSpawnPosition, grenadeSpawnRotation));
                //Set Velocity
                var initialVelocity = new PhysicsVelocity();
                initialVelocity.Linear = localToWorldTransformLookup[anchorPoint.ValueRO.SpawnPoint].Forward * config.InitialVelocity;
                commandBuffer.SetComponent(grenadeEntity, initialVelocity);
                //Set Detonation Timer
                var grenadeData = new GrenadeData() { DestroyTimer = (float)time + config.BlastTimer };
                //Set Grenade SpawnID                
                grenadeData.SpawnId = networkTime.ServerTick.SerializedData << 11 | (uint)player.GhostOwner.ValueRO.NetworkId;
                commandBuffer.SetComponent(grenadeEntity, grenadeData);
                //Set Grenade Owner
                commandBuffer.SetComponent(grenadeEntity, new GhostOwner { NetworkId = player.GhostOwner.ValueRO.NetworkId });
                //Set Grenade Color on Client
                if(isClient)
                {
                    if (fireCounter.Value % 2 == 1)
                        commandBuffer.SetComponent(grenadeEntity, new URPMaterialPropertyBaseColor() { Value = new float4(1, 0, 0, 1) });
                    else
                        commandBuffer.SetComponent(grenadeEntity, new URPMaterialPropertyBaseColor() { Value = new float4(0, 1, 0, 1) });
                }
                //increment Fire Counter
                commandBuffer.SetComponent(fireCounterEntity, new FireCounter() { Value = fireCounter.Value + 1 });
            }
        }
        commandBuffer.Playback(state.EntityManager);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}