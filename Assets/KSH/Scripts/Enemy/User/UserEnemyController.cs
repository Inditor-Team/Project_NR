using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class UserEnemyController : EnemyBaseController, IPoolObjectBase
{
    private enum UserStat
    {
        Patrol,
        Detect, // 플레이어 인지
        Track, // 추적
        Falling, // 보스 때 생성 시 낙하
        Dead
    }
    UserStat currentStat = UserStat.Patrol;
    
    [SerializeField] private EnemyScope explodeScope; // 폭발 범위 스코프
    [SerializeField] private Collider2D explodeCollider;
    
    private Vector2 currentTrackDir;   // 추적 방향
    
    [Header("벽 회피 관련")]
    [SerializeField] private float wallCheckDistance = 1.2f; // 레이 길이
    private float[] wallCheckAngles = { 0f, 45f, -45f, 90f, -90f }; // 레이 방향
    private Vector2 prevTangent = Vector2.zero;
    
    // 방향 계산 가중치, 벽 피하기 우선
    private float seekWeight = 1f;
    private float avoidWeight = 2f; 
    
    // 오브젝트 풀링
    private GameObject originPrefab; // 보스 맵에서만 오브젝트 풀링 사용
    private bool isSpawnForBoss = false;
    public event Action<UserEnemyController> OnUserExpired;
    
    // 낙하 스폰 연출
    private float fallDuration = 0.55f;  // 낙하~착지 소요 시간
    private float fallPeakHeight = 2f;   // 낙하 정점 높이 (월드 유닛)
    private float shadowMinScale = 0.4f; // 정점에서 그림자 축소 비율
    private float shadowMaxAlpha = 0.6f; // 착지 상태에서 그림자 기본 알파
    
    private Vector3 shadowBaseLocalPos;
    private Vector3 shadowBaseScale;
    private Coroutine fallRoutine;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        explodeScope.OnScopeTriggerEnter += DoExplosion;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        explodeScope.OnScopeTriggerEnter -= DoExplosion;
        
        if (fallRoutine != null)
        {
            StopCoroutine(fallRoutine);
            fallRoutine = null;
        }
    }
    
    public void SetOriginPrefab(GameObject prefab)
    {
        originPrefab = prefab;
    }
    
    public override void TakeDamage(float damegeAmount)
    {
        if (currentStat == UserStat.Patrol)
            ChangeStat(UserStat.Detect);
        
        if (currentStat == UserStat.Dead || currentStat == UserStat.Falling) return;
        
        base.TakeDamage(damegeAmount);
    }

    private void FixedUpdate()
    {
        if (isPaused) return;
        switch (currentStat)
        {
            case UserStat.Patrol:
                DoPatrol(false);
                break;
            case UserStat.Track:
                TrackPlayer();
                break;
        }
    }
    
    private void ChangeStat(UserStat newStat)
    {
        currentStat = newStat;
        switch (currentStat)
        {
            case UserStat.Patrol:
                anim.SetBool("isMove", true);
                break;
            case UserStat.Detect: // 플레이어 인지
                anim.SetBool("isMove", false);
                DetectPlayer();
                break;
            case UserStat.Track:
                anim.SetBool("isMove", true);
                break;
            case UserStat.Falling:
                anim.SetBool("isMove", false);
                break;
            case UserStat.Dead: // 한 번만 실행이라 여기서 동작
                detectEffect.transform.DOKill();
                OnUserExpired?.Invoke(this);
                SetDead();
                break;
        }
    }
    
    protected override void OnDeadAnimationOver() // dead 애니메이션 재생 종료 후 호출 
    {
        base.OnDeadAnimationOver();
        if(isSpawnForBoss) PoolManager.Instance.Release(originPrefab, gameObject);
    }

    private void TrackPlayer()
    {
        Vector2 playerDir = (target.transform.position - transform.position).normalized;
        Vector2 seekForce = playerDir; // 찾는 건 플레이어 방향 그대로
        Vector2 avoidForce = ComputeAvoidance(playerDir); // 평균 낸 회피 방향
        Vector2 finalDir = (seekForce * seekWeight + avoidForce * avoidWeight).normalized; // 가중치 적용

        // 테스트용
        seekForceDebug = seekForce;
        avoidForceDebug = avoidForce;
            
        // 자연스럽게 이동하는 방향으로 설정
        currentTrackDir = Vector2.Lerp(currentTrackDir, finalDir, 10f * Time.fixedDeltaTime).normalized;
        
        Vector2 moveVec = currentTrackDir * defaultSpeed * Time.fixedDeltaTime * GameTime.WorldTimeScale;
        rigid.MovePosition(rigid.position + moveVec);
        sprite.flipX = currentTrackDir.x < 0f;
    }

    private Vector2 ComputeAvoidance(Vector2 playerDir)
    {
        // ㄷ자 골목은 회피 못함, 나중에 다양한 맵에서 테스트해보기
        Vector2 accumulated = Vector2.zero;
        Vector2 snapshotPrevTangent = prevTangent; // 이전 프레임 값
        Vector2 strongestTangent = Vector2.zero;
        int hitCount = 0;
        float strongestWeight = 0f;

        foreach (float angle in wallCheckAngles)
        {
            Vector2 rayDir = Quaternion.Euler(0f, 0f, angle) * playerDir;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, wallCheckDistance, wallLayer);
            if (hit.collider == null) continue;

            hitCount++;
            float weight = 1f - (hit.distance / wallCheckDistance);
            Vector2 normal = hit.normal;
            Vector2 tangentA = new Vector2(-normal.y, normal.x); // 벽에서 밀려나는 방향으로 90도
            Vector2 tangentB = -tangentA; // 반대로 90도

            // 플레이어랑 가까움 + 이전 프레임에서 택한 방향이랑 가까움 합산 (갑자기 끝에 가서 방향 뒤집혀지는 거 방지)
            // 0.5보다 올리면 방향은 안정적인데 급커브 안해서 좀 그럼 일단 0.5로 하기
            float dotA = Vector2.Dot(playerDir, tangentA) + Vector2.Dot(snapshotPrevTangent, tangentA) * 0.5f;
            float dotB = Vector2.Dot(playerDir, tangentB) + Vector2.Dot(snapshotPrevTangent, tangentB) * 0.5f;
            Vector2 tangent = dotA >= dotB ? tangentA : tangentB;

            // normal은 벽에서 밀려나는 힘, tangent는 벽 타고 흐르기(슬라이딩?)
            accumulated += (normal * 0.6f + tangent * 0.4f) * weight;

            // 가장 가까운 레이의 tangent == 대표값
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestTangent = tangent;
            }
        }

        if (hitCount == 0) return Vector2.zero;
        prevTangent = strongestTangent; // 갱신
        return accumulated / hitCount; // 평균 내기
    }

    // 테스트용 기즈모
