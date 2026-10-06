using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class StageSaveSnapshotProxy : MonoBehaviour, ISaveable
{
    public static StageSaveSnapshotProxy Instance { get; private set; }

    private HashSet<string> stageEntryFlags = new();
    private bool stageEntryStoreHackFlag;
    private bool stageEntryAttemptHack;
    private bool hasStageEntrySnapshot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.Unregister(this);
    }

    public void CaptureStageEntryState()
    {
        if (DialogueEventBinder.Instance == null)
        {
            hasStageEntrySnapshot = false;
            return;
        }

        SaveDataStruct tempData = new SaveDataStruct();

        // KSH 코드를 수정하지 않고 공개된 SaveDataTo를 이용해
        // 현재 상태만 읽어옵니다.
        DialogueEventBinder.Instance.SaveDataTo(tempData);

        stageEntryFlags = tempData.flags != null
            ? new HashSet<string>(tempData.flags)
            : new HashSet<string>();

        stageEntryStoreHackFlag = tempData.storeHackFlag;
        stageEntryAttemptHack = tempData.attemptHack;
        hasStageEntrySnapshot = true;

        // 실제 Save 시 KSH가 먼저 현재값을 쓴 뒤
        // 이 Proxy가 마지막에 입장값으로 덮어쓰도록 등록 순서를 뒤로 보냅니다.
        if (SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.Unregister(this);
            SaveSlotManager.Instance.Register(this);
        }
    }

    public void SaveDataTo(SaveDataStruct data)
    {
        if (!hasStageEntrySnapshot)
            return;

        if (!SceneController.IsStageScene(SceneController.Instance.curScene))
            return;

        data.flags = new HashSet<string>(stageEntryFlags);
        data.storeHackFlag = stageEntryStoreHackFlag;
        data.attemptHack = stageEntryAttemptHack;
    }

    public void LoadDataFrom(SaveDataStruct data)
    {
        stageEntryFlags = data.flags != null
            ? new HashSet<string>(data.flags)
            : new HashSet<string>();

        stageEntryStoreHackFlag = data.storeHackFlag;
        stageEntryAttemptHack = data.attemptHack;
        hasStageEntrySnapshot = true;
    }
}