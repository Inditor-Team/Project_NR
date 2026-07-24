using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFailUI : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        GameManager.Instance.OnSectionFail += Play;
        anim.enabled = false;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnSectionFail -= Play;
    }

    void Play()
    {
        anim.enabled = true;
        Invoke("BackToMap", 1.1f);
    }

    void BackToMap()
    {
        SceneController.Instance.ChangeScene(SceneController.Scene.Scene_Map);
    }
}
