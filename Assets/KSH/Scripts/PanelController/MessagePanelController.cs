using UnityEngine;
using System;
using TMPro;

public class MessagePanelController : MonoBehaviour
{
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text desriptionText;
    private Action yesFunction;

    private void Start()
    {
        UIManager.Instance.msgController = this;
        gameObject.SetActive(false);
    }

    public void SetMessagePanel(string text, Action func)
    {
        desriptionText.text = text;
        yesFunction = func;

        UIManager.Instance.Show(messagePanel);
    }

    public void HideMessagePanel()
    {
        UIManager.Instance.Hide(messagePanel);
    }

    public void OnClickYesButton()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.UIConfirm);
        Action funcToRun = yesFunction; 
        yesFunction = null;
        
        funcToRun?.Invoke();
    }
    
    public void OnClickNoButton()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.UICancel);
        yesFunction = null;
        UIManager.Instance.Hide(messagePanel);
    }
}
