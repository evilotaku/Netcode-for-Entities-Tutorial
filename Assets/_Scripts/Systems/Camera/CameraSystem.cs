using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateAfter(typeof(PresentationSystemGroup))]
[BurstCompile]
partial struct CameraSystem : ISystem
{
    public static readonly float3 Offset = new float3(0, 2, -5);

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {       
        state.RequireForUpdate<NetworkStreamInGame>();
        state.RequireForUpdate<Character>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var camera = UnityEngine.Camera.main;
        
        foreach (var character in SystemAPI.Query<CharacterAspect>().WithAll<GhostOwnerIsLocal>())
        {
            camera.transform.rotation = math.mul(quaternion.RotateY(character.Input.Look.x), quaternion.RotateX(-character.Input.Look.y));
            var cameraPos = math.rotate(camera.transform.rotation, Offset);
            camera.transform.position = character.Transform.ValueRO.Position + cameraPos;
        }
    }
}