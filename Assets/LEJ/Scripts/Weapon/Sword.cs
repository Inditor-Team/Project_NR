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

    public event UnityAction OnSwing;
    public float swingTime = 1f;

    public bool inactive = false;

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
            SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Hit);

            if (inactive) //칼의 무적상태
                return;

            if (swingRoutine == null)
                swingRoutine = StartCoroutine(SwingTime());

            return;
        }
        else if (damageable != null)
        {
            SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Hit);

            if (inactive) //칼의 무적상태
                return;

            damageable.TakeDamage(damage);

            if (swingRoutine == null)
                swingRoutine = StartCoroutine(SwingTime());
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

    Coroutine swingRoutine;
    IEnumerator SwingTime()
    {
        //임시 이펙트 처리
        effect.SetActive(false);
        effect.SetActive(true);
        Invoke("HideEffect", 0.4f);

        OnSwing?.Invoke();
        swingCount++;

        if (swingCount >= swordLife)
        {
            OnBroke();
            yield break;
        }

        yield return new WaitForSeconds(swingTime);
        swingRoutine = null;
    }
}
