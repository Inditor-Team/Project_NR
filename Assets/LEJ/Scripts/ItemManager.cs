using UnityEngine;

/// <summary>
/// 맵 내 아이템 스폰과 디스폰을 관리
/// </summary>
public class ItemManager : MonoBehaviour
{
    static ItemManager instance;
    public static ItemManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<ItemManager>();
            return instance;
        }

    }

    [SerializeField] GameObject itemObjectPrefab;
    int poolSize = 10;

    public void SetPool()
    {
        PoolManager.Instance.MakeInitPool(itemObjectPrefab, poolSize);
    }

    public GameObject SpawnItem(ItemSO item, Transform spawnPos)
    {
        //풀 매니저에서 오브젝트 가져오기
        GameObject newGO = PoolManager.Instance.Get(itemObjectPrefab);
        newGO.transform.position = spawnPos.position;
        
        //ItemObject 로 아이템 정보 세팅
        newGO.GetComponent<ItemObject>().SetItem(item);

        newGO.SetActive(true);

        return newGO;
    }

    public void DespawnItem(GameObject instance)
    {
        //풀 매니저에서 오브젝트 반환
        PoolManager.Instance.Release(itemObjectPrefab, instance);
    }
}
