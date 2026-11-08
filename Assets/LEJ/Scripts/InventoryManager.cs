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

    //현재 보유 중인 카드 id
    private int[] myCards = new int[CardSlotCount] { -1, -1, -1 };
    public int[] MyCards => myCards;
    int cardSlotIndex = 0;
    public event UnityAction<int> OnGetCard;
    public event UnityAction OnCardsRestored; //실패 등으로 입장 시 상태로 되돌아감 (카드 UI 갱신용)

    //스테이지 입장 시 상태 스냅샷
    private int[] stageEntryCards = new int[CardSlotCount] { -1, -1, -1 };
    private int stageEntryCredit;
    private int stageEntryCardSlotIndex;
    private bool hasStageEntrySnapshot;

    //현재 보유 재화
    [SerializeField] private int curCredit;
    public int CurCredit => curCredit;
    public event UnityAction<int> OnCreditChanged;

    /*[Header("Item Save")]
    [SerializeField] private ItemSO[] itemDatabase;*/

    private ItemDatabase itemDatabase;
    private ItemDatabase ItemDB
    {
        get
        {
            if (itemDatabase == null)
            {
                itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
                if (itemDatabase == null)
                    Debug.LogError("[InventoryManager] Resources/ItemDatabase 에셋을 찾을 수 없습니다.");
            }
            return itemDatabase;
        }
    }
    
    /// <summary>
    /// 스테이지 입장 시 상태를 저장
    /// </summary>
    public void CaptureStageEntryState()
    {
        stageEntryCards = (int[])myCards.Clone();
        stageEntryCredit = curCredit;
        stageEntryCardSlotIndex = cardSlotIndex;
        hasStageEntrySnapshot = true;
    }

    /// <summary>
    /// 클리어 전 이탈(실패 포함) 시 입장 시 상태로 복귀 (StatDataManager.OnLeaveScene)
    /// 스탯도 같은 시점으로 되돌아가므로 카드 효과는 따로 해제X
    /// </summary>
    public void RestoreStageEntryState()
    {
        if (!hasStageEntrySnapshot)
            return;

        myCards = (int[])stageEntryCards.Clone();
        cardSlotIndex = stageEntryCardSlotIndex;
        SetCredit(stageEntryCredit);

        OnCardsRestored?.Invoke();
    }

    #region Credit
    public void AddCredit(int addAmount)
    {
        curCredit += addAmount;
        OnCreditChanged?.Invoke(addAmount);
    }
    
    public void SetCredit(int newAmount)
    {
        curCredit = newAmount;
        OnCreditChanged?.Invoke(0);
    }

    #endregion

    #region Card
    public void GetCard(int cardId)
    {
        int slotIndex = -1;

        //2버전 카드 id 목록
        int[] versionTwoIds = new int[4]
        {
        (int)LevelCardSO.LevelCardType.BioreinforcementB,
        (int)LevelCardSO.LevelCardType.EvasionB,
        (int)LevelCardSO.LevelCardType.NeuralAccelerationB,
        (int)LevelCardSO.LevelCardType.RecoveryAlgorithmB
        };

        //획득한 카드가 2버전이라면
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

        //2버전이 아니거나, 혹은 1버전 슬롯을 찾지 못했다면 다음 슬롯 사용
        if (slotIndex == -1)
        {
            slotIndex = cardSlotIndex;

            cardSlotIndex = (cardSlotIndex + 1) % CardSlotCount; //슬롯 3칸을 순환
        }

        int prevCardId = myCards[slotIndex];
        myCards[slotIndex] = cardId;

        //슬롯에서 밀려난 카드(1버전 포함)의 효과를 되돌리고 새 카드 효과 적용
        if (StatDataManager.Instance != null)
        {
            if (prevCardId != -1)
                StatDataManager.Instance.RevertCardEffect(prevCardId);

            StatDataManager.Instance.ApplyCardEffect(cardId);
        }

        OnGetCard?.Invoke(cardId);
    }
    #endregion

    /// <summary>
    /// 세이브 데이터에 인벤토리 상태 저장
    /// 스테이지 클리어 전이면 입장 시 상태를, 클리어 후면 현재 상태를 저장합니다
    /// </summary>
    /// <param name="data"></param>
    public void SaveDataTo(SaveDataStruct data)
    {
        bool useStageEntry =
            StatDataManager.IsUncommittedStage()
            && hasStageEntrySnapshot;

        int[] cardsToSave = useStageEntry ? stageEntryCards : myCards;

        data.credit = useStageEntry ? stageEntryCredit : curCredit;
        data.cards = cardsToSave != null ? (int[])cardsToSave.Clone() : new int[CardSlotCount];

        ItemSO item = GameManager.Instance.TempItemCaching;
        data.itemId = item != null ? item.Id : ItemId.None;
    }

    public void LoadDataFrom(SaveDataStruct data)
    {
        SetCredit(data.credit);
        myCards = new int[CardSlotCount];
        if (data.cards != null)
        {
            int len = Mathf.Min(data.cards.Length, CardSlotCount);
            System.Array.Copy(data.cards, myCards, len);
        }
        SetCardSlotIndexByCards();

        ItemId itemId = data.itemId;
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

    private ItemSO FindItemById(ItemId id)
    {
        return ItemDB != null ? ItemDB.Get(id) : null;
    }
}