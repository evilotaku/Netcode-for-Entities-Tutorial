using Unity.Entities;
using UnityEngine;

public struct WeaponTarget : IComponentData
{
    public float Speed;
    public float MovingRange;
    public float Moved;
}

public class WeaponTargetAuthoring : MonoBehaviour
{
    public float Speed = 5f;
    public float MovingRange = 10f;

    class Baker : Baker<WeaponTargetAuthoring>
	{
		public override void Bake(WeaponTargetAuthoring authoring)
		{
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new WeaponTarget
            {
                Speed = authoring.Speed,
                MovingRange = authoring.MovingRange
            });
		}
	}
}