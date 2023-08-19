using NUnit.Framework;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public struct PlayerPrefab : IComponentData
{
    public Entity Player, Character, Camera;
    
}
public class PlayerPrefabAuthoring : MonoBehaviour
{
    public GameObject Player, Character, Camera;    

    class Baker : Baker<PlayerPrefabAuthoring>
    {       
        public override void Bake(PlayerPrefabAuthoring authoring)
        {     
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new PlayerPrefab
            {
                Player = GetEntity(authoring.Player, TransformUsageFlags.Dynamic),
                Character = GetEntity(authoring.Character, TransformUsageFlags.Dynamic),
                Camera = GetEntity(authoring.Camera, TransformUsageFlags.Dynamic)
            });
        }
    }
}
