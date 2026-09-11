using UnityEngine;

public class Exit : MonoBehaviour
{
    public SceneController.Scene targetScene;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SceneController.Instance.ChangeScene(targetScene);
    }
}
