using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbySwitch : MonoBehaviour, IInteractable
{
    enum SwitchType { None, GameStart, HowTo }
    [SerializeField] SwitchType switchType;
    [SerializeField] SceneController.Scene nextSceneName = SceneController.Scene.Scene_Map;
    [SerializeField] GameObject startAlert;
    [SerializeField] GameObject uiCanvas;

    public void CloseUI()
    {
        GameManager.Instance.ReleasePause();// Pause(false);
    }

    public void GameStart() 
    {
        SceneController.Instance.ChangeScene(nextSceneName);
    }

    void HowTo()
    {
        uiCanvas.SetActive(!uiCanvas.activeSelf);
    }

    public void OnInteract()
    {
        switch (switchType)
        {
            case SwitchType.GameStart:
                if (uiCanvas.activeSelf)
                {
                    uiCanvas.SetActive(false); // 튜토리얼 UI 켜져있으면 종료
                    GameManager.Instance.ReleasePause();
                }

                UIManager.Instance.Show(startAlert);
                GameManager.Instance.RequestPause();// Pause(true);
                SoundManager.Instance.PlaySFX(Sound_SFX.UIOpen);
                break;
            case SwitchType.HowTo:
                HowTo();
                break;
        }
    }
}
