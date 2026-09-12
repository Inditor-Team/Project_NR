using System.Collections.Generic;
using UnityEngine;

// 대화 데이터 이벤트 관련, 이벤트 등록 및 조건 체크
public class DialogueEventBinder : MonoBehaviour
{
    private readonly HashSet<string> flags = new();
    
    private void Awake()
    {
        RegisterEvents();
        RegisterConditions();
    }

    private void RegisterEvents()
    {
        DialogueEventDispatcher.RegisterEvent(
            "HEAL",
            HandleHeal);

        DialogueEventDispatcher.RegisterEvent(
            "ADD_MAX_HP",
            HandleAddMaxHp);

        DialogueEventDispatcher.RegisterEvent(
            "ADD_CREDIT",
            HandleAddCredit);

        DialogueEventDispatcher.RegisterEvent(
            "GIVE_RANDOM_ITEM",
            HandleGiveRandomItem);

        DialogueEventDispatcher.RegisterEvent(
            "SET_FLAG",
            HandleSetFlag);

        DialogueEventDispatcher.RegisterEvent(
            "OPEN_UI",
            HandleOpenUI);
    }

    private void RegisterConditions()
    {
        DialogueEventDispatcher.RegisterCondition(
            "HAS_CREDIT",
            CheckHasCredit);

        DialogueEventDispatcher.RegisterCondition(
            "CHECK_FLAG",
            CheckFlag);

        DialogueEventDispatcher.RegisterCondition(
            "RANDOM_CHANCE",
            CheckRandomChance);
    }

    # region 이벤트 핸들러
    private void HandleHeal(string[] args)
    {
        Debug.Log("체력 회복 : " + args[0]);
    }

    private void HandleAddMaxHp(string[] args)
    {
        Debug.Log("최대 체력 증가 : " + args[0]);
    }

    private void HandleAddCredit(string[] args)
    {
        Debug.Log("크레딧 변경 : " + args[0]);
    }

    private void HandleGiveRandomItem(string[] args)
    {
        Debug.Log("랜덤 아이템 지급" + " / Pool : " + args[0] + " / Count : " + args[1]);
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

    private bool CheckHasCredit(string[] args)
    {
        Debug.Log("크레딧 보유량 검사 : " + args[0]);

        return true;
    }

    private bool CheckFlag(string[] args)
    {
        string flagId = args[0];
        bool hasFlag = flags.Contains(flagId);

        Debug.Log("Flag 검사" + " / Flag : " + flagId + " / 결과 : " + hasFlag);

        return hasFlag;
    }

    private bool CheckRandomChance(string[] args)
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