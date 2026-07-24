using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    Animator anim;

    [Header("ĳ���� ������Ʈ")]
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
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        this.moveInput = moveInput;

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

    void RollAnim()
    {
        anim.SetBool("isRoll", true);
    }
}
