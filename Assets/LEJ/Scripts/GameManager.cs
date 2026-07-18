using UnityEngine;

/// <summary>
/// ���� �� �ð��� �ӵ��� ������ �޴´ٸ� �Ʒ� Ŭ���� ��� (�÷��̾�, UI ����)
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

    public void SectionClear() // 맵 내의 적 전부 처리 시 실행
    {
        Debug.Log("Section Clear !");
    }
}
