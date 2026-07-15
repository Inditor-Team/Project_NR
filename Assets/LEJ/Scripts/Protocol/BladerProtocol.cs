using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

/// <summary>
/// ***����� ���� EnemyBullet �� Velocity = rigid.linearVelocity; �� ĳ���ؾ���
/// ���ƿ��� �Ѿ��� �ݻ��ϴ� ��������
/// </summary>
public class BladerProtocol : ProtocolBase
{
    [Header("�ӽ� ����")]
    [SerializeField] float duration = 5f;
    [SerializeField] GameObject debug_effect; //�ӽ� �ð� ȿ��

    GameObject curBullet; //���� �ݻ� �� �Ѿ�

    new Dictionary<ProtocolCard.Buff, float> buffValues = new Dictionary<ProtocolCard.Buff, float>()
    {
        { ProtocolCard.Buff.LessCoolTime, 1f },
        { ProtocolCard.Buff.KillToCool, 1f },
        { ProtocolCard.Buff.DeadmanSwitch, 1f },
        { ProtocolCard.Buff.BloodLeak, 1f },
    };

    private void Awake()
    {
        speedMultiplier = 1.5f; //�������� ���� �� �̼� ����
        isInvincible = true; //�������� ���� �� ���� ����
    }

    public override void UpgradeProtocol(ProtocolCard.Buff type, float level)
    {
        if (!buffValues.ContainsKey(type))
            return;

        buffValues[type] = level;
    }

    internal override void TryProtocol()
    {
        DoProtocol();
    }

    internal override void DoProtocol()
    {
        debug_effect.SetActive(true);
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

        //�ε��� ��ü�� enemyBullet �� �ƴ϶��, ��� �� �ε��� enemyBullet �̶�� ����
        var enemyBullet = collision.GetComponent<EnemyBullet>();

        if (enemyBullet == null || curBullet == enemyBullet.gameObject)
            return;

        curBullet = enemyBullet.gameObject; //���� �Ѿ˰� �� ���� ĳ��

        //�� �Ѿ˷� �����ϱ�
        BulletBase newBullet = Instantiate(collision.gameObject, enemyBullet.transform.position, Quaternion.identity).AddComponent<BulletBase>();
        Destroy(newBullet.GetComponent<EnemyBullet>());
        newBullet.gameObject.name = "duplicateBullet";
        //newBullet.Init(1f, enemyBullet.velocity.magnitude); //�ӽ� ������
        //newBullet.OnFire(-enemyBullet.velocity.normalized);
    }

    internal override void EndProtocol()
    {
        debug_effect.SetActive(false);
        isActive = false;
    }

}
