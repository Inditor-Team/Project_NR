using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;

public class EnemyLandMine : MonoBehaviour, IPoolObjectBase
{
    private enum MineState { Idle, Armed, Exploding, Falling }
    private MineState state;

    
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private Collider2D bombScope;
    [SerializeField] private SpriteRenderer bombEffectSprite;
    
    private float armDelay = 0.75f;
    private float armTimer;
    private float waitTime;
    private float damage;
    private GameObject originPrefab;
    
    // 폭파 애니메이션
    private SpriteRenderer sprite;
    private Animator anim;

    // 낙하 스폰 연출
    private float fallDuration = 0.5f;
    private float fallPeakHeight = 2f;
    private Coroutine fallRoutine;
    private Vector2 fallTargetPos; // 낙하 목표 위치
    
    public event Action<EnemyLandMine> OnMineExpired;
    
    public void SetOriginPrefab(GameObject prefab) => originPrefab = prefab;
    public void SetValue(float newTime, float newDamage)
    {
        waitTime = newTime;
        damage = newDamage;
    }

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }
    
    private void OnEnable()
    {
        state = MineState.Idle;
        armTimer = armDelay;
        sprite.DOKill();
        sprite.color = Color.white;
        bombEffectSprite.DOKill();
        SetBombEffectAlpha(0f);
    }

    private void OnDisable()
    {
        if (fallRoutine != null)
        {
            StopCoroutine(fallRoutine);
            fallRoutine = null;
        }
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
                if (waitTime <= 0f) // 시간 만료 경로
                    DoExplosion();
                break;
        }
    }
    
    private void EnterArmedState()
    {
        // TODO: 카운트 효과음 추가
        state = MineState.Armed;
        sprite.DOColor(Color.red, 0.2f).SetLoops(-1, LoopType.Yoyo);
        bombEffectSprite.DOFade(0.35f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
    
    private void DoExplosion()
    {
        if (state == MineState.Exploding) return; // 중복 방지
        state = MineState.Exploding;

        sprite.DOKill(); // 점멸 정지
        
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            bombScope.bounds.center,
            bombScope.bounds.size,
            bombScope.transform.eulerAngles.z,
            playerLayer); // tag로 감지하면 더 복잡해져서 여기선 레이어로 감지

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(damage);
        }
        
        bombEffectSprite.DOKill();
        SetBombEffectAlpha(1f); 
        anim.SetTrigger("isBomb"); 
        // TODO: 폭발 효과음 추가
        bombEffectSprite.DOFade(0f, 0.5f).OnComplete(() =>
        {
            anim.Rebind(); // 애니메이션 초기화
            PoolManager.Instance.Release(originPrefab, gameObject);
            OnMineExpired?.Invoke(this);
        });
    }
    
    private void SetBombEffectAlpha(float alpha)
    {
        Color c = bombEffectSprite.color;
        c.a = alpha;
        bombEffectSprite.color = c;
    }

    public void ExpireByBossDeath() // 강제 삭제, 보스맵 전용
    {
        if (state == MineState.Falling) // 낙하 중이면 즉시 타겟 위치로 이동 후 폭파
        {
            if (fallRoutine != null)
            {
                StopCoroutine(fallRoutine);
                fallRoutine = null;
            }
            transform.position = fallTargetPos;
            Color c = sprite.color;
            c.a = 1f;
            sprite.color = c;
        }
        
        // 바로 폭파 가능한 상태로 만들기
        state = MineState.Armed;
        waitTime = 0f;
    }
    
    public void StartFallSpawn(Vector2 startPos, Vector2 targetPos)
    {
        fallTargetPos = targetPos;

        if (fallRoutine != null) StopCoroutine(fallRoutine);
        state = MineState.Falling;
        fallRoutine = StartCoroutine(FallAndLand(startPos, targetPos));
    }

    private IEnumerator FallAndLand(Vector2 startPos, Vector2 targetPos)
    {
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

            spriteColor.a = Mathf.Clamp01(clamped / 0.5f);
            sprite.color = spriteColor;

            yield return null;
        }

        // 착지 보정
        transform.position = targetPos;
        spriteColor.a = 1f;
        sprite.color = spriteColor;

        fallRoutine = null;

        // 착지 완료 후 armDelay 카운트 시작
        state = MineState.Idle;
        armTimer = armDelay;
    }
}