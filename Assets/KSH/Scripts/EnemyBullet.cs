using System;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyBullet : MonoBehaviour
{
    public Vector2 velocity; 
    private IObjectPool<GameObject> pool;
    private Rigidbody2D rigid; // 캐싱
    private bool isReleased;
    
    private Vector2 direction;
    private float speed;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    
    public void Launch(Vector2 direction, float speed)
    {
        this.direction = direction;
        this.speed = speed;
        isReleased = false; // 발사될 때 반납 상태 초기화
        rigid.AddForce(direction * speed * GameTime.WorldTimeScale, ForceMode2D.Impulse);
        
        velocity = rigid.linearVelocity; //추가
    }

    private void FixedUpdate()
    {
        rigid.linearVelocity = direction * speed * GameTime.WorldTimeScale;
    }

    public void SetPool(IObjectPool<GameObject> pool)
    {
        this.pool = pool;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) // Wall이나 맵 tag 나중에 추가
            return;
        
        if (isReleased) return;
        isReleased = true;
        
        pool.Release(gameObject); // 반납하면 OnReturnedToPool에서 SetActive를 false
    }
}
