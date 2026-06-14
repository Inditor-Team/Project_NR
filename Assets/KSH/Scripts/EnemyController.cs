using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints; // 지점별 순찰 방식, NavMeshAgent는 일단 보류
    [SerializeField] private EnemyScope detectScope;
    [SerializeField] private GameObject bullet;

    [SerializeField] private Transform target; // 나중에 GameManager에서 받아오도록 수정
    
    // 스프라이트 적용
    private SpriteRenderer sprite;
    private Animator anim;
    public Vector2 nextvec;
    
    
    public float speed = 0.5f;
    private bool isPatrol; // 순찰(패트롤) 상태인지 판단 여부
    private int currentPatrolIndex; // 순찰 지점 인덱스
    private Transform patrolNextPosition;
    private Rigidbody2D rigid;

    private void Start()
    {
        isPatrol = true;
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

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

    private void FixedUpdate()
    {
        if (!isPatrol)
            return;
        
        patrolNextPosition = patrolPoints[currentPatrolIndex];
        Vector2 dir = patrolNextPosition.position - transform.position;
        nextvec = dir.normalized * speed * Time.fixedDeltaTime;
        
        rigid.MovePosition(rigid.position + nextvec);
        rigid.linearVelocity = Vector2.zero; // 유니티6는 velocity에서 linearVelocity로 변경, 추후 찾아보기

        if (Vector3.Distance(transform.position, patrolNextPosition.position) < 0.2f) // 근처 도착
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void LateUpdate()
    {
        if (!isPatrol)
            return;
        
        if (nextvec.x != 0)
        {
            sprite.flipX = nextvec.x < 0;
        }
    }

    private void OnScopeEnter(Collider2D other) // 추적 시작
    { 
        if (!other.CompareTag("Player"))
            return;

        isPatrol = false;
        
        Debug.Log("Scope Enter");
    }
    
    private void OnScopeExit(Collider2D other) // 계속 추적하며 공격 혹은 순찰 상태 복귀
    {
        if (!other.CompareTag("Player"))
            return;

        isPatrol = true;
        
        Debug.Log("Scope Exit");
    }
    
    private void OnScopeStay(Collider2D other) // 횡 이동
    {
        /*if (!other.CompareTag("Player"))
            return;*/
    }
}
