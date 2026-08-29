using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneManager 대신 사용합니다
/// </summary>
public class SceneController : MonoBehaviour
{
    static SceneController instance;
    public static SceneController Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<SceneController>();

            DontDestroyOnLoad(instance);

            return instance;
        }

    }

    private void Start()
    {
        StartBGM();
    }

    public enum Scene { None, Scene_Lobby, Scene_Map, 
        Scene_NormalA, Scene_NormalB, Scene_NormalC, Scene_NormalD,
        Scene_HardA, Scene_HardB, 
        Scene_EventA, Scene_EventB, Scene_Shop,
        Scene_MiddleBoss,
        Count }
    public Scene prevScene = Scene.None;
    public Scene curScene = Scene.None;
    public event UnityAction<Scene> OnSceneChanged;

    public void ChangeScene(Scene sceneName)
    {
        if (curScene != Scene.None)
            prevScene = curScene;

        SceneManager.LoadScene(sceneName.ToString());
        GameManager.Instance.ForcedRelease(); // Pause(false);
        
        curScene = sceneName;
        StartBGM();
        FindPlayer();
        SoundManager.Instance.StopAllSFX(); // 재생 중인 효과음 전체 종료

        OnSceneChanged?.Invoke(curScene);
    }

    public void StartBGM()
    {
        switch (curScene)
        {
            case Scene.Scene_Lobby:
                SoundManager.Instance.PlayBGM(Sound_BGM.Lobby);
                break;
            case Scene.Scene_Map:
                SoundManager.Instance.PlayBGM(Sound_BGM.Map);
                break;
            case Scene.Scene_NormalA:
                SoundManager.Instance.PlayBGM(Sound_BGM.Stage1);
                break;
        }
    }

    void FindPlayer()
    {
        if (curScene == Scene.Scene_Map)
            return;

        GameManager.Instance.FindPlayer();
    }
}
