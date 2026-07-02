using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Pool;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] gunTransform;
    
    private IObjectPool<GameObject> bulletPool;
    private int defaultCapacity = 10;
    private int maxPoolSize = 20;
    
    private float shootSpeed = 10f;
    private float fireInterval = 0.2f; // 발사 간격, 0.2
    private float shootTimeInterval = 2f; // 1회 간격, 2
    private float reloadTime = 5f;

    private Transform target; // 플레이어
    private Coroutine shootRoutine;

    private int fireCount = 10;
    private int shootTimeCount = 5;
    
    void Awake()
    {
        bulletPool = new ObjectPool<GameObject>(
            CreatePooledItem,    // 풀이 비었을 때 새로 생성하는 메서드
            OnTakeFromPool,      // 풀에서 가져갈 때 호출되는 메서드 (초기화)
            OnReturnedToPool,    // 풀에 반환될 때 호출되는 메서드 (정리)
            OnDestroyPoolObject, // 풀이 가득 찼거나 파괴될 때 호출되는 메서드
            true,                // Collection Check: 중복 릴리즈 검사
            defaultCapacity,
            maxPoolSize
        );
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
        StopShooting();
        Debug.Log("재장전");
        yield return new WaitForSeconds(reloadTime);

        StartShooting(target);
    }
    
    private void Shoot(Transform gun)
    {
        if (target == null) return;

        Vector2 spawnPos = gun.position;
        Vector2 direction = ((Vector2)target.position - spawnPos).normalized;
        
        GameObject enemyBullet = bulletPool.Get();
        enemyBullet.transform.position = spawnPos;
        
        enemyBullet.GetComponent<EnemyBullet>().Launch(direction, shootSpeed);
    }
    
    // 오브젝트 풀링
    GameObject CreatePooledItem()
    {
        GameObject obj = Instantiate(bulletPrefab);
        obj.GetComponent<EnemyBullet>().SetPool(bulletPool);
        return obj;
    }

    void OnTakeFromPool(GameObject bullet)
    {
        bullet.SetActive(true); 
        bullet.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // 속도 리셋
    }
    
    void OnReturnedToPool(GameObject bullet)
    {
        bullet.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // 속도 리셋
        bullet.SetActive(false);
    }

    void OnDestroyPoolObject(GameObject bullet)
    {
        Destroy(bullet.gameObject);
    }
}