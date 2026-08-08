using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public ItemSO spawnItem;
    public Transform spawnPos;

    public void SpawnItem()
    {
        ItemManager.Instance.SpawnItem(spawnItem, spawnPos);
    }
}
