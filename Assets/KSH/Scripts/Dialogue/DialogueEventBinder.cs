using System.Collections.Generic;
using UnityEngine;

// 대화 데이터 이벤트 관련, 이벤트 등록 및 조건 체크
public class DialogueEventBinder : MonoBehaviour
{
    private OpeningController openingCon;
    private readonly HashSet<string> flags = new();
    private int resultInt; // 값 변환에 사용 
    private static Store store;
    
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
        DialogueEventDispatcher.RegisterEvent("STORE_HACK", HandleStoreHack);
        DialogueEventDispatcher.RegisterEvent("NPC_ACTION", HandleNPC);
    }

    private void RegisterConditions()
    {
        DialogueEventDispatcher.RegisterCondition("HAS_CREDIT", CheckHasCredit);
        DialogueEventDispatcher.RegisterCondition("CHECK_FLAG", CheckFlag);
        DialogueEventDispatcher.RegisterCondition("RANDOM_CHANCE", CheckRandomChance);
        DialogueEventDispatcher.RegisterCondition("HAS_CREDIT_FOR_ITEM", CheckHasCreditForItem);
        DialogueEventDispatcher.RegisterCondition("SHOP_ITEM_AVAILABLE", CheckShopItemAvailable); // 추가
    }
    
    # region 상점 관련
    private static Store GetStore()
    {
        if (store == null) // Unity null 체크: 씬 전환으로 파괴된 경우도 재탐색
            store = FindFirstObjectByType<Store>();
        return store;
    }

    private static bool TryParseSlot(string[] args, out int slot)
    {
        slot = -1;
        if (args == null || args.Length == 0 || !int.TryParse(args[0], out slot))
        {
            Debug.LogError("상점 슬롯 인덱스 변환 실패");
            return false;
        }
        return true;
    }
    
    # endregion

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
            InventoryManager.Instance.AddCredit(resultInt);
        else
            Debug.LogError("크레딧 조건 체크 실패, args[0]가 숫자가 아님 args[0]" + args[0]);
    }

    private void HandleGiveRandomItem(string[] args)
    {
        Debug.Log("랜덤 아이템 지급" + " / Pool : " + args[0] + " / Count : " + args[1]);
    }
    
    private void HandleBuyItem(string[] args)
    {
        if (!TryParseSlot(args, out int slot)) return;

        Store s = GetStore();
        if (s == null)
        {
            Debug.LogError("Store를 찾을 수 없음"); 
            return;
        }

        if (!s.TryGetItem(slot, out ItemSO item))
        {
            Debug.LogError($"구매 불가 슬롯 : {slot}");
            return;
        }

        // 혹시 모르니 한 번 더 조건 체크
        if (InventoryManager.Instance.CurCredit < item.Price)
        {
            Debug.LogWarning($"크레딧 부족 : {item.Name} / {item.Price}");
            return;
        }

        InventoryManager.Instance.AddCredit(-item.Price);
        s.BuyItem(slot);

        Debug.Log($"아이템 구매 완료 : {item.Name} / 가격 : {item.Price}");
    }

    // 해킹 결과는 대화 JSON의 RANDOM_CHANCE 분기 결과를 Store에 전달
    private void HandleStoreHack(string[] args)
    {
        Store s = GetStore();
        if (s == null)
        {
            Debug.LogError("Store를 찾을 수 없음"); 
            return;
        }

        s.TryHack(args.Length > 0 && args[0] == "success");
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
    private bool CheckShopItemAvailable(string[] args)
    {
        if (!TryParseSlot(args, out int slot)) return false;

        Store s = GetStore();
        return s != null && !s.IsSoldOut(slot);
    }

    private bool CheckHasCreditForItem(string[] args)
    {
        if (!TryParseSlot(args, out int slot)) return false;

        Store s = GetStore();
        if (s == null || !s.TryGetItem(slot, out ItemSO item)) return false;

        bool canBuy = InventoryManager.Instance.CurCredit >= item.Price;
        Debug.Log($"구매 가능 검사 / {item.Name} / 가격 : {item.Price} / 결과 : {canBuy}");
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