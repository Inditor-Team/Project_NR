using UnityEngine;

/// <summary>
/// 총알의 기본 클래스
/// </summary>
public class BulletBase : MonoBehaviour
{
    private GameObject owner;
    private float damage;
    private float speed;
    private Vector2 dir;
    private float inactiveTime = 2f;
    float timer = 0;

    /// <summary>
    /// 총에서 총알을 생성하고 초기화 합니다
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="speed"></param>
    public virtual void Init(float damage, float speed)
    {
        this.damage = damage;
        this.speed = speed;
    }

    public virtual void Init(float damage, float speed, GameObject owner)
    {
        this.damage = damage;
        this.speed = speed;
        this.owner = owner;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > inactiveTime)
            DestroyBullet();
    }

    public virtual void FixedUpdate()
    {
        if (dir == Vector2.zero)
            return;

        //기본 총알 이동
        transform.Translate(dir * speed * Time.fixedDeltaTime * GameTime.WorldTimeScale, Space.World);
    }

    public virtual void OnFire(Vector2 dir)
    {
        gameObject.SetActive(true);
        this.dir = dir.normalized;
        timer = 0;
    }

    public void OnFire(Vector2 dir, float speed)
    {
        this.speed = speed;
        gameObject.SetActive(true);
        this.dir = dir.normalized;
        timer = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null)
        {
            //총알 발사자가 설정 되었고, 발사자와 충돌한 경우
            if (owner != null && collision.gameObject == owner) 
                return;

            DestroyBullet();
            target.TakeDamage(damage); //데미지 전달
        }
    }

    private void DestroyBullet()
    {
        //소유자가 없으면 오브젝트 풀 내 총알이 아니라고 판단, 그대로 destroy
        if (owner == null)
            Destroy(gameObject);

        gameObject.SetActive(false); //오브젝트 풀 내 자신 비활성화
    }
}
