using UnityEngine;
using System.Collections.Generic;


public class SpawnUserEnemy : MonoBehaviour
{
    [SerializeField] private GameObject userPrefab;
    private LayerMask wallLayer;
    
    private List<UserEnemyController> userList = new List<UserEnemyController>();
    
    // 유저 스폰 관련
    private float userSpawnCollisionRadius = 1.5f;
    private const float userSpawnMinRadius = 2.5f;
    private const float userSpawnMaxRadius = 4f;
    private const float userMinAngleGap = 120f; // 두 개체간 최소 각도 차이
    private const int userSpawnMaxAttempts = 8;

    private void Start()
    {
        PoolManager.Instance.MakeInitPool(userPrefab, 5);
    }
    
    public void SetLayerMask(LayerMask wall)
    {
        wallLayer = wall;
    }

    public void SpwanUser()
    {
        LandUserAttack();
    }
    
    private void LandUserAttack() // 유저 사출
    {
        float firstAngle = Random.Range(0f, 360f);

        // 1번 개체: 실패해도 마지막엔 강제로 스폰
        Vector2 firstPos = FindValidSpawnPos(firstAngle, out bool firstFound);
        if (!firstFound)
            firstPos = GetSpawnPosAtAngle(firstAngle, userSpawnMinRadius); // 벽 무시하고 강제 소환

        SpawnUser(firstPos);

        // 2번 개체: 1번과 각도 차이를 두고 시도, 실패하면 스킵하기
        float secondAngle = firstAngle + userMinAngleGap * (Random.value < 0.5f ? 1f : -1f);
        Vector2 secondPos = FindValidSpawnPos(secondAngle, out bool secondFound);
        if (secondFound) SpawnUser(secondPos);
    }

    // 주어진 각도 기준으로 반경 내 랜덤 위치를 뽑아 벽 체크, 성공하면 위치와 true 반환
    private Vector2 FindValidSpawnPos(float baseAngle, out bool found)
    {
        for (int i = 0; i < userSpawnMaxAttempts; i++)
        {
            // 각도에 약간의 편차를 줘서 매번 완전히 동일한 위치가 나오지 않도록
            float angle = baseAngle + Random.Range(-15f, 15f);
            float radius = Random.Range(userSpawnMinRadius, userSpawnMaxRadius);
            Vector2 candidate = GetSpawnPosAtAngle(angle, radius);

            if (!Physics2D.OverlapCircle(candidate, userSpawnCollisionRadius, wallLayer))
            {
                found = true;
                return candidate;
            }
        }

        found = false;
        return Vector2.zero;
    }

    private Vector2 GetSpawnPosAtAngle(float angleDeg, float radius)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        return (Vector2)transform.position + offset;
    }

    private void SpawnUser(Vector2 pos)
    {
        GameObject user = PoolManager.Instance.Get(userPrefab);
        UserEnemyController userScript = user.GetComponent<UserEnemyController>();
        userScript.OnUserExpired -= RemoveMineFromList; // 혹시 모르는 중복 방지
        userScript.OnUserExpired += RemoveMineFromList;
        userList.Add(userScript); // 리스트에 저장
        userScript.SpawnForBoss(pos);
    }
    
    private void RemoveMineFromList(UserEnemyController obj)
    {
        userList.Remove(obj);
    }
    
    public void ClearAllUsers() // 보스 사망시 호출되는 함수, 지뢰 폭파
    {
        foreach (UserEnemyController user in userList)
        {
            user.ExpireByBossDeath();
        }
        userList.Clear();
    }
}