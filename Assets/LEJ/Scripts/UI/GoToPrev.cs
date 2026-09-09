using UnityEngine;

public class ChangeScene : MonoBehaviour
{
    public SceneController.Scene targetScene;

    public void Change()
    {
        SceneController.Instance.ChangeScene(targetScene);
    }
}
