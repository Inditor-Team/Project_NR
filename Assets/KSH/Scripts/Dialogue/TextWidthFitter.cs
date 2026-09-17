using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TextWidthFitter : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private RectTransform targetRect; // 크기를 조절 대상
    [SerializeField] private float horizontalPadding = 20f;
    [SerializeField] private float minWidth = 125f;
    private float maxWidth = 500f;

    public void UpdateWidth()
    {
        targetText.enableWordWrapping = true; // wrap 활성화

        // 실제 오브젝트 크기를 측정
        float wrapConstraint = maxWidth - horizontalPadding;
        Vector2 preferred = targetText.GetPreferredValues(targetText.text, wrapConstraint, 0f);

        // 가장 긴 줄 기준
        float width = Mathf.Clamp(preferred.x + horizontalPadding, minWidth, maxWidth);

        Vector2 size = targetRect.sizeDelta;
        size.x = width;
        targetRect.sizeDelta = size; // 가로만 변경
    }
}