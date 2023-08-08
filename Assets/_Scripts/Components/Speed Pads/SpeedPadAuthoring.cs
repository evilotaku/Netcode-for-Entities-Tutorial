using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct SpeedPad : IComponentData
{
    public float VelocityFactor;
}

public class SpeedPadAuthoring : MonoBehaviour
{
    public float VelocityFactor;

    class Baker : Baker<SpeedPadAuthoring>
    {
        public override void Bake(SpeedPadAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new SpeedPad { VelocityFactor = authoring.VelocityFactor });
        }
    }
}