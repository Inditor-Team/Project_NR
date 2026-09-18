using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] private float railDistance = 3f;
    [SerializeField] private float railDuration = 0.7f;

    [Header("Hack")]
    [Range(0f, 1f)]
    [SerializeField] private float hackSuccessProbability = 0.5f;
    [SerializeField] private float hackFailDamage = 1f;


    private bool isHackAttempted;
    public bool IsHackAttempted => isHackAttempted;

    public event UnityAction OnHackSuccess;
    public event UnityAction OnHackFailed;

    private void Start()
    {
        GenerateItems();
    }

    /// <summary>
    /// 중복되지 않는 아이템을 무작위로 뽑습니다.
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
    /// 선택한 아이템을 구매합니다
    /// </summary>
    /// <param name="index"></param>
    public void BuyItem(int index)
    {
        if (!IsValidIndex(index))
            return;

        ItemSO item = shopItems[index];

        if (GameManager.Instance.Credit < item.Price)
            return;

        GameManager.Instance.Credit -= item.Price;

        SpawnItem(item);

        shopItems[index] = null;
    }

    /// <summary>
    /// 구매한 아이템을 레일 위에 생성합니다.
    /// </summary>
    public void SpawnItem(ItemSO item)
    {
        ItemObject itemObject = Instantiate(
            itemObjectPrefab,
            itemSpawnPoint.position,
            Quaternion.identity
        );

        itemObject.SetItem(item);

        StartCoroutine(MoveItem(itemObject.transform));
    }

    /// <summary>
    /// 생성된 아이템을 오른쪽으로 이동시킵니다.
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
    /// 해킹을 시도합니다.
    /// </summary>
    public void TryHack()
    {
        if (isHackAttempted)
            return;

        isHackAttempted = true;

        if (UnityEngine.Random.value <= hackSuccessProbability)
        {
            StartCoroutine(SpawnAllItems());
            OnHackSuccess?.Invoke();
            return;
        }

        playerStat.TakeDamage(hackFailDamage);
        OnHackFailed?.Invoke();
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
