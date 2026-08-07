using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 플레이어의 인벤토리 입니다.
/// </summary>
public class PlayerInventory : MonoBehaviour, IItemHolder
{
    [SerializeField] ItemSlotUI ui;
    [SerializeField] PlayerStat playerStat; //추후 매니저에서 Player 참조 시 그쪽으로 연결
    [SerializeField] LevelCardProvider levelCardProvider; //추후 맵매니저에서 참조하기

    private ItemSO curItem;
    public ItemSO CurItem => curItem;

    /// <summary>
    /// 획득 할 때 플레이어의 슬롯이 비어있지 않다면, 기존 아이템을 땅에 떨구고 새로운 아이템을 슬롯에 장착합니다.
    /// </summary>
    /// <param name="itemObject"></param>
    public void HoldItem(ItemObject itemObject)
    {
        if (curItem == null)
            return;

        //만약 재화 아이템이라면 획득 시 바로 사용
        if (itemObject.MyItem.Type == ItemSO.ItemType.GetCredit)
        {
            GameManager.Instance.Credit += (int)itemObject.MyItem.Amount;
            ItemManager.Instance.DespawnItem(itemObject); //월드 내 아이템 오브젝트 Destroy
            return;
        }

        //이미 아이템을 들고 있다면, 획득하려는 아이템과 교체
        if (curItem != null)
        {
            var temp = itemObject.MyItem;
            itemObject.SetItem(curItem); //월드에 있는 아이템 오브젝트를 현재 슬롯 아이템으로 교체
            curItem = temp; //월드에 있던 아이템을 슬롯에 장착
        }
        else //아이템을 들고 있지 않다면 그대로 슬롯에 장착
        {
            curItem = itemObject.MyItem; //슬롯에 장착 후
            ItemManager.Instance.DespawnItem(itemObject); //월드 내 아이템 오브젝트 Destroy
        }

        if (ui != null)
            ui.UpdateUI(curItem);
    }

    public void UseItem()
    {
        switch (curItem.Type)
        {
            case ItemSO.ItemType.DamagedCore:
                //TO DO : 코어 손상 아이템 사용 구현
                break;
            case ItemSO.ItemType.GetCard:
                levelCardProvider.ProvideByUI();
                break;
            case ItemSO.ItemType.GetCredit:
                GameManager.Instance.Credit += (int)curItem.Amount;
                break;
            case ItemSO.ItemType.GetHP:
                playerStat.EarnLife(curItem.Amount);
                break;
            case ItemSO.ItemType.BlankBullet:
                //TO DO : 공포탄 아이템 사용 구현
                break;
        }

        if (ui != null)
            ui.UpdateUI(curItem);
    }
}
