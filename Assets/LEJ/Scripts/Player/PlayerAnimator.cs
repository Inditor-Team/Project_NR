using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    Animator anim;

    [SerializeField] RotateByAim rotateByAnim;
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;

    Vector2 moveInput;

    bool doFlip = false;
    public bool DoFlip { get { return doFlip; } 
        set { 
            doFlip = value;
            if (doFlip)
                rotateByAnim.enabled = false;
            else
                rotateByAnim.enabled = true;
        } 
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (DoFlip)
            FlipByMoveInput();

        if (moveInput == null || moveInput == Vector2.zero)
            anim.SetBool("IsMove", false);
        else
            anim.SetBool("IsMove", true);
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        this.moveInput = moveInput;

        if (SoundManager.Instance == null)
            return;

        if (this.moveInput != Vector2.zero)
            SoundManager.Instance.PlayPlayerMoveSound();
        else
            SoundManager.Instance.StopyPlayerMoveSound();
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

    public void RollAnim(bool doAnim)
    {
        anim.SetBool("IsRoll", doAnim);
    }

    public void DieAnim()
    {
        anim.SetBool("IsDie", true);
    }
}
