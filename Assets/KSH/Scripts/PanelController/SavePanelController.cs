using System;
using UnityEngine;

public class SavePanelController : MonoBehaviour
{
    [SerializeField] private GameObject savePanel;
    private bool isSavePanelActive = false;

    public void ShowForSave()
    {
        SaveSlotManager.Instance.SetMode(SaveLoadMode.Save);
        Show();
    }
    
    public void ShowForLoad()
    {
        SaveSlotManager.Instance.SetMode(SaveLoadMode.Load);
        Show();
    }
    
    private void Show()
    {
        if (savePanel == null) return;
        
        if (isSavePanelActive) return; // 중복 켜지기 방지, 메시지 창 띄우기?
        isSavePanelActive = true;
        
        GameManager.Instance.RequestPause();// Pause(true);
        UIManager.Instance.Show(savePanel);
    }

    public void HideSavePanel()
    {
        UIManager.Instance.Hide(savePanel);
        GameManager.Instance.ReleasePause();// Pause(false);
        isSavePanelActive = false;
    }
}
