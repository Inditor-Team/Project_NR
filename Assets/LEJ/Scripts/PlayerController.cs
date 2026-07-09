using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 입력에 따른 플레이어블 캐릭터의 움직임과 애니메이션 제어
/// </summary>
public class PlayerController : MonoBehaviour, IDamageable
{
    #region Variables
    PlayerInputActions input;

    Rigidbody2D rb;
    Animator anim;

    Vector2 moveInput;

    [Header("캐릭터 스탯")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rollSpeed = 10f;
    [SerializeField] float rollDuration = 0.2f;
    [SerializeField] float attackDuration = 0.1f;

    [Header("캐릭터 프로토콜")]
    [SerializeField] ProtocolBase protocol;

    float rollTimer;
    float attackTimer;

    Quaternion swordHoldRot = Quaternion.Euler(0f, 0f, 0.25f);

    [Header("캐릭터 오브젝트")]
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;
    [SerializeField] WeaponBase sword;
    [SerializeField] Transform swordHoldPoint;
    [SerializeField] WeaponBase gun;
    [SerializeField] Transform gunHoldPoint;
    SpriteRenderer gunSprite;
    SpriteRenderer swordSprite;
    Camera mainCam;

    enum PlayerState
    {
        Idle,
        Move,
        PrimaryAttack, //Sword
        SecondaryAttack, //Gun
        Roll
    }

    PlayerState currentState;
    #endregion

    #region Cycle
    void Awake()
    {
        input = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        (sword as Sword).SetOwner(gameObject); //검 Init
        (gun as Gun).SetOwner(gameObject); //총 Init

        mainCam = Camera.main;
    }

    private void Start()
    {
        GetWeaponComponent();
    }

    void OnEnable()
    {
        input.Player.Enable();

        input.Player.PrimaryAttack.performed += _ => TryPrimaryAttack();
        input.Player.SecondaryAttack.performed += _ => TrySecondaryAttack();
        input.Player.Roll.performed += _ => TryRoll();
        input.Player.SpecialSkill.performed += _ => TrySpecialSkill();
    }

    void OnDisable()
    {
        input.Player.Disable();
    }

    void Update()
    {
        if (currentState != PlayerState.Roll) //구르기 시 마지막 입력 방향으로 구르기 방향이 고정 됨
            moveInput = input.Player.Move.ReadValue<Vector2>();

        HandleState();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        Move();
    }

    void LateUpdate()
    {
        SetWeaponPos();
    }
    #endregion

    #region FSM
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
                //anim.SetBool("IsSword", true);
                attackTimer = attackDuration;
                break;

            case PlayerState.SecondaryAttack:
                //anim.SetBool("IsGun", true);
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
                anim.SetBool("IsMove", false);
                if (moveInput.magnitude > 0)
                {
                    anim.SetBool("IsMove", true);
                    ChangeState(PlayerState.Move);
                }
                break;

            case PlayerState.Move:
                if (moveInput.magnitude == 0)
                {
                    anim.SetBool("IsMove", false);
                    ChangeState(PlayerState.Idle);
                }
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
                    //anim.SetBool("IsSword", false);
                }
                break;

            case PlayerState.SecondaryAttack:
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    ChangeState(PlayerState.Idle);
                    //anim.SetBool("IsGun", false );
                }
                break;
        }
    }
    #endregion

    #region Act
    /// <summary>
    /// 검 공격 시도. Roll 과 Gun 도중 불가
    /// </summary>
    void TryPrimaryAttack()
    {
        if (currentState == PlayerState.Roll) return;
        if (currentState == PlayerState.SecondaryAttack) return;

        ChangeState(PlayerState.PrimaryAttack);

        if (sword != null)
            sword.TryAttack();
    }

    /// <summary>
    /// 총 공격 시도. Roll 과 Sword 도중 불가
    /// </summary>
    void TrySecondaryAttack()
    {
        if (currentState == PlayerState.Roll) return;
        if (currentState == PlayerState.PrimaryAttack) return;

        ChangeState(PlayerState.SecondaryAttack);

        if (gun != null)
            gun.TryAttack();
    }

    /// <summary>
    /// 프로토콜 스킬 시도
    /// </summary>
    void TrySpecialSkill()
    {
        if (protocol.IsActive)
            return;

        protocol.TryProtocol();
    }

    /// <summary>
    /// 구르기 시도. Gun 과 Sword 도중 불가
    /// </summary>
    void TryRoll()
    {
        if (currentState == PlayerState.PrimaryAttack) return;
        if (currentState == PlayerState.SecondaryAttack) return;
        if (moveInput == Vector2.zero) return; //이동하고 있는 경우가 아니면 대쉬 X

        ChangeState(PlayerState.Roll);
    }

    /// <summary>
    /// 캐릭터 이동. Roll 상태에선 이속 증가
    /// </summary>
    void Move()
    {
        //프로토콜 스킬에 따른 이속 변화 존재(기본 값은 1)
        float speed = protocol.IsActive ? moveSpeed * protocol.SpeedMultiplier: moveSpeed; 

        if (currentState == PlayerState.Roll)
            speed = rollSpeed;

        rb.linearVelocity = moveInput * speed;
    }
    #endregion

    #region Animate 
    /// <summary>
    /// 플레이어 입력에 따라 애니메이션 파타미터를 업데이트
    /// </summary>
    void UpdateAnimator()
    {
        if (moveInput.x > 0)
            model.flipX = true;
        else if (moveInput.x < 0)
            model.flipX = false;
    }

    /// <summary>
    /// 마우스 위치에 따라 총과 검의 방향 변경
    /// </summary>
    void SetWeaponPos()
    {
        //사용자 마우스 위치 받아서 좌표 계산
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseScreen3D = new Vector3(mouseScreen.x, mouseScreen.y, -mainCam.transform.position.z);
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(mouseScreen3D);
        Vector2 aim = (mouseWorld - transform.position).normalized;
        float gunAngle = Mathf.Atan2(-aim.y, -aim.x) * Mathf.Rad2Deg;

        if (gun != null)
        {
            gunHoldPoint.transform.rotation = Quaternion.Euler(0, 0, gunAngle);
            gunHoldPoint.transform.position = (Vector2)transform.position + aim * 0.5f; //총이 캐릭터에서 약간 떨어지도록 위치 조정

            //총이 캐릭터보다 왼쪽에 있을 때 총을 뒤집어서 보이도록 처리
            gunSprite.flipY = (aim.x > 0);
        }
        if (sword != null)
        {
            //칼 위치는 swordHoldPoint 에 고정하되, 캐릭터가 flip 될 때 칼도 같이 좌우 반전되도록 처리
            swordHoldPoint.transform.rotation = (model.flipX ? Quaternion.Euler(0, -180, swordHoldRot.z) : swordHoldRot);
        }
    }
    #endregion

    #region State
    private void GetWeaponComponent()
    {
        if (gun != null)
        {
            gunSprite = gun.GetComponent<SpriteRenderer>();
        }
        if (sword != null)
        {
            swordSprite = sword.GetComponent<SpriteRenderer>();
        }
    }

    public void TakeDamage(float damage)
    {
        //프로토콜이 실행 중이고 해당 프로토콜이 무적 상태라면 데미지 무시
        if (protocol.IsActive && protocol.isInvincible)
            return;
    }
    #endregion
}