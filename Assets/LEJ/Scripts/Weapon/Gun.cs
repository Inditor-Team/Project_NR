using System;
using UnityEngine;

public class Gun : WeaponBase
{
    [Header("총 스탯")]
    [SerializeField] float damage;
    [SerializeField] float speed;
    [SerializeField] float fireRate; //발사 속도

    [Header("외부 오브젝트")]
    [SerializeField] Transform firePoint; //총구 위치

    float lastFireTime;

    [SerializeField] BulletBase bulletPrefab; //총알 프리팹
    private BulletBase[] bulletPool; //총알 오브젝트 풀
    private int bulletPoolSize = 20;
    private int _bulletIndex = 0;
    private int bulletIndex {
        get { return _bulletIndex; }
        set { _bulletIndex = value % bulletPoolSize; }
    }

    private SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        MakeBulletPool();
        lastFireTime = -fireRate; //처음에 바로 발사할 수 있도록 초기화
    }

    private void MakeBulletPool()
    {
        bulletPool = new BulletBase[bulletPoolSize];

        for (int i = 0; i < bulletPoolSize; i++) 
        {
            BulletBase bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bulletPool[i] = bullet;
            bullet.Init(damage, speed); //총알 초기화
            bullet.gameObject.SetActive(false); 
        }
    }

    public override void TryAttack()
    {
        if (Time.time - lastFireTime < fireRate) 
            return;

        Attack();
        lastFireTime = Time.time;
    }

    internal override void Attack()
    {
        bulletPool[bulletIndex].transform.position = firePoint.position; //총알 위치 초기화
        bulletPool[bulletIndex].gameObject.SetActive(true); //총알 활성화
        bulletPool[bulletIndex].OnFire(-firePoint.right);
        bulletIndex++;
    }
}
