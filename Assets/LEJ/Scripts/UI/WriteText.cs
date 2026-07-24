using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class WriteText : MonoBehaviour
{
    [SerializeField] private float interval = 0.05f;
    [SerializeField] private bool playOnEnable = true;

    private TMP_Text text;
    private Coroutine typingCoroutine;

    public bool IsTyping { get; private set; }

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    public void Play()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeRoutine());
    }

    public void Play(string newText)
    {
        text.text = newText;
        Play();
    }

    private IEnumerator TypeRoutine()
    {
        IsTyping = true;

        text.ForceMeshUpdate();

        int characterCount = text.textInfo.characterCount;
        text.maxVisibleCharacters = 0;

        for (int i = 1; i <= characterCount; i++)
        {
            text.maxVisibleCharacters = i;
            yield return new WaitForSeconds(interval);
        }

        IsTyping = false;
        typingCoroutine = null;
    }

    public void Skip()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        text.ForceMeshUpdate();
        text.maxVisibleCharacters = text.textInfo.characterCount;
        IsTyping = false;
    }
}