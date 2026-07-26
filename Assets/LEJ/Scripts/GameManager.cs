using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
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
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
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

    [SerializeField] private ProtocolCard.Protocol curProtocol = ProtocolCard.Protocol.None;
    public ProtocolCard.Protocol CurProtocol => curProtocol;
    public void SetProtocol(ProtocolCard.Protocol protocol)
    {
        curProtocol = protocol;
    }

    public UnityAction OnSectionClear;
    bool isSetionOneClear = false;
    public bool IsSetionOneClear => isSetionOneClear;
    public UnityAction OnSectionFail;

    public void SectionClear() // 맵 내의 적 전부 처리 시 실행
    {
        Debug.Log("Section Clear !");
        OnSectionClear?.Invoke();
        isSetionOneClear = true;
    }

    public void SectionFail()
    {
        Debug.Log("Section Fail!");
        OnSectionFail?.Invoke();
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
    public UnityAction<bool> OnPauseGame;
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
        GameTime.SetTimeScale(shouldPause ? 0f : 1f); // 1f 가 아닌 기존에 설정된 값으로?
        OnPauseGame?.Invoke(shouldPause);
    }
    
    /*public void Pause(bool isPause)
    {
        GameTime.SetTimeScale(isPause ? 0f : 1f);
        OnPauseGame?.Invoke(isPause);
    }*/
}
