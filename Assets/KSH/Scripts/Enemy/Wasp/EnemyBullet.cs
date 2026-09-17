using System;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyBullet : MonoBehaviour, IPoolObjectBase
{
    public Vector2 velocity; 
    private Rigidbody2D rigid; // 캐싱
    private bool isReleased;
    
    private Vector2 direction;
    private float speed;
    private float damage;

    private Animator anim;
    
    private GameObject originPrefab; // 오리진 프리팹
    [SerializeField] LayerMask playerLayer;
    public event Action<EnemyBullet> OnBulletExpired; // 총알 사라졌을 때 호출

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        rigid.linearVelocity = Vector2.zero;
    }

    private void OnDisable()
    {
        rigid.linearVelocity = Vector2.zero;
    }
    
    public void Launch(Vector2 direction, float speed, float damage)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        isReleased = false; // 발사될 때 반납 상태 초기화
        
        velocity = direction * speed * GameTime.WorldTimeScale; //추가
    }

    private void FixedUpdate()
    {
        rigid.linearVelocity = direction * speed * GameTime.WorldTimeScale;
    }

    public void SetOriginPrefab(GameObject prefab)
    {
        originPrefab = prefab;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Wall"))
            return;
        
        if (isReleased) return;
        isReleased = true;
        
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null) target.TakeDamage(damage);
        
        DestroyBullet();
        OnBulletExpired?.Invoke(this);
    }

    public void DestroyBullet()
    {
        if (gameObject.activeSelf) // 중복 Release 방지?
            PoolManager.Instance.Release(originPrefab, gameObject);
    }

    public void ExpireByBossDeath() // 강제 삭제, 보스맵 전용
    {
        speed = 0f; // 이동 못 하게 처리
        anim.SetTrigger("isExplosion");
        // TODO: 폭파 사운드
    }

    public void OnExplosionAnimationEnd()
    {
        DestroyBullet();
    }
}
