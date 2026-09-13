using System.Linq;
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
    private int[] myCards = new int[3] { -1, -1, -1 };
    public int[] MyCards => myCards;
    int cardSlotIndex = 0;
    public event UnityAction<int> OnGetCard;

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
                //증가 ---
                //생체 보강의 경우 최대 체력 증가
                case LevelCardSO.LevelCardType.BioreinforcementA:
                case LevelCardSO.LevelCardType.BioreinforcementB:
                //신경 가속의 경우 이속 증가
                case LevelCardSO.LevelCardType.NeuralAccelerationA:
                case LevelCardSO.LevelCardType.NeuralAccelerationB:
                    playerStat.AddStat(cardDic[cardId].Elements[0].targetStat, cardDic[cardId].Elements[0].upgradeAmount);
                    break;

                //배율 증가 ---
                //과열 탄창의 경우 공격력 증가, 공격 간격 증가
                case LevelCardSO.LevelCardType.OverheatedMagazine:
                //오버클럭의 경우 공격력 증가 및 피해량 증가
                case LevelCardSO.LevelCardType.OverClock:
                //연사 프로토콜의 경우 공격 간격 감소
                case LevelCardSO.LevelCardType.FullAutoProtocol:
                //보조 기어의 경우 프로토콜 대기 시간 감소
                case LevelCardSO.LevelCardType.SubGear:
                    playerStat.IncreaseStat(cardDic[cardId].Elements[0].targetStat, cardDic[cardId].Elements[0].upgradeAmount);
                    if (cardDic[cardId].Elements[1] != null)
                        playerStat.IncreaseStat(cardDic[cardId].Elements[1].targetStat, cardDic[cardId].Elements[1].upgradeAmount);
                    if (cardDic[cardId].Elements[2] != null)
                        playerStat.IncreaseStat(cardDic[cardId].Elements[2].targetStat, cardDic[cardId].Elements[2].upgradeAmount);
                    break;

                //토글 ---
                //회복 알고리즘의 경우 처치 시 체력 회복 토글 On
                case LevelCardSO.LevelCardType.RecoveryAlgorithmA:
                case LevelCardSO.LevelCardType.RecoveryAlgorithmB:
                //민첩 알고리즘의 경우 일정 확률로 적 공격 방어 토글 On
                case LevelCardSO.LevelCardType.EvasionA:
                case LevelCardSO.LevelCardType.EvasionB:
                //불안정 코어의 경우 일정 확률 연사 및 이속 디버프 토글 On
                case LevelCardSO.LevelCardType.InstableCore:
                    playerStat.SpecialToggle(cardType, cardDic[cardId].Elements[0].upgradeAmount);
                    break;

            }

            Debug.Log($"{(LevelCardSO.LevelCardType)(myCards[i])} 효과 적용");
        }
    }
    #endregion
}
