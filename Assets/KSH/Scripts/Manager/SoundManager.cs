using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sound_BGM
{
    Game
}

// TODO: 이후 ScriptableObject 등으로 관리
public enum Sound_SFX
{
    Player_Move,
    Player_GunShoot,
    Player_SwordAttack,
    Player_Hit,
    Enemy_Dead
    /*Player_Dead,
    Enemy_Attack,
    Enemy_Hit,
    Enemy_Dead,
    ButtonClick*/
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

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
        
        InitializeAudioSources();
        InitDictionary();
    }
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips; 
    [SerializeField] private AudioClip[] sfxClips; // TODO: 리소스 추가 후 sfx clip을 Decompress On Load로 설정, enum 순서에 맞게 배치
    
    private AudioSource bgmSource; // BGM 재생 AudioSource
    private List<AudioSource> sfxSources = new List<AudioSource>(); // SFX 재생 AudioSource
    private AudioSource playerMoveSource; // 플레이어 이동 전용 오디오

    private Dictionary<Sound_BGM, AudioClip> bgmDict; // BGM Dictionary
    private Dictionary<Sound_SFX, AudioClip> sfxDict; // UI SFX Dictionary

    // 볼륨 설정 (0.0 ~ 1.0)
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    
    private int maxSfxSources = 10; // 동시에 재생 가능한 효과음 수
    private bool isFading = false; // 페이드 효과 진행 여부
    
    #region Init 관련 메서드
    private void InitializeAudioSources() // 오디오소스 초기화
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;
        }
        
        if (sfxSources.Count == 0)
        {
            for (int i = 0; i < maxSfxSources; i++)
            {
                AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
                sfxSources.Add(sfxSource);
            }
        }

        if (playerMoveSource == null)
        {
            playerMoveSource = gameObject.AddComponent<AudioSource>();
            playerMoveSource.loop = false;
            playerMoveSource.playOnAwake = false;
            playerMoveSource.volume = sfxVolume;
        }
    }
    
    private void InitDictionary() // Dictionary 초기화
    {
        bgmDict = new Dictionary<Sound_BGM, AudioClip>();
        for (int i = 0; i < bgmClips.Length; i++)
            bgmDict[(Sound_BGM)i] = bgmClips[i];
        
        sfxDict = new Dictionary<Sound_SFX, AudioClip>();
        for (int i = 0; i < sfxClips.Length; i++)
            sfxDict[(Sound_SFX)i] = sfxClips[i];
    }
    
    #endregion
    
    #region 배경음 (BGM) 메서드
    
    public void PlayBGM(Sound_BGM clipName, bool fade = false, float fadeTime = 1f)
    {
        if (!bgmDict.ContainsKey(clipName)) return;
        
        PlayBGM(bgmDict[clipName], fade, fadeTime);
    }
    
    // 오디오 클립으로 배경음을 재생
    public void PlayBGM(AudioClip clip, bool fade = false, float fadeTime = 1f)
    {
        if (clip == null) return;
        if (bgmSource == null) InitializeAudioSources(); // 초기화 안됐을 경우 다시 초기화
        if (bgmSource.clip == clip && bgmSource.isPlaying) return; // 중복 재생 막기
        
        if (fade && !isFading)
        {
            StartCoroutine(FadeBGM(clip, fadeTime)); // TODO: 코루틴 객체 참조 변수 추가 -> 재생 중인지 확인하기
        }
        else
        {
            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
    }
    
    public void StopBGM(bool fade = false, float fadeTime = 1f) // 배경음 정지
    {
        if (bgmSource == null || !bgmSource.isPlaying) return;
        
        if (fade && !isFading)
        {
            StartCoroutine(FadeOutBGM(fadeTime));
        }
        else
        {
            bgmSource.Stop();
        }
    }
    
    public void SetBGMVolume(float volume) // 배경음 볼륨 설정
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource == null) InitializeAudioSources();
        
        bgmSource.volume = bgmVolume;
    }
    
    #endregion
    
    #region 효과음 (SFX) 메서드
    
    public AudioSource PlaySFX(Sound_SFX clipName)
    {
        if (!sfxDict.ContainsKey(clipName)) return null;
        
        return PlaySFX(sfxDict[clipName]);
    }
    
    // 오디오 클립으로 효과음을 재생
    public AudioSource PlaySFX(AudioClip clip)
    {
        if (clip == null) return null;
        // if (sfxSources == null || sfxSources.Count == 0) InitializeAudioSources(); // 초기화
    
        // 사용 가능한 효과음 소스 찾기
        AudioSource sfxSource = null;
        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
            {
                sfxSource = source;
                break;
            }
        }
        
        // 모든 소스가 사용 중이면 첫 번째 소스 재사용
        if (sfxSource == null && sfxSources.Count > 0)
        {
            sfxSource = sfxSources[0]; // TODO: 코루틴 충돌 위험성 존재
        }
        
        sfxSource.clip = clip;
        sfxSource.volume = sfxVolume;
        sfxSource.Play();
        
        return sfxSource;
    }

    // 특정 이름의 효과음을 모두 페이드아웃
    public void FadeOutSFXByName(Sound_SFX clipName, float fadeTime = 0.5f)
    {
        foreach (var source in sfxSources)
        {
            if (source.isPlaying && source.clip != null && 
                source.clip == sfxDict.GetValueOrDefault(clipName))
            {
                StartCoroutine(FadeOutSFXCoroutine(source, fadeTime));
            }
        }
    }
    
    // 효과음 페이드아웃 코루틴
    private IEnumerator FadeOutSFXCoroutine(AudioSource sfxSource, float fadeTime)
    {
        float startVolume = sfxSource.volume;
        float time = 0;
    
        while (time < fadeTime)
        {
            sfxSource.volume = Mathf.Lerp(startVolume, 0, time / fadeTime);
            time += Time.deltaTime;
            yield return null;
        }
    
        sfxSource.Stop();
        sfxSource.volume = sfxVolume;
    }
    
    public void StopAllSFX() // 효과음 전체 정지
    {
        foreach (var source in sfxSources)
        {
            source.Stop();
        }
    }
    
    public void SetSFXVolume(float volume) // 효과음 볼륨 설정
    {
        sfxVolume = Mathf.Clamp01(volume);
        foreach (var source in sfxSources)
        {
            source.volume = sfxVolume;
        }
    }
    
    #endregion

    #region // 플레이어 이동 오디오, 중복 방지를 위해 따로 처리

    public void PlayPlayerMoveSound()
    {
        if (playerMoveSource.isPlaying)
            return;
        
        playerMoveSource.clip = sfxDict[Sound_SFX.Player_Move];
        playerMoveSource.volume = sfxVolume;
        playerMoveSource.Play();
    }

    public void StopyPlayerMoveSound()
    {
        playerMoveSource.Stop();
    }

    #endregion
    
    #region 전체 사운드 제어
    
    public void StopAllSounds() // 전체 사운드 정지
    {
        StopBGM();
        StopAllSFX();
    }

    public void SetAllSoundVolume(float bgmVolume, float sfxVolume) // 전체 사운드 볼륨 조절
    {
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
    }
    
    #endregion
    
    #region 페이드 효과
    
    // 배경음을 페이드 인/아웃하며 전환
    private IEnumerator FadeBGM(AudioClip newClip, float fadeTime)
    {
        isFading = true;
        
        // 현재 재생 중인 배경음이 있으면 페이드 아웃
        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            float time = 0;
            
            while (time < fadeTime / 2)
            {
                bgmSource.volume = Mathf.Lerp(startVolume, 0, time / (fadeTime / 2));
                time += Time.deltaTime;
                yield return null;
            }
            
            bgmSource.Stop();
        }
        
        // 새 클립 설정 후 페이드 인
        bgmSource.clip = newClip;
        bgmSource.volume = 0;
        bgmSource.Play();
        
        float fadeInTime = 0;
        while (fadeInTime < fadeTime / 2)
        {
            bgmSource.volume = Mathf.Lerp(0, bgmVolume, fadeInTime / (fadeTime / 2));
            fadeInTime += Time.deltaTime;
            yield return null;
        }
        
        bgmSource.volume = bgmVolume;
        isFading = false;
    }
    
    // 배경음을 페이드 아웃
    private IEnumerator FadeOutBGM(float fadeTime)
    {
        isFading = true;
        
        float startVolume = bgmSource.volume;
        float time = 0;
        
        while (time < fadeTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, time / fadeTime);
            time += Time.deltaTime;
            yield return null;
        }
        
        bgmSource.Stop();
        bgmSource.volume = bgmVolume;
        isFading = false;
    }
    
    #endregion
}
