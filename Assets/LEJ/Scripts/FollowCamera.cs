using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("추적 속도")]
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocity;

    [Header("카메라 위치 제한 최대, 최소값 X, Y")]
    [SerializeField] Vector2 clampMax;
    [SerializeField] Vector2 clampMin;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(transform.position,targetPosition,ref velocity,smoothTime);

        //카메라 위치 제한
        if (transform.position.x > clampMax.x)
            transform.position = new Vector3(clampMax.x, transform.position.y, transform.position.z);
        if (transform.position.x < clampMin.x)
            transform.position = new Vector3(clampMin.x, transform.position.y, transform.position.z);

        if (transform.position.y > clampMax.y)
            transform.position = new Vector3(transform.position.x, clampMax.y, transform.position.z);
        if (transform.position.y < clampMin.y)
            transform.position = new Vector3(transform.position.x, clampMin.y, transform.position.z);
    }
}