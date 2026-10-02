using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageDropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    [SerializeField] private TMP_Text displayLabel;

    private readonly List<string> fullNames = new List<string> { "한국어", "English" };
    private readonly string[] shortNames = { "K O", "E N" };
    private readonly string[] localeCodes = { "ko", "en" };
    
    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(fullNames);
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void Start()
    {
        LocalizationManager.Instance.LanguageChanged += SyncWithCurrent;
        SyncWithCurrent();
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged -= SyncWithCurrent;
    }

    private void OnLanguageChanged(int index)
    {
        LocalizationManager.Instance.SetLanguage(localeCodes[index]);
    }

    private void SyncWithCurrent()
    {
        int index = (int)LocalizationManager.Instance.CurLang;
        dropdown.SetValueWithoutNotify(index);
        displayLabel.text = shortNames[index];
    }
}