using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyData", menuName = "KSH/EnemyData")]
public class EnemyDataBase : ScriptableObject
{
    public float moveSpeed;
    public float health;
    public float damage;
}
