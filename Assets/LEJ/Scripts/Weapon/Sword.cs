using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Sword : WeaponBase
{
    [SerializeField] SwordHitBox hitBox;
    [SerializeField] SpriteRenderer model;
    [SerializeField] GameObject effect;
    public SpriteRenderer Model => model;

    float damage;
    int swordLife = 5;
    public int SwordLife => swordLife;
    int swingCount = 0;

    public event UnityAction OnHitted;

    private void Awake()
    {
        if (hitBox != null)
        {
            hitBox.OnHit += OnHit;
            hitBox.enabled = false;
        }

        effect.SetActive(false);
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
            OnHitted?.Invoke();

            //임시 이펙트 처리
            effect.SetActive(false);
            effect.SetActive(true);
            Invoke("HideEffect", 0.4f);
            SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Hit);

            swingCount++;
            if (swingCount >= swordLife)
                OnBroke();

            return;
        }
        else if (damageable != null)
        {
            damageable.TakeDamage(damage);
            OnHitted?.Invoke();

            //임시 이펙트 처리
            effect.SetActive(false);
            effect.SetActive(true);
            Invoke("HideEffect", 0.4f);

            swingCount++;

            if (swingCount >= swordLife)
                OnBroke();
        }
    }

    void OnBroke()
    {
        hitBox.enabled = false;
        this.gameObject.SetActive(false);
    }

    void HideEffect()
    {
        effect.SetActive(false);
    }

}
