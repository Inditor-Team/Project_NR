using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class EnemyBaseController : MonoBehaviour, IDamageable
{
    [SerializeField] protected Transform[] patrolPoints; // 지점별 순찰 방식, NavMeshAgent는 일단 보류
    [SerializeField] protected LayerMask wallLayer; 
    [SerializeField] protected GameObject detectEffect;
    [SerializeField] protected EnemyScope detectScope;
    [SerializeField] protected EnemyDataBase data;
    
    [Header("체력 UI 관련")]
    [SerializeField] protected GameObject healthUI;
    [SerializeField] protected Slider healthSlider;
    
    protected Transform target;
    
    // 스프라이트 관련
    protected SpriteRenderer sprite;
    protected Animator anim;
    protected Vector2 nextvec;
    protected Collider2D collider;
    
    protected int currentPatrolIndex; // 순찰 지점 인덱스
    protected Transform patrolNextPosition;
    protected Rigidbody2D rigid;
    
    // 기본 속성
    protected float defaultSpeed;
    protected float maxHealth;
    protected float health;
    protected float damage;
    
    protected bool isPaused; // 정지 관련
    
    // 랜덤 순찰(배회?)
    protected float patrolRadius = 3f; // 순찰 돌아다니는 반경
    protected float arriveDist = 0.2f; // 도착 판점 위치
    protected float patrolWaitTimeMin = 0.5f; 
    protected float patrolWaitTimeMax = 1.5f;

    protected Vector2 originPos;      // 배회 기준
    protected Vector2 randomPatrolTarget;   // 순찰 목적지
    protected bool hasPatrolTarget;   // 목적지가 있는가
    protected float patrolWaitTimer;  
    
    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();

        defaultSpeed = data.moveSpeed;
        maxHealth = data.health;
        health = data.health;
        damage = data.damage;
        
        healthSlider.value = health / maxHealth;
        isPaused = false;
    }

    protected virtual void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPauseGame += Pause;

        target = GameManager.Instance.Player.gameObject.transform;
    }

    protected virtual void OnEnable()
    {
        detectScope.OnScopeTriggerEnter += OnScopeEnter;

        // 재설정
        health = maxHealth;
        healthSlider.value = health / maxHealth;
        healthUI.SetActive(false);
        collider.isTrigger = false;
        
        originPos = transform.position; // 활성화된 시점의 위치를 기준점으로 고정
        hasPatrolTarget = false;
        patrolWaitTimer = 0f;
        
        ResetStateMachine();
    }

    protected virtual void OnDisable()
    {
        detectScope.OnScopeTriggerEnter -= OnScopeEnter;
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPauseGame -= Pause;
    }

    public virtual void TakeDamage(float damegeAmount)
    {
        health -= damegeAmount;

        SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Hit);

        if (health <= 0)
        {
            healthSlider.value = 0;
            OnHealthDepleted(); // 판정은 base 에서
            return;
        }

        sprite.DOColor(Color.red, 0.2f).OnComplete(() =>
        {
            sprite.DOColor(Color.white, 0.2f);
        });
        healthSlider.value = health / maxHealth;
    }
    
    protected void DoPatrol(bool isFliped)
    {
        // 나중에 고정 순찰 삭제?
        if (patrolPoints != null && patrolPoints.Length > 0)
            DoFixedPatrol(isFliped);
        else
            DoRandomPatrol(isFliped);
    }

    protected void DoFixedPatrol(bool isFliped)
    {
        patrolNextPosition = patrolPoints[currentPatrolIndex];
        Vector2 dir = patrolNextPosition.position - transform.position;
        Vector2 normalizedDir = dir.normalized; // 애니메이션용 벡터
    
        nextvec = normalizedDir * defaultSpeed * Time.fixedDeltaTime * GameTime.WorldTimeScale; // 이동용
            
        rigid.MovePosition(rigid.position + nextvec);
        
        if (isFliped)
            sprite.flipX = normalizedDir.x > 0f;
        else
            sprite.flipX = normalizedDir.x < 0f;
        
        if (Vector3.Distance(transform.position, patrolNextPosition.position) < arriveDist) // 근처 도착
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
    
    protected void DoRandomPatrol(bool isFliped)
    {
        if (patrolWaitTimer > 0f) // 대기중
        {
            // TODO: 대기 중엔 idle 애니메이션 재생
            patrolWaitTimer -= Time.fixedDeltaTime * GameTime.WorldTimeScale;
            return;
        }

        if (!hasPatrolTarget) // 랜덤 목적지 뽑기
        {
            hasPatrolTarget = TryPickWanderTarget(out randomPatrolTarget);
            if (!hasPatrolTarget) return; 
        }

        Vector2 dir = randomPatrolTarget - (Vector2)transform.position;
        Vector2 normalizedDir = dir.normalized;

        nextvec = normalizedDir * defaultSpeed * Time.fixedDeltaTime * GameTime.WorldTimeScale;
        rigid.MovePosition(rigid.position + nextvec);

        if (isFliped)
            sprite.flipX = normalizedDir.x > 0f;
        else
            sprite.flipX = normalizedDir.x < 0f;

        if (dir.magnitude < arriveDist) // 목적지 도착
        {
            hasPatrolTarget = false;
            patrolWaitTimer = Random.Range(patrolWaitTimeMin, patrolWaitTimeMax); // 대기
        }
    }

    private bool TryPickWanderTarget(out Vector2 result)
    {
        const int maxAttempts = 8; // 무한루프 방지용, 일단 8번 시도

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
            Vector2 randomPos = originPos + randomOffset;

            // 벽 체크
            if (Physics2D.OverlapCircle(randomPos, 0.1f, wallLayer)) continue; // 벽이랑 겹치는 가?
            if (Physics2D.Linecast(transform.position, randomPos, wallLayer)) continue; // 가는 길에 벽이 있는가?

            result = randomPos;
            return true;
        }

        result = Vector2.zero;
        return false;
    }
    
    protected void DetectPlayer() // 플레이어 감지 - 공통 로직
    {
        detectEffect.SetActive(true);
        detectEffect.transform.DOLocalMoveY(detectEffect.transform.localPosition.y + 1.0f, 0.5f)
            .SetEase(Ease.OutCubic).OnComplete(() =>
            {
                detectEffect.transform.position = gameObject.transform.position;
                detectEffect.SetActive(false);

                if (!IsCurrentlyDetecting()) return; // 자식에게 위임

                if (!healthUI.activeSelf) healthUI.SetActive(true);
                OnDetectComplete(); // 자식에게 위임
            });
    }
    
    public virtual void SetDead()
    {
        healthUI.SetActive(false);
        collider.isTrigger = true; // 충돌 무시
        anim.SetTrigger("isDead");
        SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Dead);
    }

    public void OnDeadAnimationOver() // dead 애니메이션 재생 종료 후 호출 
    {
        gameObject.SetActive(false);
        SectorManager.Instance.DestroyedEnemy(); 
    }

    public abstract void Pause(bool isPause);
    protected abstract void OnHealthDepleted();
    protected abstract void OnScopeEnter(Collider2D other);
    protected abstract void ResetStateMachine();
    
    // Detect 상태 관련
    protected abstract bool IsCurrentlyDetecting(); // detect 상태인가?
    protected abstract void OnDetectComplete(); // detect가 끝났나?
}
