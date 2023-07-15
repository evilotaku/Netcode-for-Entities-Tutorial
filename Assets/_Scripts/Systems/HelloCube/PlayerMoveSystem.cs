using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Services.Lobbies.Models;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct PlayerMoveSystem : ISystem
{    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();
    }

    public void OnUpdate(ref SystemState state)
    {       
        var speed = SystemAPI.Time.DeltaTime * 20;
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (transform,input, velocity,mass) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerInput>,RefRW<PhysicsVelocity>, PhysicsMass>().WithAll<Simulate>())
        {
            float2 moveInput = input.ValueRO.movement;
            moveInput = math.normalizesafe(moveInput) * speed;
            //transform.ValueRW.Position += new float3(moveInput.x, 0, moveInput.y);
            velocity.ValueRW.ApplyLinearImpulse(mass, new float3(moveInput.x, 0, moveInput.y));

            var objs = new NativeList<DistanceHit>(Allocator.Temp);
            if (input.ValueRO.fire.IsSet)
            {
                Debug.Log("System Click!");
                if (physicsWorld.OverlapSphere(transform.ValueRO.Position, 5.0f, ref objs, CollisionFilter.Default))
                {
                    Debug.Log("Boom!");
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
            }
        }
    }
}
