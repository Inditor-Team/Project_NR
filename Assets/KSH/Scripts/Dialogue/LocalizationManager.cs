using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public enum Language { KO, EN }

[Serializable]
public class LocalizationEntry { public string key; public string value; }

[Serializable]
public class LocalizationTable { public List<LocalizationEntry> entries; }

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public event Action LanguageChanged;

    private Dictionary<string, string> currentTable;
    private Language currentLanguage = Language.KO;
    public Language CurLang => currentLanguage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);

        LoadLanguage(currentLanguage);

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        LocalizationSettings.InitializationOperation.Completed +=
            _ => OnLocaleChanged(LocalizationSettings.SelectedLocale);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    public void SetLanguage(string code) // "ko", "en"
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        if (locale == null) { Debug.LogError($"Locale 없음: {code}"); return; }

        LocalizationSettings.SelectedLocale = locale;
        PlayerPrefs.SetString("selected-locale", code);
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (locale == null) return;

        Language lang = locale.Identifier.Code.StartsWith("en") ? Language.EN : Language.KO;
        if (lang == currentLanguage) return;

        LoadLanguage(lang);
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

        LanguageChanged?.Invoke();
    }

    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (currentTable.TryGetValue(key, out string value)) return value;

        Debug.LogWarning($"로컬라이징 키 없음: {key}");
        return key; // 키를 못 찾으면 키를 그냥 리턴
    }

    public string GetFormat(string key, params object[] args)
    {
        string template = Get(key);
        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            Debug.LogError($"로컬라이징 포맷 실패 : key={key}, template={template}");
            return template;
        }
    }
}