using Unity.Entities;
using UnityEngine;


public struct ControllerConfig : IComponentData
{
    public float Speed;
    public float JumpSpeed;
    public float Gravity;
}

public class ControllerConfigAuthoring : MonoBehaviour
{

	public float Speed = 5f;
	public float JumpForce = 5f;
	public float Gravity = 9.81f;

	class Baker : Baker<ControllerConfigAuthoring>
	{
		public override void Bake(ControllerConfigAuthoring authoring)
		{
			AddComponent(GetEntity(TransformUsageFlags.Dynamic), new ControllerConfig
			{
				Speed = authoring.Speed,
				JumpSpeed = authoring.JumpForce,
				Gravity = authoring.Gravity,
			});
		}
	}
}