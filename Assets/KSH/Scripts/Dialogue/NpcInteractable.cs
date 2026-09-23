using UnityEngine;

public enum DialogueID
{
    store_npc_0,
    event_npc_A_1,
    event_npc_B_1,
    intro_npc_1,
    end_npc_1,
    store2_npc_0
}

public class NpcInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueID startDialogueId;
    public void OnInteract()
    {
        if (SceneController.Instance.curScene == SceneController.Scene.ShopB)
        {
            startDialogueId = DialogueID.store2_npc_0; // 두번째 상점일 때 대화 지점 강제 설정
        }
        
        if (DialogueManager.Instance.IsBubbleWindowOpen()) // 중복 방지
        {
            Debug.Log("이미 말풍선 창이 켜져 있습니다.");
            return;
        }
        
        DialogueManager.Instance.StartDialogue(startDialogueId.ToString());
    }
}