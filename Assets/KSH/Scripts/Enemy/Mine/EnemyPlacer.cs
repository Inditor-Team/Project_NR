using UnityEngine;

public class EnemyPlacer : MonoBehaviour
{
    [SerializeField] private GameObject minePrefab;
    
    private float waitTime; // 터지기 전까지 대기 시간
    private float damage;
    
    [SerializeField] private float mineCollisionRadius = 1.5f; // 벽 겹침 검사용 지뢰 반지름 크기
    private const int MaxPlaceAttempts = 10; // 최대 재시도 횟수
    
    private LayerMask wallLayer;
    
    private void Start() // 테스트 끝나면 Awake로 변경
    {
        PoolManager.Instance.MakeInitPool(minePrefab, 5);
    }

    public void SetValue(float newTime, float newDamage)
    {
        waitTime = newTime;
        damage = newDamage;
    }

    public void SetLayerMask(LayerMask wallLayer)
    {
        this.wallLayer = wallLayer;
    }
    
    public void PlaceMine(Vector2 position)
    {
        GameObject mineObject = PoolManager.Instance.Get(minePrefab);

        if (mineObject == null) return; //null 뜨는 경우가 있어 예외처리
        mineObject.GetComponent<EnemyLandMine>().SetValue(waitTime, damage);

        mineObject.transform.position = position;
        
        // 나중에 설치 효과음 추가
        // if (SoundManager.Instance != null)
            // SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Attack);
    }

    
    // targetPos를 중심으로 minRadius~maxRadius 사이의 랜덤 위치에 지뢰를 설치 시도합니다.
    // 벽과 겹치면 최대 MaxPlaceAttempts회까지 재시도하고, 실패 시 false를 반환합니다.
    public bool PlaceMineNear(Vector2 targetPos, float minRadius, float maxRadius)
    {
        for (int i = 0; i < MaxPlaceAttempts; i++)
        {
            Vector2 candidate = GetRandomPointInAnnulus(targetPos, minRadius, maxRadius);

            if (!Physics2D.OverlapCircle(candidate, mineCollisionRadius, wallLayer))
            {
                PlaceMine(candidate);
                return true;
            }
        }

        return false; // 재시도 끝까지 유효 위치를 못 찾음 -> 이번 설치는 스킵
    }

    // 반지름 균일 분포를 위해 sqrt 보정 (중심 쏠림 방지)
    private Vector2 GetRandomPointInAnnulus(Vector2 center, float minR, float maxR)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float sqRandom = Random.Range(minR * minR, maxR * maxR);
        float radius = Mathf.Sqrt(sqRandom);

        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        return center + offset;
    }
}
