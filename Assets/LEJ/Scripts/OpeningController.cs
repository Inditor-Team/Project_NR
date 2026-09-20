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
    [SerializeField] private float goggleShowDuration = 3f;

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
    private bool isCommandsEnd; // commands 종료 체크

    private Coroutine openingCoroutine;

    private void Start()
    {
        redGoggleObject.SetActive(false);

        openingCoroutine = StartCoroutine(StartOpening());
    }

    private void Update()
    {
        if (isFinished || isCommandsEnd)
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
            isCommandsEnd = true;
        }
    }

    public void MoveNPCToWidnow()
    {
        StartCoroutine(MoveNPC(windowPoint.position));
    }

    public void MoveNPCToPlayer()
    {
        StartCoroutine(MoveNPC(whisperPoint.position));
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

    public IEnumerator ShowRedGoggle()
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
        
        //전환 될 때 어색한 부분이 있어 임의로 추가했습니다
        writeText.gameObject.SetActive(false);
        blackScreen.SetActive(true); 

        redGoggleObject.SetActive(false);

        FinishOpening();
    }

    public void FinishOpening()
    {
        isFinished = true;
        isCommand = false;

        StartCoroutine(ChangeScene()); // 약간 대기 후 씬 이동
    }
    
    public IEnumerator ChangeScene()
    {
        yield return new WaitForSecondsRealtime(1f); //1초로 살짝 수정했습니다!
        SceneController.Instance.ChangeScene(SceneController.Scene.Lobby);
    }
}