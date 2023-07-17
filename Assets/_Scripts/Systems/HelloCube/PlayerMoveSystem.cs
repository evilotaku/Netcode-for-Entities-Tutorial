using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;


[UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct PlayerMoveSystem : ISystem
{    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {           
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var ecbSystem = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (transform, input, speed, velocity,mass) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerInput>,RefRO<MoveSpeed>, RefRW<PhysicsVelocity>, PhysicsMass>().WithAll<Simulate>())
        {
            float2 moveInput = input.ValueRO.movement;
            moveInput = math.normalizesafe(moveInput) * speed.ValueRO.Velocity;
            //transform.ValueRW.Position += new float3(moveInput.x, 0, moveInput.y);
            velocity.ValueRW.ApplyLinearImpulse(mass, new float3(moveInput.x, 0, moveInput.y));
            
            if (input.ValueRO.fire.IsSet)
            {
                var objs = new NativeList<DistanceHit>(Allocator.Temp);
                if (physicsWorld.OverlapSphere(transform.ValueRO.Position, 5.0f, ref objs, CollisionFilter.Default))
                {                    
                    for (int i = 0; i < objs.Length; i++)
                    {
                        ecb.AddComponent(objs[i].Entity, typeof(Exploded));
                        ecb.SetComponent(objs[i].Entity, new Exploded
                        {
                            Position = transform.ValueRO.Position,
                            Force = 5f,
                            Radius = 5.0f
                        });                        
                    }
                }
                velocity.ValueRW.ApplyLinearImpulse(mass, new float3(moveInput.x, 10f, moveInput.y));
            }
        }
    }
}
