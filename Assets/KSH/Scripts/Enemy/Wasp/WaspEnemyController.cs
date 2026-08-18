using UnityEngine;

public class WaspEnemyController : EnemyBaseController
{
    [SerializeField] private EnemyShooter enemyShooter;
    
    // 횡 이동
    private bool isRightSide = true;
    private Vector2 startPos;
    private float sideLimit = 3f; // 몇 만큼 횡 이동 하는 지
    private float combatTargetDist = 5f; // 플레이어와 떨어진 간격
    
    // 재장전
    private float reloadSpeed;
    private float reloadTargetDist = 10f;
    
    private float correctionFactor = 0.5f; // 보정 계수

    // FSM 관련 변수
    private enum WaspStat
    {
        Patrol,
        Detect,
        Combat,
        Reloading,
        Dead
    }
    WaspStat currentStat = WaspStat.Patrol;

    protected override void Awake()
    {
        base.Awake();
        startPos = transform.position;
        enemyShooter.SetDamage(damage);
        
        reloadSpeed = defaultSpeed * 3f; // 일반 이동 속도의 3배
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        enemyShooter.OnReloadStart += OnReloadStart;
        enemyShooter.OnReloadEnd += OnReloadEnd;
        isRightSide = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        enemyShooter.OnReloadStart -= OnReloadStart;
        enemyShooter.OnReloadEnd -= OnReloadEnd;
    }
    
    public override void TakeDamage(float damegeAmount)
    {
        if (currentStat == WaspStat.Patrol) // 순찰 중에 피격되면 Combat으로 전환
            ChangeStat(WaspStat.Detect);
        
        if (currentStat == WaspStat.Dead) return; // 이미 Dead면 중복 실행되지 않도록 처리
        
        base.TakeDamage(damegeAmount);
    }

    private void FixedUpdate()
    {
        if (isPaused) return;
        switch (currentStat)
        {
            case WaspStat.Patrol:
                DoPatrol(true);
                break;
            case WaspStat.Detect:
                DetectPlayer();
                break;
            case WaspStat.Combat:
                Move(defaultSpeed, combatTargetDist, true);
                break;
            case WaspStat.Reloading:
                Move(reloadSpeed, reloadTargetDist, false);
                break;
        }
    }

    private void ChangeStat(WaspStat newStat)
    {
        // Exit 처리 필요하면 나중에 추가
        currentStat = newStat;
        switch (currentStat)
        {
            case WaspStat.Combat: 
                if (!healthUI.activeSelf) healthUI.SetActive(true);
                enemyShooter.StartShooting(target.transform);
                startPos = transform.position;  // 시작 위치 초기화
                break;
            case WaspStat.Reloading:
                enemyShooter.StopShooting();
                // 애니메이션 재생
                break;
            case WaspStat.Dead: // 한 번만 실행이라 여기서 동작
                SetDead();
                break;
        }
    }
    
    private void Move(float speed, float targetDist, bool useSideWalk)
    {
        // 목표 거리 계산
        Vector2 dirToPlayer = (Vector2)target.transform.position - (Vector2)transform.position; // 위치 차이를 나타내는 벡터
        float currentDist = dirToPlayer.magnitude - targetDist; // 현재 거리 차이, 음수면 가깝고 양수면 멀음
        Vector2 normalizedDir = dirToPlayer.normalized; // 방향만
            
        // 간격 보정 벡터
        Vector2 gapVector = normalizedDir * currentDist * correctionFactor; // 마지막은 보정 계수(임시로 0.5로 설정)

        Vector2 sideVec = Vector2.zero;
        
        if (useSideWalk) // 추적 상태만 횡 이동
        {
            Vector2 sideAxis = new Vector2(normalizedDir.y, -normalizedDir.x);
            
            Vector2 displacement = (Vector2)transform.position - startPos; // 시작과 비교하여 얼마나 이동하였는지
            float sideDist = Vector2.Dot(displacement, sideAxis); 
                
            if (sideDist > sideLimit || sideDist < -sideLimit)
            {
                isRightSide = !isRightSide;
                startPos = transform.position; // 기준점 갱신
            }
                
            sideVec = isRightSide ? sideAxis : -sideAxis;
        }
        
        Vector2 finalMove = (sideVec + gapVector).normalized * speed * Time.fixedDeltaTime * GameTime.WorldTimeScale;
        
        rigid.linearVelocity = Vector2.zero;
        rigid.MovePosition(rigid.position + finalMove);
        
        sprite.flipX = normalizedDir.x > 0f;
    }

    private void OnReloadStart()
    {
        ChangeStat(WaspStat.Reloading);
    }
    
    private void OnReloadEnd()
    {
        ChangeStat(WaspStat.Combat);
    }
    
    protected override void OnScopeEnter(Collider2D other) // 추적 시작
    {
        if (currentStat != WaspStat.Patrol || !other.CompareTag("Player"))
            return;
        
        ChangeStat(WaspStat.Detect);
    }

    public override void SetDead()
    {
        enemyShooter.StopShooting();
        base.SetDead();
    }
    
    public override void Pause(bool isPause)
    {
        bool activeControl = !isPause;
        isPaused = isPause;

        enemyShooter.Pause(isPause); 
        enemyShooter.enabled = activeControl;
    }
    
    protected override void ResetStateMachine() // 상태 초기회
    {
        currentStat = WaspStat.Patrol;
        isRightSide = true;
    }
    
    protected override void OnHealthDepleted()
    {
        ChangeStat(WaspStat.Dead);
    }
    
    protected override bool IsCurrentlyDetecting() => currentStat == WaspStat.Detect;
    protected override void OnDetectComplete() => ChangeStat(WaspStat.Combat);
}
