using UnityEngine;

/// <summary>
/// 무기 기본 클래스
/// </summary>
public class WeaponBase : MonoBehaviour
{
    /// <summary>
    /// 작동 시도
    /// </summary>
    public virtual void TryAttack()
    {
    }

    /// <summary>
    /// 작동
    /// </summary>
    internal virtual void Attack()
    {

    }
}
