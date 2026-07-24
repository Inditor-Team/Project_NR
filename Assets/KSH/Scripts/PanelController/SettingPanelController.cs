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
        //Time.timeScale = 0f; // 게임 정지
        GameManager.Instance.Pause(true);
        SoundManager.Instance.PlaySFX(Sound_SFX.UIOpen);

        UIManager.Instance.Show(settingPanel);
    }

    public void HideSettingPanel()
    {
        UIManager.Instance.Hide(settingPanel);
        SoundManager.Instance.PlaySFX(Sound_SFX.UICancel);
        //Time.timeScale = 1f; // 게임 재개
        GameManager.Instance.Pause(false);
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
    
    // 게임 종료 버튼
    public void OnClickExitGame()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.UICancel);
        UIManager.Instance.SetMsgPanel("게임을 종료하시겠습니까?", GameManager.Instance.ExitGame);
    }
}
