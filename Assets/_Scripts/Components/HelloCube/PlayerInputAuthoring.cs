using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct PlayerInput : IInputComponentData
{
    [GhostField] public NetworkTick Tick { get; set; }
    [GhostField] public float2 Movement;
    [GhostField] public float2 Look;
    [GhostField] public InputEvent PrimeFire;
    [GhostField] public InputEvent SecondFire;


}

public class PlayerInputAuthoring : MonoBehaviour
{
    class Baking : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            AddComponent<PlayerInput>(GetEntity(TransformUsageFlags.Dynamic));
        }
    }
}
   