using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;


[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateBefore(typeof(PhysicsInitializeGroup))]
public partial class PlayerMoveSystem : SystemBase
{

    protected override void OnCreate()
    {
        RequireForUpdate<PlayerInput>();
    }

    protected override void OnUpdate()
    {
        var tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        var ecbSystem = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        
        new PlayerMoveJob { tick = tick }.ScheduleParallel();
        new PlayerJumpJob
        {
            tick = tick,
            physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>(),
            ecb = ecbSystem.CreateCommandBuffer(World.Unmanaged)
        }.Schedule();
    }
}

[BurstCompile]
[WithAll(typeof(Simulate))]
public partial struct PlayerMoveJob : IJobEntity
{
    public NetworkTick tick;
    
    [BurstCompile]
    void Execute(DynamicBuffer<PlayerInput> inputBuffer, ref PhysicsVelocity velocity, in MoveSpeed speed)
    {
        inputBuffer.GetDataAtTick(tick, out var input);
        float3 direction = new(input.movement.x, 0, input.movement.y);
        if (math.lengthsq(direction) > 0.5)
        {
            direction = math.normalize(direction);
            direction *= speed.Velocity;
        }

        velocity.Linear = direction;
    }
}

[BurstCompile]
[WithAll(typeof(Simulate))]
public partial struct PlayerJumpJob : IJobEntity
{
    public NetworkTick tick;
    public PhysicsWorldSingleton physicsWorld;
    public EntityCommandBuffer ecb;

    void Execute(ref PhysicsVelocity velocity, DynamicBuffer<PlayerInput> inputBuffer, in PhysicsMass mass, ref LocalTransform transform)
    {
        inputBuffer.GetDataAtTick(tick, out var input);
        float2 moveInput = input.movement;

        if (input.fire.IsSet)
        {
            var objs = new NativeList<DistanceHit>(Allocator.Temp);
            if (physicsWorld.OverlapSphere(transform.Position, 5.0f, ref objs, CollisionFilter.Default))
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    ecb.AddComponent(objs[i].Entity, ComponentType.ReadOnly<Exploded>());
                    ecb.SetComponent(objs[i].Entity, new Exploded
                    {
                        Position = transform.Position,
                        Force = 5f,
                        Radius = 5.0f
                    });
                }
            }
            velocity.ApplyLinearImpulse(mass, new float3(moveInput.x, 10f, moveInput.y));
        }
    }
}

