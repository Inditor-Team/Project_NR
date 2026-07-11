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
        
        velocity = rigid.linearVelocity; //추가
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
        if (!other.CompareTag("Player")) // Wall이나 맵 tag 나중에 추가
            return;
        
        if (isReleased) return;
        isReleased = true;
        
        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null)
            target.TakeDamage(damage);
        
        PoolManager.Instance.Release(originPrefab, this.gameObject);
    }
}
