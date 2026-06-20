using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 입력에 따른 플레이어블 캐릭터의 움직임과 애니메이션 제어
/// </summary>
public class PlayerController : MonoBehaviour
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

    float rollTimer;
    float attackTimer;

    Vector2 mousePosition;

    Vector3 swordHoldPos = new Vector3(0.25f, -0.13f, 0f);
    Quaternion swordHoldRot = Quaternion.Euler(0f, 0f, 18f);

    [Header("캐릭터 오브젝트")]
    [SerializeField] SpriteRenderer model;
    [SerializeField] WeaponBase primaryWeapon;
    [SerializeField] WeaponBase secondaryWeapon;
    GameObject gun;
    GameObject sword;
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

        if (primaryWeapon != null)
            primaryWeapon.TryAttack();
    }

    /// <summary>
    /// 총 공격 시도. Roll 과 Sword 도중 불가
    /// </summary>
    void TrySecondaryAttack()
    {
        if (currentState == PlayerState.Roll) return;
        if (currentState == PlayerState.PrimaryAttack) return;

        ChangeState(PlayerState.SecondaryAttack);

        if (secondaryWeapon != null)
            secondaryWeapon.TryAttack();
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

        /* 8방향 캐릭터 스프라이트 시 적용
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
        */
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
            gun.transform.rotation = Quaternion.Euler(0, 0, gunAngle);
            gun.transform.position = (Vector2)transform.position + aim * 0.5f; //총이 캐릭터에서 약간 떨어지도록 위치 조정

            //총이 캐릭터보다 왼쪽에 있을 때 총을 뒤집어서 보이도록 처리
            gunSprite.flipY = (aim.x > 0) ? true : false;
        }
        if (sword != null)
        {
            //칼을 캐릭터 flip 에 맞춰 좌우 반전
            swordSprite.flipX = model.flipX ? true : false;
            //칼 위치는 swordHoldPoint 에 고정하되, 캐릭터가 flip 될 때 칼도 같이 좌우 반전되도록 처리
            sword.transform.localPosition = swordHoldPos + swordHoldPos.x * (model.flipX ? Vector3.left * 2 : Vector3.zero);
            sword.transform.rotation = (model.flipX ? Quaternion.Inverse(swordHoldRot) : swordHoldRot);
        }
    }
    #endregion

    #region State
    /// <summary>
    /// 총과 검 키 입력 변경을 대응하기 위함
    /// TO DO : 왜 이렇게 짰더라 걍 키 바인딩 바꾸면 되는데
    /// </summary>
    private void GetWeaponComponent()
    {
        if (primaryWeapon != null)
        {
            WeaponBase primaryWeapon = this.primaryWeapon as WeaponBase;

            if (primaryWeapon as Gun)
            {
                gun = primaryWeapon.gameObject;
                gunSprite = gun.GetComponent<SpriteRenderer>();
            }
            else if (primaryWeapon as Sword)
            {
                sword = primaryWeapon.gameObject;
                swordSprite = sword.GetComponent<SpriteRenderer>();
            }
        }
        if (secondaryWeapon != null)
        {
            WeaponBase secondaryWeapon = this.secondaryWeapon as WeaponBase;

            if (secondaryWeapon as Gun)
            {
                gun = secondaryWeapon.gameObject;
                gunSprite = gun.GetComponent<SpriteRenderer>();
            }
            else if (secondaryWeapon as Sword)
            {
                sword = secondaryWeapon.gameObject;
                swordSprite = sword.GetComponent<SpriteRenderer>();
            }
        }
    }
    #endregion
}