using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyScope detectScope;

    private void OnEnable()
    {
        detectScope.OnScopeTriggerEnter += OnScopeEnter;
        detectScope.OnScopeTriggerExit += OnScopeExit;
        detectScope.OnScopeTriggerStay += OnScopeStay;
    }

    private void OnDisable()
    {
        detectScope.OnScopeTriggerEnter -= OnScopeEnter;
        detectScope.OnScopeTriggerExit -= OnScopeExit;
        detectScope.OnScopeTriggerStay -= OnScopeStay;
    }

    private void OnScopeEnter(Collider2D other) // 추적 시작
    { 
        if (!other.CompareTag("Player"))
            return;
        
        Debug.Log("Scope Enter");
    }
    
    private void OnScopeExit(Collider2D other) // 계속 추적 혹은 공격 중지
    {
        if (!other.CompareTag("Player"))
            return;
        
        Debug.Log("Scope Exit");
    }
    
    private void OnScopeStay(Collider2D other) // 횡 이동
    {
        if (!other.CompareTag("Player"))
            return;
    }
}
