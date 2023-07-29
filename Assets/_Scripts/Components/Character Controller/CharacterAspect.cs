using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

public readonly partial struct CharacterAspect : IAspect
{
    public readonly Entity Self;
    public readonly RefRW<LocalTransform> Transform;

    readonly RefRO<AutoCommandTarget> m_AutoCommandTarget;
    readonly RefRW<Character> m_Character;
    readonly RefRW<PhysicsVelocity> m_Velocity;
    readonly RefRO<PlayerInput> m_Input;
    readonly RefRO<GhostOwner> m_Owner;

    public AutoCommandTarget AutoCommandTarget => m_AutoCommandTarget.ValueRO;
    public PlayerInput Input => m_Input.ValueRO;
    public int OwnerNetworkId => m_Owner.ValueRO.NetworkId;
    public ref Character Character => ref m_Character.ValueRW;
    public ref PhysicsVelocity Velocity => ref m_Velocity.ValueRW;
}