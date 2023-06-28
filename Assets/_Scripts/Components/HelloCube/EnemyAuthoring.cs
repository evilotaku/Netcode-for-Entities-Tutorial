using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PrefabVisuals : IComponentData 
{
    public GameObject prefab;
}
public class PrefabAnimator : IComponentData
{
    public Animator Anim;
}
public class PrefabTransform : ICleanupComponentData
{ 
    public Transform Transform;
}
public struct Speed : IComponentData
{
    public float value;
}

public class EnemyAuthoring : MonoBehaviour
{
    public GameObject enemy;
    public float speed;
}    

public class EnemyBaker : Baker<EnemyAuthoring>
{
    public override void Bake(EnemyAuthoring authoring)
    {
        AddComponentObject(GetEntity(TransformUsageFlags.Dynamic), new PrefabVisuals { prefab = authoring.enemy});
        AddComponent(GetEntity(TransformUsageFlags.Dynamic), new Speed { value = authoring.speed });
    }
}

