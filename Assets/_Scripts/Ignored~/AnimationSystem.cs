using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct AnimationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

    }
    public void OnUpdate(ref SystemState state)
    {
        var ecbBeginSim = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var ecbEndSim = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var(obj,entity) in SystemAPI.Query<PrefabVisuals>().WithEntityAccess())
        {
            GameObject go = GameObject.Instantiate(obj.prefab);
            ecbBeginSim.AddComponent(entity, new PrefabTransform { Transform = go.transform });
            ecbBeginSim.AddComponent(entity, new PrefabAnimator { Anim = go.GetComponent<Animator>()});
            ecbBeginSim.RemoveComponent<PrefabVisuals>(entity);
        }
        
        foreach (var (prefabTransfrom, prefabAnim, transform, speed) in SystemAPI.Query<PrefabTransform, PrefabAnimator, LocalToWorld, RefRO<Speed>>())
        {
            prefabTransfrom.Transform.position = transform.Position;
            prefabTransfrom.Transform.rotation = transform.Rotation;
            prefabAnim.Anim.SetFloat("speed", speed.ValueRO.value);
        }

        foreach (var (prefabTransfrom, entity) in SystemAPI.Query<PrefabTransform>()
            .WithNone<LocalToWorld>()
            .WithEntityAccess())
        {
            GameObject.Destroy(prefabTransfrom.Transform.gameObject);
            ecbEndSim.RemoveComponent<PrefabTransform>(entity);
        }
    }
    public void OnDestroy(ref SystemState state)
    {

    }
}
