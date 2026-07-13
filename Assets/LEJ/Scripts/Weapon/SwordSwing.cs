using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SwordSwing : MonoBehaviour
{
    [Header("플레이어 중심에 배치된 칼 피벗")]
    [SerializeField] private Transform swordPivot;

    [Header("공격 시간")]
    [SerializeField] private float swingDuration = 0.3f;

    [Header("휘두르는 각도")]
    [SerializeField] private float startAngle = 0f;
    [SerializeField] private float endAngle = -220f;

    [Header("휘두르는 속도 그래프")]
    [SerializeField]
    private AnimationCurve swingCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.15f, 0.05f),
        new Keyframe(0.55f, 0.8f),
        new Keyframe(1f, 1f)
    );

    [Header("공격 후 원래 자세로 돌아오는 시간")]
    [SerializeField] private float returnDuration = 0.12f;

    [SerializeField]
    private AnimationCurve returnCurve = AnimationCurve.EaseInOut(
        0f, 0f,
        1f, 1f
    );

    private Quaternion defaultRotation;
    private Coroutine swingCoroutine;
    private bool isSwinging;

    public bool IsSwinging => isSwinging;

    public UnityAction OnStopSwing;

    private void Awake()
    {
        if (swordPivot == null)
            swordPivot = transform;

        defaultRotation = swordPivot.localRotation;
    }

    public void Swing()
    {
        if (isSwinging)
            return;

        swingCoroutine = StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        isSwinging = true;

        yield return RotatePivot(
            startAngle,
            endAngle,
            swingDuration,
            swingCurve
        );

        yield return RotatePivot(
            endAngle,
            startAngle,
            returnDuration,
            returnCurve
        );

        swordPivot.localRotation = defaultRotation;

        isSwinging = false;
        swingCoroutine = null;

        OnStopSwing?.Invoke(); 
    }

    private IEnumerator RotatePivot(
        float fromAngle,
        float toAngle,
        float duration,
        AnimationCurve curve)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(
                elapsedTime / duration
            );

            float curveValue = curve.Evaluate(normalizedTime);

            float currentAngle = Mathf.LerpUnclamped(
                fromAngle,
                toAngle,
                curveValue
            );

            swordPivot.localRotation =
                defaultRotation *
                Quaternion.Euler(0f, 0f, currentAngle);

            yield return null;
        }

        swordPivot.localRotation =
            defaultRotation *
            Quaternion.Euler(0f, 0f, toAngle);
    }

    private void OnDisable()
    {
        if (swingCoroutine != null)
            StopCoroutine(swingCoroutine);

        swordPivot.localRotation = defaultRotation;

        swingCoroutine = null;
        isSwinging = false;
    }
}