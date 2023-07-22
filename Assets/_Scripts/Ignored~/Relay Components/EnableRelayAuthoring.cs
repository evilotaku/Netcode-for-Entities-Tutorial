using Unity.Entities;
using UnityEngine;


public struct EnableRelay : IComponentData
{

}


public class EnableRelayAuthoring : MonoBehaviour
{	

	class Baker : Baker<EnableRelayAuthoring>
	{
		public override void Bake(EnableRelayAuthoring authoring)
		{
            AddComponent<EnableRelay>(GetEntity(TransformUsageFlags.Dynamic));
		}
	}
}