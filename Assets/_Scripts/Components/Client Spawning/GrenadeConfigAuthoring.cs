using Unity.Entities;
using UnityEngine;

public struct GrenadeConfig : IComponentData
{
    public int InitialVelocity;
    public float BlastTimer;
    public int BlastRadius;
    public int BlastPower;
    public float ExplosionTimer;
}

public class GrenadeConfigAuthoring : MonoBehaviour
{
    public int InitialVelocity = 30;
    public float BlastTimer = 3f;
    public int BlastRadius = 40;
    public int BlastPower = 10;
    public float ExplosionTimer = 1.9f;

    class Baker : Baker<GrenadeConfigAuthoring>
    {
        public override void Bake(GrenadeConfigAuthoring authoring)
        {            
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new GrenadeConfig
            {
                InitialVelocity = authoring.InitialVelocity,
                BlastTimer = authoring.BlastTimer,
                BlastRadius = authoring.BlastRadius,
                BlastPower = authoring.BlastPower,
                ExplosionTimer = authoring.ExplosionTimer
            });
        }
    }
}