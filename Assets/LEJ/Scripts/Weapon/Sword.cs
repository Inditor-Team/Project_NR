using System.Collections.Generic;
using UnityEngine;

public class Sword : WeaponBase
{
    [SerializeField] float damage;

    [SerializeField] float attackRate; //검 휘두르는 것에 대한 쿨타임
    float lastAttackTime;

    float animOriginSpeed = 1f;
    Collider2D col;
    HashSet<IDamageable> hitTargets = new(); //하나의 적이 여러 번 공격 인정 되는 것을 방지

    GameObject owner;
    [SerializeField] SwordSwing swing;

    private void Awake()
    {
        col = GetComponent<Collider2D>();

        DisableAttackCollider();
        swing.OnStopSwing += DisableAttackCollider;
    }

    public override void SetOwner(GameObject owner)
    {
        this.owner = owner;
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
        swing.Swing();
    }

    internal void Attack(float speed)
    {
        swing.Swing();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        //데미지를 안 받는 오브젝트거나 공격자 자신이면 무시
        if (target == null || collision.gameObject == owner)
            return;
        
        //콜리젼에 여러 번 들어오는 것을 방지하기 위한 중복 체크
        if (hitTargets.Contains(target))
            return;

        hitTargets.Add(target);
        target.TakeDamage(damage);
    }

    public void DisableAttackCollider()
    {
        col.enabled = false;
    }

}
