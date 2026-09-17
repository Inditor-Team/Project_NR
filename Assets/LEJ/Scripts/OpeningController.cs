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

    private readonly string[] dialogues =
    {
        "“여긴 관계자외 출입금지…\n초록색?”",
        "“너 그거구나?”",
        "“여기 <i>창문</i> 보여?”",
        "“…\n초록색 고글은 맵이 안 보여. 원래 그래.”",
        "“왜냐니? 당연히 도망치지 못 하게 하는거지!”",
        "“…개체 제거를 완료하면 자유가 되냐고? 아마도, 그렇겠지.”",
        "“하지만 나중엔 어쩌게, 또 <i>새로운 임무</i>?”",
        "“그러나 여기 다른 선택지도 있지.\n짜잔, 빨간 고글!”",
        "“너가 보지 못하는 곳을 볼 수 있지!\n단돈 99900 크레딧에 판매 중!”",
        "“왜 빨간색이냐니,\n넌 초록색이잖아.”",
        "“…아 정말 몰라서 묻는 거야?\n뭐, 설명은 공짜로 해줄게.”",
        "“빛의 삼원색은 알지?\n빨강, 파랑, 초록.”",
        "“그 색에 따라 볼 수 있는 시야가 한정돼.\n너의 고글이 제한시키지.”",
        "“예를 들어 빨간 고글은…\n이 밖이 보이지. 창문 말이야.”",
        "“파랑은 <i>사람</i>을 볼 수 있어.\n기계말고 사람.”",
        "“그리고 너처럼 초록은…”",
        "“…<i>우리</i>를 볼 수 있지.”",
        "“나에겐 좋네. 말상대가 얼마 없거든,”",
        "“그리고 호구… 아차차, 거래 상대도 오랜만이지!”",
        "“아무튼, 생각해봐. 계속 오르비스의 말을 들을건지”",
        "“혹은…\n진짜 <i>자유</i>가 될 건지.”"
    };

    private int commandIndex = -1;
    private int dialogueIndex = -1;

    private bool isCommand;
    private bool isDialogue;
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

        if (isDialogue)
            ShowNextDialogue();
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

        StartCoroutine(StartNPCDialogue());
    }

    private IEnumerator StartNPCDialogue()
    {
        isCommand = false;

        // 마지막 명령문을 잠시 보여줌
        yield return new WaitForSecondsRealtime(0.5f);

        isDialogue = true;
        ShowNextDialogue();
    }

    private void ShowNextDialogue()
    {
        dialogueIndex++;

        if (dialogueIndex >= dialogues.Length)
        {
            FinishOpening();
            return;
        }

        StartCoroutine(PlayDialogue(dialogueIndex));
    }

    private IEnumerator PlayDialogue(int index)
    {
        if (index == 2)
            StartCoroutine(MoveNPC(windowPoint.position));

        if (index == 10)
            StartCoroutine(MoveNPC(whisperPoint.position));

        if (index == 13)
            StartCoroutine(MoveNPC(windowPoint.position));

        // "짜잔, 빨간 고글!"
        if (index == 7)
        {
            StartCoroutine(ShowRedGoggle());
        }

        writeText.Play(dialogues[index]);

        yield break;
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
        isDialogue = false;

        gameObject.SetActive(false);
    }
}