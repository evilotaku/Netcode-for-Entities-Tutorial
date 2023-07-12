using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
public partial struct AttractorSystem : ISystem
{   
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Moving>();
        state.RequireForUpdate<Target>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NativeList<float3> players = new(Allocator.Temp);
        foreach (var player in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Player>())
        {
            players.Add(player.ValueRO.Position);
        }

        foreach (var (target, transform, moving, velocity) in SystemAPI.Query<RefRO<Target>, RefRO<LocalTransform>, RefRO<Moving>, RefRW<PhysicsVelocity>>())
        {
            foreach (var player in players)
            {
                var originPosition = transform.ValueRO.Position;
                var direction = math.normalize(player - originPosition);

                if (math.distance(player, originPosition) < target.ValueRO.MaxDistance)
                    velocity.ValueRW.Linear = moving.ValueRO.Velocity * direction;
                else
                    velocity.ValueRW.Linear = new float3(0, 0, 0);
            }
        }

        players.Dispose();
       
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
       
    }
}