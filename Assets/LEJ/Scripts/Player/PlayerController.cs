using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어의 입력에 따른 플레이어블 캐릭터 제어
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Variables

    PlayerInputActions input;
    Vector2 moveInput;

    PlayerStat stat;
    public PlayerStat Stat => stat;
    PlayerAnimator animator;
    GunShooter gunShooter;
    SwordAttacker swordAttacker;
    ProtocolExecutor protocolExecutor;
    PlayerInventory inventory;

    Rigidbody2D rb;

    float rollTimer;
    float lastRollTime;
    
    private bool isPointerOverUI; // UI 요소인지 감지

    float lastProtocolTime;
    private bool isPaused = false;

    IInteractable curInteractable;

    enum PlayerState
    {
        Idle,
        Move,
        Roll,
        Die
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
        swordAttacker = GetComponent<SwordAttacker>();
        gunShooter = GetComponent<GunShooter>();
        protocolExecutor = GetComponent<ProtocolExecutor>();
        inventory = GetComponent<PlayerInventory>();

        swordAttacker.RegisterStat(stat);
        gunShooter.RegisterStat(stat);
        protocolExecutor.RegisterStat(stat);    
    }

    private void Start()
    {
        GameManager.Instance.OnPauseGame += Pause;
    }

    void OnEnable()
    {
        EnableInput();
    }

    private void OnDestroy()
    {
        DisableInput();

        if (GameManager.Instance != null)
            GameManager.Instance.OnPauseGame -= Pause;
    }

    void Update()
    {
        if (curState != PlayerState.Roll) //구르기 시 마지막 입력 방향으로 구르기 방향이 고정 됨
            moveInput = input.Player.Move.ReadValue<Vector2>();

        if (moveInput != null && animator != null)
            animator.SetMoveInput(moveInput); //애니메이터에게 moveInput 전달

        HandleState();
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //현재 상호작용 가능한 오브젝트와 트리거 됐다면 캐싱합니다
        curInteractable = collision.gameObject.GetComponent<IInteractable>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //기존에 캐싱했던 상호작용 가능한 오브젝트와 트리커 Exit 됐다면 캐싱을 풉니다
        if (curInteractable != null && curInteractable == collision.gameObject.GetComponent<IInteractable>())
            curInteractable = null;
    }

    #endregion

    #region Input
    void EnableInput()
    {
        //Input System 활성화 후 입력 받아오기
        input.Player.Enable();

        input.Player.PrimaryAttack.performed += _ => TryGunAttack();
        input.Player.SecondaryAttack.performed += _ => TrySwordAttack();
        //input.Player.Roll.performed += _ => TryRoll();
        input.Player.SpecialSkill.performed += _ => TryProtocol();
        input.Player.Interact.performed += _ => Interact();
        input.Player.Use.performed += _ => Use();
    }

    void DisableInput()
    {
        input.Player.Disable();

        input.Player.PrimaryAttack.performed -= _ => TryGunAttack();
        input.Player.SecondaryAttack.performed -= _ => TrySwordAttack();
        //input.Player.Roll.performed -= _ => TryRoll();
        input.Player.SpecialSkill.performed -= _ => TryProtocol();
        input.Player.Interact.performed -= _ => Interact();
        input.Player.Use.performed -= _ => Use();
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
                if (stat.StatDic[PlayerStat.Stat.Life] <= 0)
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(Sound_SFX.Player_Dead);

                    curState = PlayerState.Die;
                    animator.DieAnim();
                    SectorManager.Instance.SectorFail();
                }
                break;

            case PlayerState.Move:
                if (moveInput.magnitude == 0)
                    curState = PlayerState.Idle;
                if (stat.StatDic[PlayerStat.Stat.Life] <= 0)
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(Sound_SFX.Player_Dead);

                    curState = PlayerState.Die;
                    animator.DieAnim();

                    SectorManager.Instance.SectorFail();

                    Pause(true);
                }
                break;

            case PlayerState.Roll:
                rollTimer -= Time.deltaTime;
                if (rollTimer <= 0)
                {
                    animator.RollAnim(false);
                    animator.DoFlip = false;
                    curState = PlayerState.Idle;
                }
                if (stat.StatDic[PlayerStat.Stat.Life] <= 0)
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(Sound_SFX.Player_Dead);

                    animator.DieAnim();
                    curState = PlayerState.Die;
                    SectorManager.Instance.SectorFail();

                    Pause(true);
                }
                break;
            case PlayerState.Die:
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
        if (isPointerOverUI) return;
        if (curState == PlayerState.Roll) return;

        if (swordAttacker != null)
            swordAttacker.DoAttack();
    }

    /// <summary>
    /// 총 공격 시도. Roll 과 Sword 도중 불가
    /// </summary>
    void TryGunAttack()
    {
        if (isPointerOverUI) return; // UI 요소인지 판단, 클릭 이벤트에 적용
        if (curState == PlayerState.Roll) return;
        
        if (gunShooter != null)
            gunShooter.DoAttack();
    }

    /// <summary>
    /// 구르기 시도. Gun 과 Sword 도중 불가
    /// </summary>
    void TryRoll()
    {
        if (moveInput == Vector2.zero) return; //이동하고 있는 경우가 아니면 대쉬 X
        if (Time.time - lastRollTime < stat.StatDic[PlayerStat.Stat.RollRate]) //대시 간격 주기
            return;

        animator.RollAnim(true);
        animator.DoFlip = true;

        rollTimer = stat.StatDic[PlayerStat.Stat.RollDuration];
        curState = PlayerState.Roll;
        lastRollTime = Time.time;
    }

    void TryProtocol()
    {
        if (protocolExecutor != null)
            protocolExecutor.TryProtocol();
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

    public void Pause(bool isPause)
    {
        Debug.Log($"Is Player Pause {isPause}");
        isPaused = isPause;
        bool activeControl = !isPause; //일시정지라면 active false

        // rb.simulated = activeControl; -> 트리거 관련 이벤트에서 문제가 생기기에 입력 시스템을 멈추는 방식으로 변경

        if (isPause)
            DisableInput(); // 입력 자체를 차단
        else
            EnableInput();
    }

    void Interact()
    {
        if (curInteractable == null)
            return;

        curInteractable.OnInteract();
    }

    void Use()
    {
        if (inventory == null)
            return;

        inventory.UseItem();
    }
    #endregion
}