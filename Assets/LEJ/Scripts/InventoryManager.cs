using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    //현재 가지고 있는 카드들 id
    private string[] myCards = new string[3];
    public string[] MyCards => myCards;
    int cardSlotIndex = 0;
    public event UnityAction<string> OnGetCard;

    //현재 가진 재화
    private int curCredit;
    public int CurCredit => curCredit;
    public event UnityAction OnCreditChanged;

    //현재 가진 아이템
    private ItemSO curItem;
    public ItemSO CurItem => curItem;

    #region Credit
    public void SetCredit(int amount)
    {
        curCredit += amount;
        OnCreditChanged();
    }

    #endregion

    #region Card
    public void GetCard(string cardId)
    {
        myCards[cardSlotIndex] = cardId;
        OnGetCard?.Invoke(cardId);

        cardSlotIndex = (cardSlotIndex + 1) % 3; //슬롯 3칸만 쓰도록
    }
    #endregion

    #region Item
    /// <summary>
    /// 섹터가 끝날 시에 플레이어 슬롯에 들고있는 아이템 정보를 저장합니다
    /// </summary>
    /// <param name="item"></param>
    public void RegisterItemOnSectorClose(ItemSO item)
    {
        this.curItem = item;
    }

    /// <summary>
    /// 섹터 시작 시에 플레이어 슬롯에 들고 있게 합니다
    /// </summary>
    public void SetItemOnSectorStart()
    {
        if (curItem == null)
            return;

        GameManager.Instance.Player.GetComponent<PlayerInventory>().HoldItem(curItem);
        curItem = null;
    }
    #endregion
}
