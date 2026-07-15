using System;
using UnityEngine;

public class Gun : WeaponBase
{

    [Tooltip("ÃÑ±¸ À§Ä¡")]
    [SerializeField] Transform firePoint; //ÃÑ±¸ À§Ä¡
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;

    float fireRate;
    float lastFireTime;
    float speed;
    float damage;

    [SerializeField] GameObject bulletPrefab; //ÃÑ¾Ë ÇÁ¸®ÆÕ
    private int bulletPoolSize = 20;
    BulletBase curBullet;

    private void Start()
    {
        MakeBulletPool();
    }

    private void MakeBulletPool()
    {
        PoolManager.Instance.PoolInit(bulletPrefab, bulletPoolSize);
    }

    public void TryAttack(float fireRate, float speed, float damage)
    {
        if (Time.time - lastFireTime < fireRate) 
            return;

        this.fireRate = fireRate;
        this.speed = speed;
        this.damage = damage;

        curBullet = PoolManager.Instance.Get(bulletPrefab).GetComponent<BulletBase>();
        curBullet.transform.position = firePoint.position; //ÃÑ¾Ë À§Ä¡ ÃÊ±âÈ­

        Attack();
        lastFireTime = Time.time;
    }

    internal override void Attack()
    {
        if (curBullet == null)
            return;

        curBullet.OnFire(-firePoint.right, speed, damage, bulletPrefab);
    }
}
