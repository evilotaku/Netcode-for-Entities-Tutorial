using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct SphereSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Sphere>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spawner = SystemAPI.GetSingleton<Sphere>();
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        for (int i = 0; i < spawner.Count; i++)
        {
            var entity = ecb.Instantiate(spawner.Prefab);
            ecb.SetComponent(entity, new LocalTransform
            {
                Position = new float3(0, i * 2, 0),
                Rotation = quaternion.identity,
                Scale = 1
            });
        }

        state.Enabled = false;
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}