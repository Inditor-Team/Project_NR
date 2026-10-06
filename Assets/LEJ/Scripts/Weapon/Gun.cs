using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Gun : WeaponBase
{
    [Tooltip("총구 위치")]
    [SerializeField] Transform firePoint; //총구 위치
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;
    float maxDistance = 20f;

    float speed;
    float damage;

    [SerializeField] GameObject bulletPrefab; //총알 프리팹
    [SerializeField] GameObject reflectBulletPrefab; //반사 총알 프리팹
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
        MakeBulletPool();
    }

    private void MakeBulletPool()
    {
        PoolManager.Instance.PoolInit(bulletPrefab, bulletPoolSize);

        //블레이더 프로토콜의 경우 반사 총알을 위한 추가 풀이 필요합니다
        if (GameManager.Instance.CurProtocol == ProtocolCard.Protocol.Blader)
            PoolManager.Instance.PoolInit(reflectBulletPrefab, bulletPoolSize);
    }

    public void TryAttack(float speed, float damage)
    {
        this.damage = damage;
        
        // 총알 사용
        this.speed = speed;

        curBullet = PoolManager.Instance.Get(bulletPrefab).GetComponent<BulletBase>();
        curBullet.GetComponent<SpriteRenderer>().color = originColor;
        curBullet.transform.position = firePoint.position; //총알 위치 초기화

        Attack();
    }


    internal override void Attack()
    {
        OnShoot?.Invoke();

        // 총알 사용
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
