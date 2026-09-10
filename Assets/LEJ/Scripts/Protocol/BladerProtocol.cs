using System;
using System.Collections;
using UnityEngine;

public class BladerProtocol : ProtocolBase
{
    [SerializeField] SwordAttacker swordAttacker;
    [SerializeField] Animator bladerEffectAnim;
    [SerializeField] SpriteRenderer bladerEffectSR;
    [SerializeField] Gun gun;

    float duration;
    float damage;

    public override void UpgradeProtocol(ProtocolCard.Buff type, float level)
    {
        if (!buffValues.ContainsKey(type))
            return;

        buffValues[type] = level;
    }

    internal override void TryProtocol(float duration)
    {
        this.duration = duration;
        DoProtocol();
    }

    internal override void DoProtocol()
    {
        isActive = true;
        Debug.Log("Player: Blader Protocol! ");
        
        if (protocolRoutine == null)
            protocolRoutine = StartCoroutine(ProtocolTime());
    }

    Coroutine protocolRoutine;

    IEnumerator ProtocolTime()
    {
        Effect();

        yield return WaitForSecondsPausable(duration);

        EndProtocol();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        var enemyBullet = collision.GetComponent<EnemyBullet>();

        if (enemyBullet == null)
            return;

        gun.ReflectAttack(enemyBullet.transform.position, -enemyBullet.velocity.normalized, enemyBullet.velocity.magnitude);
        enemyBullet.DestroyBullet();
    }

    internal override void EndProtocol()
    {
        EndEffect();

        isActive = false;
        protocolRoutine = null;
    }


    private void Effect()
    {
        bladerEffectAnim.speed = 3f / duration; //애니메이션이 3초이므로 duration 만큼 시간 조절
        bladerEffectSR.gameObject.SetActive(true);
        Color magenta = new Color(1f, 0f, 1f);

        magenta.a = 0.5f;
        bladerEffectSR.color = magenta;
    }


    private void EndEffect()
    {
        bladerEffectSR.gameObject.SetActive(false);
        bladerEffectSR.color = Color.white;
    }

    private IEnumerator WaitForSecondsPausable(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (!GameManager.Instance.IsPaused)
                timer += Time.deltaTime;
            yield return null;
        }
    }
}
