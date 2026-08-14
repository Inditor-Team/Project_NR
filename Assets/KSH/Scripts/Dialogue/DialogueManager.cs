using UnityEngine;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

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

    [SerializeField] private BubbleController bubbleController;

    public bool IsBubbleWindowOpen()
    {
        return bubbleController.IsBubbleWindowOpen();
    }

    public void StartDialogue(string dialogueId)
    {
        if (bubbleController == null) FindBubbleController();
        
        bubbleController.ShowWindow();
        bubbleController.StartDialogueById(dialogueId);
    }

    public void EndDialogue()
    {
        bubbleController.HideWindow();
    }

    private void FindBubbleController()
    {
        bubbleController = FindFirstObjectByType<BubbleController>(FindObjectsInactive.Include);
    }
}
