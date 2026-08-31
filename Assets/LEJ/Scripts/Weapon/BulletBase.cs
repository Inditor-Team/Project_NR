using UnityEngine;

/// <summary>
/// 총알의 기본 클래스
/// </summary>
public class BulletBase : MonoBehaviour
{
    [SerializeField] LayerMask ownerLayer;
    [SerializeField] LayerMask wallLayer;

    private float damage;
    private float speed;
    private Vector2 dir;
    private float inactiveTime = 5f;

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

    public void OnFire(Vector2 dir, float speed, float damage, GameObject originPrefab = null)
    {
        this.damage = damage;
        this.speed = speed;
        this.dir = dir.normalized;
        if (this.originPrefab == null)
            this.originPrefab = originPrefab;
        timer = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((ownerLayer.value & (1 << collision.gameObject.layer)) != 0)
            return;

        if ((wallLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            DestroyBullet();
            return;
        }

        IDamageable damageable = collision.GetComponent<IDamageable>();
        IInteractable interactable = collision.GetComponent<IInteractable>();

        if (interactable != null || damageable != null)
        {
            damageable?.TakeDamage(damage);
            interactable?.OnInteract();
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        PoolManager.Instance.Release(originPrefab, this.gameObject); //오브젝트 풀 내 자신 비활성화
    }
}
