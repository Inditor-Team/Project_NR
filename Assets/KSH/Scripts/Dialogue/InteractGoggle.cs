using UnityEngine;

public class InteractGoggle : MonoBehaviour, IInteractable
{
    [SerializeField] private NpcInteractable storeNpc;
    private DialogueID goggleID = DialogueID.buy_goggle_1;
    
    public void OnInteract()
    {
        if (DialogueManager.Instance.IsBubbleWindowOpen()) return;
        DialogueManager.Instance.StartDialogue(goggleID.ToString());
    }
}
