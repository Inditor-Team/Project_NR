using UnityEngine;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour
{
    [SerializeField] Image[] cardSlotElement;
    [SerializeField] LevelCardData cardData;
    int slotIndex = 0;

    private void Start()
    {
        foreach (var element in InventoryManager.Instance.MyCards)
        {
            if (!string.IsNullOrWhiteSpace(element))
                SetCardSlotUI(element);
        }
    }

    void SetCardSlotUI(string cardId)
    {
        cardSlotElement[slotIndex].sprite = cardData.LevelCardDic[cardId].CardIcon;
        cardSlotElement[slotIndex].gameObject.SetActive(true);

        slotIndex = (slotIndex + 1) % 3;
    }
}
