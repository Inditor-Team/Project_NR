using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

public enum TextEffect
{
    Heal,
    AddMaxHp,
    AddCredit,
    ChangeStat
}

public class PlayerFloatingText : MonoBehaviour
{
    public static PlayerFloatingText instance;

    [SerializeField] private TMP_Text floatingText;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float moveDistance = 0.6f;

    private Vector3 baseLocalPos;
    private Sequence seq;

    private void Awake()
    {
        instance = this;
        baseLocalPos = transform.localPosition; // 기준점 고정
        floatingText.alpha = 0f;
    }

    private void Show(string text, Color color)
    {
        seq?.Kill();
        transform.localPosition = baseLocalPos;
        floatingText.text = text;
        floatingText.color = color;

        seq = DOTween.Sequence()
                     .Join(transform.DOLocalMoveY(baseLocalPos.y + moveDistance, duration).SetEase(Ease.OutCubic))
                     .Join(floatingText.DOFade(0f, duration).SetEase(Ease.InQuad))
                     .OnComplete(() => transform.localPosition = baseLocalPos);
    }

    public void ShowFloatingText(int value, TextEffect effect)
    {
        switch (effect)
        {
            case TextEffect.Heal:
                Show("HP + " + value, Color.green);
                break;
            case TextEffect.AddMaxHp:
                Show("MAX HP + " + value, Color.green);
                break;
            case TextEffect.AddCredit:
                Show("Credit + " + value, Color.yellow);
                break;
            case TextEffect.ChangeStat:
                Show("Damage X 4\nFireRate X 3", Color.white);
                break;
        }
    }
}