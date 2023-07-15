using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
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
        var physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var velocites = SystemAPI.GetComponentLookup<PhysicsVelocity>();
        foreach (var (transform, speed, velocity, mass) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveSpeed>,RefRW<PhysicsVelocity>, PhysicsMass>())
        {
            foreach (var (player, input) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerInput>>().WithAll<Player>())
            {                
                var direction = math.normalize(player.ValueRO.Position - transform.ValueRO.Position);

                var distance = math.distance(player.ValueRO.Position, transform.ValueRO.Position);

                if (distance > 2)
                { 
                    velocity.ValueRW.ApplyLinearImpulse(mass,speed.ValueRO.Velocity * direction * SystemAPI.Time.DeltaTime);
                }
                else
                {
                    velocity.ValueRW.Linear = new float3(0, 0, 0);
                }

               
            }
        }
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
       
    }
}