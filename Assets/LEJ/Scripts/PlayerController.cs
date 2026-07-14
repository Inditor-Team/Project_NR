using Unity.Burst.Intrinsics;
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
    [SerializeField] float swordSwingSpeed = 3f;
    [SerializeField] float bulletSpeed = 3f;

    //[Header("캐릭터 프로토콜")]
    //[SerializeField] ProtocolBase protocol;

    float rollTimer;
    float attackTimer;

    [Header("캐릭터 오브젝트")]
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;
    [SerializeField] Sword sword;
    [SerializeField] Gun gun;
    Camera mainCam;

    enum PlayerState
    {
        Idle,
        Move,
        SwordAttack, //Sword
        GunAttack, //Gun
        Roll
    }

    PlayerState curState;
    #endregion

    #region Cycle
    void Awake()
    {
        input = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        sword.SetOwner(gameObject); //검 Init
        gun.SetOwner(gameObject); //총 Init

        mainCam = Camera.main;
    }

    void OnEnable()
    {
        //Input System 활성화 후 입력 받아오기
        input.Player.Enable();

        input.Player.PrimaryAttack.performed += _ => TrySwordAttack();
        input.Player.SecondaryAttack.performed += _ => TryGunAttack();
        input.Player.Roll.performed += _ => TryRoll();
        //input.Player.SpecialSkill.performed += _ => TrySpecialSkill();
        //input.Player.Easteregg.performed += _ => PlayUkulele();
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

        HandleState();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        Move();
    }
    #endregion

    #region FSM
    /// <summary>
    /// FSM 상태 변경 & 애니메이션 제어
    /// </summary>
    /// <param name="newState"></param>
    void ChangeState(PlayerState newState)
    {
        curState = newState;

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

            case PlayerState.SwordAttack:
                anim.SetBool("IsAttack", true);
                attackTimer = 1f / swordSwingSpeed;
                break;

            case PlayerState.GunAttack:
                anim.SetBool("IsAttack", true);
                attackTimer = 1f / swordSwingSpeed;
                break;
        }
    }

    /// <summary>
    /// FSM 상태 전이 & 애니메이션 변경
    /// </summary>
    void HandleState()
    {
        switch (curState)
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
                    anim.SetBool("IsRoll", false);
                    ChangeState(PlayerState.Idle);
                }
                break;

            case PlayerState.SwordAttack:
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    anim.SetBool("IsAttack", false);

                    if (!gun.gameObject.activeSelf) //칼 공격 시 총이 꺼져 있었다면
                        gun.gameObject.SetActive(true);

                    ChangeState(PlayerState.Idle);
                }
                break;

            case PlayerState.GunAttack:
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    anim.SetBool("IsAttack", false);

                    if (!sword.gameObject.activeSelf) //총 공격 시 칼이 꺼져 있었다면
                        sword.gameObject.SetActive(true);

                    ChangeState(PlayerState.Idle);
                }
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
        if (curState == PlayerState.GunAttack) return;

        ChangeState(PlayerState.SwordAttack);


        if (gun != null) //칼 공격 시에 총이 안 보이게
            gun.gameObject.SetActive(false);

        if (sword != null)
        {
            sword.gameObject.SetActive(true); //칼은 공격시에만 모습이 보입니다
            sword.TryAttack(swordSwingSpeed);
        }
    }

    /// <summary>
    /// 총 공격 시도. Roll 과 Sword 도중 불가
    /// </summary>
    void TryGunAttack()
    {
        if (curState == PlayerState.Roll) return;
        if (curState == PlayerState.SwordAttack) return;

        ChangeState(PlayerState.GunAttack);

        if (gun != null)
            gun.TryAttack(bulletSpeed);
    }

    /// <summary>
    /// 프로토콜 스킬 시도
    /// </summary>
    //void TrySpecialSkill()
    //{
    //    if (protocol.IsActive)
    //        return;

    //    protocol.TryProtocol();
    //}

    /// <summary>
    /// 구르기 시도. Gun 과 Sword 도중 불가
    /// </summary>
    void TryRoll()
    {
        if (curState == PlayerState.SwordAttack) return;
        if (curState == PlayerState.GunAttack) return;
        if (moveInput == Vector2.zero) return; //이동하고 있는 경우가 아니면 대쉬 X

        ChangeState(PlayerState.Roll);
    }

    /// <summary>
    /// 캐릭터 이동. Roll 상태에선 이속 증가
    /// </summary>
    void Move()
    {
        float speed = moveSpeed;

        ////프로토콜 여부에 따른 이속 변화 존재(기본 값은 1)
        //if (protocol != null)
        //    speed = protocol.IsActive ? moveSpeed * protocol.SpeedMultiplier: moveSpeed;

        if (curState == PlayerState.Roll)
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
        //4방향 그 이상일 때 blend tree 로 플레이어 방향에 맞는 애니메이션 적용
        //if (curState == PlayerState.Idle || curState == PlayerState.Move || curState == PlayerState.Roll)
        //{
        //    anim.SetFloat("DirX", moveInput.x);
        //    anim.SetFloat("DirY", moveInput.y);
        //}

        //플레이어 2방향일 때
        if (moveInput.x > 0)
            model.flipX = true;
        else if (moveInput.x < 0)
            model.flipX = false;
    }

    #endregion

    #region State

    public void TakeDamage(float damage)
    {
        //프로토콜이 실행 중이고 해당 프로토콜이 무적 상태라면 데미지 무시
        //if (protocol.IsActive && protocol.isInvincible)
        //    return;
    }
    #endregion

    #region Prefs
    //void PlayUkulele()
    //{
    //    if (curState != PlayerState.Idle)
    //        return;

    //    anim.SetBool("IsPlayUkulele", true);
    //    Invoke("StopPlayUkulele", 1.5f);
    //}

    //void StopPlayUkulele()
    //{
    //    anim.SetBool("IsPlayUkulele", false);
    //}
    #endregion
}