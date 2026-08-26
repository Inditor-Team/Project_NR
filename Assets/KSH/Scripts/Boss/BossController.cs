using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

public class BossController : MonoBehaviour, IDamageable
{
    private enum BossAttack // 기본 공격 패턴
    {
        Shoot = 0,
        LandMine = 1,
        SpawnUser = 2
    }

    private enum BossPatern // 페이즈2 패턴
    {
        ShootAndLandMine = 0,
        ShootAndUser = 1,
        LandMineAndUser = 2
    }
    
    private enum BossStat
    {
        Wait, // 공격 중 대기
        PhaseOneMove, // 페이즈 1 이동
        PhaseOneFire, // 페이즈 1 탄막 공격, 이동 정지(유저 사출, 지뢰는 이동 중 공격) -> 지우고 코드로 처리?
        PhaseTwoMove, // 페이즈 2 이동 
        PhaseTwoFire, // 페이즈 2 탄막 공격
        PhaseSwitch, // 페이즈 변경
        Heal, // 회복 패턴
        Dead // 사망, hp = 0
    }

    // 현재 스탯 및 공격 정보
    private BossStat currentStat;
    private BossAttack currentAttack;
    private BossPatern currentPattern;
    
    [SerializeField] private LayerMask wallLayer; 
    [SerializeField] private EnemyDataBase data;
    
    [Header("체력 UI 관련")]
    [SerializeField] private Slider healthSlider;
    
    [Header("공격 관련")]
    [SerializeField] private EnemyShooter enemyShooter; // 탄막
    [SerializeField] private EnemyPlacer enemyPlacer; // 지뢰
    [SerializeField] private GameObject userEnemy; // 유저 소환
    
    private SpriteRenderer sprite;
    private Animator anim;
    private Vector2 nextvec;
    private Collider2D collider;
    private Rigidbody2D rigid;
    private bool isPaused;
    
    // 에너미랑 같은 구조
    private float defaultSpeed;
    private float maxHealth;
    private float health;
    private float damage;
    
    // 대기 (공격 패턴 선택)
    private float waitTIme = 1.5f; // 대기 시간(얼마나 대기할지)
    private float waitTimer; // 대기 시간 타이머
    
    // 이동
    private float targetDist = 5f; // 플레이어와 떨어진 간격
    private float correctionFactor = 0.5f; // 보정 계수
    
    // 공격 관련
    private Transform target;
    private bool isPhaseTwo = false;
    private bool isFire = false;
    private bool isMovingDuringInterval = false; // 사격 중 이동
    
    // 지뢰 관련
    private float landMineWaitTime = 3f; // 터지기 전 대기 시간
    public float mineDropInterval = 1.5f; // 지뢰를 뿌리는 간격
    private float mineDropTimer;
    private float landMineElapsed;
    private float landMineMaxTime = 5f; // 강제 지뢰 설치 종료 타이머
    private int mineSpawnedCount; // 현재까지 설치한 개수
    private int mineTargetCount; // 목표 지뢰 설치 개수

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();

        defaultSpeed = data.moveSpeed;
        maxHealth = data.health;
        health = data.health;
        damage = data.damage;
        currentStat = BossStat.Wait;
        
