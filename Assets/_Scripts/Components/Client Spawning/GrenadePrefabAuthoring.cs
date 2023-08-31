using Unity.Entities;
using UnityEngine;

public struct GrenadePrefab : IComponentData
{
    public Entity Grenade;
    public Entity Explosion;
}

public class GrenadePrefabAuthoring : MonoBehaviour
{

    public GameObject Grenade;
    public GameObject Explosion;

    class Baker : Baker<GrenadePrefabAuthoring>
    {
        public override void Bake(GrenadePrefabAuthoring authoring)
        {            
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new GrenadePrefab
            {
                Grenade = GetEntity(authoring.Grenade, TransformUsageFlags.Dynamic),
                Explosion = GetEntity(authoring.Explosion, TransformUsageFlags.Dynamic)
            });
        }
    }
}