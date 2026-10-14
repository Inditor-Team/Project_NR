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

    //���� ������ �ִ� ī��� id
    private int[] myCards = new int[CardSlotCount] { -1, -1, -1 };
    public int[] MyCards => myCards;
    int cardSlotIndex = 0;
    public event UnityAction<int> OnGetCard;

    //�������� ���� ���� ������
    private int[] stageEntryCards = new int[CardSlotCount] { -1, -1, -1 };
    private int stageEntryCredit;
    private int stageEntryCardSlotIndex;
    private bool hasStageEntrySnapshot;

    //���� ���� ��ȭ
    [SerializeField] private int curCredit;
    public int CurCredit => curCredit;
    public event UnityAction OnCreditChanged;

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
    /// �������� ���� ������ ����
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

        //2���� ī�� id ��
        int[] versionTwoIds = new int[4]
        {
        (int)LevelCardSO.LevelCardType.BioreinforcementB,
        (int)LevelCardSO.LevelCardType.EvasionB,
        (int)LevelCardSO.LevelCardType.NeuralAccelerationB,
        (int)LevelCardSO.LevelCardType.RecoveryAlgorithmB
        };

        //���� �������� ī�尡 2�����̶��
        if (versionTwoIds.Contains(cardId))
        {
            //1���� ī�尡 ����ִ� ���� ã��
            for (int i = 0; i < myCards.Length; i++)
            {
                if (myCards[i] == cardId - 1)
                {
                    slotIndex = i;
                    break;
                }
            }
        }

        //2������ �ƴϰų�, Ȥ�� 1���� ������ ã�� ���ߴٸ� �� ���� ���
        if (slotIndex == -1)
        {
            slotIndex = cardSlotIndex;

            cardSlotIndex = (cardSlotIndex + 1) % 3; //���� 3ĭ�� ������
        }

        myCards[slotIndex] = cardId;

        OnGetCard?.Invoke(cardId);
    }
    #endregion

    #region Item

    /// <summary>
    /// ���� ���� �ÿ� �����ϰ� �ִ� ī���� �ɷ�ġ�� �����մϴ�
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

            Debug.Log($"{(LevelCardSO.LevelCardType)(myCards[i])} ȿ�� ����");
        }
    }
    #endregion

    /// <summary>
    /// ���̺� �����Ϳ� �κ��丮 ���� ����
    /// </summary>
    /// <param name="data"></param>
    public void SaveDataTo(SaveDataStruct data)
    {
        bool useStageEntry =
            SceneController.IsStageScene(SceneController.Instance.curScene)
            && hasStageEntrySnapshot;

        int[] cardsToSave = useStageEntry ? stageEntryCards : myCards;

        data.credit = useStageEntry ? stageEntryCredit : curCredit;
        data.cards = cardsToSave != null ? (int[])cardsToSave.Clone() : new int[CardSlotCount];
        data.cardSlotIndex = useStageEntry ? stageEntryCardSlotIndex : cardSlotIndex;

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

        // 슬롯 인덱스
        if (data.cardSlotIndex >= 0 && data.cardSlotIndex < CardSlotCount)
            cardSlotIndex = data.cardSlotIndex;
        else
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
