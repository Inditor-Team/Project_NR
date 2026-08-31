using System.Linq;
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

    [SerializeField] private GameObject wasp_prefab;
    [SerializeField] private GameObject mine_prefab;
    [SerializeField] private GameObject user_prefab;

    [SerializeField] private GameObject enemyHealthBar;
    [SerializeField] private Slider enemyHealthSlider;
    private int remainingCount = -1; //맵에 남은 적

    private void Start()
    {
        remainingCount = curSectorSO.EnemyData.Length;
        GameManager.Instance.RegisterSectorManagerEvent(SceneController.Instance.curScene);
    }

    public void SpawnEnemy()
    {
        if (curSectorSO == null)
            return;

        GameObject curPrefab = null;

        for (int i = 0; i < curSectorSO.EnemyData.Length; i++)
        {
            switch (curSectorSO.EnemyData[i].type)
            {
                case SectorSO.EnemyType.Wasp:
                    curPrefab = wasp_prefab;
                    break;
                case SectorSO.EnemyType.Mine:
                    curPrefab = mine_prefab;
                    break;
                case SectorSO.EnemyType.User:
                    curPrefab = user_prefab;
                    break;
            }

            Instantiate(curPrefab, curSectorSO.EnemyData[i].spawnPos, Quaternion.identity);
        }
    }

    public void DestroyedEnemy() // 적이 파괴되면 호출
    {
        remainingCount--;

        if (remainingCount <= 0)
            SectorClear();
    }

    public event UnityAction<SectorSO.SectorType> OnSectorClear;
    public event UnityAction<SectorSO.SectorType> OnSectorFail;

    public void SectorClear() // 맵 내의 적 전부 처리 시 실행
    {
        Debug.Log("Section Clear!");
        OnSectorClear?.Invoke(curSectorSO.Type);
    }

    public void SectorFail()
    {
        Debug.Log("Section Fail!");
        OnSectorFail?.Invoke(curSectorSO.Type);
    }
}
