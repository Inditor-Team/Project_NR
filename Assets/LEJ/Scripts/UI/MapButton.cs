using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapButton : MonoBehaviour
{
    [SerializeField] GameObject lockIcon;
    [SerializeField] GameObject mapIcon;
    public SceneController.Scene sceneName;
    [SerializeField] Button stageAlertConfirmButton;

    private void Start()
    {
        //TO DO : ClearedSector 매개변수 수정
        //lockIcon.SetActive(!GameManager.Instance.ClearedSector[sceneName]);  
        //mapIcon.SetActive(GameManager.Instance.ClearedSector[sceneName]);  
    }
    public void OnClick()
    {
        stageAlertConfirmButton.onClick.RemoveAllListeners();
        stageAlertConfirmButton.onClick.AddListener(() => { SceneController.Instance.ChangeScene(sceneName); });
    }
}
