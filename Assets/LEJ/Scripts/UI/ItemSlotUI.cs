using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아이템 슬롯을 UI 에 표시합니다.
/// </summary>
public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Image itemSprite;
    Color transparent = new Color(0, 0, 0, 0);
    Color original = new Color(1, 1, 1, 1);

    public void UpdateUI(ItemSO curItem)
    {
        if (curItem == null)
        {
            itemSprite.color = transparent;
            return;
        }

        itemSprite.sprite = curItem.Sprite;
        itemSprite.color = original;
    }
}
