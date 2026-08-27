using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

public partial class BossController : MonoBehaviour, IDamageable
{
    # region 공격 패턴 설정
    
    private void AttackPhaseOne(BossAttack attack) // 1개의 공격 타입
    {
        currentAttack = attack;
        attackIncludesLandMine = (attack == BossAttack.LandMine);
        
        switch (attack)
        {
            case BossAttack.Shoot:
                Debug.Log("AttackPhaseOne: Shoot");
                ChangeStat(BossStat.PhaseOneFire);
                ShootAttackStart();
                break;
            case BossAttack.LandMine:
                Debug.Log("AttackPhaseOne: LandMine");
                ChangeStat(BossStat.PhaseOneMove);
                StartLandMineAttack();
                break;
            case BossAttack.SpawnUser:
                Debug.Log("AttackPhaseOne: SpawnUser");
                ChangeStat(BossStat.PhaseOneMove);
                LandUserAttack();
                ChangeStat(BossStat.Wait);
                break;
        }
    }

    private void AttackPhaseTwo(BossPattern pattern) // 2개의 공격 타입
    {
        currentPattern = pattern;
        attackIncludesLandMine = (pattern == BossPattern.ShootAndLandMine || pattern == BossPattern.LandMineAndUser);
        
        switch (pattern)
        {
            case BossPattern.ShootAndLandMine:
                Debug.Log("AttackPhaseTwo: ShootAndLandMine");
                ChangeStat(BossStat.PhaseTwoFire);
                ShootAttackStart();
                StartLandMineAttack();
                break;
            case BossPattern.ShootAndUser:
                Debug.Log("AttackPhaseTwo: ShootAndUser");
                ChangeStat(BossStat.PhaseTwoFire);
                ShootAttackStart();
                LandUserAttack();
                break;
            case BossPattern.LandMineAndUser:
                Debug.Log("AttackPhaseTwo: LandMineAndUser");
                ChangeStat(BossStat.PhaseTwoMove);
                StartLandMineAttack();
                LandUserAttack();
                break;
        }
    }
    
    # endregion
    
    # region 탄막 공격
    
    private void ShootAttackStart()
    {
        isFire = true;
        enemyShooter.StartShooting(target.transform);
    }
    
    private void ShootAttackEnd()
    {
        isFire = false;
        enemyShooter.StopShooting();
    }

    private void OnReloadStart()
    {
        ShootAttackEnd();
        ChangeStat(BossStat.Wait);
    }

    private void OnShootIntervalStart()
    {
        anim.SetBool("isFire", false);
        anim.SetBool("isMove", true);
        isMovingDuringInterval = true;
    }

    private void OnShootIntervalEnd()
    {
        anim.SetBool("isFire", true);
        anim.SetBool("isMove", false);
        isMovingDuringInterval = false;
    }
    
    # endregion

    # region 지뢰 설치 공격
    
    private void StartLandMineAttack()
    {
        mineSpawnedCount = 0;
        mineTargetCount = Random.Range(2, 4);
        mineDropTimer = 0f; // 바로 1개 설치되도록
        landMineElapsed = 0f;
    }

    private void LandMineAttack()
    {
        landMineElapsed += Time.fixedDeltaTime * GameTime.WorldTimeScale; // 지뢰 설치 사이 간격
        mineDropTimer -= Time.fixedDeltaTime * GameTime.WorldTimeScale; // 지뢰 설치 최대 시간(초)

        if (mineDropTimer <= 0f)
        {
            if (enemyPlacer.PlaceMineNear(target.position, 2f, 4.5f))
                mineSpawnedCount++;
            mineDropTimer = mineDropInterval;
        }

        if (mineSpawnedCount >= mineTargetCount || landMineElapsed >= landMineMaxTime)
            ChangeStat(BossStat.Wait);
    }
    
    # endregion
    
    # region 유저 공격
    
    private void LandUserAttack() // 유저 사출
    {
        float firstAngle = Random.Range(0f, 360f);

        // 1번 개체: 실패해도 마지막엔 강제로 스폰
        Vector2 firstPos = FindValidSpawnPos(firstAngle, out bool firstFound);
        if (!firstFound)
            firstPos = GetSpawnPosAtAngle(firstAngle, userSpawnMinRadius); // 벽 무시하고 강제 소환

        SpawnUser(firstPos);

        // 2번 개체: 1번과 각도 차이를 두고 시도, 실패하면 스킵하기
        float secondAngle = firstAngle + userMinAngleGap * (Random.value < 0.5f ? 1f : -1f);
        Vector2 secondPos = FindValidSpawnPos(secondAngle, out bool secondFound);
        if (secondFound) SpawnUser(secondPos);
    }

    // 주어진 각도 기준으로 반경 내 랜덤 위치를 뽑아 벽 체크, 성공하면 위치와 true 반환
    private Vector2 FindValidSpawnPos(float baseAngle, out bool found)
    {
        for (int i = 0; i < userSpawnMaxAttempts; i++)
        {
            // 각도에 약간의 편차를 줘서 매번 완전히 동일한 위치가 나오지 않도록
            float angle = baseAngle + Random.Range(-15f, 15f);
            float radius = Random.Range(userSpawnMinRadius, userSpawnMaxRadius);
            Vector2 candidate = GetSpawnPosAtAngle(angle, radius);

            if (!Physics2D.OverlapCircle(candidate, userSpawnCollisionRadius, wallLayer))
            {
                found = true;
                return candidate;
            }
        }

        found = false;
        return Vector2.zero;
    }

    private Vector2 GetSpawnPosAtAngle(float angleDeg, float radius)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        return (Vector2)transform.position + offset;
    }

    private void SpawnUser(Vector2 pos)
    {
        GameObject user = Instantiate(userEnemy);
        user.GetComponent<UserEnemyController>().SpawnForBoss(pos);
    }
    
    # endregion
}
