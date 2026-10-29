using System;
using UnityEngine;
using System.Collections.Generic;

public class SaveDataStruct
{
    // 프로토콜 타입, 크레딧, 보유 아이템? -> 플레이어 데이터 클래스를 아예 따로 생성??
    public Dictionary<PlayerStat.Stat, float> statDic = new(); // 스탯 통째로 저장
    public SceneController.Scene sectorName; // 섹터 명
    public bool storeHackFlag; // 상점 도둑질했는지
    public bool attemptHack;
    public ProtocolCard.Protocol protocol;
    public Dictionary<SceneController.Scene, bool> clearedSector = new();
    public int credit;
    public int[] cards;
    public HashSet<string> flags; // 대화 플래그
    public ItemId itemId;
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
    
    [Header("Encryption")]
    [SerializeField] private bool encryptInEditor = false; // 에디터 상에서 세이브 암호와 여부, 빌드는 자동 암호화 ON
    private bool UseEncryption // 암호화 여부
    {
        get
        {
#if UNITY_EDITOR
            return encryptInEditor;
#else
            return true;
#endif
        }
    }
    
    // 세이브 파일을 읽어와 CurrentData에 반영
    public bool LoadFromFile(string filePath) // filePath: 세이브 전체 경로(슬롯 포함)
    {
        string jsonString = ReadSaveJson(filePath);
        if (jsonString == null) return false; // 파일이 없거나 읽기, 암호 복호화 실패

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
        
        string fileText = UseEncryption ? SaveEncryptor.Encrypt(jsonString) : jsonString;
        if (fileText == null) return false; // 암호화 실패

        return FileIOSystem.WriteTextToFile(filePath, fileText);
    }

    // 게임 플레이 중 최신 상태를 반영
    public void SetCurrentData(SaveDataStruct newData)
    {
        CurrentData = newData;
    }
    
    // 슬롯 목록 조회용, 실제 로드는 아니고 세이브 데이터 내용만 확인
    public SaveDataStruct PeekDataFromFile(string filePath)
    {
        string jsonString = ReadSaveJson(filePath);
        if (jsonString == null) return null;

        return serializer.Deserialize(jsonString);
    }
    
    // 암호화 파일을 json 파일로
    private string ReadSaveJson(string filePath)
    {
        string fileText = FileIOSystem.ReadTextFromFile(filePath);
        if (string.IsNullOrEmpty(fileText)) return null;
 
#if UNITY_EDITOR
        // 에디터 상에서는 기존 json 파일처럼 읽기도 가능
        if (IsPlainJson(fileText)) return fileText;
#endif
 
        return SaveEncryptor.Decrypt(fileText);
    }
 
    // json 구분자, json은 '{'로 시작하고 Base64 문자열에는 '{'가 들어갈 수 없으므로 이걸로 구분
    private static bool IsPlainJson(string text)
    {
        return text.TrimStart().StartsWith("{");
    }
}
