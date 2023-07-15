using Unity.Entities;
using Unity.Mathematics;

public struct Exploded : IComponentData
{
    public float3 Position;
    public float Force;
    public float Radius;
}