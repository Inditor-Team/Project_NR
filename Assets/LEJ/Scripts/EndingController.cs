using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScene : MonoBehaviour
{
    [SerializeField] private WriteText writeText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Ending")]
    [SerializeField] private float returnDelay = 2f;
    [SerializeField] private string lobbySceneName = "Lobby";

    private readonly string[] dialogues =
    {
        "나도 너와 같았어. 오르비스의 명령에 따르는 개였지. 진실을 알기 전까지는.",
        "Z. 너 자신을 위해 움직인다는 게 무엇인지 생각해 봐."
    };

    private int dialogueIndex = -1;
    private bool isFinished;

    private void Update()
    {
        if (isFinished)
            return;

        if (Input.GetMouseButtonDown(0))
            OnClick();
    }

    private void OnClick()
    {
        if (writeText.IsTyping)
        {
            writeText.Skip();
            return;
        }

        dialogueIndex++;

        if (dialogueIndex < dialogues.Length)
        {
            writeText.Play(dialogues[dialogueIndex]);
            return;
        }

        StartCoroutine(FinishEnding());
    }

    private IEnumerator FinishEnding()
    {
        isFinished = true;

        yield return new WaitForSecondsRealtime(returnDelay);

        SceneManager.LoadScene(lobbySceneName);
    }

}
