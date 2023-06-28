using Unity.Entities;
using UnityEngine;


public struct EnableRPC : IComponentData { }

public class EnableRPCAuthoring : MonoBehaviour
{
	class Baker : Baker<EnableRPCAuthoring>
	{
		public override void Bake(EnableRPCAuthoring authoring)
		{
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new EnableRPC());
		}
	}
}