using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LanguageDropdown : MonoBehaviour
{
     private TMP_Dropdown dropdown;
    [SerializeField] private TMP_Text displayLabel; // 짧은 표시용 텍스트

    private readonly List<string> fullNames = new List<string> { "한국어", "English" };
    private readonly string[] shortNames = { "K O", "E N" };

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(fullNames);
        dropdown.onValueChanged.AddListener(OnLanguageChanged);

        RefreshDisplay(dropdown.value);
    }

    private void OnLanguageChanged(int index)
    {
        RefreshDisplay(index);
        switch (index) // 언어 변경
        {
            case 0:
                LocalizationManager.Instance.LoadLanguage(Language.KO);
                break;
            case 1:
                LocalizationManager.Instance.LoadLanguage(Language.EN);
                break;
        }
    }

    private void RefreshDisplay(int index)
    {
        displayLabel.text = shortNames[index];
    }

    // 외부에서 언어 변경
    public void SetLanguage(int index) // 0: 한국어, 1: 영어
    {
        dropdown.SetValueWithoutNotify(index);
        RefreshDisplay(index);
    }
}