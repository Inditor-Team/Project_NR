using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

public partial class BossController : MonoBehaviour, IDamageable
{
    # region 변수 모음
    
    private enum BossAttack // 일반 공격 종류
    {
        Shoot = 0,
        LandMine = 1,
        SpawnUser = 2
    }

    private enum BossPattern // 페이즈2 공격 패턴
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

    // 현재 스탯 혹은 공격
    private BossStat currentStat;
    private BossAttack currentAttack;
    private BossPattern currentPattern;
    
    [SerializeField] private LayerMask wallLayer; 
    [SerializeField] private EnemyDataBase data;
    
    [Header("체력 UI 관련")]
    [SerializeField] private Slider healthSlider;
    
    [Header("공격 관련")]
    [SerializeField] private EnemyShooter enemyShooter; // 탄막
    [SerializeField] private EnemyPlacer enemyPlacer; // 지뢰 설치
    [SerializeField] private GameObject userEnemy; // 유저 소환
    [SerializeField] private SpawnUserEnemy spawnUserEnemy;
    
    private SpriteRenderer sprite;
    private Animator anim;
    private Vector2 nextvec;
    private Collider2D col;
    private Rigidbody2D rigid;
    private bool isPaused;
    
    // 기본 설정 값
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
    private float wallCheckDist = 0.8f; // 벽 검사 거리
    private float wallCheckRadius = 1.5f;
    
    // 공격 관련
    private Transform target;
    private bool isPhaseTwo = false;
    private bool isFire = false;
    private bool isMovingDuringInterval = false; // 총 발사 간격 중 이동인지
    
    // 지뢰 관련
    private float landMineWaitTime = 3f; // 터지기 전 대기 시간
    public float mineDropInterval = 1.5f; // 지뢰를 뿌리는 간격
    private float mineDropTimer;
    private float landMineElapsed;
    private float landMineMaxTime = 5f; // 강제 지뢰 설치 종료 타이머
    private int mineSpawnedCount; // 현재까지 설치한 개수
    private int mineTargetCount; // 목표 지뢰 설치 개수
    private bool attackIncludesLandMine; // 현재 공격에 지뢰 설치가 포함되어 있는지
    
    // 회복 패턴 관련
    private bool isHealed = false;
    private float healStartHealth;
    private float goalDamageAmount = 5f; // 목표로 하는 피해량, 이정도 피해 입혀야 회복 취소
    private float healAmount = 15f; // 회복량
    private float healTimer;
    private float healMaxTime = 5f; // 5초 안에 목표 피해량 달성
    
    # endregion

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

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
        enemyShooter.SetDamage(damage);
        enemyShooter.OnReloadStart += OnReloadStart;
        enemyShooter.OnShootIntervalStart += OnShootIntervalStart;
        enemyShooter.OnShootIntervalEnd += OnShootIntervalEnd;
        
        enemyPlacer.SetValue(landMineWaitTime, damage);
        enemyPlacer.SetLayerMask(wallLayer);
        
        spawnUserEnemy.SetLayerMask(wallLayer);
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
        
        // 보스 피격 효과음은 에너미랑 같은 효과음? 다르게?
        // SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Hit);

        if (health <= 0) // 사망
        {
            healthSlider.value = 0;
            ChangeStat(BossStat.Dead);
            return;
        } 
        
