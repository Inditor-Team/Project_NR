using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PlayerInputActions input;

    Rigidbody2D rb;
    Animator anim;

    Vector2 moveInput;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rollSpeed = 10f;
    [SerializeField] float rollDuration = 0.2f;
    [SerializeField] float attackDuration = 0.1f;

    float rollTimer;
    float attackTimer;

    enum PlayerState
    {
        Idle,
        Move,
        PrimaryAttack, //Sword
        SecondaryAttack, //Gun
        Roll
    }

    PlayerState currentState;

    void Awake()
    {
        input = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        input.Player.Enable();

        input.Player.PrimaryAttack.performed += _ => TryPrimaryAttack();
        input.Player.SecondaryAttack.performed += _ => TrySecondaryAttackAttack();
        input.Player.Roll.performed += _ => TryRoll();
    }

    void OnDisable()
    {
        input.Player.Disable();
    }

    void Update()
    {
        moveInput = input.Player.Move.ReadValue<Vector2>();

        HandleState();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        Move();
    }


    /// <summary>
    /// FSM 상태 변경 & 애니메이션 제어
    /// </summary>
    /// <param name="newState"></param>
    void ChangeState(PlayerState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case PlayerState.Idle:
                break;

            case PlayerState.Move:
                break;

            case PlayerState.Roll:
                anim.SetBool("IsRoll", true);
                rollTimer = rollDuration;
                break;

            case PlayerState.PrimaryAttack:
                anim.SetBool("IsSword", true);
                attackTimer = attackDuration;
                break;

            case PlayerState.SecondaryAttack:
                anim.SetBool("IsGun", true);
                attackTimer = attackDuration;
                break;
        }
    }

    /// <summary>
    /// FSM 상태 전이 조건 처리
    /// </summary>
    void HandleState()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                if (moveInput.magnitude > 0)
                    ChangeState(PlayerState.Move);
                break;

            case PlayerState.Move:
                if (moveInput.magnitude == 0)
                    ChangeState(PlayerState.Idle);
                break;

            case PlayerState.Roll:
                rollTimer -= Time.deltaTime;
                if (rollTimer <= 0)
                {
                    ChangeState(PlayerState.Idle);
                    anim.SetBool("IsRoll", false);
                }
                break;

            case PlayerState.PrimaryAttack:
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    ChangeState(PlayerState.Idle);
                    anim.SetBool("IsSword", false);
                }
                break;

            case PlayerState.SecondaryAttack:
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    ChangeState(PlayerState.Idle);
                    anim.SetBool("IsGun", false );
                }
                break;
        }
    }

    
    void TryPrimaryAttack()
    {
        if (currentState == PlayerState.Roll) return;
        if (currentState == PlayerState.PrimaryAttack) return;
        if (currentState == PlayerState.SecondaryAttack) return;

        ChangeState(PlayerState.PrimaryAttack);
    }

    void TrySecondaryAttackAttack()
    {
        if (currentState == PlayerState.Roll) return;
        if (currentState == PlayerState.PrimaryAttack) return;
        if (currentState == PlayerState.SecondaryAttack) return;

        ChangeState(PlayerState.SecondaryAttack);
    }

    void TryRoll()
    {
        if (currentState == PlayerState.Roll) return;
        if (currentState == PlayerState.PrimaryAttack) return;
        if (currentState == PlayerState.SecondaryAttack) return;

        ChangeState(PlayerState.Roll);
    }

    void Move()
    {
        float speed = moveSpeed;

        if (currentState == PlayerState.Roll)
            speed = rollSpeed;

        rb.linearVelocity = moveInput * speed;
    }

    void UpdateAnimator()
    {
        float speed = moveInput.magnitude;
        anim.SetFloat("DirX", moveInput.x);
        anim.SetFloat("DirY", moveInput.y);
        anim.SetFloat("Speed", speed);
    }

    /// <summary>
    /// 공격 애니메이션의 마지막 프레임에서 호출되는 애니메이션 이벤트 함수입니다.
    /// </summary>
    public void EndAttack()
    {
        if (moveInput.magnitude > 0)
            ChangeState(PlayerState.Move);
        else
            ChangeState(PlayerState.Idle);
    }
}