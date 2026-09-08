using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 슬롯마다 적용되는 클래스, 세이브 정보를 슬롯 UI에 반영하여 표기
public class SaveSlotUIItem : MonoBehaviour
{
    [SerializeField] private TMP_Text saveInfoText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Button slotButton;

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
        bool success = SaveSlotManager.Instance.OnSlotClicked(SlotIndex);

        // TODO: 후속 처리
    }
}
