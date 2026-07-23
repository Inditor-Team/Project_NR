using UnityEngine;
using DG.Tweening;
using System;
using TMPro;

public class UIManager : MonoBehaviour
{ 
    public static UIManager Instance { get; private set; }
    private bool isMove = false;

    [SerializeField] private MassagePanelController msgController;

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
    }
    
    public void Show(GameObject panel)
    {
        if (!isMove)
        {
            isMove = true;
            
            panel.SetActive(true);
            panel.transform.localPosition = new Vector3(0, -300, 0);
            
            panel.transform.DOLocalMoveY(panel.transform.localPosition.y + 300f, 0.5f).SetEase(Ease.OutCubic).OnComplete(() => 
            {
                isMove = false; 
            });
        }
    }

    public void Hide(GameObject panel)
    {
        if (!isMove)
        {
            isMove = true;
            panel.transform.DOLocalMoveY(panel.transform.localPosition.y - 1000f, 0.5f).SetEase(Ease.OutCubic).OnComplete(() => 
            {
                panel.SetActive(false);
                isMove = false; 
            });
        }
    }

    public void SetMsgPanel(string text, Action func)
    {
        msgController.SetMessagePanel(text, func);
    }
}
