using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract class PoolObjectBase : MonoBehaviour
{
    public abstract void SetOriginPrefab(GameObject prefab);
}

public class PoolManager : MonoBehaviour
{
    private Dictionary<GameObject, ObjectPool<GameObject>> pools;
    
    public static PoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        
        pools = new Dictionary<GameObject, ObjectPool<GameObject>>();
    }
    
    public void PoolInit(GameObject prefab, int defaultCapacity = 10, int maxPoolSize = 20)
    {
        var pool = new ObjectPool<GameObject>(
            () => CreatePooledItem(prefab),    // 풀이 비었을 때 새로 생성하는 메서드
            OnTakeFromPool,      // 풀에서 가져갈 때 호출되는 메서드 (초기화)
            OnReturnedToPool,    // 풀에 반환될 때 호출되는 메서드 (정리)
            OnDestroyPoolObject, // 풀이 가득 찼거나 파괴될 때 호출되는 메서드
            true,                // Collection Check: 중복 릴리즈 검사
            defaultCapacity,
            maxPoolSize
        );
        pools[prefab] = pool;
    }

    public GameObject Get(GameObject prefab) // 오브젝트 풀을 가져올 때 사용
    {
        if (!pools.ContainsKey(prefab))
            return null;
          
        return pools[prefab].Get();
    }

    public void Release(GameObject prefab, GameObject instance) // 오브젝트 반납
    {
        if (pools.ContainsKey(prefab))
        {
            pools[prefab].Release(instance);
        }
    }

    public void ClearPool() // 씬 전환 등으로 풀을 비울 때 사용
    {
        foreach (var pool in pools)
        {
            pools[pool.Key].Clear();
        }
        pools.Clear();
    }

    protected virtual GameObject CreatePooledItem(GameObject prefab)
    {
        GameObject newObj = Instantiate(prefab);
        PoolObjectBase newObjScript = newObj.GetComponent<PoolObjectBase>();
        
        if (newObjScript)
        {
            newObjScript.SetOriginPrefab(prefab);
        }

        return newObj;
    }
    
    protected virtual void OnTakeFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }
    
    private void OnReturnedToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }
}
