using System;
using UnityEngine;

public class HardEnemyPatch : MonoBehaviour
{
    [SerializeField] private EnemyShooter[] hardEnemyShooters;
    private float hardFireInterval = 0.3f; // 발사 간격, 0.2
    private float hardShootTimeInterval = 1.5f; // 1회 간격, 2

    private void Start()
    {
        foreach (var shooter in hardEnemyShooters)
        {
            shooter.SetInterval(hardFireInterval, hardShootTimeInterval);
        }
    }
}
