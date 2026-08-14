using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 대화 데이터 클래스
[Serializable]
public class DialogueOptionStruct
{
    public string text;
    public string nextId;
}

[Serializable]
public class DialogueStruct
{
    public string id;          // 대화 고유 ID, 혹시 모를 세이브용
    public string npcId;       // npc 종류
    public string name;
    public string text;
    public string nextId;
    public DialogueOptionStruct[] options;
    public string phase;       // 단계 (tutorial, store, eventroom etc...)
}

[Serializable]
public class DialogueData // 전체 데이터 클래스
{
    public DialogueData()
    {
        dialogues = new List<DialogueStruct>();
    }
    public List<DialogueStruct> dialogues;
}

public class BubbleController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text bubbleText;
    [SerializeField] private Image clickIndicator;
    [SerializeField] private GameObject chatWindowObject; // 대화 종료용 이벤트 (예시 플레이어 일시 정지 해제 등)

    [Header("옵션")] 
    [SerializeField] private GameObject[] optionsObject;
    [SerializeField] private TMP_Text[] optionsText;
    
    private Coroutine typingCoroutine;
    private Coroutine clickCoroutine;
    private string inputText;
    
    public delegate void OnComplete();
    public OnComplete onComplete; // 혹시 말풍선 종료되었을 때 추가 처리가 필요하다면 이쪽
    
    private DialogueStruct currentEntry;
    private DialogueDataLoad dialogueDataLoad;
    
    private void Awake()
    {
        chatWindowObject.SetActive(false); // 일단 비활성화로 시작
    }
    
    private void Start()
    {
        dialogueDataLoad = new DialogueDataLoad(this);
    }
    
    public void PlayDialogue(DialogueStruct entry)
    {
        currentEntry = entry;
        inputText = LocalizationManager.Instance.Get(entry.text);
        HideAllOptions();
        
        // 클릭 인디케이터 활성화
        var clickIndicatorColor = clickIndicator.color;
        clickIndicatorColor.a = 1;
        clickIndicator.color = clickIndicatorColor;
        
        // 이전 타이핑 코루틴 중단
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // 새 타이핑 코루틴 시작
        typingCoroutine = StartCoroutine(TypingEffectCoroutine(inputText));
    }
    
    // 타이핑 완료 후 호출
    private void OnTypingComplete()
    {
        if (currentEntry.options != null && currentEntry.options.Length > 0)
            ShowOptions(currentEntry.options); // 클릭 인디케이터 대신 선택지 표시
        else
            clickCoroutine = StartCoroutine(ClickIndicatorCoroutine());
    }

    // 대화창 클릭 시, 옵션 없는 경우만 실행
    private void OnAdvanceClicked()
    {
        if (string.IsNullOrEmpty(currentEntry.nextId))
        {
            HideWindow();
            return;
        }
        dialogueDataLoad.RequestNext(currentEntry.nextId); // 다음 텍스트 불러오기
    }
    
    private void ShowOptions(DialogueOptionStruct[] options)
    {
        for (int i = 0; i < optionsObject.Length; i++)
        {
            bool active = i < options.Length;
            optionsObject[i].SetActive(active);

            if (!active) continue;

            optionsText[i].text = LocalizationManager.Instance.Get(options[i].text);
        
            string capturedNextId = options[i].nextId;
            Button btn = optionsObject[i].GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnOptionClicked(capturedNextId));
        }
    }

    private void HideAllOptions()
    {
        foreach (var obj in optionsObject)
            obj.SetActive(false);
    }

    // 옵션 클릭 시
    private void OnOptionClicked(string nextId)
    {
        if (string.IsNullOrEmpty(nextId))
        {
            HideWindow();
            return;
        }
        dialogueDataLoad.RequestNext(nextId);
    }
    
    #region Show and Hide
    
    // 대화창 표시
    public void ShowWindow()
    {
        if(!chatWindowObject.activeSelf)
            chatWindowObject.SetActive(true);
    }
    
    // 대화창 숨기기
    public void HideWindow()
    {
        chatWindowObject.SetActive(false);
        HideAllOptions();
        onComplete?.Invoke(); // 콜백 호출
        
        // 진행 중인 모든 코루틴 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        if (clickCoroutine != null)
        {
            StopCoroutine(clickCoroutine);
            clickCoroutine = null;
        }
    }

    public bool IsBubbleWindowOpen()
    {
        return chatWindowObject.activeSelf;
    }

    #endregion

    // id로 대화 시작
    public void StartDialogueById(string id)
    {
        dialogueDataLoad.RequestNext(id);
    }
    
    //텍스트 타이핑효과 코루틴
    private IEnumerator TypingEffectCoroutine(string text)
    {
        StringBuilder strText = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            strText.Append(text[i]);
            bubbleText.text = strText.ToString();
            yield return new WaitForSeconds(0.05f);
            // TODO: 타이핑 효과음 추가
        }
        
        typingCoroutine = null;
        OnTypingComplete();
    }
    
    private IEnumerator ClickIndicatorCoroutine()
    {
        bool flag = true;
        var clickIndicatorColor = clickIndicator.color;
        while (true)
        {
            clickIndicatorColor.a = flag? 0:1;
            flag = !flag;
            clickIndicator.color = clickIndicatorColor;
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    //대화창 클릭 시 호출 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!chatWindowObject.activeSelf) // 실제론 대화창이 off 상태인데 호출되는 상황 방지
            return;
        
        if (typingCoroutine != null) // 전체 출력
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            if (bubbleText != null) bubbleText.text = inputText;
            if (clickIndicator != null) clickCoroutine = StartCoroutine(ClickIndicatorCoroutine());
            OnTypingComplete();
        }
        else
        {
            if (clickCoroutine != null)
            {
                StopCoroutine(clickCoroutine);
                clickCoroutine = null;
            }

            if (currentEntry.options == null || currentEntry.options.Length == 0)
                OnAdvanceClicked();   // 옵션 없는 노드만 클릭으로 진행
        }
    }
}
