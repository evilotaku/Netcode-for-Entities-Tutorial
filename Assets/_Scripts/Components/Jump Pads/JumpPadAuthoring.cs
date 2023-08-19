using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct JumpPad : IComponentData
{
    public float3 JumpForce;
}

public class JumpPadAuthoring : MonoBehaviour
{
    public float3 JumpForce;

    class Baker : Baker<JumpPadAuthoring>
    {
        public override void Bake(JumpPadAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new JumpPad { JumpForce = authoring.JumpForce });
        }
    }
}