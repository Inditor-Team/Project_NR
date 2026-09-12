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
    public SectorSO.SectorType sectorType;
    public SectorSO.SectorType prevSectorA;
    public SectorSO.SectorType prevSectorB;
    public SectorSO.SectorType prevSectorC;
    [SerializeField] Button stageAlertConfirmButton;
    [SerializeField] TMP_Text stageAlertConfirmText;

    private void Start()
    {
        bool canEnter = CanEnterStage();

        lockIcon.SetActive(!canEnter);
        mapIcon.SetActive(canEnter);
    }
    public void OnClick()
    {
        //이전 스테이지 A, B, C 중 하나 이상 클리어 되었다면
        if (!CanEnterStage())
            return;

        //이미 클리어 된 맵은 다시 플레이 불가
        if (GameManager.Instance.ClearedSector[sectorType])
            return;

        stageAlertConfirmText.text = $"{stageName} 로 이동하시겠습니까?";
        stageAlertConfirmButton.onClick.RemoveAllListeners();
        stageAlertConfirmButton.onClick.AddListener(() => { SceneController.Instance.ChangeScene(sceneName); });

        UIManager.Instance.Show(UIManager.Instance.msgController.gameObject);
    }

    private bool CanEnterStage()
    {
        //선행 스테이지가 아예 없는 경우
        if (prevSectorA == SectorSO.SectorType.None &&
            prevSectorB == SectorSO.SectorType.None &&
            prevSectorC == SectorSO.SectorType.None)
            return true;

        //A, B, C 중 하나라도 클리어했다면 입장 가능
        return
            (prevSectorA != SectorSO.SectorType.None &&
             GameManager.Instance.ClearedSector[prevSectorA]) ||

            (prevSectorB != SectorSO.SectorType.None &&
             GameManager.Instance.ClearedSector[prevSectorB]) ||

            (prevSectorC != SectorSO.SectorType.None &&
             GameManager.Instance.ClearedSector[prevSectorC]);
    }
}
