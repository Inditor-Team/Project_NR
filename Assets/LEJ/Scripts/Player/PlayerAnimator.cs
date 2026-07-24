using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    Animator anim;

    [Header("캐릭터 오브젝트")]
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;

    Vector2 moveInput;

    public bool DoFlip = true;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        //FlipByMoveInput();

        if (moveInput == null || moveInput == Vector2.zero)
            anim.SetBool("IsMove", false);
        else
            anim.SetBool("IsMove", true);
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        this.moveInput = moveInput;
    }

    void FlipByMoveInput()
    {
        if (moveInput == null || moveInput == Vector2.zero)
            return;

        if (moveInput.x > 0)
            model.flipX = true;
        else if (moveInput.x < 0)
            model.flipX = false;
    }

    void RollAnim()
    {
        anim.SetBool("IsRoll", true);
    }

    public void DieAnim()
    {
        anim.SetBool("IsDie", true);
    }
}
