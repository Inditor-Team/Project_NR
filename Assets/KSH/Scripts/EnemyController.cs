using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private Transform[] patrolPoints; // 지점별 순찰 방식, NavMeshAgent는 일단 보류
    [SerializeField] private EnemyScope detectScope;
    [SerializeField] private EnemyShooter enemyShooter;

    [SerializeField] private Transform target; // 나중에 GameManager에서 받아오도록 수정
    
    // 스프라이트 적용
    private SpriteRenderer sprite;
    private Animator anim;
    [SerializeField] private Vector2 nextvec;
    private const float MinDirectionMagnitude = 0.05f;
    
    public float speed = 0.5f;
    private int currentPatrolIndex; // 순찰 지점 인덱스
    private Transform patrolNextPosition;
    private Rigidbody2D rigid;
    
    // 횡 이동
    private bool isRightSide = true;
    private Vector2 startPos;
    private float sideLimit = 3f; // 몇 만큼 횡 이동 하는 지
    
    // 플레이어와의 간격 유지
    public float targetDist = 5f; // 플레이어와 떨어진 간격
    public float correctionFactor = 0.5f; // 보정 계수

    // FSM 관련 변수
    private enum EnemyStat
    {
        Patrol,
        Combat,
        Reloading,
        Dead
    }
    EnemyStat currentStat = EnemyStat.Patrol;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        startPos = transform.position;
    }

    private void OnEnable()
    {
        detectScope.OnScopeTriggerEnter += OnScopeEnter;
        enemyShooter.OnReloadStart += OnReloadStart;
        enemyShooter.OnReloadEnd += OnReloadEnd;
        isRightSide = true;
        currentStat = EnemyStat.Patrol;
    }

    private void OnDisable()
    {
        detectScope.OnScopeTriggerEnter -= OnScopeEnter;
        enemyShooter.OnReloadStart -= OnReloadStart;
        enemyShooter.OnReloadEnd -= OnReloadEnd;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Take Damage {damage} !");
        // 체력 0 이하일시 ChangeStat(EnemyStat.Dead);
    }

    private void FixedUpdate()
    {
        switch (currentStat)
        {
            case EnemyStat.Patrol:
                DoPatrol();
                break;
            case EnemyStat.Combat:
                DoCombat();
                break;
            case EnemyStat.Reloading:
                DoReload();
                break;
        }
    }

    private void ChangeStat(EnemyStat newStat)
    {
        // Exit 처리 필요하면 나중에 추가
        currentStat = newStat;
        switch (currentStat)
        {
            case EnemyStat.Combat: 
                startPos = transform.position;  // 시작 위치 초기화
                break;
            case EnemyStat.Dead: // 한 번만 실행이라 여기서 동작
                SetDead();
                break;
        }
    }

    private void DoPatrol()
    {
        patrolNextPosition = patrolPoints[currentPatrolIndex];
        Vector2 dir = patrolNextPosition.position - transform.position;
        Vector2 normalizedDir = dir.normalized; // 애니메이션용 벡터
    
        nextvec = normalizedDir * speed * Time.fixedDeltaTime; // 이동용
            
        rigid.MovePosition(rigid.position + nextvec);
            
        UpdateDirection(normalizedDir); // 스프라이트 업데이트, 이동용
    
        if (Vector3.Distance(transform.position, patrolNextPosition.position) < 0.2f) // 근처 도착
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void DoCombat()
    {
        // 목표 거리 계산
        Vector2 dirToPlayer = (Vector2)target.transform.position - (Vector2)transform.position; // 위치 차이를 나타내는 벡터
        float currentDist = dirToPlayer.magnitude - targetDist; // 현재 거리 차이, 음수면 가깝고 양수면 멀음
        Vector2 normalizedDir = dirToPlayer.normalized; // 방향만
            
        // 간격 보정 벡터
        Vector2 gapVector = normalizedDir * currentDist * correctionFactor; // 마지막은 보정 계수(임시로 0.5로 설정)

        Vector2 sideAxis = new Vector2(normalizedDir.y, -normalizedDir.x);
            
        Vector2 displacement = (Vector2)transform.position - startPos; // 시작과 비교하여 얼마나 이동하였는지
        float sideDist = Vector2.Dot(displacement, sideAxis); 
            
        if (sideDist > sideLimit || sideDist < -sideLimit)
        {
            isRightSide = !isRightSide;
            startPos = transform.position; // 기준점 갱신
        }
            
        Vector2 sideVec = isRightSide ? sideAxis : -sideAxis;
            
        Vector2 finalMove = (sideVec + gapVector).normalized * speed * Time.fixedDeltaTime;
            
        // rigid.linearVelocity = Vector2.zero; -> rigidbody bodytype을 kinematic으로 변경
        rigid.MovePosition(rigid.position + finalMove);
            
        UpdateDirection(normalizedDir); // 스프라이트 업데이트, 전투용
    }

    private void DoReload()
    {
        Debug.Log("reload!");
    }

    private void OnReloadStart()
    {
        ChangeStat(EnemyStat.Reloading);
    }
    
    private void OnReloadEnd()
    {
        ChangeStat(EnemyStat.Combat);
    }
    
    private void OnScopeEnter(Collider2D other) // 추적 시작
    { 
        if (!other.CompareTag("Player"))
            return;

        ChangeStat(EnemyStat.Combat);
        enemyShooter.StartShooting(other.transform);
        
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

    public void SetDead()
    {
        enemyShooter.StopShooting();
        anim.SetTrigger("isDead");
        // destroy ?
    }
}
