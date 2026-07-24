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
    [SerializeField] PlayerStat stat; //추후 매니저에서 Player 참조 시 그쪽으로 연결

    private int cardCount = 3;

    public void ProvideByUI()
    {
        if (ui == null)
            return;

        //카드 데이터 중 3개의 카드를 선점
        LevelCard[] choosen = ChooseLevelCard(3);

        //선점된 카드를 ui 로 제공
        if (ui == null)
            return;

        for (int i = 0; i < cardCount; i++)
        {
            //버튼을 눌렀을 때 스탯이 증가하도록 SetStat 을 전달
            Action setStatAction = null;
            foreach (var element in choosen[i].Elements)
                setStatAction += () => { SetStat(element.targetStat, element.upgradeAmount); };
            setStatAction += () => { ui.CloseUI(); };

            //ui 에게 설정을 명령
            ui.SetUIElement(choosen[i], i, setStatAction);
        }

        stat.TakeDamage(100); //일단 죽은 것으로 처리
        ui.ShowUI();
    }

    /// <summary>
    /// 플레이어의 스탯을 업그레이드
    /// </summary>
    /// <param name="target"></param>
    /// <param name="amount"></param>
    void SetStat(PlayerStat.Stat target, float amount)
    {
        stat.UpdateStat(target, amount);
    }

    /// <summary>
    /// 확률에 따라 레벨 카드를 제공
    /// </summary>
    LevelCard[] ChooseLevelCard(int count)
    {
        //TO DO: 확률 구현
        LevelCard[] result = new LevelCard[count];
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
