using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Buildings : IBufferElementData
{
	public Entity Prefab;
}

public class BuildingList : MonoBehaviour
{
	public List<GameObject> buildiings = new List<GameObject>();

	class Baker : Baker<BuildingList>
	{
		public override void Bake(BuildingList authoring)
		{
			var buffer = AddBuffer<Buildings>(GetEntity(TransformUsageFlags.Renderable));
			foreach(var building in authoring.buildiings)
			{
				buffer.Add(new Buildings { Prefab = GetEntity(building, TransformUsageFlags.Renderable) });
			}
		}
	}
}