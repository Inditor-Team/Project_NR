using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFailUI : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        SectorManager.Instance.OnSectorFail += Play;
        anim.enabled = false;
    }

    private void OnDestroy()
    {
        SectorManager.Instance.OnSectorFail -= Play;
    }

    void Play(SectorSO.SectorType type)
    {
        anim.enabled = true;
        Invoke("BackToMap", 1.1f);
    }

    void BackToMap()
    {
        SceneController.Instance.ChangeScene(SceneController.Scene.Scene_Map);
    }
}
