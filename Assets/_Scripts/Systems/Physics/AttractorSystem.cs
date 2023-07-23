using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct AttractorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MoveSpeed>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        foreach (var (transform, speed, velocity, mass) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveSpeed>, RefRW<PhysicsVelocity>, PhysicsMass>())
        {
            foreach (var player in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Player>())
            {
                var direction = math.normalize(player.ValueRO.Position - transform.ValueRO.Position);
                var distance = math.distance(player.ValueRO.Position, transform.ValueRO.Position);

                if (distance > 1)
                {
                    velocity.ValueRW.ApplyLinearImpulse(mass, speed.ValueRO.Velocity * direction * SystemAPI.Time.DeltaTime);
                }
            }
        }
    }
}