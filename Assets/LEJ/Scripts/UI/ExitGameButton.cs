using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ExitGameButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => { Exit(); });
    }
    public void Exit()
    {
        GameManager.Instance.ExitGame();
    }
}
