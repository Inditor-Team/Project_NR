using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class EnemyBaseController : MonoBehaviour, IDamageable
{
    [SerializeField] protected Transform[] patrolPoints; // 지점별 순찰 방식, NavMeshAgent는 일단 보류
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
        
        ResetStateMachine();
    }

    protected virtual void OnDisable()
    {
        detectScope.OnScopeTriggerEnter -= OnScopeEnter;
    }

    protected virtual void OnDestroy()
    {
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
    
    protected void DoPatrol()
    {
        patrolNextPosition = patrolPoints[currentPatrolIndex];
        Vector2 dir = patrolNextPosition.position - transform.position;
        Vector2 normalizedDir = dir.normalized; // 애니메이션용 벡터
    
        nextvec = normalizedDir * defaultSpeed * Time.fixedDeltaTime * GameTime.WorldTimeScale; // 이동용
            
        rigid.MovePosition(rigid.position + nextvec);
        
        sprite.flipX = normalizedDir.x > 0f;
    
        if (Vector3.Distance(transform.position, patrolNextPosition.position) < 0.2f) // 근처 도착
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
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
        SpawnManager.Instance.DestroyedEnemy(); 
    }

    public virtual void Pause(bool isPause)
    {
        bool activeControl = !isPause;
        this.enabled = activeControl;
    }

    protected abstract void OnHealthDepleted();
    protected abstract void OnScopeEnter(Collider2D other);
    protected abstract void ResetStateMachine();
}
