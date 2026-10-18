using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 아이템 슬롯을 UI 에 표시합니다.
/// </summary>
public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image itemSprite;
    Color transparent = new Color(0, 0, 0, 0);
    Color original = new Color(1, 1, 1, 1);
    [SerializeField] GameObject descriptionUI;
    [SerializeField] TMP_Text text_itemName;
    [SerializeField] TMP_Text text_description;
    PlayerInventory playerInventory;
    float showDescriptionTime = 3f;

    private void Awake()
    {
        itemSprite.color = transparent;
    }

    void Start()
    {
        playerInventory = GameManager.Instance.Player.GetComponent<PlayerInventory>();
    }

    public void UpdateUI(ItemSO curItem)
    {
        if (curItem == null)
        {
            itemSprite.color = transparent;
            return;
        }

        itemSprite.sprite = curItem.Sprite;
        itemSprite.color = original;

        text_itemName.text = $"<color=green>{curItem.Name.GetLocalizedString()}</color>";
        text_description.text = curItem.Description.GetLocalizedString();

        //아이템을 얻을 시에 잠깐동안 설명을 보여줍니다
        ShowDescriptionUI();
        Invoke("HideDescriptionUI", showDescriptionTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //슬롯 UI 위에 마우스를 올릴 시 잠깐동안 설명을 보여줍니다
        ShowDescriptionUI();
        Invoke("HideDescriptionUI", showDescriptionTime);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //슬롯 UI 에서 마우스를 뗄 시 바로 set active false
        HideDescriptionUI();
    }

    void ShowDescriptionUI()
    {
        //아이템을 갖고 있지 않다면 UI 표시 X
        if (playerInventory.CurItem == null)
            return;

        descriptionUI.SetActive(true);
    }

    void HideDescriptionUI()
    {
        descriptionUI.SetActive(false);
    }

}
