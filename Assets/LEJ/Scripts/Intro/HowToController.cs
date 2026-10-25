using System.Collections;
using UnityEngine;

public class HowToController : MonoBehaviour
{
    [Header("NPC")]
    [SerializeField] private Transform npc;
    [SerializeField] private NpcInteractable npcInteractable;

    [Header("Opening")]
    [SerializeField] private GameObject blackScreen;
    [SerializeField] private float commandDelay = 1f;


    private void Start()
    {
        blackScreen.GetComponent<Animator>().enabled = false;
        StartCoroutine(WaitToStartHowTo());
    }

    IEnumerator WaitToStartHowTo()
    {
        yield return new WaitForSeconds(0.5f);
        npcInteractable.OnInteract();
    }

    public void SkipHowTo()
    {
        StopAllCoroutines();

        FinishHowto();
    }

    public void FinishHowto()
    {
        StartCoroutine(ChangeScene()); // �ణ ��� �� �� �̵�
    }

    public IEnumerator ChangeScene()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        blackScreen.GetComponent<Animator>().enabled = true;
        blackScreen.SetActive(true);

        SceneController.Instance.ChangeScene(SceneController.Scene.Lobby);
    }
}