using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladerProtocol : ProtocolBase
{
    GameObject curBullet;
    float duration;

    new Dictionary<ProtocolCard.Buff, float> buffValues = new Dictionary<ProtocolCard.Buff, float>()
    {
        { ProtocolCard.Buff.LessCoolTime, 1f },
        { ProtocolCard.Buff.KillToCool, 1f },
        { ProtocolCard.Buff.DeadmanSwitch, 1f },
        { ProtocolCard.Buff.BloodLeak, 1f },
    };

    private void Awake()
    {
        speedMultiplier = 1.5f; 
        isInvincible = true;
    }

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
        yield return new WaitForSeconds(duration);
        EndProtocol();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        var enemyBullet = collision.GetComponent<EnemyBullet>();

        if (enemyBullet == null || curBullet == enemyBullet.gameObject)
            return;

        curBullet = enemyBullet.gameObject;

        BulletBase newBullet = Instantiate(collision.gameObject, enemyBullet.transform.position, Quaternion.identity).AddComponent<BulletBase>();
        Destroy(newBullet.GetComponent<EnemyBullet>());
        newBullet.gameObject.name = "duplicateBullet";
        //newBullet.Init(1f, enemyBullet.velocity.magnitude);
        //newBullet.OnFire(-enemyBullet.velocity.normalized);
    }

    internal override void EndProtocol()
    {
        isActive = false;
    }

}
