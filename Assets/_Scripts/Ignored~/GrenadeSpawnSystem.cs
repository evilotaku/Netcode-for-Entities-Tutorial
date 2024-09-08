using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Assembly_CSharp.Generated;
using Unity.Rendering;
using Unity.Mathematics;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct GrenadeSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {       
        state.RequireForUpdate<ThirdPersonPlayerInputs>();
        state.RequireForUpdate<GrenadePrefab>();       
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
        var localToWorldTransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        var time = state.WorldUnmanaged.Time;
        var isClient = state.WorldUnmanaged.IsClient();

        foreach (var (player, owner, inputBuffer, anchorPoint) in SystemAPI.Query<ThirdPersonPlayerInputs, GhostOwner, DynamicBuffer<ThirdPersonPlayerInputs>, RefRO<AnchorPoint>>().WithAll<Simulate>())
        {
            //Account for lost or batched ticks
            var grenadesToSpawn = player.FirePressed.Count;
            if(grenadesToSpawn <= 0) continue;
            SystemAPI.GetSingleton<NetDebug>().Log($"Fire in the Hole!");
            
            //cache current count for later
            inputBuffer.GetDataAtTick(networkTime.ServerTick, out var currentInput);

            //Clamp Max Grenades to prevent cheating
            const int maxGrenadesPerPlayerPerServerTick = 5;
            if (grenadesToSpawn > maxGrenadesPerPlayerPerServerTick)
            {
                SystemAPI.GetSingleton<NetDebug>().Log($"Clamping player input, as they're attempting to spawn {grenadesToSpawn} grenades in one tick (max: {maxGrenadesPerPlayerPerServerTick})!");
                grenadesToSpawn = maxGrenadesPerPlayerPerServerTick;
            }

            //Batch Create grenade
            using var grenadeEntities = new NativeArray<Entity>((int)grenadesToSpawn, Allocator.Temp);
            commandBuffer.Instantiate(grenadePrefab, grenadeEntities);

            for (int spawnId = 0; spawnId < grenadesToSpawn; spawnId++)
            {
                var grenadeEntity = grenadeEntities[spawnId];

                // Set position
                var spawnPointEntity = anchorPoint.ValueRO.SpawnPoint;
                var grenadeSpawnPosition = localToWorldTransformLookup[spawnPointEntity].Position;
                var grenadeSpawnRotation = localToWorldTransformLookup[spawnPointEntity].Rotation;
                //Set Velocity
                var initialVelocity = new PhysicsVelocity();
                initialVelocity.Linear = localToWorldTransformLookup[anchorPoint.ValueRO.SpawnPoint].Forward * config.InitialVelocity;
                //offset batched spawns
                var spawnIdFraction = (float)spawnId / maxGrenadesPerPlayerPerServerTick;
                grenadeSpawnPosition += (spawnIdFraction * time.DeltaTime) * initialVelocity.Linear;


                commandBuffer.SetComponent(grenadeEntity, LocalTransform.FromPositionRotation(grenadeSpawnPosition, grenadeSpawnRotation));                
                commandBuffer.SetComponent(grenadeEntity, initialVelocity);

                //Set Detonation Timer
                var grenadeData = new GrenadeData() { DestroyTimer = (float)time.ElapsedTime + config.BlastTimer };
                //Set Grenade SpawnID
                uint secondaryFireCount = (uint) (currentInput.InternalInput.FirePressed.Count - spawnId);
                grenadeData.SpawnId = (uint)owner.NetworkId << 16 | secondaryFireCount;
                commandBuffer.SetComponent(grenadeEntity, grenadeData);
                //Set Grenade Owner
                commandBuffer.SetComponent(grenadeEntity, new GhostOwner { NetworkId = owner.NetworkId });
            }
        }
        commandBuffer.Playback(state.EntityManager);        
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
[BurstCompile]
public partial struct SetGrenadeColorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GrenadePrefab>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // The Change filter ensures we only set this color if the GrenadeData component changes, which will only happen once (when it spawns).
        foreach (var (urpColorRw, grenadeDataRo) in SystemAPI.Query<RefRW<URPMaterialPropertyBaseColor>, RefRO<GrenadeData>>().WithChangeFilter<GrenadeData>())
        {
            urpColorRw.ValueRW.Value = grenadeDataRo.ValueRO.SpawnId % 2 == 1
                ? new float4(1, 0, 0, 1)
                : new float4(0, 1, 0, 1);
        }
    }
}