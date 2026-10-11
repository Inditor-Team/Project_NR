using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static SectorSO;

public static class GameTime
{
    private static float worldTimeScale = 1f;
    public static float WorldTimeScale => worldTimeScale;
    
    public static float WorldDeltaTime =>
        Time.deltaTime * WorldTimeScale;
    
    // 임시 시간 변수
    private static float beforeWorldTimeScale = 1f; // 0이 아닌 시간을 저장해서 이전 타임 스케일로 돌아오도록 함
    public static float BeforeWorldTimeScale => beforeWorldTimeScale;

    public static void SetTimeScale(float timeScale)
    {
        if (timeScale > 0f) beforeWorldTimeScale = timeScale;
        worldTimeScale = timeScale;
    }
}

public class GameManager : MonoBehaviour, ISaveable
{
    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<GameManager>();

            DontDestroyOnLoad(instance);

            return instance;
        }
    }

    [SerializeField] private GameObject player;
    public GameObject Player
    {
        get
        {
            if (player == null)
                player = GameObject.FindWithTag("Player").transform.parent.gameObject; //model 의 부모 오브젝트로 되어있음

            return player;
        }
    }

    private float life = 5f;
    public float Life => life;

    private float baseMaxLife = 5f;
    public float BaseMaxLife => baseMaxLife;

    //스테이지 안에서 얻은 최대 체력 증가량(임시로 두고 클리어 시 실제 적용)
    private float tempMaxLife = 0f;

    private ItemSO tempItemCaching;
    public ItemSO TempItemCaching => tempItemCaching;
    public void SetTempItemCaching(ItemSO item)
    {
        tempItemCaching = item;
    }

    [SerializeField] private ProtocolCard.Protocol curProtocol = ProtocolCard.Protocol.None;
    public ProtocolCard.Protocol CurProtocol => curProtocol;
    public event UnityAction OnProtocolChanged;

    public void SetProtocol(ProtocolCard.Protocol protocol)
    {
        curProtocol = protocol;
        OnProtocolChanged?.Invoke();
    }

    Dictionary<SceneController.Scene, bool> clearedSector = new Dictionary<SceneController.Scene, bool>();
    public Dictionary<SceneController.Scene, bool> ClearedSector => clearedSector;

    void Awake()
    {
        ClearedSectorDicInit();
    }

    private void Start()
    {
        SaveSlotManager.Instance.Register(this);
    }
    
    private void OnDestroy()
    {
        SaveSlotManager.Instance.Unregister(this);
    }

    /// <summary>
    /// 섹터 클리어 여부를 저장하는 딕셔너리 초기화
    /// </summary>
    void ClearedSectorDicInit()
    {
        for (int i = 0; i < (int)SceneController.Scene.Count; i++)
            clearedSector.Add((SceneController.Scene)i, false);
    }

    public void RegisterSectorManagerEvent(SceneController.Scene curScene)
    {
        //로비, 맵분기 또는 이벤트 맵의 경우 제외
        if (curScene == SceneController.Scene.Lobby || curScene == SceneController.Scene.Map)
            return;

        SectorManager.Instance.OnSectorClear += OnSectorClear;
        SectorManager.Instance.OnSectorFail += OnSectorFailed;
    }

    public void UnRegisterSectorManagerEvent()
    {
        SectorManager.Instance.OnSectorClear -= OnSectorClear;
        SectorManager.Instance.OnSectorFail -= OnSectorFailed;
    }

    /// <summary>
    /// SectorManager 로 부터 Sector 의 클리어 여부를 받습니다
    /// </summary>
    public void OnSectorClear(SceneController.Scene sectorType)
    {
        clearedSector[sectorType] = true;

        Debug.Log($"{sectorType} 클리어 완료");
    }

    /// <summary>
    /// 새로운 스테이지 진입 시 임시로 저장할 최대체력 초기화
    /// </summary>
    public void BeginStage()
    {
        tempMaxLife = 0f;
    }

    /// <summary>
    /// SectorManager 로 부터 Sector 의 클리어 여부를 받습니다
    /// </summary>
    public void OnSectorFailed(SceneController.Scene sectorType)
    {
        clearedSector[sectorType] = false;

        //스테이지 클리어가 됐을 때만 최대체력 증가량 보존
        tempMaxLife = 0f;
    }

    /// <summary>
    /// 섹터 클리어가 됐을 때 씬 변경 시 남아있는 생명, 아이템을 이어받습니다
    /// </summary>
    public void OnChangeSceneWhenSectorCleared()
    {
        PlayerStat stat = player.GetComponent<PlayerController>().Stat;

        //클리어한 경우에만 현재 체력을 저장
        life = stat.StatDic[PlayerStat.Stat.Life];

        //영구 체력 역시 확정
        baseMaxLife += tempMaxLife;
        tempMaxLife = 0f;

        //사용 안 된 아이템을 다음 스테이지로 넘기기 위해 저장
        ItemSO item = player.GetComponent<PlayerInventory>().CurItem;
        tempItemCaching = item;
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 종료
        #else
        Application.Quit(); // 어플리케이션 종료
        #endif
    }

    public void FindPlayer()
    {
        GameObject playerGO = GameObject.FindWithTag("Player");

        if (playerGO != null)
            player = GameObject.FindWithTag("Player").transform.parent.gameObject;
    }

    private int pauseRequestCount = 0; // UI 창이 여러 개인 경우가 있으니 카운팅 형식으로 변경
    public bool IsPaused => pauseRequestCount > 0;
    public event UnityAction<bool> OnPauseGame;
    public void RequestPause()
    {
        pauseRequestCount++;
        ApplyPause();
    }

    public void ReleasePause()
    {
        pauseRequestCount = Mathf.Max(0, pauseRequestCount - 1);
        ApplyPause();
    }

    public void ForcedRelease() // UI 켜진 거 상관없이 강제 pause 종료, 씬 이동시 사용
    {
        pauseRequestCount = 0;
        ApplyPause();
    }

    private void ApplyPause()
    {
        bool shouldPause = pauseRequestCount > 0;
        GameTime.SetTimeScale(shouldPause ? 0f : GameTime.BeforeWorldTimeScale);
        OnPauseGame?.Invoke(shouldPause);
    }

    public void SaveDataTo(SaveDataStruct data)
    {
        data.protocol = curProtocol;
        data.clearedSector = new Dictionary<SceneController.Scene, bool>(clearedSector);

        //PlayerStat의 스테이지 진입 전 값을 저장
        data.statDic[PlayerStat.Stat.Life] = life;
        data.statDic[PlayerStat.Stat.MaxLife] = baseMaxLife;

        data.statDic[PlayerStat.Stat.None] = 1f;
    }

    public void LoadDataFrom(SaveDataStruct data)
    {
        SetProtocol(data.protocol);

        for (int i = 0; i < (int)SceneController.Scene.Count; i++)
            clearedSector[(SceneController.Scene)i] = false;

        if (data.clearedSector != null)
        {
            foreach (var sector in data.clearedSector)
                clearedSector[sector.Key] = sector.Value;
        }

        if (data.statDic != null)
        {
            if (data.statDic.TryGetValue(PlayerStat.Stat.Life, out float savedLife))
                life = savedLife;

            if (data.statDic.TryGetValue(PlayerStat.Stat.MaxLife, out float savedMaxLife))
                baseMaxLife = savedMaxLife;
        }

        tempMaxLife = 0f;
    }

    /// <summary>
    /// 플레이어의 최대 체력을 조절합니다
    /// </summary>
    /// <param name="value"></param>
    public void AddMaxLife(int value)
    {
        player.GetComponent<PlayerController>().Stat.AddStat(PlayerStat.Stat.MaxLife, value);

        if (SceneController.IsStageScene(SceneController.Instance.curScene))
            tempMaxLife += value;
        else
            baseMaxLife += value;
    }

    /// <summary>
    /// 플레이어 체력을 조절합니다
    /// 최대 체력 이상으로 가질 수 없습니다.
    /// </summary>
    /// <param name="value"></param>
    public void AddLife(int value)
    {
        player.GetComponent<PlayerController>().Stat.AddStat(PlayerStat.Stat.Life, value);
    }
}
