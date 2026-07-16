using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Sword : WeaponBase
{
    [SerializeField] SwordHitBox hitBox;
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;

    float damage;

    private void Awake()
    {
        if (hitBox != null)
        {
            hitBox.OnHit += OnHit;
            hitBox.enabled = false;
        }
    }

    public void TryAttack(float damage)
    {
        this.damage = damage;
    }

    internal override void Attack()
    {
        hitBox.enabled = true;
    }

    public void EndAttack()
    {
        hitBox.enabled = false;
    }

    void OnHit(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        EnemyBullet bullet = target.GetComponent<EnemyBullet>();

        //적의 총알일 경우
        if (bullet != null)
        {
            //반대로 날려보내기
            //bullet.Launch(-bullet.velocity.normalized, bullet.velocity.magnitude, damage);
            return;
        }
        else if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }

}
