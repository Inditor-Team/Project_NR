using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootSpeed = 10f;
    [SerializeField] private float shootInterval = 1f; // 몇 초 간격으로 shoot

    private Transform target; // 플레이어
    private Coroutine shootRoutine;
    
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
        while (true)
        {
            Shoot();
            yield return new WaitForSeconds(shootInterval); // n초 대기
        }
    }

    private void Shoot()
    {
        if (target == null) return;

        Vector2 spawnPos = transform.position;
        Vector2 direction = ((Vector2)target.position - spawnPos).normalized;

        GameObject enemyBullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity); // TODO: 추후 오브젝트 풀링 적용
        
        Rigidbody2D rb = enemyBullet.GetComponent<Rigidbody2D>(); // TODO: 오브젝트 풀링 적용할 때 rigidbody도 캐싱하기
        rb.AddForce(direction * shootSpeed, ForceMode2D.Impulse); // Impulse = 일정 속도로 AddForce 적용
    }
}