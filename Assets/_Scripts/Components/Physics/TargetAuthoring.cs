using Unity.Entities;
using UnityEngine;

public struct Target : IComponentData
{
	public Entity entity;
	public float MaxDistance;
}
public class TargetAuthoring : MonoBehaviour
{
	public GameObject Target;
	public float MaxDistance;

	class Baker : Baker<TargetAuthoring>
	{
		public override void Bake(TargetAuthoring authoring)
		{
			AddComponent( GetEntity(TransformUsageFlags.Dynamic), new Target
            {
                entity = GetEntity(authoring.Target, TransformUsageFlags.Dynamic),
                MaxDistance = authoring.MaxDistance
            });
		}
	}
}