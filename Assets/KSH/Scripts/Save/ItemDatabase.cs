using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "KSH/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private ItemSO[] items;

    private Dictionary<ItemId, ItemSO> lookup;

    public ItemSO Get(ItemId id)
    {
        if (id == ItemId.None)
            return null;

        if (lookup == null)
            BuildLookup();

        if (!lookup.TryGetValue(id, out ItemSO item))
            Debug.LogError($"[ItemDatabase] 등록되지 않은 아이템 ID: {id}");

        return item;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<ItemId, ItemSO>();

        foreach (ItemSO item in items)
        {
            if (item == null) continue;

            if (item.Id == ItemId.None)
            {
                Debug.LogWarning($"[ItemDatabase] Id가 None인 아이템: {item.name}", item);
                continue;
            }

            if (!lookup.TryAdd(item.Id, item))
                Debug.LogError($"[ItemDatabase] 중복 ID: {item.Id} ({item.name})", item);
        }
    }

    // 에디터에서 목록을 수정하면 캐시를 다시 만들도록
    private void OnValidate() => lookup = null;
    private void OnEnable() => lookup = null;
}