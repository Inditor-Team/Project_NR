using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Store : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerStat playerStat;

    [Header("Shop")]
    [SerializeField] private ItemSO[] itemPool;
    [SerializeField] private int itemCount = 3;

    private ItemSO[] shopItems;
    public ItemSO[] ShopItems => shopItems;


    [SerializeField] private ItemObject itemObjectPrefab;
    [SerializeField] private Transform itemSpawnPoint;

    [Header("Item Rail")]
    [SerializeField] private float railDistance = 10f;
    [SerializeField] private float railDuration = 0.5f;

    [Header("Hack")]
    [Range(0f, 1f)]
    [SerializeField] private float hackSuccessProbability = 0.5f;
    [SerializeField] private float hackFailDamage = 1f;

    private bool isHackAttempted;
    public bool IsHackAttempted => isHackAttempted;

    private void Start()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.StoreNPCSound); //��ǻ�� ���� ȿ����

        GenerateItems();
    }
    
    public int SlotCount => shopItems != null ? shopItems.Length : 0;

    // ���Կ� �Ǹ� ���� �������� ������ true
    public bool TryGetItem(int index, out ItemSO item)
    {
        item = null;
        if (!IsValidIndex(index))
            return false;

        item = shopItems[index];
        return true;
    }

    // ���� ������ ����ų�, ���� ������ ��� ��� true
    public bool IsSoldOut(int index)
    {
        return !IsValidIndex(index);
    }


    /// <summary>
    /// �ߺ����� �ʴ� �������� �������� �̽��ϴ�.
    /// </summary>
    public void GenerateItems()
    {
        int count = Mathf.Min(itemCount, itemPool.Length);

        shopItems = new ItemSO[count];

        List<ItemSO> candidates = new List<ItemSO>(itemPool);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);

            shopItems[i] = candidates[randomIndex];
            candidates.RemoveAt(randomIndex);
        }
    }

    /// <summary>
    /// ������ �������� �����մϴ�
    /// </summary>
    /// <param name="index"></param>
    public bool BuyItem(int index)
    {
        if (!IsValidIndex(index))
            return false;

        SpawnItem(shopItems[index]);
        shopItems[index] = null;
        return true;
    }

    /// <summary>
    /// ������ �������� ���� ���� �����մϴ�.
    /// </summary>
    public void SpawnItem(ItemSO item)
    {
        GameObject itemObject = ItemSpawner.Instance.SpawnItem(item, itemSpawnPoint);

        StartCoroutine(MoveItem(itemObject.transform));
    }

    /// <summary>
    /// ������ �������� ���������� �̵���ŵ�ϴ�.
    /// </summary>
    private IEnumerator MoveItem(Transform item)
    {
        Vector3 startPos = item.position;
        Vector3 endPos = startPos + Vector3.left * railDistance;

        float time = 0f;

        while (time < railDuration)
        {
            if (item == null)
                yield break;

            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / railDuration);
            item.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        if (item != null)
            item.position = endPos;
    }

    /// <summary>
    /// ��ŷ�� �õ��մϴ�.
    /// </summary>
    public void TryHack(bool isSuccess)
    {
        if (isHackAttempted)
            return;
        isHackAttempted = true;

        if (isSuccess)
        {
            StartCoroutine(SpawnAllItems());
        }
        else
        {
            playerStat.TakeDamage(hackFailDamage);
        }
    }
    
    private bool IsValidIndex(int index)
    {
        return shopItems != null
            && index >= 0
            && index < shopItems.Length
            && shopItems[index] != null;
    }

    private IEnumerator SpawnAllItems()
    {
        for (int i = 0; i < shopItems.Length; i++)
        {
            if (shopItems[i] == null)
                continue;

            SpawnItem(shopItems[i]);
            shopItems[i] = null;

            yield return new WaitForSecondsRealtime(0.15f);
        }
    }
}
