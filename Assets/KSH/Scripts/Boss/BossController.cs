using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

public partial class BossController : MonoBehaviour, IDamageable
{
    # region 변수 모음 
    
    private enum BossAttack // 기본 공격 패턴
    {
        Shoot = 0,
        LandMine = 1,
        SpawnUser = 2
    }

    private enum BossPattern // 페이즈2 패턴
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
    private BossPattern currentPattern;
    
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
    private Collider2D col;
    private Rigidbody2D rigid;
    private bool isPaused;
    
    // 기본 설정 값 변수
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
    private bool attackIncludesLandMine; // 현재 공격에 지뢰 설치가 포함되는가
    
    // 유저 스폰 관련
    private float userSpawnCollisionRadius = 1.5f;
    private const float userSpawnMinRadius = 2.5f;
    private const float userSpawnMaxRadius = 4f;
    private const float userMinAngleGap = 120f; // 두 개체 간 최소 각도 차이
    private const int userSpawnMaxAttempts = 8;
    
    // 회복 패턴 관련
    private bool isHealed = false;
    private float healStartHealth;
    private float goalDamageAmount = 5f; // 일단 5 데미지 입혀야 힐 취소
    private float healAmount = 15f; // 보스 회복량
    private float healTimer;
    private float healMaxTime = 5f; // 5초 동안 힐 패턴 진입
    
    
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

        if (health <= 0) // 사망
        {
            healthSlider.value = 0;
            ChangeStat(BossStat.Dead);
            return;
        } 
        
        if (health <= maxHealth * 0.25 && !isHealed) // 1회만 진입 가능
        {
             // 회복 패턴 진입
             Debug.Log("회복 패턴 진입");
             ChangeStat(BossStat.Heal);
             // 이펙트나 색상 전환으로 표시?
        } 
        else if (health <= maxHealth / 2 && !isPhaseTwo) // 페이즈 변경
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
        
        // TODO: 사망 효과음 변경?
        SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Dead);
    }

    #region 애니메이션 끝날 때 실행되는 함수

    public void OnDeadAnimationOver() // dead 애니메이션 재생 종료 후 호출 
    {
        // TODO: SectorManager에 중간 보스 제거 메소드 추가 필요, 엔딩 연출 재생도 거기서
        // SectorManager.Instance.DestroyedEnemy(); 
    }

    public void OnPhaseSwitchAnimationOver() // 페이즈 스위치 애니메이션 끝날 시 실행
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
                // TODO: 페이즈 전환 효과음
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
                if (isMovingDuringInterval) Move(); // 총 안쏘는 간격 사이에만 이동
                if (attackIncludesLandMine) LandMineAttack();
                break;
            case BossStat.Heal:
                healTimer += Time.fixedDeltaTime * GameTime.WorldTimeScale;
                if (healTimer >= healMaxTime)
                {
                    if((health - healStartHealth) < goalDamageAmount) // 목표 값보다 데미지 량이 적으면 회복
                        HealHealth();
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
        Vector2 dirToPlayer = (Vector2)target.transform.position - (Vector2)transform.position; // 위치 차이를 나타내는 벡터
        float currentDist = dirToPlayer.magnitude - targetDist; // 현재 거리 차이, 음수면 가깝고 양수면 멀음
        Vector2 normalizedDir = dirToPlayer.normalized; // 방향만
            
        // 간격 보정 벡터
        Vector2 gapVector = normalizedDir * currentDist * correctionFactor; // 마지막은 보정 계수(임시로 0.5로 설정)
        Vector2 finalMove = gapVector.normalized * defaultSpeed * Time.fixedDeltaTime * GameTime.WorldTimeScale;
        
        rigid.linearVelocity = Vector2.zero;
        rigid.MovePosition(rigid.position + finalMove);
    }

    private void HealHealth()
    {
        health += healAmount;
        // 회복은 파란색
        sprite.DOColor(Color.blue, 0.2f).OnComplete(() =>
        {
            sprite.DOColor(Color.white, 0.2f);
        });
        healthSlider.value = health / maxHealth;
        
        Debug.Log("회복 완료 !");
    }
}
