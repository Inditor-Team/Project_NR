using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public GameObject debugCanvas;
    public ItemSO spawnItem;
    public Transform spawnPos;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            debugCanvas.SetActive(!debugCanvas.activeInHierarchy);
            Debug.Log($"디버그창 {debugCanvas.activeInHierarchy}");
        }
    }

    public void SpawnItem()
    {
        ItemManager.Instance.SpawnItem(spawnItem, spawnPos);
    }

    public void SetNeuroAction()
    {
        GameManager.Instance.SetProtocol(ProtocolCard.Protocol.NeuroAction);
    }

    public void SetBlader()
    {
        GameManager.Instance.SetProtocol(ProtocolCard.Protocol.Blader);
    }

    public void SetBlitz()
    {
        GameManager.Instance.SetProtocol(ProtocolCard.Protocol.Blitz);
    }

    public void ForceSectorClear()
    {
        SectorManager.Instance.SectorClear();
    }
}
