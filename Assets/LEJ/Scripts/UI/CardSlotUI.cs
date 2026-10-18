using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image[] cardSlotElement;
    int slotIndex = 0;
    [SerializeField] GameObject descriptionUI;
    [SerializeField] TMP_Text descriptionText;
    float showDescriptionTime = 3f;
    RectTransform descriptionUIRect;

    private void Start()
    {
        foreach (var element in InventoryManager.Instance.MyCards)
        {
            if (element != -1)
                SetCardSlotUI(element);
        }

        descriptionUIRect = descriptionUI.GetComponent<RectTransform>();
        descriptionUI.SetActive(false);
    }

    void SetCardSlotUI(int cardId)
    {
        cardSlotElement[slotIndex].sprite = LevelCardData.Instance.LevelCardDic[cardId].CardIcon;
        cardSlotElement[slotIndex].gameObject.SetActive(true);

        slotIndex = (slotIndex + 1) % 3;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowDescriptionUI();

        //UI 에 마우스를 갖다댈 시 잠깐 동안 설명을 보여줍니다
        Invoke("HideDescriptionUI", showDescriptionTime);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //UI 에서 마우스를 뗄 시 즉시 설명을 닫습니다
        HideDescriptionUI();
    }

    void ShowDescriptionUI()
    {
        //설명 및 크기 초기화
        descriptionText.text = "";
        descriptionUIRect.sizeDelta = new Vector2(descriptionUIRect.sizeDelta.x, 0);

        for (int i = 0; i < InventoryManager.Instance.MyCards.Length; i++) 
        {
            //슬롯에 카드를 가진 적 없다면 해당 슬롯 넘기기
            if (InventoryManager.Instance.MyCards[i] == -1)
                return;

            var curCard = LevelCardData.Instance.LevelCardDic[InventoryManager.Instance.MyCards[i]];
            //내용에 추가
            descriptionText.text += $"<color=green>{curCard.CardName.GetLocalizedString()}</color>\n" + $"{curCard.CardDescription.GetLocalizedString()}\n";
            //UI 사이즈 조절
            descriptionUIRect.sizeDelta = new Vector2(descriptionUIRect.sizeDelta.x, descriptionUIRect.sizeDelta.y + 130f);
        }

        descriptionUI.SetActive(true);
    }

    void HideDescriptionUI()
    {
        descriptionUI.SetActive(false);
    }
}
