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
    public event UnityAction OnSwordLifeChanged;

    bool canConsumeSword = true;

    private void Awake()
    {
        if (hitBox != null)
        {
            hitBox.OnHit += OnHit;
            hitBox.enabled = false;
        }

        effect.SetActive(false);
    }

    private void OnDestroy()
    {
        hitBox.OnHit -= OnHit;
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
        canConsumeSword = true;
    }

    void OnHit(GameObject target)
    {
        if (!gameObject.activeSelf) //gameObject 가 SetActive false 된 상태에서 Coroutine null 오류 때문에 추가
            return;

        IDamageable damageable = target.GetComponent<IDamageable>();
        EnemyBullet bullet = target.GetComponent<EnemyBullet>();

        //적의 총알일 경우
        if (bullet == null && damageable == null)
            return;

        SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Hit);

        //damageable 일 경우 damage 전달
        damageable?.TakeDamage(damage);

        //임시 이펙트 처리
        effect.SetActive(false);
        effect.SetActive(true);
        Invoke("HideEffect", 0.4f);

        //칼은 한 번 휘두를 때 한 번만 소모 됩니다
        if (canConsumeSword)
        {
            swordLife--;
            OnSwordLifeChanged?.Invoke();
        }

        canConsumeSword = false; //칼이 다 휘둘러진 다음 true 가 됩니다

        //칼의 수명이 다하면 부서집니다
        if (swordLife <= 0)
            OnBroke();
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
