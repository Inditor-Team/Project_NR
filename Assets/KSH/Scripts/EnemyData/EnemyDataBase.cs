using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyData", menuName = "KSH/EnemyData")]
public class EnemyDataBase : ScriptableObject
{
    public float moveSpeed;
    public float combatTargetDist; // 플레이어와의 간격
    public float health;
    public float damage;
}
