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
        CloseUI();
    }

    void Init()
    {
        elements = new LevelCardUIElement[3];

        for (int i = 0; i < 3; i++)
            elements[i] = Instantiate(prefab, transform);
    }

    public void ShowUI()
    {
        for (int i = 0; i < elements.Length; i++)
            elements[i].gameObject.SetActive(true);
    }

    public void CloseUI()
    {
        for (int i = 0; i < elements.Length; i++)
            elements[i].gameObject.SetActive(false);
    }

    public void SetUIElement(LevelCard data, int index, Action buttonAction)
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
        curElement.choiceButton.onClick.AddListener(() => { SceneManager.LoadScene("MapScene_LEJ"); });

        //카드 색상 변경 (임시)
        switch (data.type)
        {
            case LevelCard.LevelCardType.Attack:
                curElement.image.color = Color.red;
                break;
            case LevelCard.LevelCardType.Shield:
                curElement.image.color = Color.blue;
                break;
            case LevelCard.LevelCardType.Move:
                curElement.image.color = Color.yellow;
                break;
            case LevelCard.LevelCardType.Special:
                curElement.image.color = Color.magenta;
                break;
        }
    }
}
