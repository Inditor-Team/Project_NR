using UnityEngine;
using DG.Tweening;

public class MineEnemyController : EnemyBaseController
{
    private enum MineStat
    {
        Patrol,
        Detect, // 플레이어 인지
        LandMine, // 지뢰 뿌리기 상태
        Reset, // 다시 재순찰
        Dead
    }
    MineStat currentStat = MineStat.Patrol;

    private float landMineSpeed; // 지뢰 뿌리는 상태 속도

    [SerializeField] private EnemyPlacer enemyPlacer; 
    
    [Header("도주 관련")]
    [SerializeField] private Transform mapCenter;
    [SerializeField] private float fleeRecalcInterval = 0.75f; // 방향 재계산 간격
    [SerializeField] private float fleeResetDist = 10f; // Reset 상태로 전이하는 간격
    
    [Header("이동 가중치, 임시")]
    [SerializeField] private float weightPlayerDistance = 1.0f;
    [SerializeField] private float weightGoToOpenSpace = 0.6f;
    [SerializeField] private float weightNoise = 0.2f;
    [SerializeField] private float weightAvoidWall = 1.5f;

    private Vector2 currentFleeDir;   // 도주 방향
    private float fleeRecalcTimer;    // 다음 재계산까지 타이머
    
    // 벽에 막혔을 시 재계산 관련
    private Vector2 lastCheckedPos; // 마지막 Pos
    private float stuckTimer; // 벽 막힌 시간 타이머
    private float stuckThreshold = 0.3f; // 몇 초 막혔을 때 재계산?
    private float stuckDistThreshold = 0.02f; // 거리 차이
    
    [Header("벽 회피 관련")]
    [SerializeField] private float wallCheckDistance = 1.2f; // 레이 길이
    private float[] wallCheckAngles = { 0f, 45f, -45f, 90f, -90f }; // 레이 방향
    
    // 리셋 상태 관련
    private float resetCooldown = 10f; // 이정도 시간동안 떨어져있으면 순찰 상태로 변경
    private float resetTimer;
    private float resetConfirmDuration = 0.5f; // 이 시간 동안 플레이어와 떨어져 있어야 Reset으로 변경시키기
    private float resetConfirmTimer;
    
    // 지뢰 관련
    private float waitTime = 3f; // 터지기 전 대기 시간
    public float mineDropInterval = 1.5f; // 지뢰를 뿌리는 간격
    private float mineDropTimer; // 다음 지뢰까지 타이머
    
    // landmine 상태 관련
    private float landMineMinDuration = 3f; // 최소 유지 시간, Reset으로 안빠짐
    private float landMineElapsed; // LandMine 상태 경과 시간
    
    protected override void Awake()
    {
        base.Awake();
        landMineSpeed = defaultSpeed * 3f; // 임시, 순찰이랑 지뢰 뿌리는 속도가 다르게
        enemyPlacer.SetValue(waitTime, damage);
        detectEffect.SetActive(false);
    }
    
    public override void TakeDamage(float damegeAmount)
    {
        if (currentStat == MineStat.Patrol || currentStat == MineStat.Reset) // 순찰 중에 피격되면 인지 -> 지뢰 공격
            ChangeStat(MineStat.Detect);
        
        if (currentStat == MineStat.Dead) return; // 이미 Dead면 중복 실행되지 않도록 처리
        
        base.TakeDamage(damegeAmount);
    }
    
    private void FixedUpdate()
    {
        if (isPaused) return;
        switch (currentStat)
        {
            case MineStat.Patrol:
                DoPatrol(true);
                break;
            case MineStat.LandMine:
                SetLandMine();
                break;
            case MineStat.Reset:
                resetTimer -= Time.fixedDeltaTime * GameTime.WorldTimeScale;
                if (resetTimer <= 0f)
                    ChangeStat(MineStat.Patrol); // 재순찰
                break;
        }
    }

    private void ChangeStat(MineStat newStat)
    {
        // Exit 처리 필요하면 나중에 추가
        Debug.Log($"새 상태 : {newStat}");
        currentStat = newStat;
        switch (currentStat)
        {
            case MineStat.Patrol:
                anim.SetBool("isMove", true);
                break;
            case MineStat.Detect: // 플레이어 인지
                anim.SetBool("isMove", false);
                DetectPlayer();
                break;
            case MineStat.LandMine:
                anim.SetBool("isMove", true);
                fleeRecalcTimer = 0f;
                mineDropTimer = 0f; // 바로 지뢰 설치
                lastCheckedPos = transform.position;
                stuckTimer = stuckThreshold;
                landMineElapsed = 0f;
                break;
            case MineStat.Reset:
                anim.SetBool("isMove", false);
                resetTimer = resetCooldown;
                break;
            case MineStat.Dead: // 한 번만 실행이라 여기서 동작
                detectEffect.transform.DOKill();
                SetDead();
                break;
        }
    }

