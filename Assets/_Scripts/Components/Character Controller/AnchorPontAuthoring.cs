using Unity.Entities;
using UnityEngine;

public struct AnchorPoint : IComponentData
{
    public Entity SpawnPoint;
    public Entity WeaponSlot;
}

public class AnchorPointAuthoring : MonoBehaviour
{
    public GameObject SpawnPoint;
    public GameObject WeaponSlot;

    class Baker : Baker<AnchorPointAuthoring>
    {
        public override void Bake(AnchorPointAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new AnchorPoint
            {
                SpawnPoint = GetEntity(authoring.SpawnPoint, TransformUsageFlags.Dynamic),
                WeaponSlot = GetEntity(authoring.WeaponSlot, TransformUsageFlags.Dynamic)
            });
        }
    }
}