using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct GrenadeData : IComponentData
{
    [GhostField]
    public uint SpawnId;

    public float DestroyTimer;
}

public struct ExplosionData : IComponentData
{
    public float Timer;
}

public struct GrenadeClientCleanupData : ICleanupComponentData
{
    public float3 Position;
}

public class GrenadeDataAuthoring : MonoBehaviour
{   

    class Baker : Baker<GrenadeDataAuthoring>
	{
		public override void Bake(GrenadeDataAuthoring authoring)
		{
            AddComponent<GrenadeData>(GetEntity(TransformUsageFlags.Dynamic));
        }
	}
}