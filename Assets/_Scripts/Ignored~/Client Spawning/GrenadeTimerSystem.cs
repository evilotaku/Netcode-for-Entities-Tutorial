using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct GrenadeLauncherRotateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {        
        state.RequireForUpdate<NetworkId>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
        foreach (var (character, anchorPoint) in SystemAPI.Query<ThirdPersonCharacterAspect, RefRO<AnchorPoint>>().WithAll<Simulate>())
        {
            // This is the weapon slot and rotating that will make the launcher move correctly (it's anchored on the end)
            var grenadeLauncher = anchorPoint.ValueRO.WeaponSlot;
            var followCameraRotation = quaternion.RotateX(-character.CharacterControl.ValueRO.Look.x);

            var transform = state.EntityManager.GetComponentData<LocalTransform>(grenadeLauncher);
            commandBuffer.SetComponent(grenadeLauncher, transform.WithRotation(followCameraRotation));

        }
        commandBuffer.Playback(state.EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(GrenadeLauncherRotateSystem))]
[BurstCompile]
public partial struct GrenadeTimerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }   
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
        var time = state.WorldUnmanaged.Time.ElapsedTime;
        var config = SystemAPI.GetSingleton<GrenadeConfig>();

        foreach (var (data, grenadeTransform, entity) in SystemAPI.Query<RefRO<GrenadeData>, RefRO<LocalTransform>>().WithAll<Simulate>().WithEntityAccess())
        {
            //Check Timer
            if (time > data.ValueRO.DestroyTimer)
            {
                //Apply Blast Radius
                foreach (var (velocity, transform) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<LocalTransform>>().WithAll<Simulate>())
                {
                    var direction = transform.ValueRO.Position - grenadeTransform.ValueRO.Position;
                    var distanceSqrt = math.lengthsq(direction);
                    if (distanceSqrt < config.BlastRadius && distanceSqrt != 0)
                    {
                        var scaledPower = 1.0f - distanceSqrt / config.BlastRadius;
                        velocity.ValueRW.Linear = config.BlastPower * scaledPower * (direction / math.sqrt(distanceSqrt));
                    }
                }
                //Destroy Grenade(except for Cleanup Component)
                commandBuffer.DestroyEntity(entity);
            }
        }
        commandBuffer.Playback(state.EntityManager);

    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[BurstCompile]
public partial struct ExplosionSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
        var time = state.WorldUnmanaged.Time.ElapsedTime;
        var explosionPrefab = SystemAPI.GetSingleton<GrenadePrefab>().Explosion;
        var config = SystemAPI.GetSingleton<GrenadeConfig>();

        //Add Cleanup Component to incoming server ghost snapshots
        foreach (var (grenadeData, entity) in SystemAPI.Query<RefRO<GrenadeData>>().WithNone<PredictedGhostSpawnRequest>().WithNone<GrenadeClientCleanupData>().WithEntityAccess())
        {
            commandBuffer.AddComponent<GrenadeClientCleanupData>(entity);
        }
        //Save current position to cleanup data for later
        foreach (var (transform, cleanupData) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<GrenadeClientCleanupData>>())
        {
            cleanupData.ValueRW.Position = transform.ValueRO.Position;
        }
        //Create Explosion after grenade is destroyed only the cleanup data remains
        foreach (var (grenade, entity) in SystemAPI.Query<RefRO<GrenadeClientCleanupData>>().WithNone<GrenadeData>().WithEntityAccess())
        {
            var explosion = commandBuffer.Instantiate(explosionPrefab);

            commandBuffer.SetComponent(explosion, LocalTransform.FromPosition(grenade.ValueRO.Position));

            commandBuffer.AddComponent(explosion, new ExplosionData() { Timer = (float)time + config.ExplosionTimer });
            commandBuffer.RemoveComponent<GrenadeClientCleanupData>(entity);
        }
        //Destroy the explosion
        foreach (var (explosionData, entity) in SystemAPI.Query<RefRO<ExplosionData>>().WithAll<Simulate>().WithEntityAccess())
        {
            if (explosionData.ValueRO.Timer < time)
                commandBuffer.DestroyEntity(entity);
        }
        commandBuffer.Playback(state.EntityManager);
    }
}