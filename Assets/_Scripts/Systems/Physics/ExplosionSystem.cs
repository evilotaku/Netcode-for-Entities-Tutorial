using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ExplosionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Exploded>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {        
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        foreach (var (explosion, collider, transform, mass, velocity, entity) in SystemAPI.Query<RefRO<Exploded>, PhysicsCollider, RefRO<LocalTransform>, PhysicsMass, RefRW<PhysicsVelocity>>().WithEntityAccess())
        {
            velocity.ValueRW.ApplyExplosionForce(mass, collider, transform.ValueRO.Position, transform.ValueRO.Rotation, explosion.ValueRO.Force, explosion.ValueRO.Position, explosion.ValueRO.Radius, 1f, math.up(), 1f, ForceMode.Impulse);
            ecb.RemoveComponent<Exploded>(entity);
        }
        ecb.Playback(state.EntityManager);
    }
}