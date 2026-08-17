using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class GameTime
{
    private static float worldTimeScale = 1f;
    public static float WorldTimeScale => worldTimeScale;
    
    public static float WorldDeltaTime =>
        Time.deltaTime * WorldTimeScale;

    public static void SetTimeScale(float timeScale)
    {
        worldTimeScale = timeScale;
    }
}

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<GameManager>();

            return instance;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        SceneController.Instance.OnSceneChanged += RegisterSectorManagerEvent;
    }

    [SerializeField] private GameObject player;
    public GameObject Player
    {
        get
        {
            if (player == null)
                player = GameObject.FindWithTag("Player");

            return player;
        }
    }

    private int credit = 0; //게임 내 재화
    public int Credit
    {
        get => credit;
        set
        {
            credit = value;
            OnCreditChanged?.Invoke(credit);
        }
    }

    public event UnityAction<int> OnCreditChanged;

    [SerializeField] private ProtocolCard.Protocol curProtocol = ProtocolCard.Protocol.None;
    public ProtocolCard.Protocol CurProtocol => curProtocol;

    public event UnityAction OnProtocolChanged;

    public void SetProtocol(ProtocolCard.Protocol protocol)
    {
        curProtocol = protocol;
        OnProtocolChanged?.Invoke();
    }

    /// <summary>
    /// TO DO : SectorManager 의 동일명 함수로 수정하기
    /// </summary>
    public void SectionClear() // 맵 내의 적 전부 처리 시 실행
    {
        Debug.Log("현재 GameManager 에서 실행됐습니다. SectorManager 의 SectionClear 로 바꿔주세요");
    }

    public void SectionFail()
    {
        Debug.Log("현재 GameManager 에서 실행됐습니다. SectorManager 의 SectionFail 로 바꿔주세요");
    }

    Dictionary<SectorSO.SectorType, bool> clearedSector = new Dictionary<SectorSO.SectorType, bool>();
    public Dictionary<SectorSO.SectorType, bool> ClearedSector => clearedSector;

    void RegisterSectorManagerEvent(SceneController.Scene curScene)
    {
        //로비, 맵분기 또는 이벤트 맵의 경우 제외
        if (curScene == SceneController.Scene.Scene_Lobby || curScene == SceneController.Scene.Scene_Map)
            return;

        SectorManager.Instance.OnSectorClear += OnSectorClear;
        SectorManager.Instance.OnSectorFail += OnSectorFailed;
    }

    public void UnRegisterSectorManagerEvent()
    {
        if (SectorManager.Instance == null)
            return;

        SectorManager.Instance.OnSectorClear -= OnSectorClear;
        SectorManager.Instance.OnSectorFail -= OnSectorFailed;
    }

    /// <summary>
    /// SectorManager 로 부터 Sector 의 클리어 여부를 받습니다
    /// </summary>
    public void OnSectorClear(SectorSO.SectorType sectorType)
    {
        if (!clearedSector.ContainsKey(sectorType))
            clearedSector.Add(sectorType, false);

        clearedSector[sectorType] = true;
    }

    /// <summary>
    /// SectorManager 로 부터 Sector 의 클리어 여부를 받습니다
    /// </summary>
    public void OnSectorFailed(SectorSO.SectorType sectorType)
    {
        if (!clearedSector.ContainsKey(sectorType))
            clearedSector.Add(sectorType, false);

        clearedSector[sectorType] = false;
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
        player = GameObject.FindWithTag("Player");
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
        GameTime.SetTimeScale(shouldPause ? 0f : GameTime.WorldTimeScale);
        OnPauseGame?.Invoke(shouldPause);
    }
    
    /*public void Pause(bool isPause)
    {
        GameTime.SetTimeScale(isPause ? 0f : 1f);
        OnPauseGame?.Invoke(isPause);
    }*/
}
