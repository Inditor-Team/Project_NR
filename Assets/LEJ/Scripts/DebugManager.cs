using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public GameObject debugCanvas;
    public ItemSO spawnItem;
    public Transform spawnPos;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
        {
            debugCanvas.SetActive(!debugCanvas.activeInHierarchy);
            Debug.Log($"디버그창 {debugCanvas.activeInHierarchy}");
        }
    }

    public void SpawnItem()
    {
        ItemSpawner.Instance.SpawnItem(spawnItem, spawnPos);
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

    public void KillAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = enemies.Length - 1; i >= 0; i--)
        {
            var enemyController = enemies[i].GetComponent<EnemyBaseController>();

            if (enemyController != null)
                enemyController.TakeDamage(100);
        }
    }
}
