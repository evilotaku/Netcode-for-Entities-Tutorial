using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInput : IInputComponentData
{
    public float2 movement;
    public float2 look;
    public InputEvent fire;
}

public class PlayerInputAuthoring : MonoBehaviour
{
    class Baking : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new PlayerInput());
        }
    }
}
   