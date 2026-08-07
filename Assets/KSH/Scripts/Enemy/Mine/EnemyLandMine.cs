using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class EnemyLandMine : PoolObjectBase
{
    private enum MineState { Idle, Armed, Exploding }
    private MineState state;

    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private EnemyScope detectScope; // 지뢰 근처 범위
    [SerializeField] private LayerMask playerLayer;
    
    // 연출용
    [SerializeField] private Transform verticalScale;
    [SerializeField] private Transform horizontalScale;
    
    [SerializeField] private BoxCollider2D horizontalHitbox;
    [SerializeField] private BoxCollider2D verticalHitbox;
    
    private float armDelay = 0.5f;
    private float armTimer;
    private float waitTime;
    private float damage;
    private GameObject originPrefab;
    
    private Vector3 verticalTargetScale; 
    private Vector3 horizontalTargetScale;

    public override void SetOriginPrefab(GameObject prefab) => originPrefab = prefab;
    public void SetValue(float newTime, float newDamage)
    {
        waitTime = newTime;
        damage = newDamage;
    }

    private void Awake()
    {
        verticalTargetScale = verticalScale.localScale;
        horizontalTargetScale = horizontalScale.localScale;
    }

    private void OnEnable()
    {
        state = MineState.Idle;
        armTimer = armDelay;
        sprite.DOKill();
        sprite.color = Color.white;
        verticalScale.localScale = Vector3.zero;
        horizontalScale.localScale = Vector3.zero;
        
        detectScope.OnScopeTriggerEnter += EnterDetectScope;
        detectScope.enabled = false;
    }
    
    private void OnDisable()
    {
        detectScope.OnScopeTriggerEnter -= EnterDetectScope;
    }
    
    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime * GameTime.WorldTimeScale;

        switch (state)
        {
            case MineState.Idle:
                armTimer -= dt;
                if (armTimer <= 0f)
                    EnterArmedState();
                break;

            case MineState.Armed:
                waitTime -= dt;
                if (waitTime <= 0f)
                    TriggerExplosion(); // 시간 만료 경로
                break;
        }
    }

    private void EnterArmedState()
    {
        state = MineState.Armed;
        detectScope.enabled = true; // 감지 시작
        sprite.DOColor(Color.red, 0.2f).SetLoops(-1, LoopType.Yoyo);
    }

    private void EnterDetectScope(Collider2D other)
    {
        if (state != MineState.Armed) return; // Armed일 때만 실행?
        if (!other.CompareTag("Player")) return;
        
        TriggerExplosion();
    }
    
    private void TriggerExplosion()
    {
        if (state == MineState.Exploding) return; // 중복 방지
        state = MineState.Exploding;

        detectScope.enabled = false;
        sprite.DOKill(); // 점멸 정지
        
        Collider2D[] horizontalHits = Physics2D.OverlapBoxAll(
            horizontalHitbox.bounds.center,
            horizontalHitbox.bounds.size,
            horizontalHitbox.transform.eulerAngles.z,
            playerLayer);

        Collider2D[] verticalHits = Physics2D.OverlapBoxAll(
            verticalHitbox.bounds.center,
            verticalHitbox.bounds.size,
            verticalHitbox.transform.eulerAngles.z,
            playerLayer);

        HashSet<Collider2D> uniqueHits = new HashSet<Collider2D>(horizontalHits);
        uniqueHits.UnionWith(verticalHits);

        foreach (var hit in uniqueHits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(damage);
        }

        // 폭발 연출, TODO: 효과음 추가
        verticalScale.DOScale(verticalTargetScale, 0.12f);
        horizontalScale.DOScale(horizontalTargetScale, 0.12f).OnComplete(() =>
        {
            PoolManager.Instance.Release(originPrefab, gameObject);
        });
    }
}