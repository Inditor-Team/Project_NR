using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// SceneManager 대신 사용합니다
/// </summary>
public class SceneController : MonoBehaviour, ISaveable
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

        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.Unregister(this);
    }

    // TODO: 스테이지별 숫자 적기
    public enum Scene { None, Lobby, Map, 
        NormalA, NormalB, NormalC, NormalD,
        HardA, HardB, 
        EventA, EventB, Shop,
        Boss,
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
            case Scene.Lobby:
                SoundManager.Instance.PlayBGM(Sound_BGM.Lobby);
                break;
            case Scene.Map:
                SoundManager.Instance.PlayBGM(Sound_BGM.Map);
                break;
            case Scene.NormalA:
                SoundManager.Instance.PlayBGM(Sound_BGM.Stage1);
                break;
        }
    }

    void FindPlayer()
    {
        if (curScene == Scene.Map)
            return;

        GameManager.Instance.FindPlayer();
    }
    
    // 세이브 관련
    public void SaveDataTo(SaveDataStruct data)
    {
        data.sectorName = curScene;
    }

    public void LoadDataFrom(SaveDataStruct data)
    {
        ChangeScene(data.sectorName); // 씬 전환
    }
}
