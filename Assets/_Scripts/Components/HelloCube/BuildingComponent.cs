using Unity.Entities;
using Unity.Physics;

public struct BuildingComponent : IBufferElementData
{
    public RaycastInput value;
    public int index;
}