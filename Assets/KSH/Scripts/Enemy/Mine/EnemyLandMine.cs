using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class EnemyLandMine : PoolObjectBase
{
    private enum MineState { Idle, Armed, Exploding }
    private MineState state;

    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private Collider2D bombScope;
    [SerializeField] private SpriteRenderer bombEffectSprite;
    
    private float armDelay = 0.75f;
    private float armTimer;
    private float waitTime;
    private float damage;
    private GameObject originPrefab;
    
    public override void SetOriginPrefab(GameObject prefab) => originPrefab = prefab;
    public void SetValue(float newTime, float newDamage)
    {
        waitTime = newTime;
        damage = newDamage;
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
        
        // 폭발 연출, TODO: 효과음 추가
        bombEffectSprite.DOKill();
        SetBombEffectAlpha(1f); 
        bombEffectSprite.DOFade(0f, 0.4f).OnComplete(() =>
        {
            PoolManager.Instance.Release(originPrefab, gameObject);
        });
    }
    
    private void SetBombEffectAlpha(float alpha)
    {
        Color c = bombEffectSprite.color;
        c.a = alpha;
        bombEffectSprite.color = c;
    }
}