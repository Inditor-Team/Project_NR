using UnityEngine;
using UnityEngine.Serialization;

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

    private Vector2 currentFleeDir;   // 도주 방향
    private float fleeRecalcTimer;    // 다음 재계산까지 타이머
    
    // 리셋 상태 관련
    private float resetCooldown = 10f; // 이정도 시간동안 떨어져있으면 순찰 상태로 변경
    private float resetTimer;
    
    // 지뢰 관련
    private float waitTime = 1.5f; // 터지기 전 대기 시간
    public float mineDropInterval = 1.5f; // 지뢰를 뿌리는 간격
    private float mineDropTimer; // 다음 지뢰까지 타이머
    
    protected override void Awake()
    {
        base.Awake();
        landMineSpeed = defaultSpeed * 3f; // 임시, 순찰이랑 지뢰 뿌리는 속도가 다르게
        enemyPlacer.SetValue(waitTime, damage);
    }
    
    public override void TakeDamage(float damegeAmount)
    {
        if (currentStat == MineStat.Patrol) // 순찰 중에 피격되면 인지 -> 지뢰 공격
            ChangeStat(MineStat.Detect);
        
        if (currentStat == MineStat.Dead) return; // 이미 Dead면 중복 실행되지 않도록 처리
        
        base.TakeDamage(damegeAmount);
    }
    
    private void FixedUpdate()
    {
        switch (currentStat)
        {
            case MineStat.Patrol:
                DoPatrol();
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
            case MineStat.Detect: // 플레이어 인지
                if (!healthUI.activeSelf) healthUI.SetActive(true);
                DetectPlayer();
                break;
            case MineStat.LandMine:
                fleeRecalcTimer = 0f;
                mineDropTimer = 0f; // 바로 지뢰 설치
                break;
            case MineStat.Reset:
                resetTimer = resetCooldown;
                break;
            case MineStat.Dead: // 한 번만 실행이라 여기서 동작
                SetDead();
                break;
        }
    }

    private void DetectPlayer() // 플레이어 감지
    {
        // 0.5초 대기하면서 ! 이펙트 같은 거 추가?
        ChangeStat(MineStat.LandMine);
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
        
        // 지뢰 관련
        mineDropTimer -= Time.fixedDeltaTime * GameTime.WorldTimeScale;
        if (mineDropTimer <= 0f)
        {
            enemyPlacer.PlaceMine(); // 지뢰 설치
            mineDropTimer = mineDropInterval;
        }

        // 상태 리셋 관련
        CheckFleeExitCondition();
    }
    
    private Vector2 CalculateFleeDirection()
    {
        Vector2 playerDistance = (transform.position - target.position).normalized; // 플레이어와 차이
        Vector2 goToOpenSpace = (mapCenter.position - transform.position).normalized; // 맵 중앙
        Vector2 avoidOwnMines = Vector2.zero; // 지뢰 회피? 나중에 추가
        Vector2 noise = Random.insideUnitCircle.normalized; // 랜덤 값
        
        Vector2 combined = playerDistance * weightPlayerDistance +
                           goToOpenSpace * weightGoToOpenSpace +
                           avoidOwnMines +               // 나중에 가중치 곱하기, 플레이어보다 크고 맵 중앙 보다 작게?
                           noise * weightNoise;
        
        /*Debug.Log($"1) 플레이어로부터 멀어지는 벡터 {awayFromPlayer}, 2) 맵 중앙 {towardOpenSpace}, " +
                  $"4) 랜덤 노이즈 {noise}, 다 합쳐서 {combined}");*/
        
        if (combined.sqrMagnitude < 0.001f) // 결과값 애매하면 플레이어 피하는 방향으로만
            return playerDistance;

        return combined.normalized;
    }
    
    private void CheckFleeExitCondition()
    {
        float distToPlayer = Vector2.Distance(transform.position, target.position);

        if (distToPlayer >= fleeResetDist)
            ChangeStat(MineStat.Reset);
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
        currentStat = MineStat.Patrol;
    }
}
