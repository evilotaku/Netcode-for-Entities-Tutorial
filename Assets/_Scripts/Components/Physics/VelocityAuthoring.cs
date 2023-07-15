using Unity.Entities;
using UnityEngine;

public struct MoveSpeed : IComponentData
{
    public float Velocity;
}
public class VelocityAuthoring : MonoBehaviour
{
	public float Velocity;
	class Baker : Baker<VelocityAuthoring>
	{
		public override void Bake(VelocityAuthoring authoring)
		{
			AddComponent(GetEntity(TransformUsageFlags.Dynamic), new MoveSpeed
			{
				Velocity = authoring.Velocity
			});
		}
	}
}