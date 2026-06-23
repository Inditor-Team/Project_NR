using UnityEngine;
using System;

public class EnemyScope : MonoBehaviour
{
    public event Action<Collider2D> OnScopeTriggerEnter;
    public event Action<Collider2D> OnScopeTriggerStay;

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnScopeTriggerEnter?.Invoke(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        OnScopeTriggerStay?.Invoke(other);
    }
}
