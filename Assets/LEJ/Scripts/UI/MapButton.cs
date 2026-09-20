using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapButton : MonoBehaviour
{
    public string stageName;
    [SerializeField] Animator mapIcon;
    public SceneController.Scene curSector;
    public SceneController.Scene[] prevSectors;
    [SerializeField] Button stageAlertConfirmButton;
    [SerializeField] TMP_Text stageAlertConfirmText;

    private void Start()
    {
        bool canEnter = CanEnterStage();

        if (!canEnter)
        {
            mapIcon.SetTrigger("Lock");
            return; 
        }

        //선행 스테이지를 막 깬 경우라면 Unlock 재생
        if (canEnter && prevSectors.Length == 0 || canEnter && prevSectors.Contains(SceneController.Instance.prevScene))
            mapIcon.SetTrigger("Unlock");
        else
            mapIcon.SetTrigger("Map");
    }

    public void OnClick()
    {
        //이전 스테이지 A, B, C 중 하나 이상 클리어 되었다면
        if (!CanEnterStage())
            return;

        //이미 클리어 된 맵은 다시 플레이 불가
        if (GameManager.Instance.ClearedSector[curSector])
            return;

        stageAlertConfirmText.text = $"{stageName} 로 이동하시겠습니까?";
        stageAlertConfirmButton.onClick.RemoveAllListeners();
        stageAlertConfirmButton.onClick.AddListener(() => { SceneController.Instance.ChangeScene(curSector); });

        UIManager.Instance.Show(UIManager.Instance.msgController.gameObject);
    }

    private bool CanEnterStage()
    {
        //선행 스테이지가 아예 없는 경우
        if (prevSectors.Length == 0)
            return true;

        //A, B, C 중 하나라도 클리어했다면 입장 가능
        foreach (var sector in prevSectors)
        {
            if (GameManager.Instance.ClearedSector[sector])
                return true;
        }
        return false;
    }
}
