using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// LevelCardProvider 에 의해 선점된 레벨 카드들을 보여주는 UI 입니다
/// </summary>
public class LevelCardUI : MonoBehaviour
{
    [SerializeField] LevelCardUIElement prefab;
    private LevelCardUIElement[] elements;

    private void Awake()
    {
        Init();
        gameObject.SetActive(false);
    }

    void Init()
    {
        elements = new LevelCardUIElement[3];

        for (int i = 0; i < 3; i++)
            elements[i] = Instantiate(prefab, transform);
    }

    public void ShowUI()
    {
        UIManager.Instance.Show(this.gameObject);
    }

    public void CloseUI()
    {
        UIManager.Instance.Hide(this.gameObject);
    }

    public void SetUIElement(LevelCardSO data, int index, Action buttonAction)
    {
        LevelCardUIElement curElement = elements[index];

        //카드 이름과 설명 텍스트
        curElement.cardName.text = data.CardName;
        curElement.cardDescription.text = data.CardDescription;

        //카드 능력치 텍스트 추가
        string cardAbilityText = "";
        
        foreach (var element in data.Elements)
            cardAbilityText += $"{element.targetStat} 이 {element.upgradeAmount} \n";
        
        curElement.cardAbility.text = cardAbilityText;

        //버튼 이벤트 설정
        if (buttonAction == null)
            return;

        if (curElement.choiceButton.onClick != null) //버튼 이벤트 초기화
            curElement.choiceButton.onClick.RemoveAllListeners(); 

        curElement.choiceButton.onClick.AddListener(() => { buttonAction?.Invoke(); }); //버튼 이벤트 매핑
        curElement.choiceButton.onClick.AddListener(() => { SceneController.Instance.ChangeScene(SceneController.Scene.Scene_Map); });

        //카드 색상 변경 (임시)
        switch (data.type)
        {
            case LevelCardSO.LevelCardType.Attack:
                curElement.image.color = Color.red;
                break;
            case LevelCardSO.LevelCardType.Shield:
                curElement.image.color = Color.blue;
                break;
            case LevelCardSO.LevelCardType.Evasion:
                curElement.image.color = Color.yellow;
                break;
            case LevelCardSO.LevelCardType.Speed:
                curElement.image.color = Color.yellowGreen;
                break;
            case LevelCardSO.LevelCardType.Risk:
                curElement.image.color = Color.magenta;
                break;
        }
    }
}
