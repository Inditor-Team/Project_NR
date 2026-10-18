using UnityEngine;
using UnityEngine.UI;

public class ControlMapScroll : MonoBehaviour
{
    [SerializeField] Scrollbar horizontal;
    [SerializeField] Scrollbar vertical;
    [SerializeField] float scrollSpeed = 0.5f;

    private void OnEnable()
    {
        //방금 끝난 섹터의 다음 섹터로 위치 설정
        SetScroll();
    }

    private void Update()
    {
        //키보드 방향키 입력에 따라 스크롤 움직이기
        horizontal.value += Input.GetAxis("Horizontal") * scrollSpeed * Time.deltaTime;
        vertical.value += Input.GetAxis("Vertical") * scrollSpeed * Time.deltaTime;
    }

    void SetScroll()
    {
        var prevScene = SceneController.Instance.prevScene;

        switch (prevScene)
        {
            case SceneController.Scene.NormalA:
                horizontal.value = 0.12f;
                vertical.value = 0.84f;
                break;
            case SceneController.Scene.NormalB:
                horizontal.value = 0.3f;
                vertical.value = 0.7f;
                break;
            case SceneController.Scene.EventA:
            case SceneController.Scene.HardA:
                horizontal.value = 0.6f;
                vertical.value = 0.5f;
                break;
            case SceneController.Scene.ShopA:
            case SceneController.Scene.NormalC:
                horizontal.value = 0.7f;
                vertical.value = 0.2f;
                break;
            case SceneController.Scene.NormalD:
            case SceneController.Scene.EventB:
            case SceneController.Scene.HardB:
                horizontal.value = 0.9f;
                vertical.value = 0.2f;
                break;
            case SceneController.Scene.ShopB:
                horizontal.value = 1f;
                vertical.value = 0f;
                break;
        }
    }
}
