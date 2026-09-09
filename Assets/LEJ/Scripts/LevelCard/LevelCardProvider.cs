using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어에게 레벨 카드를 제공합니다
/// </summary>
public class LevelCardProvider : MonoBehaviour
{
    [SerializeField] LevelCardData data; //추후 리소스를 통해 프리팹 생성으로 참조 또는 시트 연결
    [SerializeField] LevelCardUI ui;
    PlayerStat stat; 

    private int cardCount = 3;

    private void Start()
    {
        stat = GameManager.Instance.Player.GetComponent<PlayerController>().Stat;
        SectorManager.Instance.OnSectorClear += ProvideByUI;
    }

    private void OnDestroy()
    {
        if (SectorManager.Instance != null)
            SectorManager.Instance.OnSectorClear -= ProvideByUI;
    }

    void ProvideByUI(SectorSO.SectorType type)
    {
        ProvideByUI();
    }

    public void ProvideByUI()
    {
        if (ui == null)
            return;

        //카드 데이터 중 3개의 카드를 선점
        LevelCardSO[] choosen = ChooseLevelCard(3);

        //선점된 카드를 ui 로 제공
        if (ui == null)
            return;

        for (int i = 0; i < cardCount; i++)
        {
            Action onClickAction = null;
            int index = i;

            //버튼을 눌렀을 때 InventoryManager 에 카드 획득 등록
            onClickAction += () => {InventoryManager.Instance.GetCard(choosen[index].Id); }; 

            //버튼을 눌렀을 때 스탯이 증가하도록 SetStat 을 전달
            foreach (var element in choosen[i].Elements)
            {
                onClickAction += () => {
                    SetStat(element.targetStat, element.upgradeAmount);
                    ui.CloseUI();
                };
            }

            //ui 에게 설정을 명령
            ui.SetUIElement(choosen[i], i, onClickAction);
        }

        GameManager.Instance.RequestPause();// Pause(true);

        ui.ShowUI();
    }

    // <summary>
    /// 플레이어의 스탯을 업그레이드
    /// </summary>
    void SetStat(PlayerStat.Stat target, float amount)
    {
        stat.UpdateStat(target, amount);
    }

    /// <summary>
    /// 확률에 따라 레벨 카드를 제공
    /// </summary>
    LevelCardSO[] ChooseLevelCard(int count)
    {
        //카드가 3개 미만일 때 스택 오버플로우 방지 
        if (data.LevelCards.Length < 3) return null;

        //TO DO: 확률 구현
        LevelCardSO[] result = new LevelCardSO[count];
        int index = 0;
        
        while (index < 3)
        {
            int randNum = UnityEngine.Random.Range(0, data.LevelCards.Length);

            if (result.Contains(data.LevelCards[randNum]))
                continue;

            result[index++] = data.LevelCards[randNum];
        }

        return result;
    }
}
