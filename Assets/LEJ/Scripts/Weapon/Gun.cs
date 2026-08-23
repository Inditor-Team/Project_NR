using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Gun : WeaponBase
{
    [Tooltip("ÃÑ±¸ À§Ä¡")]
    [SerializeField] Transform firePoint; //ÃÑ±¸ À§Ä¡
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;
    //[SerializeField] LineRenderer lineRenderer;
    //[SerializeField] private float laserDuration = 1f;
    //private float disableTime;
    float maxDistance = 20f;

    float speed;
    float damage;

    [SerializeField] GameObject bulletPrefab; //ÃÑ¾Ë ÇÁ¸®ÆÕ
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
    }

    public void TryAttack(float speed, float damage)
    {
        this.damage = damage;
        
        // ÃÑ¾Ë »ç¿ë
        this.speed = speed;

        curBullet = PoolManager.Instance.Get(bulletPrefab).GetComponent<BulletBase>();
        curBullet.GetComponent<SpriteRenderer>().color = originColor;
        curBullet.transform.position = firePoint.position; //ÃÑ¾Ë À§Ä¡ ÃÊ±âÈ­

        Attack();
    }

    internal override void Attack()
    {
        OnShoot?.Invoke();

        // ÃÑ¾Ë »ç¿ë
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

        //PlayLaserEffect(startPosition, endPosition);
    }

    public void FireReflectBullet(Vector2 startPos, Vector2 dir, float speed, float damage)
    {
        GameObject bulletObject = PoolManager.Instance.Get(bulletPrefab);

        if (bulletObject == null)
            return;

        bulletObject.SetActive(true);

        curBullet = bulletObject.GetComponent<BulletBase>();

        if (curBullet == null)
            return;

        curBullet.transform.position = startPos;
        curBullet.GetComponent<SpriteRenderer>().color = Color.green;

        curBullet.OnFire(dir, speed, damage, bulletPrefab);
    }

    //private Coroutine laserCoroutine;


    //public void PlayLaserEffect(Vector2 start, Vector2 end)
    //{
    //    if (laserCoroutine == null)
    //        laserCoroutine = StartCoroutine(PlayLaserRoutine(start, end));
    //}
    
    //private IEnumerator PlayLaserRoutine(Vector2 start, Vector2 end)
    //{
    //    const float duration = 0.06f;
    
    //    laserTrail.emitting = false;
    //    laserTrail.Clear();

    //    yield return null;

    //    laserTrail.transform.position = start;
    //    laserTrail.emitting = true;
    
    //    float elapsed = 0f;
    
    //    while (elapsed < duration)
    //    {
    //        float t = Mathf.Clamp01(elapsed / duration);
    
    //        laserTrail.transform.position = Vector2.Lerp(start, end, t);
    
    //        elapsed += Time.deltaTime;
    
    //        yield return null;
    //    }
    //    laserTrail.transform.position = end;

    //    laserTrail.emitting = false;
    //    laserCoroutine = null;
    //}
}
