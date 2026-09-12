using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵 내 아이템 스폰과 디스폰을 관리
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    static ItemSpawner instance;
    public static ItemSpawner Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<ItemSpawner>();

            DontDestroyOnLoad(instance);
            return instance;
        }
    }

    [SerializeField] List<ItemSO> itemSOs;
    [SerializeField] GameObject itemObjectPrefab;
    int poolSize = 10;

    float itemSpawnProbability = 0.4f;

    public void SetPool()
    {
        PoolManager.Instance.MakeInitPool(itemObjectPrefab, poolSize);
    }

    public void SpawnItem(Vector2 spawnPos)
    {
        //아이템 스폰 확률 적용
        if (Random.value > itemSpawnProbability)
            return;

        //풀 매니저에서 오브젝트 가져오기
        GameObject newGO = PoolManager.Instance.Get(itemObjectPrefab);
        newGO.transform.position = spawnPos;

        //가중치 적용 해 스폰 될 아이템 고르기
        ItemSO pickItem = PickItem();

        if (pickItem == null)
            return;
        
        //ItemObject 로 아이템 정보 세팅
        newGO.GetComponent<ItemObject>().SetItem(pickItem);

        //스폰 위치 세팅
        newGO.SetActive(true);
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

    /// <summary>
    /// 가중치를 적용해 아이템을 고릅니다
    /// </summary>
    /// <returns></returns>
    ItemSO PickItem()
    {
        int totalWeight = 0;
        foreach (var element in itemSOs)
            totalWeight += element.weight;

        if (totalWeight <= 0)
            return null;

        int random = Random.Range(0, totalWeight);

        foreach (var element in itemSOs)
        {
            if (random < element.weight)
                return element;

            random -= element.weight;
        }

        return null;
    }
}
