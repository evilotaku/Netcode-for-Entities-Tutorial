using System;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;


[Serializable]
public struct AIController : IComponentData
{
    public float DetectionDistance;
    public PhysicsCategoryTags DetectionFilter;
}
public class AIControllerAuthoring : MonoBehaviour
{
    public AIController AIController;

    class Baker : Baker<AIControllerAuthoring>
    {
        public override void Bake(AIControllerAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), authoring.AIController);
        }
    }
}