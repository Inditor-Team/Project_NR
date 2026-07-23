using UnityEngine;
using System;
using TMPro;

public class MassagePanelController : MonoBehaviour
{
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text desriptionText;
    private Action yesFunction;
    
    public void SetMessagePanel(string text, Action func)
    {
        desriptionText.text = text;
        yesFunction = func;
        
        UIManager.Instance.Show(messagePanel);
    }

    public void OnClickYesButton()
    {
        yesFunction?.Invoke();
    }
    
    public void OnClickNoButton()
    {
        UIManager.Instance.Hide(messagePanel);
    }
}