        if (health <= maxHealth * 0.25 && !isHealed) // 1번만 회복 패턴 들어가게?
        {
             // 회복 패턴 진입
             Debug.Log("회복 패턴 진입");
             ChangeStat(BossStat.Heal);
             // 회복하는 애니메이션 재생?
        } 
        else if (health <= maxHealth / 2 && !isPhaseTwo) // 페이즈 전환
        {
            ChangeStat(BossStat.PhaseSwitch);
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
        col.isTrigger = true; // 충돌 무시
        anim.SetTrigger("isDead");
        
        // 맵에 남은 오브젝트 파괴
        enemyShooter.ClearAllBullets(); // 총알 제거
        enemyPlacer.ClearAllMines(); // 지뢰 제거
        spawnUserEnemy.ClearAllUsers(); // 유저 제거
        
        // TODO: 사망 효과음 변경?
        SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Dead);
    }

    #region 애니메이션 끝날 때 실행되는 함수

    public void OnDeadAnimationOver() // dead 애니메이션 재생 종료 후 호출
    {
        // TODO: SectorManager에 중간 보스 제거 메소드 추가 필요, 엔딩 연출 재생도 거기서
        // SectorManager.Instance.DestroyedEnemy(); 
    }

    public void OnPhaseSwitchAnimationOver() // 페이즈 전환 끝나고 호출되는 함수
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
            case BossStat.PhaseTwoMove:
                anim.SetBool("isFire", false);
                anim.SetBool("isMove", true);
                break;
            case BossStat.PhaseOneFire:
            case BossStat.PhaseTwoFire:
                anim.SetBool("isFire", true);
                anim.SetBool("isMove", false);
                break;
            case BossStat.PhaseSwitch:
                anim.SetTrigger("switchPhase");
                anim.SetBool("isPhaseTwo", true);
                isPhaseTwo = true;
                // TODO: 페이즈 전환 효과음?
                break;
            case BossStat.Heal:
                anim.SetBool("isMove", false); // 정지
                anim.SetBool("isFire", false);
                isHealed = true;
                healStartHealth = health; // 현재 체력 저장
                healTimer = 0f;
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
                Move(); // 페이즈 상관없이 이동은 동일
                if (attackIncludesLandMine) LandMineAttack();
                break;
            case BossStat.PhaseOneFire:
            case BossStat.PhaseTwoFire:
                if (isMovingDuringInterval) Move(); // 발사 간격 사이에 잠깐 이동시키기(이 편이 자연스러움)
                if (attackIncludesLandMine) LandMineAttack();
                break;
            case BossStat.Heal:
                healTimer += Time.fixedDeltaTime * GameTime.WorldTimeScale;
                if (healTimer >= healMaxTime)
                {
                    if((healStartHealth - health) < goalDamageAmount) // 목포 피해량 미달성 시
                        HealHealth(); // 회복
                    ChangeStat(BossStat.Wait);
                }
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
                    BossPattern newPattern = (BossPattern)random;
                    if (newPattern != currentPattern)
                    {
                        waitTimer = 0;
                        AttackPhaseTwo(newPattern);
                        break;
                    }
                }
            }
        }
    }

    private void Move()
    {
        // 목표 거리 계산
        Vector2 dirToPlayer = (Vector2)target.transform.position - (Vector2)transform.position;
        float currentDist = dirToPlayer.magnitude - targetDist;
        Vector2 normalizedDir = dirToPlayer.normalized;

        // 간격 보정 벡터
        Vector2 gapVector = normalizedDir * currentDist * correctionFactor;
        Vector2 desiredDir = gapVector.normalized; // 원래 가려던 방향

        // 벽 회피가 필요한지 체크
        Vector2 finalDir = GetMoveDirWithWallAvoidance(desiredDir);

        Vector2 finalMove = finalDir * defaultSpeed * Time.fixedDeltaTime * GameTime.WorldTimeScale;

        rigid.linearVelocity = Vector2.zero;
        rigid.MovePosition(rigid.position + finalMove);
    }

    private Vector2 GetMoveDirWithWallAvoidance(Vector2 desiredDir)
    {
        if (desiredDir == Vector2.zero) return Vector2.zero;

        // 보스 주위를 반지름으로 체크
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position, wallCheckRadius, desiredDir, wallCheckDist, wallLayer);

        if (hit.collider == null)
            return desiredDir; // 벽 없는 경우 -> 원래 방향 그대로

        // 벽에 막힌 경우 -> 벽 표면을 따라 미끄러지는 방향 계산
        Vector2 slideDir = Vector2.Perpendicular(hit.normal); // 법선 수직인 벡터 방향을 리턴하는 메서드
        if (Vector2.Dot(slideDir, desiredDir) < 0f)
            slideDir = -slideDir; // 원래 가려던 방향과 더 가까운 쪽 선택

        return slideDir.normalized;
    }

    private void HealHealth()
    {
        health += healAmount;
        // TODO: 회복 완료 애니메이션 혹은 이펙트?? 일단 파랗게 점멸
        sprite.DOColor(Color.blue, 0.2f).OnComplete(() =>
        {
            sprite.DOColor(Color.white, 0.2f);
        });
        
        healthSlider.value = health / maxHealth;
        
        Debug.Log("회복 완료 !");
    }
}
