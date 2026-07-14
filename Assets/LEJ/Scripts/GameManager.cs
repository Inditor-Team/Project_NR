using UnityEngine;

/// <summary>
/// 월드 내 시간의 속도에 영향을 받는다면 아래 클래스 사용 (플레이어, UI 제외)
/// </summary>
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
}
