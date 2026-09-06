using UnityEngine;
using System.Collections.Generic;

public class SaveDataStruct
{
    // 프로토콜 타입, 크레딧, 보유 아이템? -> 플레이어 데이터 클래스를 아예 따로 생성??
    public Dictionary<PlayerStat.Stat, float> statDic; // 스탯 통째로 저장
    public SceneController.Scene sectorName; // 섹터 명
    public bool isStolen; // 상점 도둑질했는지
}

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }
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

    public SaveDataStruct CurrentData { get; private set; }
    private JsonSaveDataSerializer serializer = new JsonSaveDataSerializer();
    
    // 세이브 파일을 읽어와 CurrentData에 반영
    public bool LoadFromFile(string filePath) // filePath: 세이브 전체 경로(슬롯 포함)
    {
        string jsonString = FileIOSystem.ReadTextFromFile(filePath);
        if (jsonString == null) return false; // 파일이 없거나 읽기 실패

        SaveDataStruct loaded = serializer.Deserialize(jsonString);

        if (loaded == null) return false;

        CurrentData = loaded;
        return true;
    }
    
    // 현재 데이터(CurrentData)를 파일로 저장
    public bool SaveToFile(string filePath)
    {
        string jsonString = serializer.Serialize(CurrentData);
        if (jsonString == null) return false;

        return FileIOSystem.WriteTextToFile(filePath, jsonString);
    }

    // 게임 플레이 중 최신 상태를 반영
    public void SetCurrentData(SaveDataStruct newData)
    {
        CurrentData = newData;
    }

    // 데이터 초기화 (새게임 시작 상태)
    public void ResetToDefault()
    {
        // TODO: 기본값으로 초기화된 SaveDataStruct 생성 후 CurrentData에 대입, 기본 값을 어떻게 할지? 그대로?
        SaveDataStruct defalutData = new SaveDataStruct();
        CurrentData = defalutData;
    }
    
    // 슬롯 목록 조회용, 실제 로드는 아니고 세이브 데이터 내용만 확인
    public SaveDataStruct PeekDataFromFile(string filePath)
    {
        string jsonString = FileIOSystem.ReadTextFromFile(filePath);
        if (jsonString == null) return null;

        return serializer.Deserialize(jsonString);
    }
}
