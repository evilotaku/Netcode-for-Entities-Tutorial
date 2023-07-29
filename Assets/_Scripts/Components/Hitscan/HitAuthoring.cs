using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;


public struct HitScan : IComponentData
{
    public Entity Target;
    public NetworkTick Tick;
    public float3 HitPoint;
}

public class HitAuthoring : MonoBehaviour
{
    class Baker : Baker<HitAuthoring>
    {
        public override void Bake(HitAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new HitScan());
        }
    }
}