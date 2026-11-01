using UnityEngine;

// 게임 버전 관리
// 현재 버전은 Player Settings -> Version 값을 사용
public class VersionManager
{
    private const string LastVersionKey = "LastRunVersion";
    public static string CurrentVersion => Application.version; // Player Settings -> Version
    public static string PreviousVersion { get; private set; } = ""; // 이전 버전

    // 첫 씬 로드 전에 자동 실행
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        PreviousVersion = PlayerPrefs.GetString(LastVersionKey, "");

        PlayerPrefs.SetString(LastVersionKey, CurrentVersion);
        PlayerPrefs.Save();
        Debug.Log($"[VersionManager] Current: {CurrentVersion}, Previous: " +
                  (string.IsNullOrEmpty(PreviousVersion) ? "(none)" : PreviousVersion));
    }}
