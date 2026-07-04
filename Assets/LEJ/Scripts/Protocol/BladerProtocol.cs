using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

/// <summary>
/// ***사용을 위해 EnemyBullet 에 Velocity = rigid.linearVelocity; 를 캐싱해야함
/// 날아오는 총알을 반사하는 프로토콜
/// </summary>
public class BladerProtocol : ProtocolBase
{
    [Header("임시 스탯")]
    [SerializeField] float duration = 5f;
    [SerializeField] GameObject debug_effect; //임시 시각 효과

    GameObject curBullet; //현재 반사 된 총알

    new Dictionary<ProtocolCard.Buff, float> buffValues = new Dictionary<ProtocolCard.Buff, float>()
    {
        { ProtocolCard.Buff.LessCoolTime, 1f },
        { ProtocolCard.Buff.KillToCool, 1f },
        { ProtocolCard.Buff.DeadmanSwitch, 1f },
        { ProtocolCard.Buff.BloodLeak, 1f },
    };

    private void Awake()
    {
        speedMultiplier = 1.5f; //프로토콜 실행 시 이속 증가
        isInvincible = true; //프로토콜 실행 시 무적 상태
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

        //부딪힌 물체가 enemyBullet 이 아니라면, 방금 전 부딪힌 enemyBullet 이라면 무시
        var enemyBullet = collision.GetComponent<EnemyBullet>();

        if (enemyBullet == null || curBullet == enemyBullet.gameObject)
            return;

        curBullet = enemyBullet.gameObject; //다음 총알과 비교 위해 캐싱

        //내 총알로 생성하기
        BulletBase newBullet = Instantiate(collision.gameObject, enemyBullet.transform.position, Quaternion.identity).AddComponent<BulletBase>();
        Destroy(newBullet.GetComponent<EnemyBullet>());
        newBullet.gameObject.name = "duplicateBullet";
        newBullet.Init(1f, enemyBullet.Velocity.magnitude); //임시 데미지
        newBullet.OnFire(-enemyBullet.Velocity.normalized);
    }

    internal override void EndProtocol()
    {
        debug_effect.SetActive(false);
        isActive = false;
    }

}
