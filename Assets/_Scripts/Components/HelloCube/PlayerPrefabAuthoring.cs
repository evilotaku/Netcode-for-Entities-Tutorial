using Unity.Entities;
using UnityEngine;


public struct PlayerPrefab : IComponentData
{
    public Entity Player;
}
public class PlayerPrefabAuthoring : MonoBehaviour
{
    public GameObject Player;

    class Baker : Baker<PlayerPrefabAuthoring>
    {
        public override void Bake(PlayerPrefabAuthoring authoring)
        {            
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new PlayerPrefab
            {
                Player = GetEntity(authoring.Player, TransformUsageFlags.Dynamic)
            });
        }
    }
}
