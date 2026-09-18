using System.Collections;
using UnityEngine;

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private WriteText writeText;

    [Header("NPC")]
    [SerializeField] private Transform npc;
    [SerializeField] private NpcInteractable npcInteractable;
    [SerializeField] private Transform windowPoint;
    [SerializeField] private Transform whisperPoint;
    [SerializeField] private float npcMoveDuration = 0.5f;

    [Header("Red Goggle")]
    [SerializeField] private GameObject redGoggleObject;
    [SerializeField] private float goggleShowDuration = 1.5f;

    [Header("Opening")]
    [SerializeField] private GameObject blackScreen;
    [SerializeField] private float commandDelay = 1f;

    private readonly string[] commands =
    {
        "Z. 폐기 AI ‘마더 코어’의 반란 징후가 확인되었다.",
        "해당 개체와 모든 파생 개체를 제거하라."
    };

    private int commandIndex = -1;

    private bool isCommand;
    private bool isFinished;

    private Coroutine openingCoroutine;

    private void Start()
    {
        redGoggleObject.SetActive(false);

        openingCoroutine = StartCoroutine(StartOpening());
    }

    private void Update()
    {
        if (isFinished)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (writeText.IsTyping)
        {
            writeText.Skip();
            return;
        }

        if (isCommand)
        {
            ShowNextCommand();
            return;
        }
    }

    private IEnumerator StartOpening()
    {
        blackScreen.SetActive(true);

        yield return new WaitForSecondsRealtime(commandDelay);

        isCommand = true;
        ShowNextCommand();
    }

    private void ShowNextCommand()
    {
        commandIndex++;

        if (commandIndex < commands.Length)
        {
            writeText.Play(commands[commandIndex]);
            return;
        }
        else
        {
            writeText.gameObject.SetActive(false);
            npcInteractable.OnInteract();
            isCommand = false;
        }
    }

    private IEnumerator MoveNPC(Vector3 target)
    {
        Vector3 start = npc.position;
        float time = 0f;

        while (time < npcMoveDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / npcMoveDuration);
            npc.position = Vector3.Lerp(start, target, t);

            yield return null;
        }

        npc.position = target;
    }

    private IEnumerator ShowRedGoggle()
    {
        redGoggleObject.SetActive(true);

        yield return new WaitForSecondsRealtime(goggleShowDuration);

        redGoggleObject.SetActive(false);
    }

    /// <summary>
    /// 프롤로그 전체 스킵 버튼
    /// </summary>
    public void SkipOpening()
    {
        if (isFinished)
            return;

        StopAllCoroutines();

        writeText.Skip();

        blackScreen.SetActive(false);
        redGoggleObject.SetActive(false);

        FinishOpening();
    }

    private void FinishOpening()
    {
        isFinished = true;
        isCommand = false;

        gameObject.SetActive(false);
    }
}