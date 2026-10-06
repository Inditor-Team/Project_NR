using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour, ISaveable
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

    private void Start()
    {
        SaveSlotManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        SaveSlotManager.Instance.Unregister(this);
    }

    private const int CardSlotCount = 3;
    private const int SavedItemIndex = 3;
    private const int SavedCardSlotIndex = 4;

    //현재 가지고 있는 카드들 id
    private int[] myCards = new int[CardSlotCount] { -1, -1, -1 };
    public int[] MyCards => myCards;
    int cardSlotIndex = 0;
    public event UnityAction<int> OnGetCard;

    //스테이지 입장 시점 스냅샷
    private int[] stageEntryCards = new int[CardSlotCount] { -1, -1, -1 };
    private int stageEntryCredit;
    private int stageEntryCardSlotIndex;
    private bool hasStageEntrySnapshot;

    //현재 가진 재화
    [SerializeField] private int curCredit;
    public int CurCredit => curCredit;
    public event UnityAction OnCreditChanged;

    [Header("Item Save")]
    [SerializeField] private ItemSO[] itemDatabase;

    /// <summary>
    /// 스테이지 입장 시점을 저장
    /// </summary>
    public void CaptureStageEntryState()
    {
        stageEntryCards = (int[])myCards.Clone();
        stageEntryCredit = curCredit;
        stageEntryCardSlotIndex = cardSlotIndex;
        hasStageEntrySnapshot = true;
    }

    #region Credit
    public void AddCredit(int addAmount)
    {
        curCredit += addAmount;
        OnCreditChanged?.Invoke();
    }
    
    public void SetCredit(int newAmount)
    {
        curCredit = newAmount;
        OnCreditChanged?.Invoke();
    }

    #endregion

    #region Card
    public void GetCard(int cardId)
    {
        int slotIndex = -1;

        //2버전 카드 id 들
        int[] versionTwoIds = new int[4]
        {
        (int)LevelCardSO.LevelCardType.BioreinforcementB,
        (int)LevelCardSO.LevelCardType.EvasionB,
        (int)LevelCardSO.LevelCardType.NeuralAccelerationB,
        (int)LevelCardSO.LevelCardType.RecoveryAlgorithmB
        };

        //지금 가지려는 카드가 2버전이라면
        if (versionTwoIds.Contains(cardId))
        {
            //1버전 카드가 들어있는 슬롯 찾기
            for (int i = 0; i < myCards.Length; i++)
            {
                if (myCards[i] == cardId - 1)
                {
                    slotIndex = i;
                    break;
                }
            }
        }

        //2버전이 아니거나, 혹시 1버전 슬롯을 찾지 못했다면 새 슬롯 사용
        if (slotIndex == -1)
        {
            slotIndex = cardSlotIndex;

            cardSlotIndex = (cardSlotIndex + 1) % 3; //슬롯 3칸만 쓰도록
        }

        myCards[slotIndex] = cardId;

        OnGetCard?.Invoke(cardId);
    }
    #endregion

    #region Item

    /// <summary>
    /// 섹터 시작 시에 보유하고 있는 카드의 능력치를 적용합니다
    /// </summary>
    public void SetCardStatOnSectorStart()
    {
        for (int i = 0; i < myCards.Length; i++) 
        {
            if (myCards[i] == -1)
                continue;

            var cardId = myCards[i];
            var cardType = (LevelCardSO.LevelCardType)(myCards[i]);
            var playerStat = GameManager.Instance.Player.GetComponent<PlayerController>().Stat;
            var cardDic = LevelCardData.Instance.LevelCardDic;

            switch (cardType)
            {
                case LevelCardSO.LevelCardType.OverheatedMagazine:
                case LevelCardSO.LevelCardType.OverClock:
                case LevelCardSO.LevelCardType.FullAutoProtocol:
                case LevelCardSO.LevelCardType.SubGear:
                    foreach (var element in cardDic[cardId].Elements)
                    {
                        if (element == null)
                            continue;

                        playerStat.IncreaseStat(
                            element.targetStat,
                            element.upgradeAmount
                        );
                    }
                    break;
            }

            Debug.Log($"{(LevelCardSO.LevelCardType)(myCards[i])} 효과 적용");
        }
    }
    #endregion

    /// <summary>
    /// 세이브 데이터에 인벤토리 상태 저장
    /// </summary>
    /// <param name="data"></param>
    public void SaveDataTo(SaveDataStruct data)
    {
        bool useStageEntry =
            SceneController.IsStageScene(SceneController.Instance.curScene)
            && hasStageEntrySnapshot;

        int[] cardsToSave = useStageEntry ? stageEntryCards : myCards;
        int creditToSave = useStageEntry ? stageEntryCredit : curCredit;
        int slotIndexToSave = useStageEntry ? stageEntryCardSlotIndex : cardSlotIndex;

        data.credit = creditToSave;

        data.cards = new int[5];

        for (int i = 0; i < CardSlotCount; i++)
            data.cards[i] = cardsToSave[i];

        ItemSO item = GameManager.Instance.TempItemCaching;

        data.cards[SavedItemIndex] =
            item != null ? (int)item.Id : (int)ItemSO.ItemId.None;

        data.cards[SavedCardSlotIndex] = slotIndexToSave;
    }

    public void LoadDataFrom(SaveDataStruct data)
    {
        SetCredit(data.credit);

        for (int i = 0; i < CardSlotCount; i++)
        {
            if (data.cards != null && i < data.cards.Length)
                myCards[i] = data.cards[i];
            else
                myCards[i] = -1;
        }

        if (data.cards != null && data.cards.Length > SavedCardSlotIndex)
        {
            int savedSlotIndex = data.cards[SavedCardSlotIndex];

            if (savedSlotIndex >= 0 && savedSlotIndex < CardSlotCount)
                cardSlotIndex = savedSlotIndex;
            else
                SetCardSlotIndexByCards();
        }
        else
        {
            SetCardSlotIndexByCards();
        }

        ItemSO.ItemId itemId = ItemSO.ItemId.None;

        if (data.cards != null && data.cards.Length > SavedItemIndex)
            itemId = (ItemSO.ItemId)data.cards[SavedItemIndex];

        GameManager.Instance.SetTempItemCaching(FindItemById(itemId));

        CaptureStageEntryState();
    }

    private void SetCardSlotIndexByCards()
    {
        for (int i = 0; i < myCards.Length; i++)
        {
            if (myCards[i] == -1)
            {
                cardSlotIndex = i;
                return;
            }
        }

        cardSlotIndex = 0;
    }

    private ItemSO FindItemById(ItemSO.ItemId id)
    {
        if (id == ItemSO.ItemId.None)
            return null;

        return itemDatabase.FirstOrDefault(
            item => item != null && item.Id == id
        );
    }

}
