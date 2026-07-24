using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySwitch : MonoBehaviour, IInteractable
{
    enum SwitchType { None, GameStart, HowTo }
    [SerializeField] SwitchType switchType;
    [SerializeField] string nextSceneName = "MapScene";
    [SerializeField] GameObject startAlert;
    [SerializeField] GameObject uiCanvas;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnInteract();
    }

    public void GameStart() 
    {
        SceneManager.LoadScene(nextSceneName);
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
                break;
            case SwitchType.HowTo:
                HowTo();
                break;
        }
    }
}
