using System;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyBullet : PoolObjectBase
{
    public Vector2 velocity; 
    private Rigidbody2D rigid; // 캐싱
    private bool isReleased;
    
    private Vector2 direction;
    private float speed;
    private float damage;
    
    private GameObject originPrefab; // 오리진 프리팹

    [SerializeField] LayerMask playerLayer;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
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

    public override void SetOriginPrefab(GameObject prefab)
    {
        originPrefab = prefab;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Wall"))
            return;
        
        if (isReleased) return;
        isReleased = true;
        
        if ((playerLayer.value & (1 << other.gameObject.layer)) != 0) // 이미 위에서 tag로 처리하는데 tag로 통일 하는 건 어떠신지
        {
            IDamageable target = other.GetComponent<IDamageable>();

            if (target != null)
                target.TakeDamage(damage);
        }
        
        PoolManager.Instance.Release(originPrefab, this.gameObject);
    }
}
