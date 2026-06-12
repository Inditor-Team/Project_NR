using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 입력에 따른 플레이어블 캐릭터의 움직임과 애니메이션 제어
/// </summary>
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

    Vector2 mousePosition;

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

    /// <summary>
    /// 검 공격 시도. Roll 과 Gun 도중 불가
    /// </summary>
    void TryPrimaryAttack()
    {
        if (currentState == PlayerState.Roll) return;
        if (currentState == PlayerState.SecondaryAttack) return;

        ChangeState(PlayerState.PrimaryAttack);
    }

    /// <summary>
    /// 총 공격 시도. Roll 과 Sword 도중 불가
    /// </summary>
    void TrySecondaryAttackAttack()
    {
        if (currentState == PlayerState.Roll) return;
        if (currentState == PlayerState.PrimaryAttack) return;

        ChangeState(PlayerState.SecondaryAttack);
    }

    /// <summary>
    /// 구르기 시도. Gun 과 Sword 도중 불가
    /// </summary>
    void TryRoll()
    {
        if (currentState == PlayerState.PrimaryAttack) return;
        if (currentState == PlayerState.SecondaryAttack) return;

        ChangeState(PlayerState.Roll);
    }

    /// <summary>
    /// 캐릭터 이동. Roll 상태에선 이속 증가
    /// </summary>
    void Move()
    {
        float speed = moveSpeed;

        if (currentState == PlayerState.Roll)
            speed = rollSpeed;

        rb.linearVelocity = moveInput * speed;
    }

    /// <summary>
    /// 플레이어 입력에 따라 애니메이션 파타미터를 업데이트
    /// </summary>
    void UpdateAnimator()
    {
        //키보드 입력에 따라 애니메이터에서 캐릭터의 Run 방향 변경
        float speed = moveInput.magnitude;
        anim.SetFloat("DirX", moveInput.x);
        anim.SetFloat("DirY", moveInput.y);
        anim.SetFloat("Speed", speed);

        //마우스 위치에 따라 애니메이터에서 캐릭터의 Idle 방향 변경
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Vector3 mouseScreen3D = new Vector3(mouseScreen.x, mouseScreen.y, -Camera.main.transform.position.z);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen3D);

        Vector2 aim = (mouseWorld - transform.position);

        anim.SetFloat("AimX", aim.x);
        anim.SetFloat("AimY", aim.y);
    }

}