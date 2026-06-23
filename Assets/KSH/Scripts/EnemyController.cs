using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private Transform[] patrolPoints; // 지점별 순찰 방식, NavMeshAgent는 일단 보류
    [SerializeField] private EnemyScope detectScope;
    [SerializeField] private EnemyShooter enemyShooter;

    // [SerializeField] private Transform target; // 나중에 GameManager에서 받아오도록 수정
    
    // 스프라이트 적용
    private SpriteRenderer sprite;
    private Animator anim;
    public Vector2 nextvec;
    private const float MinDirectionMagnitude = 0.05f;
    
    public float speed = 0.5f;
    private bool isPatrol; // 순찰(패트롤) 상태인지 판단 여부
    private int currentPatrolIndex; // 순찰 지점 인덱스
    private Transform patrolNextPosition;
    private Rigidbody2D rigid;

    private void Start()
    {
        isPatrol = true;
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        detectScope.OnScopeTriggerEnter += OnScopeEnter;
        detectScope.OnScopeTriggerStay += OnScopeStay;
    }

    private void OnDisable()
    {
        detectScope.OnScopeTriggerEnter -= OnScopeEnter;
        detectScope.OnScopeTriggerStay -= OnScopeStay;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Take Damage {damage} !");
    }

    private void FixedUpdate()
    {
        if (!isPatrol)
            return;
        
        patrolNextPosition = patrolPoints[currentPatrolIndex];
        Vector2 dir = patrolNextPosition.position - transform.position;
        Vector2 normalizedDir = dir.normalized; // 애니메이션용 벡터

        nextvec = normalizedDir * speed * Time.fixedDeltaTime; // 이동용
        
        rigid.MovePosition(rigid.position + nextvec);
        rigid.linearVelocity = Vector2.zero; // 유니티6는 velocity에서 linearVelocity로 변경, 추후 찾아보기
        
        UpdateDirection(normalizedDir); // 스프라이트 업데이트, 이동용

        if (Vector3.Distance(transform.position, patrolNextPosition.position) < 0.2f) // 근처 도착
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
    
    private void OnScopeEnter(Collider2D other) // 추적 시작
    { 
        if (!other.CompareTag("Player"))
            return;

        isPatrol = false; // 전투 상태
        
        enemyShooter.StartShooting(other.transform);
        
        Vector2 dirToPlayer = (Vector2)other.transform.position - (Vector2)transform.position;
        UpdateDirection(dirToPlayer); // 스프라이트 업데이트, 전투용
    }
    
    private void OnScopeStay(Collider2D other) // 횡 이동
    {
        if (!other.CompareTag("Player"))
            return;
        
        Vector2 dirToPlayer = (Vector2)other.transform.position - (Vector2)transform.position;
        UpdateDirection(dirToPlayer); // 스프라이트 업데이트, 전투용
    }
    
    // 애니메이션 관련
    public void UpdateDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < MinDirectionMagnitude * MinDirectionMagnitude) // 방향 업데이트 여부 계산
            return;

        direction.Normalize();
        
        // TODO: sprite가 정면이나 후면이면 flip 안하도록
        sprite.flipX = direction.x < 0f;

        anim.SetFloat("moveX", Mathf.Abs(direction.x));
        anim.SetFloat("moveY", direction.y);
    }

    [ContextMenu("SetDead")] // 테스트용
    public void SetDead()
    {
        isPatrol = false;
        enemyShooter.StopShooting();
        anim.SetTrigger("isDead");
        // destroy ?
    }
}
