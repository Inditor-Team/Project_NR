using System;
using UnityEngine;
using static SectorSO;

[CreateAssetMenu(fileName = "SectorSO", menuName = "LEJ/SectorSO")]
public class SectorSO : ScriptableObject
{
    public enum SectorType { None, Normal, Hard, Store, Event, Boss, Count } //수정 예정
    public enum EnemyType { None, Wasp, Mine, User, MotherCore, Count }

    public SceneController.Scene Scene;
    public SectorType Type;

    public Vector2 PlayerSpanwPos;

    public SectorEnemyData[] EnemyData;

    public GameObject[] GridPrefab;
}

[Serializable]
public class SectorEnemyData
{
    public EnemyType type;
    public Vector2 spawnPos;
}
