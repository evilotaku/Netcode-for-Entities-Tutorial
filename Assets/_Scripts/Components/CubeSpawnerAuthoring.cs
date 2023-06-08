using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;
using UnityEngine;


public struct CubeSpawner : IComponentData
{
    public Entity Cube;
}
public class CubeSpawnerAuthoring : MonoBehaviour
{
    public GameObject Cube;

    class Baker : Baker<CubeSpawnerAuthoring>
    {
        public override void Bake(CubeSpawnerAuthoring authoring)
        {            
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new CubeSpawner
            {
                Cube = GetEntity(authoring.Cube, TransformUsageFlags.Dynamic)
            });
        }
    }
}
