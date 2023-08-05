using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct MovingPlatform : IComponentData
{
    public float3 Direction;
    public float Distance;
    public float Speed;
    public float3 RotationAxis;
    public float RotationSpeed;

    [HideInInspector]
    public bool IsInitialized;
    [HideInInspector]
    public float3 OriginalPosition;
    [HideInInspector]
    public quaternion OriginalRotation;
}

public class MovingPlatformAuthoring : MonoBehaviour
{
    public MovingPlatform MovingPlatform;

    public class Baker : Baker<MovingPlatformAuthoring>
    {
        public override void Bake(MovingPlatformAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), authoring.MovingPlatform);
        }
    }
}