using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] gunTransform;
    
    
    
    private float shootSpeed = 10f;
    private float fireInterval = 0.2f; // 발사 간격, 0.2
    private float shootTimeInterval = 2f; // 1회 간격, 2

    private Transform target; // 플레이어
    private Coroutine shootRoutine;

    private int fireCount = 10;
    private int shootTimeCount = 5;

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
        
        // TODO: 재장전
    }

    private void Shoot(Transform gun)
    {
        if (target == null) return;

        Vector2 spawnPos = gun.position;
        Vector2 direction = ((Vector2)target.position - spawnPos).normalized;

        GameObject enemyBullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity); // TODO: 추후 오브젝트 풀링 적용
        
        Rigidbody2D rb = enemyBullet.GetComponent<Rigidbody2D>(); // TODO: 오브젝트 풀링 적용할 때 rigidbody도 캐싱하기
        rb.AddForce(direction * shootSpeed, ForceMode2D.Impulse); // Impulse = 일정 속도로 AddForce 적용
    }
}