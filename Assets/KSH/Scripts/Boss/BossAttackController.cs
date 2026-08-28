using UnityEngine;
using Random = UnityEngine.Random;

public partial class BossController
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
        spawnUserEnemy.SpwanUser();
    }
    
    # endregion
}
