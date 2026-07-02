using UnityEngine;

/// <summary>
/// 무기 기본 클래스
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    /// <summary>
    /// 작동 시도
    /// </summary>
    public abstract void TryAttack();

    /// <summary>
    /// 작동
    /// </summary>
    internal abstract void Attack();
}
