
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class SwordHitBox : MonoBehaviour, IDamageable
{
    private HashSet<GameObject> hitTargets = new();

    public event UnityAction<GameObject> OnHit;

    private Collider2D col;
    private bool isAttacking = false;

    private void Awake()
    {
        col = GetComponent<Collider2D>();

        isAttacking = false;
        col.enabled = false;
    }

    private void OnEnable()
    {
        if (col == null)
            col = GetComponent<Collider2D>();

        col.enabled = isAttacking;
    }

    private void OnDisable()
    {
        isAttacking = false;

        if (col != null)
            col.enabled = false;
    }

    /// <summary>
    /// 실제 공격 중에만 충돌 판정 활성화
    /// </summary>
    public void SetHitboxActive(bool active)
    {
        if (col == null)
            col = GetComponent<Collider2D>();

        isAttacking = active;

        if (active)
            hitTargets.Clear();

        col.enabled = active && isActiveAndEnabled;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hitTargets.Add(collision.gameObject))
            return;

        Debug.Log($"{collision} 이 swordHitBox 에 감지 됨");

        if (!hitTargets.Add(collision.gameObject))
            return;

        OnHit?.Invoke(collision.gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (!isAttacking)
            return;

        Debug.Log("칼이 총알을 가름");
    }
}
