using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform[] gunTransform;
    
    private float shootSpeed = 3f;
    
    public float fireInterval = 0.5f; // 발사 간격, 0.2
    public float shootTimeInterval = 2f; // 1회 간격, 2
    private float reloadTime = 5f;

    private Transform target; // 플레이어
    private Coroutine shootRoutine;

    private int fireCount = 6;
    private int shootTimeCount = 5;
    private float damage;
    
    public event Action OnReloadStart; // 재장전
    public event Action OnReloadEnd;
    public event Action OnShootIntervalStart; // 1회 간격 대기
    public event Action OnShootIntervalEnd;
    private bool isPaused;

    private List<EnemyBullet> bulletList = new List<EnemyBullet>();


    private void Start()
    {
        isPaused = false;
        PoolManager.Instance.MakeInitPool(bulletPrefab, 10);
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetCount(int shootTimeCount, int fireCount, float reloadTime)
    {
        this.shootTimeCount = shootTimeCount;
        this.fireCount = fireCount;
        this.reloadTime = reloadTime;
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
    
    public void Pause(bool isPause)
    {
        isPaused = isPause;
    }

    private IEnumerator ShootRoutine()
    {
        for (int i = 0; i < shootTimeCount; i++)
        {
            for (int j = 0; j < fireCount; j++)
            {
                yield return WaitWhilePaused(); // 혹시 모르는 일시 정지 체크
                Shoot(gunTransform[j%2]);
                yield return WaitForSecondsPausable(fireInterval);
            }
            OnShootIntervalStart?.Invoke();
            yield return WaitForSecondsPausable(shootTimeInterval);
            OnShootIntervalEnd?.Invoke();
        }
        StartCoroutine(Reload()); // TODO: 변수 만들어서 null 처리
    }

    private IEnumerator Reload()
    {
        OnReloadStart?.Invoke();
        yield return WaitForSecondsPausable(reloadTime);
        OnReloadEnd?.Invoke();
    }
    
    private void Shoot(Transform gun)
    {
        if (target == null) return;

        Vector2 spawnPos = gun.position;
        Vector2 direction = ((Vector2)target.position - spawnPos).normalized;
        
        GameObject enemyBullet = PoolManager.Instance.Get(bulletPrefab); // bulletPool.Get();

        if (enemyBullet == null) return; //null 뜨는 경우가 있어 예외처리

        enemyBullet.transform.position = spawnPos;
        
        EnemyBullet enemyBulletScript = enemyBullet.GetComponent<EnemyBullet>();
        enemyBulletScript.Launch(direction, shootSpeed, damage);
        enemyBulletScript.OnBulletExpired -= RemoveBulletFromList; // 혹시 모르는 중복 방지
        enemyBulletScript.OnBulletExpired += RemoveBulletFromList;
        bulletList.Add(enemyBulletScript); // 리스트에 저장

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Attack);
    }
    
    // 일시 정지 때 쓸 코루틴 용 시간 함수
    private IEnumerator WaitForSecondsPausable(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (!isPaused)
                timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator WaitWhilePaused()
    {
        while (isPaused)
            yield return null;
    }

    private void RemoveBulletFromList(EnemyBullet obj)
    {
        bulletList.Remove(obj);
    }
    
    public void ClearAllBullets() // 보스 사망시 호출되는 함수, 총알 소멸
    {
        // 현재는 문제 없으나 나중에 리스트 더 사용한다면, 복사한 리스트로 돌리기. foreach 순회 중 리스트 제거 문제
        foreach (EnemyBullet bullet in bulletList)
        {
            bullet.ExpireByBossDeath();
        }
        bulletList.Clear();
    }
}