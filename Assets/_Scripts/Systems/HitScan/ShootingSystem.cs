using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[RequireMatchingQueriesForUpdate]
public partial struct ShootingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<HitScan>();
    }   
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        var collisionHistory = SystemAPI.GetSingleton<PhysicsWorldHistorySingleton>();
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var ghostComponentFromEntity = SystemAPI.GetComponentLookup<GhostInstance>();
        var scaleFromEntity = SystemAPI.GetComponentLookup<PostTransformMatrix>();
        var RollbackEnabledFromEntity = SystemAPI.GetComponentLookup<RollbackEnabled>();
        var predictingTick = networkTime.ServerTick;

        // Do not perform hit-scan when rolling back, only when simulating the latest tick
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;

        foreach (var (character, interpolationDelay, hitComponent) in SystemAPI.Query<CharacterAspect, RefRO<CommandDataInterpolationDelay>, RefRW<HitScan>>().WithAll<Simulate>())
        {
            if (character.Input.SecondFire.IsSet)
            {
                hitComponent.ValueRW.Target = character.Self;
                hitComponent.ValueRW.Tick = predictingTick;
                continue;
            }
            if (!character.Input.PrimeFire.IsSet)
            {
                continue;
            }

            // Get the collision world to use given the tick currently being predicted and the interpolation delay for the connection
            var delay = RollbackEnabledFromEntity.HasComponent(character.Self)
                ? interpolationDelay.ValueRO.Delay
                : 0;

            collisionHistory.GetCollisionWorldFromTick(predictingTick, delay, ref physicsWorld, out var collWorld);

            var cameraRotation = math.mul(quaternion.RotateY(character.Input.Look.x), quaternion.RotateX(-character.Input.Look.y));
            var offset = math.rotate(cameraRotation, CameraSystem.Offset);
            var cameraPosition = character.Transform.ValueRO.Position + offset;
            var forward = math.mul(cameraRotation, math.forward());
            var rayInput = new RaycastInput
            {
                Start = cameraPosition + forward,
                End = cameraPosition + forward * 1000,
                Filter = CollisionFilter.Default
            };
            bool hit = collWorld.CastRay(rayInput, out var closestHit);

            if (!hit)
            {
                continue;
            }

            var hitEntity = Entity.Null;
            var hitPoint = closestHit.Position;
            if (ghostComponentFromEntity.HasComponent(closestHit.Entity))
            {
                hitEntity = closestHit.Entity;

                var rigidTransform = collWorld.Bodies[closestHit.RigidBodyIndex].WorldFromBody;
                hitPoint -= rigidTransform.pos;
                hitPoint = math.mul(math.inverse(rigidTransform.rot), hitPoint);

                if (scaleFromEntity.HasComponent(closestHit.Entity))
                {
                    var scaleMatrix = scaleFromEntity[closestHit.Entity].Value;
                    var scaleX = math.length(scaleMatrix.c0.xyz);
                    var scaleY = math.length(scaleMatrix.c1.xyz);
                    var scaleZ = math.length(scaleMatrix.c2.xyz);
                    hitPoint /= new float3(scaleX, scaleY, scaleZ);
                }

            }

            hitComponent.ValueRW.Target = hitEntity;
            hitComponent.ValueRW.HitPoint = hitPoint;
            hitComponent.ValueRW.Tick = predictingTick;
        }
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}