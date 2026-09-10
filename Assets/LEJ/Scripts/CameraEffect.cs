using System.Collections;
using UnityEngine;

public class CameraEffect : MonoBehaviour
{
    [SerializeField] FollowCamera follow;
    [SerializeField] Gun gun;
    PlayerStat stat;

    [Header("»ÁµÈ∏≤ ø¨√‚")]
    [SerializeField] int shakeAmount = 1; //«»ºø ¥‹¿ß
    [SerializeField] float shakeDuration = 0.08f;

    [SerializeField] int shakeAmountOnDamaged = 2; //«»ºø ¥‹¿ß
    [SerializeField] float shakeDurationOnDamaged = 0.1f;

    Coroutine ShakeRoutine;

    private void Start()
    {
        if (gun != null)
            gun.OnShoot += Shake;

        stat = GameManager.Instance.Player.GetComponent<PlayerController>().Stat;
        stat.OnDamaged += ShakeOnDamaged;
    }

    private void OnDestroy()
    {
        if (gun != null)
            gun.OnShoot -= Shake;

        if (stat != null)
            stat.OnDamaged -= ShakeOnDamaged;
    }

    void Shake()
    {
        ShakeRoutine = StartCoroutine(ShakeTime(shakeAmount, shakeDuration));
    }

    void ShakeOnDamaged()
    {
        ShakeRoutine = StartCoroutine(ShakeTime(shakeAmountOnDamaged, shakeDurationOnDamaged));
    }

    IEnumerator ShakeTime(int shakeAmount, float shakeDuration)
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
    }
}
