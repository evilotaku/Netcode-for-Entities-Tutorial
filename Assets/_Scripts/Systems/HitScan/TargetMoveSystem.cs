using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct HitTargetMoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WeaponTarget>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var timeDeltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (trans, hitTarget) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<WeaponTarget>>())

        {
            var deltaMove = timeDeltaTime * hitTarget.ValueRW.Speed;
            hitTarget.ValueRW.Moved += deltaMove;

            trans.ValueRW.Position.x += deltaMove;

            if (math.abs(hitTarget.ValueRW.Moved) > hitTarget.ValueRW.MovingRange)
            {
                hitTarget.ValueRW.Speed = -hitTarget.ValueRW.Speed;
            }
        }
    }
}