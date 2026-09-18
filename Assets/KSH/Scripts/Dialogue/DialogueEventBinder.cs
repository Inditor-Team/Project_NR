using System.Collections.Generic;
using UnityEngine;

// 대화 데이터 이벤트 관련, 이벤트 등록 및 조건 체크
public class DialogueEventBinder : MonoBehaviour
{
    private OpeningController openingCon;
    private readonly HashSet<string> flags = new();
    private struct ShopItemData
    {
        public string NameKey; // 아이템 이름 로컬라이제이션 키
        public int Price;
    }

    private static readonly Dictionary<string, ShopItemData> shopItems = new()
    {
        ["common_item1"] = new ShopItemData { NameKey = "ITEM_NAME_COMMON1", Price = 100},
        ["common_item2"] = new ShopItemData { NameKey = "ITEM_NAME_COMMON2", Price = 200},
        ["common_item3"] = new ShopItemData { NameKey = "ITEM_NAME_COMMON3", Price = 300},
    };
    
    private int resultInt; // 값 변환에 사용 
    
    private void Awake()
    {
        RegisterEvents();
        RegisterConditions();
    }

    private void RegisterEvents()
    {
        DialogueEventDispatcher.RegisterEvent("HEAL", HandleHeal);
        DialogueEventDispatcher.RegisterEvent("ADD_MAX_HP", HandleAddMaxHp);
        DialogueEventDispatcher.RegisterEvent("ADD_CREDIT", HandleAddCredit);
        DialogueEventDispatcher.RegisterEvent("GIVE_RANDOM_ITEM", HandleGiveRandomItem);
        DialogueEventDispatcher.RegisterEvent("SET_FLAG", HandleSetFlag);
        DialogueEventDispatcher.RegisterEvent("OPEN_UI", HandleOpenUI);
        DialogueEventDispatcher.RegisterEvent("BUY_ITEM", HandleBuyItem);
        DialogueEventDispatcher.RegisterEvent("NPC_ACTION", HandleNPC);
    }

    private void RegisterConditions()
    {
        DialogueEventDispatcher.RegisterCondition("HAS_CREDIT", CheckHasCredit);
        DialogueEventDispatcher.RegisterCondition("CHECK_FLAG", CheckFlag);
        DialogueEventDispatcher.RegisterCondition("RANDOM_CHANCE", CheckRandomChance);
        DialogueEventDispatcher.RegisterCondition("HAS_CREDIT_FOR_ITEM", CheckHasCreditForItem);
    }

    # region 이벤트 핸들러
    
    private void HandleNPC(string[] args)
    {
        Debug.Log("HandleNPC : " + args[0]);
        if (openingCon == null)
        {
            openingCon = FindFirstObjectByType<OpeningController>();
        }
        if (args[0].Equals("move_to_window"))
        {
            openingCon.MoveNPCToWidnow();
        }
        
        if (args[0].Equals("move_close_to_player"))
        {
            openingCon.MoveNPCToPlayer();
        }
        
        if (args[0].Equals("show_goggles"))
        {
            StartCoroutine(openingCon.ShowRedGoggle());
        }
        
        if (args[0].Equals("end_intro"))
        {
            openingCon.FinishOpening();
        }
    }
    
    private void HandleHeal(string[] args)
    {
        Debug.Log("체력 회복 : " + args[0]);
        if (int.TryParse(args[0], out resultInt))
            GameManager.Instance.AddLife(resultInt);
        else
            Debug.LogError("체력 회복 실패, args[0]가 숫자가 아님 args[0]" + args[0]);
    }

    private void HandleAddMaxHp(string[] args)
    {
        Debug.Log("최대 체력 증가 : " + args[0]);
        if (int.TryParse(args[0], out resultInt))
            GameManager.Instance.AddMaxLife(resultInt);
        else
            Debug.LogError("최대 체력 증가, args[0]가 숫자가 아님 args[0]" + args[0]);
    }

    private void HandleAddCredit(string[] args)
    {
        Debug.Log("크레딧 변경 : " + args[0]);
        if (int.TryParse(args[0], out resultInt))
            InventoryManager.Instance.SetCredit(resultInt);
        else
            Debug.LogError("크레딧 조건 체크 실패, args[0]가 숫자가 아님 args[0]" + args[0]);
    }

    private void HandleGiveRandomItem(string[] args)
    {
        Debug.Log("랜덤 아이템 지급" + " / Pool : " + args[0] + " / Count : " + args[1]);
    }
    
    private void HandleBuyItem(string[] args)
    {
        string itemId = args[0];
        if (!shopItems.TryGetValue(itemId, out var item))
        {
            Debug.LogError($"등록되지 않은 상점 아이템 ID : {itemId}");
            return;
        }

        InventoryManager.Instance.SetCredit(item.Price);
        Debug.Log($"아이템 구매 완료 : {itemId} / 가격 : {item.Price}");

        // TODO: 아이템 지급 메서드
    }

    public static string GetShopItemLabel(string itemId)
    {
        if (!shopItems.TryGetValue(itemId, out var item))
        {
            Debug.LogError($"등록되지 않은 상점 아이템 ID : {itemId}");
            return itemId;
        }

        string itemName = LocalizationManager.Instance.Get(item.NameKey);
        return LocalizationManager.Instance.GetFormat("OPT_STORE_ITEM_LABEL", itemName, item.Price);
    }

    private void HandleSetFlag(string[] args)
    {
        string flagId = args[0];
        flags.Add(flagId); // 설정
        
        Debug.Log("Flag 설정 : " + args[0]);
    }

    private void HandleOpenUI(string[] args)
    {
        Debug.Log("UI 열기 : " + args[0]); // 리스크 카드 표기, 능력치 변화 띄우기??
    }

    # endregion 

    #region 조건 체크
    private bool CheckHasCreditForItem(string[] args)
    {
        string itemId = args[0];
        if (!shopItems.TryGetValue(itemId, out var item))
        {
            Debug.LogError($"등록되지 않은 상점 아이템 ID : {itemId}");
            return false;
        }

        bool canBuy = InventoryManager.Instance.CurCredit >= item.Price;
        Debug.Log($"아이템 구매 가능 검사 / {itemId} / 가격 : {item.Price} / 결과 : {canBuy}");
        return canBuy;
    }

    private bool CheckHasCredit(string[] args)
    {
        Debug.Log("크레딧 보유량 검사 : " + args[0]);

        if (int.TryParse(args[0], out resultInt))
        {
            return resultInt <= InventoryManager.Instance.CurCredit;
        }
        
        Debug.LogError("크레딧 조건 체크 실패, args[0]가 숫자가 아님 args[0]" + args[0]);
        return false;
    }

    private bool CheckFlag(string[] args)
    {
        string flagId = args[0];
        bool hasFlag = flags.Contains(flagId);

        Debug.Log("Flag 검사" + " / Flag : " + flagId + " / 결과 : " + hasFlag);

        return hasFlag;
    }

    private bool CheckRandomChance(string[] args) // 상점 해킹 시 사용, 현재 50% 혹률
    {
        if (!int.TryParse(args[0], out int chance))
        {
            Debug.Log("확률 값 숫자 변환 실패");
            return false;
        }
        int randomValue = Random.Range(0, 100);
        bool success = randomValue < chance;

        Debug.Log("확률 검사" + " / 확률 : " + chance + "%" + " / 랜덤 값 : " + randomValue + " / 결과 : " + success);

        return success;
    }
    
    #endregion
}