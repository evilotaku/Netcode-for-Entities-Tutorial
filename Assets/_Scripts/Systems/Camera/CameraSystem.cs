using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateAfter(typeof(PresentationSystemGroup))]
[BurstCompile]
partial struct CameraSystem : ISystem
{
    public static readonly float3 k_CameraOffset = new float3(0, 2, -5);

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {       
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<Character>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var camera = UnityEngine.Camera.main;
        
        foreach (var (localToWorld, input) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
        {
            camera.transform.rotation = math.mul(quaternion.RotateY(input.ValueRO.Look.x), quaternion.RotateX(-input.ValueRO.Look.y));
            var offset = math.rotate(camera.transform.rotation, k_CameraOffset);
            camera.transform.position = localToWorld.ValueRO.Position + offset;
        }
    }
}