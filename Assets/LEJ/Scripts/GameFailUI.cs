using UnityEngine;

public class GameFailUI : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.OnSectionFail += Play;
        gameObject.SetActive(false);
    }

    void Play()
    {
        gameObject.SetActive(true);
    }
}
