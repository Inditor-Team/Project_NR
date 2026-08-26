using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapButton : MonoBehaviour
{
    public string stageName;
    [SerializeField] GameObject lockIcon;
    [SerializeField] GameObject mapIcon;
    public SceneController.Scene sceneName;
    [SerializeField] GameObject alertPanel;
    [SerializeField] Button stageAlertConfirmButton;
    [SerializeField] TMP_Text stageAlertConfirmText;

    private void Start()
    {
        //TO DO : ClearedSector 매개변수 수정
        //lockIcon.SetActive(!GameManager.Instance.ClearedSector[sceneName]);  
        //mapIcon.SetActive(GameManager.Instance.ClearedSector[sceneName]);  
    }
    public void OnClick()
    {
        UIManager.Instance.Show(alertPanel);
        stageAlertConfirmText.text = $"{stageName} 로 이동하시겠습니까?";
        stageAlertConfirmButton.onClick.RemoveAllListeners();
        stageAlertConfirmButton.onClick.AddListener(() => { SceneController.Instance.ChangeScene(sceneName); });
    }
}
