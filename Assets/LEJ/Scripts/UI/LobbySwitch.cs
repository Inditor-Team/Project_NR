using UnityEngine;

public class LobbySwitch : MonoBehaviour, IInteractable
{
    enum SwitchType { None, NewGame, HowTo, Continue, Setting, Exit }
    [SerializeField] SwitchType switchType;
    [SerializeField] SceneController.Scene nextSceneName = SceneController.Scene.Map;
    [SerializeField] GameObject startAlert;
    [SerializeField] GameObject uiCanvas;
    [SerializeField] SavePanelController savePanelController;
    [SerializeField] SettingPanelController settingPanelController;
    [SerializeField] MessagePanelController messagePanelController;

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
        SceneController.Instance.ChangeScene(SceneController.Scene.HowTo);
    }

    public void OnInteract()
    {
        switch (switchType)
        {
            case SwitchType.NewGame:
                if (uiCanvas.activeSelf)
                {
                    uiCanvas.SetActive(false); // 튜토리얼 UI 켜져있으면 종료
                    GameManager.Instance.ForcedRelease();
                }

                UIManager.Instance.Show(startAlert);
                GameManager.Instance.RequestPause();// Pause(true);
                SoundManager.Instance.PlaySFX(Sound_SFX.UIOpen);
                break;
            case SwitchType.HowTo:
                string msg = LocalizationManager.Instance.CurLang == Language.KO ?
                    "게임 방법을 다시 알려줘?" : "Need me to show you how to play again ?";
                messagePanelController.SetMessagePanel(msg, HowTo);
                break;
            case SwitchType.Continue:
                savePanelController.ShowForLoad();
                break;
            case SwitchType.Setting:
                settingPanelController.ShowSettingPanel();
                break;
            case SwitchType.Exit:
                settingPanelController.OnClickExitGame();
                break;
        }
    }
}
