using UnityEngine;
using UnityEngine.InputSystem;

public class RotateByAim : MonoBehaviour
{
    [SerializeField] SpriteRenderer model;
    [SerializeField] bool onlyFlip = false; //로테이션 말고 flip 만 반영합니다
    [SerializeField] bool FlipX = false;

    Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        var aimPos = GetAimPos();

        //왼쪽으로 뒤집기
        if (model != null)
        {
            if (FlipX)
                model.flipX = aimPos > 90 || aimPos < -90;
            else
                model.flipY = aimPos > 90 || aimPos < -90;
        }

        if (onlyFlip)
            return;
     
        transform.rotation = Quaternion.Euler(0, 0, aimPos);
    }

    /// <summary>
    /// 마우스 위치에 따라 총과 검의 방향 변경
    /// </summary>
    public float GetAimPos()
    {
        //사용자 마우스 위치 받아서 좌표 계산
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseScreen3D = new Vector3(mouseScreen.x, mouseScreen.y, -mainCam.transform.position.z);
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(mouseScreen3D);
        Vector2 aim = Vector2.zero;
        if (GameManager.Instance.Player == null)
            GameManager.Instance.FindPlayer();
        aim = (mouseWorld - GameManager.Instance.Player.transform.position).normalized;
        return Mathf.Atan2(-aim.y, -aim.x) * Mathf.Rad2Deg;
    }
}
