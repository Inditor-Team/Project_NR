using UnityEngine;

/// <summary>
/// 플레이어의 입력에 따른 플레이어블 캐릭터 제어
/// </summary>
public class PlayerContoller : MonoBehaviour
{
    #region Variables
    [SerializeField] ProtocolBase protocol;

    PlayerInputActions input;
    Vector2 moveInput;

    PlayerStat stat;
    public PlayerStat Stat => stat;
    PlayerAnimator animator;
    GunShooter shooter;
    SwordAttacker attacker;

    Rigidbody2D rb;

    float rollTimer;
    float lastRollTime;

    enum PlayerState
    {
        Idle,
        Move,
        Roll
    }

    PlayerState curState;
    #endregion

    #region Cycle
    void Awake()
    {
        input = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();

        stat = GetComponent<PlayerStat>();
        animator = GetComponent<PlayerAnimator>();
        attacker = GetComponent<SwordAttacker>();
        shooter = GetComponent<GunShooter>();

        attacker.RegisterStat(stat);
        shooter.RegisterStat(stat);
    }

    void OnEnable()
    {
        //Input System 활성화 후 입력 받아오기
        input.Player.Enable();

        input.Player.PrimaryAttack.performed += _ => TrySwordAttack();
        input.Player.SecondaryAttack.performed += _ => TryGunAttack();
        input.Player.Roll.performed += _ => TryRoll();
        input.Player.SpecialSkill.performed += _ => TryProtocol();
    }

    void OnDisable()
    {
        //Input System 비활성화
        input.Player.Disable();
    }

    void Update()
    {
        if (curState != PlayerState.Roll) //구르기 시 마지막 입력 방향으로 구르기 방향이 고정 됨
            moveInput = input.Player.Move.ReadValue<Vector2>();

        if (animator != null)
            animator.SetMoveInput(moveInput); //애니메이터에게 moveInput 전달

        HandleState();
    }

    void FixedUpdate()
    {
        Move();
    }
    #endregion

    #region FSM
    /// <summary>
    /// FSM 상태 전이 & 애니메이션 출력
    /// </summary>
    void HandleState()
    {
        switch (curState)
        {
            case PlayerState.Idle:
                if (moveInput.magnitude > 0)
                    curState = PlayerState.Move;
                break;

            case PlayerState.Move:
                if (moveInput.magnitude == 0)
                    curState = PlayerState.Idle;
                break;

            case PlayerState.Roll:
                rollTimer -= Time.deltaTime;
                if (rollTimer <= 0)
                    curState = PlayerState.Idle;
                break;
        }
    }
    #endregion

    #region Act
    /// <summary>
    /// 검 공격 시도. Roll 과 Gun 도중 불가
    /// </summary>
    void TrySwordAttack()
    {
        if (curState == PlayerState.Roll) return;

        if (attacker != null)
            attacker.DoAttack();
    }

    /// <summary>
    /// 총 공격 시도. Roll 과 Sword 도중 불가
    /// </summary>
    void TryGunAttack()
    {
        if (curState == PlayerState.Roll) return;

        if (shooter != null)
            shooter.DoAttack();
    }

    /// <summary>
    /// 구르기 시도. Gun 과 Sword 도중 불가
    /// </summary>
    void TryRoll()
    {
        if (moveInput == Vector2.zero) return; //이동하고 있는 경우가 아니면 대쉬 X
        if (Time.time - lastRollTime < stat.StatDic[PlayerStat.Stat.RollRate]) //대시 간격 주기
            return;

        rollTimer = stat.StatDic[PlayerStat.Stat.RollDuration];
        curState = PlayerState.Roll;
        lastRollTime = Time.time;
    }

    void TryProtocol()
    {
        protocol.TryProtocol();
    }

    /// <summary>
    /// 캐릭터 이동. Roll 상태에선 이속 증가
    /// </summary>
    void Move()
    {
        float speed = stat.StatDic[PlayerStat.Stat.MoveSpeed];

        ////프로토콜 여부에 따른 이속 변화 존재(기본 값은 1)
        //if (protocol != null)
        //    speed = protocol.IsActive ? moveSpeed * protocol.SpeedMultiplier: moveSpeed;

        if (curState == PlayerState.Roll)
            speed = stat.StatDic[PlayerStat.Stat.RollSpeed];

        rb.linearVelocity = moveInput * speed;
    }

    void EnableMoveFlip()
    {
        animator.DoFlip = true;
    }
    #endregion
}