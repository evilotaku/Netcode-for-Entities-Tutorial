using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Cube : IComponentData { }


public class CubeAuthoring : MonoBehaviour
{
    class Baker : Baker<CubeAuthoring>
    {
        public override void Bake(CubeAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new Cube());
        }
    }
}
