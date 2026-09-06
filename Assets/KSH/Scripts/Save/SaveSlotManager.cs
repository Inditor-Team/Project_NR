using System.Collections.Generic;
using UnityEngine;

// 슬롯 리스트 ui에 표시
public class SaveSlotInfo
{
    public int slotIndex;
    public string saveInfo; // 스테이지, 체력 정보만
    public string saveDateAndTime;
    public bool isEmpty;
}

/// <summary>
/// 어떤 슬롯을 세이브/로드/삭제할지 결정하고
/// FileIOSystem / SaveDataManager(직렬화 레이어 포함)를 조율하는 역할
/// 슬롯 번호(int)를 다루는 상위 레이어, 여기서만 슬롯 번호 다루기
/// </summary>
public class SaveSlotManager : MonoBehaviour
{
    public static SaveSlotManager Instance { get; private set; }

    // 세이브 데이터는 하나에서 관리해야 하니 싱글톤 적용
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
    
    private const int MaxSlotCount = 5; // 슬롯 개수

    // 특정 슬롯에 현재 게임 데이터(상태) 저장
    public bool SaveToSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return false; // 슬롯 번호가 0~4를 벗어남, out of index
        FileIOSystem.EnsureSaveDirectoryExists(); // 혹시 파일 없으면 생성하기
        string path = FileIOSystem.GetSlotFilePath(slotIndex);
        
        return SaveDataManager.Instance.SaveToFile(path);
    }

    // 특정 슬롯의 데이터를 읽어와 SaveDataManager.CurrentData에 반영
    public bool LoadFromSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return false;
        bool isEmpty = IsSlotEmpty(slotIndex); // 슬롯 비어있으면 true
        if (isEmpty) return false;

        string path = FileIOSystem.GetSlotFilePath(slotIndex);
        return SaveDataManager.Instance.LoadFromFile(path);
    }

    // 특정 슬롯의 세이브 파일 삭제
    public bool DeleteSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return false;
        string path = FileIOSystem.GetSlotFilePath(slotIndex);
        return FileIOSystem.DeleteFile(path);
    }

    // 특정 슬롯이 비어있는지 확인
    public bool IsSlotEmpty(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return false;
        string path = FileIOSystem.GetSlotFilePath(slotIndex);
        return !FileIOSystem.SlotFileExists(path);
    }

    // 슬롯 info 만들기
    public List<SaveSlotInfo> GetAllSlotSummaries()
    {
        List<SaveSlotInfo> saveList = new List<SaveSlotInfo>();

        for (int i = 0; i < MaxSlotCount; i++)
        {
            bool isEmpty = IsSlotEmpty(i);
            SaveSlotInfo info = new SaveSlotInfo
            {
                slotIndex = i,
                isEmpty = isEmpty
            };

            if (isEmpty) // 빈 슬롯
            {
                info.saveInfo = "Empty Slot";
                info.saveDateAndTime = "";
            }
            else
            {
                string path = FileIOSystem.GetSlotFilePath(i);
                SaveDataStruct data = SaveDataManager.Instance.PeekDataFromFile(path);

                if (data == null)
                {
                    // 파일은 있는데 파싱 실패 (손상된 세이브)
                    info.saveInfo = "Corrupted Save";
                    info.saveDateAndTime = "";
                }
                else
                {
                    float hp = data.statDic.GetValueOrDefault(PlayerStat.Stat.Life, 0f); // Life 값이 null이면 0 반환
                    string stageName = data.sectorName.ToString().Substring(6); // 일단 임시로 앞에 Scene_ 글자만 삭제
                    // TODO: 스테이지 번호로 변경하기
                    info.saveInfo = $"Stage {stageName} (HP: {hp})"; 
                    info.saveDateAndTime = FileIOSystem.GetLastWriteTime(path).ToString("yyyy.MM.dd.HH:mm");
                }
            }

            saveList.Add(info);
        }

        return saveList;
    }

    // 슬롯 인덱스가 유효한지 검사
    private bool IsValidSlotIndex(int slotIndex)
    {
        bool isValid = slotIndex is >= 0 and < MaxSlotCount;
        if (!isValid)
            Debug.LogError($"[SaveSlotManager] 유효하지 않은 슬롯 인덱스: {slotIndex}"); // 혹시 모르는 에러 로그

        return isValid;
    }
}