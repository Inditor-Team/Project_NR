using UnityEngine;
using UnityEngine.UIElements;

public class IntroPlayer : MonoBehaviour
{
    [SerializeField] Animator fadeCanvas;

    private void Awake()
    {
        //인트로 씬으로 전환 전 부자연스러워 추가
        fadeCanvas.enabled = false;
    }

    private void Start()
    {
        //PlayerPrefs 가 IntroPlayed 를 가진 적 없거나 IntroPlayed 값이 0 이면
        if (!PlayerPrefs.HasKey("IntroPlayed") || PlayerPrefs.GetInt("IntroPlayed") <= 0)
        {
            //인트로 재생
            PlayIntro();
        }
        else
        {
            fadeCanvas.enabled = true;
        }
    }

    public void PlayIntro()
    {
        PlayerPrefs.SetInt("IntroPlayed", 1);
        SceneController.Instance.ChangeScene(SceneController.Scene.Opening);
    }
}