#if UNITY_EDITOR
    private Vector2 seekForceDebug, avoidForceDebug;
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(seekForceDebug * 2f));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(avoidForceDebug * 2f));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(currentTrackDir * 2f));
    }
#endif
    
    private void DoExplosion(Collider2D other)
    {
        if (currentStat == UserStat.Dead) return;
        if (!other.CompareTag("Player")) return;
        
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null) target.TakeDamage(damage);
        
        ChangeStat(UserStat.Dead);
    }
    
    protected override void OnHealthDepleted()
    {
        ChangeStat(UserStat.Dead);
    }
    
    protected override void OnScopeEnter(Collider2D other)
    {
        if ((currentStat != UserStat.Patrol) || !other.CompareTag("Player")) return;
        
        ChangeStat(UserStat.Detect);
    }

    protected override void ResetStateMachine()
    {
        ChangeStat(UserStat.Patrol);
    }
    
    public override void Pause(bool isPause)
    {
        isPaused = isPause;
    }
    
    // 보스맵 스폰
    public void SpawnForBoss(Vector2 bossPos, Vector2 spawnPos)
    {
        rigid.simulated = true;
        currentTrackDir = Vector2.zero;
        prevTangent = Vector2.zero;
        healthUI.SetActive(true); // 체력 바 표시
        isSpawnForBoss = true; // 보스맵에서 생성된 경우 오브젝트 풀링 적용된 상태

        if (fallRoutine != null) StopCoroutine(fallRoutine);
        ChangeStat(UserStat.Falling);
        fallRoutine = StartCoroutine(FallAndLand(bossPos, spawnPos));
    }

    public void ExpireByBossDeath() // 강제 삭제, 보스맵 전용
    {
        if (fallRoutine != null)
        {
            StopCoroutine(fallRoutine);
            fallRoutine = null;
            rigid.simulated = true;
            if (explodeCollider != null) explodeCollider.enabled = true;
        }
        SetDead();
    }
    
    // 낙하
    private IEnumerator FallAndLand(Vector2 startPos, Vector2 targetPos)
    {
        rigid.simulated = false;
        if (explodeCollider != null) explodeCollider.enabled = false;

        Color spriteColor = sprite.color;
        spriteColor.a = 0f;
        sprite.color = spriteColor;
        
        transform.position = startPos;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * GameTime.WorldTimeScale / fallDuration;
            float clamped = Mathf.Clamp01(t);

            Vector2 groundPos = Vector2.Lerp(startPos, targetPos, clamped);
            float height = Mathf.Sin(Mathf.PI * clamped) * fallPeakHeight;

            transform.position = new Vector3(groundPos.x, groundPos.y + height, transform.position.z);

            // 페이드 인
            spriteColor.a = Mathf.Clamp01(clamped / 0.5f);
            sprite.color = spriteColor;
            
            yield return null;
        }

        // 착지 보정
        transform.position = targetPos;
        spriteColor.a = 1f;
        sprite.color = spriteColor;
        
        rigid.simulated = true;
        if (explodeCollider != null) explodeCollider.enabled = true;

        fallRoutine = null;
        ChangeStat(UserStat.Track); // Patrol 거치지 않고 바로 추적
    }
    
    protected override bool IsCurrentlyDetecting() => currentStat == UserStat.Detect;
    protected override void OnDetectComplete() => ChangeStat(UserStat.Track);
}
