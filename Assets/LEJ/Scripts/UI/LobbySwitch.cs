using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySwitch : MonoBehaviour, IInteractable
{
    enum SwitchType { None, GameStart, HowTo }
    [SerializeField] SwitchType switchType;
    [SerializeField] SceneController.Scene nextSceneName = SceneController.Scene.Scene_Map;
    [SerializeField] GameObject startAlert;
    [SerializeField] GameObject uiCanvas;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnInteract();
    }

    public void CloseUI()
    {
        GameManager.Instance.Pause(false);
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
                startAlert.SetActive(true);
                GameManager.Instance.Pause(true);
                SoundManager.Instance.PlaySFX(Sound_SFX.UIOpen);
                break;
            case SwitchType.HowTo:
                HowTo();
                break;
        }
    }
}
