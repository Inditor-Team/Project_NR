using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueDataLoad
{
    // 대화 데이터 관련
    private DialogueData database;
    private Dictionary<string, DialogueStruct> dialogueDict;
    private Dictionary<string, List<DialogueStruct>> phaseDialogues;

    private BubbleController chatWindow;
    private string dialogueFileName;
    
    public DialogueDataLoad(BubbleController chatWindow, string dialogueFileName = "DialogueData.json")
    {
        this.chatWindow = chatWindow;
        this.dialogueFileName = dialogueFileName;
        
        LoadDialogueData();
        OrganizeDialogues();
    }
    
    // 대화 데이터베이스 로드
    private void LoadDialogueData()
    {
        string filePath = "Dialogues/" + dialogueFileName.Replace(".json", "");
        TextAsset jsonFile = Resources.Load<TextAsset>(filePath);
        
        if (jsonFile == null)
        {
            Debug.LogError($"경로 오류로 인한 대화 데이터 로딩 실패 : {filePath}");
            database = new DialogueData { dialogues = new List<DialogueStruct>() };
            return;
        }
        
        try {
            database = JsonUtility.FromJson<DialogueData>(jsonFile.text);
            
            dialogueDict = new Dictionary<string, DialogueStruct>(); // 대화 사전 초기화
            foreach (DialogueStruct entry in database.dialogues)
                dialogueDict[entry.id] = entry;
        }
        catch (Exception e) {
            Debug.LogError($"JSON 파싱 오류: {e.Message}");
            database = new DialogueData { dialogues = new List<DialogueStruct>() };
        }
    }
    
    // 대화를 단계별로 분류
    private void OrganizeDialogues()
    {
        phaseDialogues = new Dictionary<string, List<DialogueStruct>>();
        
        foreach (DialogueStruct entry in database.dialogues)
        {
            // 단계별 분류
            if (!phaseDialogues.ContainsKey(entry.phase))
            {
                phaseDialogues[entry.phase] = new List<DialogueStruct>();
            }
            phaseDialogues[entry.phase].Add(entry);
        }
    }
    
    public void RequestNext(string dialogueId, bool isEntryPoint = false)
    {
        if (!dialogueDict.TryGetValue(dialogueId, out DialogueStruct entry))
        {
            chatWindow.HideWindow(); // 방어코드
            return;
        }

        // 진입점 조건 체크, isEntryPoint일 때
        if (isEntryPoint && !string.IsNullOrEmpty(entry.entryConditionId))
        {
            bool blocked = DialogueEventDispatcher.CheckCondition(entry.entryConditionId);
            if (blocked && !string.IsNullOrEmpty(entry.entryRedirectId))
            {
                // 대화 노드 리다이렉트
                RequestNext(entry.entryRedirectId);
                return;
            }
        }

        // 표시 전에 노드 이벤트를 먼저 실행
        if (entry.onEnterEvents != null)
        {
            foreach (string cmd in entry.onEnterEvents)
                DialogueEventDispatcher.FireEvent(cmd);
        }

        // 실제 표시
        chatWindow.PlayDialogue(entry);
    }
    
    // 외부 호출용
    public void StartDialogueById(string dialogueId)
    {
        RequestNext(dialogueId, true);
    }
}