using UnityEngine;
using System;
using TMPro;

public class MessagePanelController : MonoBehaviour
{
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text desriptionText;
    private Action yesFunction;
    
    public void SetMessagePanel(string text, Action func)
    {
        desriptionText.text = text;
        yesFunction = func;

        SoundManager.Instance.PlaySFX(Sound_SFX.UIOpen);
        UIManager.Instance.Show(messagePanel);
    }

    public void OnClickYesButton()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.UIConfirm);
        yesFunction?.Invoke();
    }
    
    public void OnClickNoButton()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.UICancel);
        UIManager.Instance.Hide(messagePanel);
    }
}
