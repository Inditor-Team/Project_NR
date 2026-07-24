using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneManager 대신 해당 클래스 사용
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

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

        StartBGM();
    }

    public enum Scene { None, Scene_Lobby, Scene_Map, Scene_Stage1, Count }
    public Scene prevScene = Scene.None;
    public Scene curScene = Scene.None;

    public void ChangeScene(Scene sceneName)
    {
        if (curScene != Scene.None)
            prevScene = curScene;

        SceneManager.LoadScene(sceneName.ToString());
        GameManager.Instance.Pause(false);
        
        curScene = sceneName;
        StartBGM();
        FindPlayer();
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
            case Scene.Scene_Stage1:
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
