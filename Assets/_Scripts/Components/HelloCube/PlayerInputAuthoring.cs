using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct PlayerInput : ICommandData
{
    [GhostField] public NetworkTick Tick { get; set; }
    [GhostField] public float2 movement;
    [GhostField] public float2 look;
    [GhostField] public InputEvent fire;

    
}

public class PlayerInputAuthoring : MonoBehaviour
{
    class Baking : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            AddBuffer<PlayerInput>(GetEntity(TransformUsageFlags.Dynamic));
        }
    }
}
   