using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public struct Sphere : IComponentData
{
	public Entity Prefab;
	public int Count;
}
public class SphereAuthoring : MonoBehaviour
{
	public GameObject Prefab;
	public int Count;

	class Baker : Baker<SphereAuthoring>
	{
		public override void Bake(SphereAuthoring authoring)
		{
			AddComponent(GetEntity(TransformUsageFlags.Dynamic), new Sphere
			{
				Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
				Count = authoring.Count
			});
		}
	}
}