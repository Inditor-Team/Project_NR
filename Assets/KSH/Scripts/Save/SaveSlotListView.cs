using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveSlotListView : MonoBehaviour
{
    [SerializeField] private SaveSlotUIItem[] slotItems = new SaveSlotUIItem[5];

    // On 상태되면 자동으로 설정하기
    private void OnEnable()
    {
        RefreshAllSlots();
    }

    public void RefreshAllSlots()
    {
        List<SaveSlotInfo> summaries = SaveSlotManager.Instance.GetAllSlotSummaries();

        for (int i = 0; i < slotItems.Length; i++)
            slotItems[i].SetData(summaries[i]); // 슬롯 당 세이브 데이터 설정
    }
}
