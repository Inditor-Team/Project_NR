using UnityEngine;

public class SectorSO : MonoBehaviour
{
    public enum SectorType { None, Stage, Store, Etc, Count } //수정 예정

    public SceneController.Scene Scene;
    public SectorType Type;

    public Transform playerSpanwPos;

    public int EnemyCount;
    public Transform[] EnemySpawnPos;

    public GameObject[] GridPrefab;
}
