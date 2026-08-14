using UnityEngine;

public class NpcInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string startDialogueId;

    public void OnInteract()
    {
        if (DialogueManager.Instance.IsBubbleWindowOpen()) // 중복 방지
        {
            Debug.Log("이미 말풍선 창이 켜져 있습니다.");
            return;
        }
        
        DialogueManager.Instance.StartDialogue(startDialogueId);
    }
}