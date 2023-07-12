using Unity.Entities;
using UnityEngine;

public struct Player : IComponentData { }


public class PlayerAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new Player());
        }
    }
}
