using UnityEngine;
using System;

public class EnemyScope : MonoBehaviour
{
    public event Action<Collider2D> OnScopeTriggerEnter;

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnScopeTriggerEnter?.Invoke(other);
    }
}
