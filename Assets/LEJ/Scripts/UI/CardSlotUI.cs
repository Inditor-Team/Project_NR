using UnityEngine;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour
{
    [SerializeField] Image[] cardSlotElement;
    int slotIndex = 0;

    private void Start()
    {
        foreach (var element in InventoryManager.Instance.MyCards)
        {
            if (element != -1)
                SetCardSlotUI(element);
        }
    }

    void SetCardSlotUI(int cardId)
    {
        cardSlotElement[slotIndex].sprite = LevelCardData.Instance.LevelCardDic[cardId].CardIcon;
        cardSlotElement[slotIndex].gameObject.SetActive(true);

        slotIndex = (slotIndex + 1) % 3;
    }
}
