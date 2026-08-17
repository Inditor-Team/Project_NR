using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 섹터(맵) 내의 스폰, 
/// </summary>
public class SectorManager : MonoBehaviour
{
    [SerializeField] SectorSO curSectorSO;

    static SectorManager instance;
    public static SectorManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<SectorManager>();

            return instance;
        }

    }

    private void Awake()
    {
        OnEnemyDestroyed += DestroyedEnemy;
    }

    private void OnDisable()
    {
        OnEnemyDestroyed -= DestroyedEnemy;
    }

    [SerializeField] private GameObject enemyObj;
    [SerializeField] private GameObject enemyHealtheBar;
    [SerializeField] private Slider enemyHealthSlider;
    private int remainingCount = 6; // 맵에 남은 적

    public event UnityAction OnEnemyDestroyed; //해당 이벤트 Invoke 해주시길 바랍니다

    public void SpawnEnemy()
    {
        if (curSectorSO == null)
            return;

        //여기에 SectorSO 를 기반한 몹 스폰 구현 예정
    }

    public void DestroyedEnemy() // 적이 파괴되면 호출
    {
        remainingCount--;

        if (remainingCount <= 0)
            GameManager.Instance.SectionClear();
    }

    public event UnityAction<SectorSO.SectorType> OnSectorClear;
    public event UnityAction<SectorSO.SectorType> OnSectorFail;

    public void SectorClear() // 맵 내의 적 전부 처리 시 실행
    {
        Debug.Log("Section Clear !");
        OnSectorClear?.Invoke(curSectorSO.Type);

        //섹터가 끝나면 GameManager 에 등록한 이벤트 등록 취소
        GameManager.Instance.UnRegisterSectorManagerEvent();
    }

    public void SectorFail()
    {
        Debug.Log("Section Fail!");
        OnSectorFail?.Invoke(curSectorSO.Type);

        //섹터가 끝나면 GameManager 에 등록한 이벤트 등록 취소
        GameManager.Instance.UnRegisterSectorManagerEvent();
    }
}
