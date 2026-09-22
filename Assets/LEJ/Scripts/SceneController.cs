using System;
using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
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

        //SkipOpening PlayerPrefs 가 설정된 적 있다면
        if (PlayerPrefs.GetInt(SkipOpeningKey, -1) > -1)
            skipOpeningBttn.SetActive(true);
        else
            skipOpeningBttn.SetActive(false);

    }

    private void OnDestroy()
    {
        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.Unregister(this);
    }

    public string SkipOpeningKey = "SkipOpening";
    [SerializeField] GameObject skipOpeningBttn;

    // TODO: 스테이지별 숫자 적기
    public enum Scene { None, Opening, Lobby, Map, 
        NormalA, NormalB, NormalC, NormalD,
        HardA, HardB, 
        EventA, EventB, ShopA, ShopB,
        Boss,
        Count }
    public Scene prevScene = Scene.None;
    public Scene curScene = Scene.None;
    public event UnityAction<Scene> OnSceneChanged;

    public void ChangeScene(Scene sceneName)
    {
        if (curScene != Scene.None)
            prevScene = curScene;

        //로비에 처음 들어오는 거라면 스킵 가능하게 PlayerPrefs 설정
        if (sceneName == Scene.Lobby && PlayerPrefs.GetInt(SkipOpeningKey, -1) < 0)
        {
            PlayerPrefs.SetInt(SkipOpeningKey, 0);
        }

        StartCoroutine(ChangeSceneWithFadeIn(sceneName));
    }

    Coroutine changeSceneRoutine;

    IEnumerator ChangeSceneWithFadeIn(Scene sceneName)
    {
        //섹터가 끝나고 이동 되는 Map 씬인지만 체크, 클리어 된 섹터인지 체크
        if (sceneName == Scene.Map && GameManager.Instance.ClearedSector[curScene])
            GameManager.Instance.OnChangeSceneWhenSectorCleared();

        var fade = GameObject.FindGameObjectWithTag("Fade");

        if (fade != null)
            fade.GetComponent<Animator>().Play("FadeIn");
        
        yield return new WaitForSeconds(0.5f);

        if (sceneName == Scene.ShopA || sceneName == Scene.ShopB)
            SceneManager.LoadScene("Store");
        else
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
            case Scene.Opening:
                SoundManager.Instance.PlayBGM(Sound_BGM.Intro, true, 3); //인트로 BGM 추가
                break;
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
