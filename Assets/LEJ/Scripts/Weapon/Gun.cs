using UnityEngine;
using UnityEngine.Events;

public class Gun : WeaponBase
{
    [Tooltip("�ѱ� ��ġ")]
    [SerializeField] Transform firePoint; //�ѱ� ��ġ
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;
    float maxDistance = 20f;

    float speed;
    float damage;

    [SerializeField] GameObject bulletPrefab; //�Ѿ� ������
    [SerializeField] GameObject reflectBulletPrefab; //�ݻ� �Ѿ� ������
    public GameObject BulletPrefab => bulletPrefab;
    private int bulletPoolSize = 20;
    BulletBase curBullet;

    [SerializeField] LayerMask hitLayer;
    [SerializeField] private TrailRenderer laserTrail;

    public event UnityAction OnShoot;
    Color originColor;

    private void Start()
    {
        originColor = bulletPrefab.GetComponent<SpriteRenderer>().color;
        // MakeBulletPool(); // 이미 풀 매니저에서 만들어서 해당 코드 제외
    }

    public void TryAttack(float speed, float damage)
    {
        this.damage = damage;
        
        // �Ѿ� ���
        this.speed = speed;

        curBullet = PoolManager.Instance.Get(bulletPrefab).GetComponent<BulletBase>();
        curBullet.GetComponent<SpriteRenderer>().color = originColor;
        curBullet.transform.position = firePoint.position; //�Ѿ� ��ġ �ʱ�ȭ

        Attack();
    }


    internal override void Attack()
    {
        OnShoot?.Invoke();

        // �Ѿ� ���
        if (curBullet == null)
            return;

        curBullet.OnFire(-firePoint.right, speed, damage, bulletPrefab);

        laserTrail.Clear();
        Vector2 endPosition;
        Vector2 startPosition = firePoint.position;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = (mouseWorldPos - startPosition).normalized;

        RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, maxDistance, hitLayer);

        if (hit.collider != null)
            endPosition = hit.point;
        else
            endPosition = startPosition + direction * maxDistance;

    }

    public void ReflectAttack(Vector2 startPos, Vector2 dir, float speed)
    {
        GameObject bulletObject = PoolManager.Instance.Get(reflectBulletPrefab);

        if (bulletObject == null)
            return;

        bulletObject.SetActive(true);

        curBullet = bulletObject.GetComponent<BulletBase>();

        if (curBullet == null)
            return;

        curBullet.transform.position = startPos;
        curBullet.GetComponent<SpriteRenderer>().color = Color.green;

        float reflectDamage =
            GameManager.Instance.Player
                .GetComponent<PlayerController>()
                .Stat.StatDic[PlayerStat.Stat.BulletDamage];

        curBullet.OnFire(
            dir,
            speed,
            reflectDamage,
            reflectBulletPrefab
        );
    }

}