        healthSlider.value = health / maxHealth;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPauseGame += Pause;
            target = GameManager.Instance.Player.gameObject.transform;
        }
        enemyShooter.OnReloadStart += OnReloadStart;
        enemyShooter.OnShootIntervalStart += OnShootIntervalStart;
        enemyShooter.OnShootIntervalEnd += OnShootIntervalEnd;
        
        enemyPlacer.SetValue(landMineWaitTime, damage);
        enemyPlacer.SetLayerMask(wallLayer);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPauseGame -= Pause;
        
        enemyShooter.OnReloadStart -= OnReloadStart;
        enemyShooter.OnShootIntervalStart -= OnShootIntervalStart;
        enemyShooter.OnShootIntervalEnd -= OnShootIntervalEnd;
    }
    
    private void Pause(bool isPause) // 일시 정지
    {
        bool activeControl = !isPause;
        isPaused = isPause;

        enemyShooter.Pause(isPause); 
        enemyShooter.enabled = activeControl;
    }

    public virtual void TakeDamage(float damegeAmount)
    {
        if (currentStat == BossStat.PhaseSwitch || currentStat == BossStat.Dead)
            return;
        
        health -= damegeAmount;

        // 보스 피격 효과음은 에너미랑 똑같이?
        // SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Hit);

        if (health <= maxHealth / 2 && !isPhaseTwo) // 페이즈 변경
        {
            isPhaseTwo = true;
            ChangeStat(BossStat.PhaseSwitch);
        }

        if (health <= maxHealth * 0.25)
        {
            // 회복 패턴 진입
            Debug.Log("회복 패턴 진입");
            ChangeStat(BossStat.Heal);
        }
        
        if (health <= 0) // 사망
        {
            healthSlider.value = 0;
            ChangeStat(BossStat.Dead);
            return;
        }

        sprite.DOColor(Color.red, 0.2f).OnComplete(() =>
        {
            sprite.DOColor(Color.white, 0.2f);
        });
        healthSlider.value = health / maxHealth;
    }
    
    private void SetDead()
    {
        enemyShooter.StopShooting();
        collider.isTrigger = true; // 충돌 무시
        anim.SetTrigger("isDead");
        
        // TODO: 사망 효과음 변경?
        SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Dead);
    }

    #region 애니메이션 끝날 때 실행되는 함수

    public void OnDeadAnimationOver() // dead 애니메이션 재생 종료 후 호출 
    {
        // TODO: SectorManager에 중간 보스 제거 메소드 추가 필요, 엔딩 연출 재생도 거기서
        // SectorManager.Instance.DestroyedEnemy(); 
    }

    public void OnPhaseSwitchAnimationOver()
    {
        ChangeStat(BossStat.Wait);
    }

    #endregion
    
    private void ChangeStat(BossStat newStat)
    {
        currentStat = newStat;
        if(isFire) ShootAttackEnd(); // 혹시 모르는 처리
        Debug.Log($"BossStat : {newStat}");
        
        switch (newStat)
        {
            case BossStat.Wait:
                anim.SetBool("isFire", false);
                anim.SetBool("isMove", false);
                break;
            case BossStat.PhaseOneMove:
                anim.SetBool("isFire", false);
                anim.SetBool("isMove", true);
                break;
            case BossStat.PhaseOneFire:
                anim.SetBool("isFire", true);
                anim.SetBool("isMove", false);
                break;
            case BossStat.PhaseTwoMove:
                anim.SetBool("isFire", false);
                anim.SetBool("isMove", true);
                break;
            case BossStat.PhaseTwoFire:
                anim.SetBool("isFire", true);
                anim.SetBool("isMove", false);
                break;
            case BossStat.PhaseSwitch:
                anim.SetTrigger("switchPhase");
                anim.SetBool("isPhaseTwo", true);
                // TODO: 페이즈 전환 효과음
                break;
            case BossStat.Heal:
                anim.SetBool("isMove", false); // 정지
                // 맞으면 되돌아오게 (hp얼마나 깎여야?)
                // 데미지 n만큼 받으면 바로 패턴 해제 or m초 동안 무조건 정지하고 n만큼 피해 받아야 회복 안됨
                break;
            case BossStat.Dead:
                SetDead();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (isPaused) return;
        switch (currentStat)
        {
            case BossStat.Wait:
                Wait();
                break;
            // 이동
            case BossStat.PhaseOneMove:
            case BossStat.PhaseTwoMove:
                // 페이즈 상관없이 이동은 동일
                if (currentAttack == BossAttack.LandMine) LandMineAttack();
                Move();
                break;
            case BossStat.PhaseOneFire:
            case BossStat.PhaseTwoFire:
                if (isMovingDuringInterval) Move();
                if (currentPattern == BossPatern.LandMineAndUser ||
                    currentPattern == BossPatern.ShootAndLandMine) LandMineAttack();
                break;
        }
    }

    private void Wait()
    {
        waitTimer += Time.fixedDeltaTime * GameTime.WorldTimeScale;
        if (waitTimer >= waitTIme)
        {
            // 공격 패턴 선택하기
            while (true)
            {
                int random = Random.Range(0, 3); // 0 ~ 2

                if (!isPhaseTwo)
                {
                    BossAttack newAttack = (BossAttack)random;
                    if (newAttack != currentAttack)
                    {
                        waitTimer = 0;
                        AttackPhaseOne(newAttack);
                        break;
                    }
                    
                }

                if (isPhaseTwo)
                {
                    BossPatern newPatern = (BossPatern)random;
                    if (newPatern != currentPattern)
                    {
                        waitTimer = 0;
                        AttackPhaseTwo(newPatern);
                        break;
                    }
                }
            }
        }
    }

    private void Move()
    {
        // 목표 거리 계산
        Vector2 dirToPlayer = (Vector2)target.transform.position - (Vector2)transform.position; // 위치 차이를 나타내는 벡터
        float currentDist = dirToPlayer.magnitude - targetDist; // 현재 거리 차이, 음수면 가깝고 양수면 멀음
        Vector2 normalizedDir = dirToPlayer.normalized; // 방향만
            
        // 간격 보정 벡터
        Vector2 gapVector = normalizedDir * currentDist * correctionFactor; // 마지막은 보정 계수(임시로 0.5로 설정)
        Vector2 finalMove = gapVector.normalized * defaultSpeed * Time.fixedDeltaTime * GameTime.WorldTimeScale;
        
        rigid.linearVelocity = Vector2.zero;
        rigid.MovePosition(rigid.position + finalMove);
    }

    private void AttackPhaseOne(BossAttack attack) // 1개의 공격 타입
    {
        currentAttack = attack;
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
                break;
        }
    }

    private void AttackPhaseTwo(BossPatern patern) // 2개의 공격 타입
    {
        currentPattern = patern;
        switch (patern)
        {
            case BossPatern.ShootAndLandMine:
                Debug.Log("AttackPhaseTwo: ShootAndLandMine");
                ChangeStat(BossStat.PhaseTwoFire);
                ShootAttackStart();
                StartLandMineAttack();
                break;
            case BossPatern.ShootAndUser:
                Debug.Log("AttackPhaseTwo: ShootAndUser");
                ChangeStat(BossStat.PhaseTwoFire);
                ShootAttackStart();
                LandUserAttack();
                break;
            case BossPatern.LandMineAndUser:
                Debug.Log("AttackPhaseTwo: LandMineAndUser");
                ChangeStat(BossStat.PhaseTwoMove);
                StartLandMineAttack();
                LandUserAttack();
                break;
        }
    }
    
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

    private void StartLandMineAttack()
    {
        mineSpawnedCount = 0;
        mineTargetCount = Random.Range(2, 4);
        mineDropTimer = 0f; // 바로 1개 설치되도록
        landMineElapsed = 0f;
    }

    private void LandMineAttack()
    {
        landMineElapsed += Time.fixedDeltaTime * GameTime.WorldTimeScale;
        mineDropTimer -= Time.fixedDeltaTime * GameTime.WorldTimeScale;

        if (mineDropTimer <= 0f)
        {
            if (enemyPlacer.PlaceMineNear(target.position, 2f, 4.5f))
                mineSpawnedCount++;
            mineDropTimer = mineDropInterval;
        }

        if (mineSpawnedCount >= mineTargetCount || landMineElapsed >= landMineMaxTime)
            ChangeStat(BossStat.Wait);
    }

    private float userSpawnCollisionRadius = 1.5f;
    private const float userSpawnMinRadius = 2.5f;
    private const float userSpawnMaxRadius = 4f;
    private const float userMinAngleGap = 120f; // 두 개체 간 최소 각도 차이
    private const int userSpawnMaxAttempts = 8;

    private void LandUserAttack() // 유저 사출
    {
        float firstAngle = Random.Range(0f, 360f);

        // 첫 번째 개체: 실패해도 마지막엔 강제로 스폰 (최소 1개체 보장)
        Vector2 firstPos = FindValidSpawnPos(firstAngle, out bool firstFound);
        if (!firstFound)
            firstPos = GetSpawnPosAtAngle(firstAngle, userSpawnMinRadius); // 폴백: 벽 무시하고 강제 위치

        SpawnUser(firstPos);

        // 두 번째 개체: 첫 번째와 각도 차이를 두고 시도, 실패하면 스킵
        float secondAngle = firstAngle + userMinAngleGap * (Random.value < 0.5f ? 1f : -1f);
        Vector2 secondPos = FindValidSpawnPos(secondAngle, out bool secondFound);
        if (secondFound) SpawnUser(secondPos);

        ChangeStat(BossStat.Wait);
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
}
