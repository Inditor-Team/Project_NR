using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 무기 기본 클래스
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    /// <summary>
    /// 공격 시 dir 을 알립니다
    /// </summary>
    public UnityAction<Vector3> OnAttack;

    /// <summary>
    /// 무기의 소유자 지정, 작동 가능 상태를 설정
    /// </summary>
    /// <param name="owner"></param>
    public abstract void SetOwner(GameObject owner);

    /// <summary>
    /// 작동 시도
    /// </summary>
    public abstract void TryAttack(float speed);

    /// <summary>
    /// 작동
    /// </summary>
    internal abstract void Attack();
}
