using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanelController : MonoBehaviour
{
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Slider BGMVolume;
    [SerializeField] private Slider SFXVolume;
    
    [SerializeField] private GameObject BGMOnImage;
    [SerializeField] private GameObject BGMOffImage;
    
    [SerializeField] private GameObject SFXOnImage;
    [SerializeField] private GameObject SFXOffImage;

    private bool isBGMOff;
    private bool isSFXOff;

    private void Start()
    {
        isBGMOff = SoundManager.Instance.bgmVolume == 0;
        isSFXOff = SoundManager.Instance.sfxVolume == 0;
        
        BGMOffImage.SetActive(isBGMOff);
        SFXOffImage.SetActive(isSFXOff);

        BGMVolume.value = SoundManager.Instance.bgmVolume;
        SFXVolume.value = SoundManager.Instance.sfxVolume;
        
        BGMVolume.onValueChanged.AddListener(OnBGMValueChanged);
        SFXVolume.onValueChanged.AddListener(OnSFXValueChanged);
    }

    public void ShowSettingPanel()
    {
        if (settingPanel == null) return;
        UIManager.Instance.Show(settingPanel);
    }

    public void HideSettingPanel()
    {
        UIManager.Instance.Hide(settingPanel);
    }

    private void OnBGMValueChanged(float value) // 변경될 때마다 적용
    {
        SoundManager.Instance.SetBGMVolume(value);

        if (value == 0) // Off
        {
            if (!isBGMOff)
            {
                isBGMOff = ChangeOnOffImage(isBGMOff, BGMOffImage);
            }
        }
        else // On
        {
            if (isBGMOff)
            {
                isBGMOff = ChangeOnOffImage(isBGMOff, BGMOffImage);
            }
        }
    }
    
    private void OnSFXValueChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
        
        if (value == 0) // Off
        {
            if (!isSFXOff)
            {
                isSFXOff = ChangeOnOffImage(isSFXOff, SFXOffImage);
            }
        }
        else // On
        {
            if (isSFXOff)
            {
                isSFXOff = ChangeOnOffImage(isSFXOff, SFXOffImage);
            }
        }
    }

    private bool ChangeOnOffImage(bool isOff, GameObject img)
    {
        isOff = !isOff;
        img.SetActive(isOff);
        return isOff;
    }
}
