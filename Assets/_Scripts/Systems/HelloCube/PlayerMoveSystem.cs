using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct PlayerMoveSystem : ISystem
{
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var speed = SystemAPI.Time.DeltaTime * 4;
        foreach (var (input, transform) in SystemAPI.Query<RefRO<PlayerInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            float2 moveInput = input.ValueRO.movement;
            moveInput = math.normalizesafe(moveInput) * speed;
            transform.ValueRW.Position += new float3(moveInput.x, 0, moveInput.y);
            //if (input.ValueRO.fire.IsSet) ;//do something!
        }
    }
}
