using UnityEngine;

public class EnemyPlacer : MonoBehaviour
{
    [SerializeField] private GameObject minePrefab;
    
    private float waitTime; // 터지기 전까지 대기 시간
    private float damage;
    
    private void Start() // 테스트 끝나면 Awake로 변경
    {
        PoolManager.Instance.MakeInitPool(minePrefab, 5);
    }

    public void SetValue(float newTime, float newDamage)
    {
        waitTime = newTime;
        damage = newDamage;
    }
    
    public void PlaceMine()
    {
        GameObject mineObject = PoolManager.Instance.Get(minePrefab);

        if (mineObject == null) return; //null 뜨는 경우가 있어 예외처리
        mineObject.GetComponent<EnemyLandMine>().SetValue(waitTime, damage);

        mineObject.transform.position = transform.position;
        
        // 나중에 설치 효과음 추가
        // if (SoundManager.Instance != null)
            // SoundManager.Instance.PlaySFX(Sound_SFX.Enemy_Attack);
    }
}
