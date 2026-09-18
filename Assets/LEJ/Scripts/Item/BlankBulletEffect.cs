using System.Collections;
using UnityEngine;

/// <summary>
/// 공포탄 사용 시 표시되는 원형 이펙트
/// </summary>
public class BlankBulletEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float duration = 0.4f;

    /// <summary>
    /// 지정된 반지름 크기로 이펙트를 재생합니다.
    /// </summary>
    public void Play(float radius)
    {
        // 기본 원형 Sprite의 지름을 기준으로 크기 설정
        transform.localScale = Vector3.one * radius * 2f;

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        spriteRenderer.enabled = true;

        Color color = spriteRenderer.color;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(time / duration);
            color.a = 1f - t;

            spriteRenderer.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}