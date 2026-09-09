using System.Collections;
using UnityEngine;

public class CameraEffect : MonoBehaviour
{
    [SerializeField] FollowCamera follow;
    [SerializeField] Gun gun;

    [Header("»ÁµÈ∏≤ ø¨√‚")]
    [SerializeField] int shakeAmount = 1; //«»ºø ¥‹¿ß
    [SerializeField] float shakeDuration = 0.08f;

    Coroutine ShakeRoutine;

    private void Awake()
    {
        if (gun != null)
            gun.OnShoot += Shake;
    }

    void Shake()
    {
        ShakeRoutine = StartCoroutine(ShakeTime());
    }

    IEnumerator ShakeTime()
    {
        if (follow != null)
            follow.enabled = false;

        Vector3 origin = transform.localPosition;

        float unitsPerPixel = 1f / 32;

        float timer = 0f;

        while (timer < shakeDuration)
        {
            int pixelX = Random.Range(-shakeAmount, shakeAmount + 1);
            int pixelY = Random.Range(-shakeAmount, shakeAmount + 1);

            transform.localPosition = origin + new Vector3(pixelX * unitsPerPixel,pixelY * unitsPerPixel,0f);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = origin;

        if (follow != null)
            follow.enabled = true;

        StopCoroutine(ShakeTime());
    }
}
