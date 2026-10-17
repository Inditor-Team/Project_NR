using UnityEngine;

public class IntroInputBlocker : MonoBehaviour
{
    public float allowInputAfterSeconds = 0;
    public PlayerController playerController;

    public void Start()
    {
        playerController.DisableInput(); 
        Invoke("DestroySelf", allowInputAfterSeconds);
    }

    void DestroySelf()
    {
        playerController.EnableInput();
        Destroy(gameObject);
    }
}
