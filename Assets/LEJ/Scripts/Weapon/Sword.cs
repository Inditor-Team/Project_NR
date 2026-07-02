using System.Collections.Generic;
using UnityEngine;

public class Sword : WeaponBase
{
    [SerializeField] float damage;

    [SerializeField] float attackRate; //검 휘두르는 것에 대한 쿨타임
    [SerializeField] float speed = 1.5f;
    float lastAttackTime;

    float animOriginSpeed = 1f;
    Collider2D col;
    Animator anim;
    HashSet<IDamageable> hitTargets = new(); //하나의 적이 여러 번 공격 인정 되는 것을 방지

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        DisableAttackCollider();
    }

    private void Start()
    {
        lastAttackTime = -attackRate; //처음에 바로 공격할 수 있도록 초기화
    }

    public override void TryAttack()
    {
        if (Time.time - lastAttackTime < attackRate)
            return;

        Attack();
        lastAttackTime = Time.time;
    }

    internal override void Attack()
    {
        anim.speed = speed;
        anim.Play("Sword_Attack");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target == null)
            return;

        if (hitTargets.Contains(target))
            return;

        hitTargets.Add(target);
        target.TakeDamage(damage);
    }

    /// <summary>
    /// 애니메이션에서 호출되는 콜라이더 on 이벤트
    /// </summary>
    public void EnableAttackCollider()
    {
        hitTargets.Clear();
        col.enabled = true;
    }

    /// <summary>
    /// 애니메이션에서 호출되는 콜라이더 off 이벤트
    /// </summary>
    public void DisableAttackCollider()
    {
        col.enabled = false;
        anim.speed = animOriginSpeed;
    }


}
