using UnityEngine;

public class UIButton : MonoBehaviour
{
    public void Button_Cancel()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.UICancel);
    }

    public void Button_Confirm()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.UIConfirm);
    }
}
