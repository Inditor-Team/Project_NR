using System;
using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] gunTransform;
    
    private int defaultCapacity = 10;
    private int maxPoolSize = 20;
    
    private float shootSpeed = 3f;
    
    public float fireInterval = 0.5f; // 발사 간격, 0.2
    public float shootTimeInterval = 2f; // 1회 간격, 2
    private float reloadTime = 5f;

    private Transform target; // 플레이어
    private Coroutine shootRoutine;

    private int fireCount = 6;
    private int shootTimeCount = 5;
    private float damage;
    
    public event Action OnReloadStart;
    public event Action OnReloadEnd;
    
    private void Start()
    {
        PoolManager.Instance.PoolInit(bulletPrefab, defaultCapacity, maxPoolSize);
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
    
    public void StartShooting(Transform playerTransform) // 아예 플레이어 transform를 참조하기, 변동되는 position 따라 잡기 위해
    {
        target = playerTransform;
        
        if (shootRoutine == null)
            shootRoutine = StartCoroutine(ShootRoutine());
    }
    
    public void StopShooting()
    {
        if (shootRoutine != null)
        {
            StopCoroutine(shootRoutine);
            shootRoutine = null;
        }
        target = null;
    }

    private IEnumerator ShootRoutine()
    {
        for (int i = 0; i < shootTimeCount; i++)
        {
            for (int j = 0; j < fireCount; j++)
            {
                Shoot(gunTransform[j%2]);
                yield return new WaitForSeconds(fireInterval);
            }
            yield return new WaitForSeconds(shootTimeInterval);
        }
        
        StartCoroutine(Reload()); // TODO: 변수 만들어서 null 처리
    }

    private IEnumerator Reload()
    {
        OnReloadStart?.Invoke();
        yield return new WaitForSeconds(reloadTime);
        OnReloadEnd?.Invoke();
    }
    
    private void Shoot(Transform gun)
    {
        if (target == null) return;

        Vector2 spawnPos = gun.position;
        Vector2 direction = ((Vector2)target.position - spawnPos).normalized;
        
        GameObject enemyBullet = PoolManager.Instance.Get(bulletPrefab); // bulletPool.Get();
        enemyBullet.transform.position = spawnPos;
        
        enemyBullet.GetComponent<EnemyBullet>().Launch(direction, shootSpeed, damage);
    }
}