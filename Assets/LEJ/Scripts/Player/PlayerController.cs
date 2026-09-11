using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    float curSpeed;

    public bool disableAttack = false;

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

        stat = transform.GetChild(0).GetComponent<PlayerStat>();
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
        gunShooter.OnShoot += ActiveInstableCore; //총알 발사 시 확률적으로 발현
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
        if (curState == PlayerState.Die)
            return;

        HandleState();

        if (stat.StatDic[PlayerStat.Stat.Life] <= 0)
            Die();

        if (curState != PlayerState.Roll) //구르기 시 마지막 입력 방향으로 구르기 방향이 고정 됨
            moveInput = input.Player.Move.ReadValue<Vector2>();

        if (moveInput != null && animator != null)
            animator.SetMoveInput(moveInput); //애니메이터에게 moveInput 전달

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
    public void EnableInput()
    {
        //Input System 활성화 후 입력 받아오기
        input.Player.Enable();

        input.Player.PrimaryAttack.performed +=  TryGunAttack;
        input.Player.SecondaryAttack.performed += TrySwordAttack;
        input.Player.Roll.performed += TryRoll;
        input.Player.SpecialSkill.performed += TryProtocol;
        input.Player.Interact.performed += Interact;
        input.Player.Use.performed += Use;
    }

    public void DisableInput()
    {
        input.Player.Disable();

        input.Player.PrimaryAttack.performed -= TryGunAttack;
        input.Player.SecondaryAttack.performed -= TrySwordAttack;
        input.Player.Roll.performed -= TryRoll;
        input.Player.SpecialSkill.performed -= TryProtocol;
        input.Player.Interact.performed -= Interact;
        input.Player.Use.performed -= Use;
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
                {
                    animator.RollAnim(false);
                    animator.DoFlip = false;
                    curState = PlayerState.Idle;
                }
                break;
        }
    }
    #endregion

    #region Act
    /// <summary>
    /// 검 공격 시도. Roll 과 Gun 도중 불가
    /// </summary>
    void TrySwordAttack(InputAction.CallbackContext callback)
    {
        if (disableAttack) return;
        if (isPointerOverUI) return;
        if (curState == PlayerState.Roll) return;

        if (swordAttacker != null)
            swordAttacker.DoAttack();
    }

    /// <summary>
    /// 총 공격 시도. Roll 과 Sword 도중 불가
    /// </summary>
    void TryGunAttack(InputAction.CallbackContext callback)
    {
        if (disableAttack) return;
        if (isPointerOverUI) return; // UI 요소인지 판단, 클릭 이벤트에 적용
        if (curState == PlayerState.Roll) return;
        
        if (gunShooter != null)
            gunShooter.DoAttack();
    }

    /// <summary>
    /// 구르기 시도. Gun 과 Sword 도중 불가
    /// </summary>
    void TryRoll(InputAction.CallbackContext callback)
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

    void TryProtocol(InputAction.CallbackContext callback)
    {
        if (protocolExecutor != null)
            protocolExecutor.TryProtocol();
    }

    /// <summary>
    /// 캐릭터 이동. Roll 상태에선 이속 증가
    /// </summary>
    void Move()
    {
        curSpeed = stat.StatDic[PlayerStat.Stat.MoveSpeed];

        if (curState == PlayerState.Roll)
            curSpeed = stat.StatDic[PlayerStat.Stat.MoveSpeed] * stat.StatDic[PlayerStat.Stat.RollSpeed];

        rb.linearVelocity = moveInput * curSpeed;
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

    void Interact(InputAction.CallbackContext callback)
    {
        if (curInteractable == null)
            return;

        curInteractable.OnInteract();
    }

    void Use(InputAction.CallbackContext callback)
    {
        if (inventory == null)
            return;

        inventory.UseItem();
    }

    void Die()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(Sound_SFX.Player_Dead);

        curState = PlayerState.Die;
        animator.DieAnim();
        rb.simulated = false;

        SectorManager.Instance.SectorFail();

        Pause(true);
    }

    /// <summary>
    /// 불안정 코어 카드를 갖고 있을 경우 확률적으로 총알 발사 시 Active 
    /// </summary>
    void ActiveInstableCore()
    {
        //불안정 코어 확률 존재 시
        if (Random.value < stat.InstableCoreProbability)
            StartCoroutine(InstableCoreTime());
    }
    IEnumerator InstableCoreTime()
    {
        stat.Model.DOColor(Color.magenta, 2f).OnComplete(() =>
        {
            stat.Model.DOColor(Color.white, 0.2f);
        });

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(0.2f); //총알 발사 간격
            gunShooter.ForceAttack(); //발사 간격과 무관하게 강제 발사
        }

        float originSpeed = stat.StatDic[PlayerStat.Stat.MoveSpeed];
        stat.IncreaseStat(PlayerStat.Stat.MoveSpeed, 0.5f); //이속 감소
        yield return new WaitForSeconds(2f);
        curSpeed = originSpeed; //복구
    }
    #endregion
}