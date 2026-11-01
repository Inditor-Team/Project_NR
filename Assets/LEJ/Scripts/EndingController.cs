using System.Collections;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

public class EndingController : MonoBehaviour
{
    [SerializeField] private WriteText writeText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("NPC")]
    [SerializeField] private Transform npc;
    [SerializeField] private NpcInteractable npcInteractable;
    [SerializeField] private Transform middleBoss;

    [Header("Ending")]
    [SerializeField] private float returnDelay = 2f;

    private readonly string[] dialogues =
    {
        "나도 너와 같았어. 오르비스의 명령에 따르는 개였지. 진실을 알기 전까지는.",
        "Z. 너 자신을 위해 움직인다는 게 무엇인지 생각해 봐."
    };
    
    private readonly string[] dialoguesEN =
    {
        "I was just like you. A dog following Orbis's orders... until I learned the TRUTH.",
        "Z. Think about what it means to act for yourself."
    };
    
    private int dialogueIndex = -1;
    private bool isFinished;

    bool lateStart = false;
    bool isWriteTextFinished = false;
    private void LateStart()
    {
        if (lateStart) return;

        npc.gameObject.SetActive(false);
        OnClick();
    
        lateStart = true;
    }

    private void Update()
    {
        LateStart();

        if (isFinished)
            return;

        if (Input.GetMouseButtonDown(0) && !isWriteTextFinished)
            OnClick();
    }

    private void OnClick()
    {
        if (isWriteTextFinished)
            return;

        if (writeText.IsTyping)
        {
            writeText.Skip();
            return;
        }

        dialogueIndex++;

        if (dialogueIndex < dialogues.Length)
        {
            SoundManager.Instance.PlaySFX(Sound_SFX.UIText);
            if (LocalizationManager.Instance.CurLang == Language.KO)   
                writeText.Play(dialogues[dialogueIndex]);
            else
                writeText.Play(dialoguesEN[dialogueIndex]);
            return;
        }

        writeText.gameObject.SetActive(false);
        middleBoss.gameObject.SetActive(false);

        SoundManager.Instance.PlaySFX(Sound_SFX.BossAttacked);
        isWriteTextFinished = true;

        Invoke("StoreNPCStart", 2f);
    }

    void StoreNPCStart()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.Robot_Laugh);
        //상인 대사 시작
        npc.gameObject.SetActive(true);
        npcInteractable.OnInteract();
    }

    public void FinishEnding()
    {
        StartCoroutine(FinishEndingTime());
    }

    private IEnumerator FinishEndingTime()
    {
        isFinished = true;

        yield return new WaitForSecondsRealtime(returnDelay);

        SceneController.Instance.ChangeScene(SceneController.Scene.Credit);
    }

}