    private void SetLandMine() // 지뢰 설치 및 이동
    {
        fleeRecalcTimer -= Time.fixedDeltaTime * GameTime.WorldTimeScale;

        // 이동 관련
        if (fleeRecalcTimer <= 0f) // 방향 재계산
        {
            currentFleeDir = CalculateFleeDirection();
            fleeRecalcTimer = fleeRecalcInterval;
        }
        
        Vector2 moveVec = currentFleeDir * landMineSpeed * Time.fixedDeltaTime * GameTime.WorldTimeScale;
        rigid.MovePosition(rigid.position + moveVec);
        sprite.flipX = currentFleeDir.x > 0f;
        
        // 만약 이동 포지션이랑 직전 포지션이 n초동안 0에 가까우면 강제 재계산 추가
        if (Vector2.Distance(transform.position, lastCheckedPos) < stuckDistThreshold)
        {
            // 막힌 상태면
            stuckTimer -= Time.fixedDeltaTime * GameTime.WorldTimeScale;
            if (stuckTimer <= 0f) // 방향 재계산
            {
                Debug.Log("벽 막힘으로 인한 재계산");
                fleeRecalcTimer = fleeRecalcInterval; 
                stuckTimer = stuckThreshold;
                
                Vector2 avoidWall = AvoidWall();
                currentFleeDir = avoidWall.normalized;
            }
        }
        else
        {
            lastCheckedPos = transform.position;
            stuckTimer = stuckThreshold;
        }
        
        // 지뢰 관련
        mineDropTimer -= Time.fixedDeltaTime * GameTime.WorldTimeScale;
        if (mineDropTimer <= 0f)
        {
            enemyPlacer.PlaceMine(transform.position); // 지뢰 설치
            mineDropTimer = mineDropInterval;
        }

        // 상태 리셋 관련
        CheckFleeExitCondition();
    }
    
    private Vector2 CalculateFleeDirection()
    {
        Vector2 playerDistance = (transform.position - target.position).normalized; // 플레이어와 차이
        Vector2 goToOpenSpace = (mapCenter.position - transform.position).normalized; // 맵 중앙
        Vector2 avoidWall = AvoidWall(); // 벽 피하기
        Vector2 noise = Random.insideUnitCircle.normalized; // 랜덤 값
        
        Vector2 combined = playerDistance * weightPlayerDistance +
                           goToOpenSpace * weightGoToOpenSpace +
                           avoidWall * weightAvoidWall +
                           noise * weightNoise;
        
        if (combined.sqrMagnitude < 0.001f) // 결과값 애매하면 플레이어 피하는 방향으로만
            return playerDistance;

        return combined.normalized;
    }
    
    private Vector2 AvoidWall()
    {
        Vector2 avoidSum = Vector2.zero;
        Vector2 baseDir = currentFleeDir.sqrMagnitude > 0.001f ? currentFleeDir : Vector2.up; 

        foreach (float angle in wallCheckAngles) // 벽 체크
        {
            Vector2 rayDir = Quaternion.Euler(0f, 0f, angle) * baseDir;
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                rayDir,
                wallCheckDistance,
                wallLayer);

            if (hit.collider == null) continue; // 벽 없으면 무시

            Vector2 pushDir = ((Vector2)transform.position - hit.point).normalized;

            float closeness = 1f - (hit.distance / wallCheckDistance);

            avoidSum += pushDir * closeness; // 누적
        }

        return avoidSum;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected() // 테스트용
    {
        if (!Application.isPlaying) return;

        Vector2 baseDir = currentFleeDir.sqrMagnitude > 0.001f ? currentFleeDir : Vector2.up;

        foreach (float angle in wallCheckAngles)
        {
            Vector2 rayDir = Quaternion.Euler(0f, 0f, angle) * baseDir;
            Gizmos.color = Physics2D.Raycast(transform.position, rayDir, wallCheckDistance, wallLayer).collider != null
                ? Color.red   // 벽 O
                : Color.green; // 벽 X
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rayDir * wallCheckDistance);
        }
    }
#endif
    
    private void CheckFleeExitCondition()
    {
        landMineElapsed += Time.fixedDeltaTime * GameTime.WorldTimeScale;
        if (landMineElapsed < landMineMinDuration) return;
        
        float distToPlayer = Vector2.Distance(transform.position, target.position);
        
        if (distToPlayer >= fleeResetDist)
        {
            resetConfirmTimer += Time.fixedDeltaTime * GameTime.WorldTimeScale;
            if (resetConfirmTimer >= resetConfirmDuration)
                ChangeStat(MineStat.Reset);
        }
        else
        {
            resetConfirmTimer = 0f; // 다시 가까워졌을 때
        }
    }
    
    protected override void OnHealthDepleted()
    {
        ChangeStat(MineStat.Dead);
    }
    
    protected override void OnScopeEnter(Collider2D other)
    {
        if ((currentStat != MineStat.Patrol &&  currentStat != MineStat.Reset) 
            || !other.CompareTag("Player"))
            return;
        
        ChangeStat(MineStat.Detect);
    }

    protected override void ResetStateMachine()
    {
        ChangeStat(MineStat.Patrol);
    }
    
    public override void Pause(bool isPause)
    {
        isPaused = isPause;
    }
    
    protected override bool IsCurrentlyDetecting() => currentStat == MineStat.Detect;
    protected override void OnDetectComplete() => ChangeStat(MineStat.LandMine);
}
