using System.Security.Cryptography.X509Certificates;
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

    private GameObject player;
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
}
