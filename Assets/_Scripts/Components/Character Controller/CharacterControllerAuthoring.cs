using System.Net.Security;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;


public struct Character : IComponentData
{
    public Entity ControllerConfig;

    [GhostField(Quantization = 1000)]
    public float3 MoveSpeed;
    [GhostField]
    public byte OnGround;
    [GhostField]
    public NetworkTick JumpStart;
}

public class CharacterControllerAuthoring : MonoBehaviour
{
    public ControllerConfigAuthoring ControllerConfig;
	class Baker : Baker<CharacterControllerAuthoring>
	{
		public override void Bake(CharacterControllerAuthoring authoring)
		{
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerInput());
            AddComponent(entity, new Character
            {
                ControllerConfig = GetEntity(authoring.ControllerConfig.gameObject, TransformUsageFlags.Dynamic),
                MoveSpeed = authoring.ControllerConfig.Speed
            });
            
		}
	}
}