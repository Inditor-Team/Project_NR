using System.Collections;
using UnityEngine;

public class BlitzProtocol : ProtocolBase
{
    [SerializeField] SwordAttacker swordAttacker;
    [Header("임시 스탯")]
    [SerializeField] float dashSpeed = 30f;
    [SerializeField] float detectRadius = 5f;
    [SerializeField] float swordSpeed = 5f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] int attackCount = 3;
    int index = 0;
    float attackDistance = 2f;

    public override void UpgradeProtocol(ProtocolCard.Buff type, float level)
    {
    }

    internal override void TryProtocol(float duration)
    {
        DoProtocol();
    }

    internal override void DoProtocol()
    {
        if (protocolRoutine != null)
            return;

        protocolRoutine = StartCoroutine(ProtocolTime());
    }

    Coroutine protocolRoutine;
    IEnumerator ProtocolTime()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            //레이어 구분 전 임시
            if (enemy.GetComponent<EnemyController>() == null)
                continue;

            while (enemy != null && Vector2.Distance(transform.position, enemy.transform.position) > attackDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position,enemy.transform.position,dashSpeed * Time.deltaTime);
                yield return null;
            }

            if (enemy != null)
            {
                swordAttacker.Swing();
                enemy.gameObject.SetActive(false);
                index++;

                if (index >= attackCount)
                    yield break;

                yield return new WaitForSeconds(0.05f);
            }
        }

        EndProtocol();
    }

    internal override void EndProtocol()
    {
    }

}
