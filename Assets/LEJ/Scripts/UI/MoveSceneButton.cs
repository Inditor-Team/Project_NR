using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MoveSceneButton : MonoBehaviour
{
    Button button;
    public SceneController.Scene sceneName;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void MoveScene()
    {
        SceneController.Instance.ChangeScene(sceneName);
    }
}
