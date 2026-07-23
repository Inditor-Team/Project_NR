using UnityEngine;

/// <summary>
/// 총알의 기본 클래스
/// </summary>
public class BulletBase : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 dir;
    private float inactiveTime = 2f;

    float timer = 0;
    GameObject originPrefab;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > inactiveTime) //총알이 무한정 뻗어나가지 않게 사라지는 시간 설정
            DestroyBullet();
    }

    public virtual void FixedUpdate()
    {
        if (dir == Vector2.zero)
            return;

        //기본 총알 이동
        transform.Translate(dir * speed * Time.fixedDeltaTime * GameTime.WorldTimeScale, Space.World);
    }

    public void OnFire(Vector2 dir, float speed, float damage, GameObject originPrefab)
    {
        this.damage = damage;
        this.speed = speed;
        this.dir = dir.normalized;
        this.originPrefab = originPrefab;
        timer = 0;

        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("switch");
        IDamageable damageable = collision.GetComponent<IDamageable>();
        IInteractable interactable = collision.GetComponent<IInteractable>();

        if (damageable != null)
            damageable.TakeDamage(damage); //데미지 전달
        if (interactable != null)
        {
            Debug.Log("interact");
            interactable.OnInteract();
        }

        DestroyBullet();
    }

    private void DestroyBullet()
    {
        PoolManager.Instance.Release(originPrefab, this.gameObject); //오브젝트 풀 내 자신 비활성화
    }
}
