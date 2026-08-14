using System;
using System.Collections.Generic;
using UnityEngine;

public enum Language { KO, EN }

[Serializable]
public class LocalizationEntry { public string key; public string value; }

[Serializable]
public class LocalizationTable { public List<LocalizationEntry> entries; }

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private Dictionary<string, string> currentTable;
    private Language currentLanguage = Language.KO;

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
        
        LoadLanguage(currentLanguage);
    }

    public void LoadLanguage(Language lang)
    {
        currentLanguage = lang;
        string path = "Dialogues/Localization_" + lang;
        TextAsset json = Resources.Load<TextAsset>(path);

        currentTable = new Dictionary<string, string>();
        if (json == null) { Debug.LogError($"로컬라이징 파일 없음: {path}"); return; }

        LocalizationTable table = JsonUtility.FromJson<LocalizationTable>(json.text);
        foreach (var e in table.entries)
            currentTable[e.key] = e.value;
    }

    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (currentTable.TryGetValue(key, out string value)) return value;

        Debug.LogWarning($"로컬라이징 키 없음: {key}");
        return key; // 키를 못 찾으면 키를 그냥 리턴
    }
}
