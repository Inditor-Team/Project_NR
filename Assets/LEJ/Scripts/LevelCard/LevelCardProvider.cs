using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// �÷��̾�� ���� ī�带 �����մϴ�
/// </summary>
public class LevelCardProvider : MonoBehaviour
{
    [SerializeField] LevelCardData data; //���� ���ҽ��� ���� ������ �������� ���� �Ǵ� ��Ʈ ����
    [SerializeField] LevelCardUI ui;
    [SerializeField] PlayerStat stat; //���� �Ŵ������� Player ���� �� �������� ����

    private int cardCount = 3;

    public void ProvideByUI()
    {
        if (ui == null)
            return;

        //ī�� ������ �� 3���� ī�带 ����
        LevelCard[] choosen = ChooseLevelCard(3);

        //������ ī�带 ui �� ����
        if (ui == null)
            return;

        for (int i = 0; i < cardCount; i++)
        {
            //��ư�� ������ �� ������ �����ϵ��� SetStat �� ����
            Action setStatAction = null;
            foreach (var element in choosen[i].Elements)
                setStatAction += () => { SetStat(element.targetStat, element.upgradeAmount); };
            setStatAction += () => { ui.CloseUI(); };

            //ui ���� ������ ����
            ui.SetUIElement(choosen[i], i, setStatAction);
        }

        GameManager.Instance.Pause(true);

        ui.ShowUI();
    }

    /// <summary>
    /// �÷��̾��� ������ ���׷��̵�
    /// </summary>
    /// <param name="target"></param>
    /// <param name="amount"></param>
    void SetStat(PlayerStat.Stat target, float amount)
    {
        stat.UpdateStat(target, amount);
    }

    /// <summary>
    /// Ȯ���� ���� ���� ī�带 ����
    /// </summary>
    LevelCard[] ChooseLevelCard(int count)
    {
        //TO DO: Ȯ�� ����
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
