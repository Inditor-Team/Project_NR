using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 슬롯마다 적용되는 클래스, 세이브 정보를 슬롯 UI에 반영하여 표기
public class SaveSlotUIItem : MonoBehaviour
{
    [SerializeField] private TMP_Text saveInfoText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Button slotButton;
    public event Action isSaveSuccess;
    
    public int SlotIndex { get; private set; }
    
    public void SetData(SaveSlotInfo info)
    {
        SlotIndex = info.slotIndex;
        saveInfoText.text = info.saveInfo;
        timeText.text = info.saveDateAndTime;
    }
    
    private void Awake()
    {
        slotButton.onClick.AddListener(HandleClick);
    }
    
    private void HandleClick()
    {
        bool isSaveMode = SaveSlotManager.Instance.CurrentMode == SaveLoadMode.Save;
        string msg;

        if (isSaveMode) // 세이브 모드일 때 텍스트 설정
            msg = LocalizationManager.Instance.CurLang == Language.KO ? 
                $"슬롯 {SlotIndex + 1}에 세이브 하시겠습니까?" : $"Save to slot {SlotIndex + 1} ?";
        else // 로드 모드
            msg = LocalizationManager.Instance.CurLang == Language.KO ? 
                $"슬롯 {{SlotIndex + 1}}을(를) 로드하시겠습니까?" : $"Load to slot {SlotIndex + 1} ?";
        
        UIManager.Instance.SetMsgPanel(msg,  () =>
        {
            bool success = SaveSlotManager.Instance.OnSlotClicked(SlotIndex);
            if (!success) // 세이브 혹은 로드 실패
            {
                Debug.LogError("세이브 슬롯 클릭 이벤트 처리 실패");
                UIManager.Instance.SetMsgPanel("Operation failed.", UIManager.Instance.HideMsgPanel);
                return;
            }
            
            // 세이브 일 때는 리프레시
            if (SaveSlotManager.Instance.CurrentMode == SaveLoadMode.Save)
            {
                // 일단은 메시지창으로 띄우는데 나중에는 UI 상으로 눈에 띄게 하기
                // TODO: 세이브 성공 사운드
                string successMsg = LocalizationManager.Instance.CurLang == Language.KO ? 
                    "세이브에 성공하였습니다." : "Game saved successfully";
                UIManager.Instance.SetMsgPanel(successMsg, UIManager.Instance.HideMsgPanel);
                isSaveSuccess?.Invoke(); // 리프레시
            }
        });
    }
}
