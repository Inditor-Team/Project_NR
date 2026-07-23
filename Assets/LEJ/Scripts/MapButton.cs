using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapButton : MonoBehaviour
{
    Button button;
    public string sceneName;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void MoveScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
