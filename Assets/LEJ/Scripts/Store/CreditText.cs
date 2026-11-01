using DG.Tweening;
using TMPro;
using UnityEngine;

public class CreditText : MonoBehaviour
{
    [SerializeField] private TMP_Text creditText;
    [SerializeField] private TMP_Text addCreditText; // 추가되는 크레딧

    private RectTransform addRect;
    private float baseY;
    private int displayCredit;
    private Sequence sequence;

    void Start()
    {
        Canvas.ForceUpdateCanvases(); // 레이아웃 계산을 먼저 끝낸 뒤 위치 저장
        addRect = addCreditText.rectTransform;
        baseY = addRect.anchoredPosition.y;
        addCreditText.alpha = 0f;

        InventoryManager.Instance.OnCreditChanged += UpdateText;
        UpdateText(0);
    }

    private void OnDestroy()
    {
        sequence?.Kill();
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnCreditChanged -= UpdateText;
    }

    // amount > 0 : 증가, amount < 0 : 감소, amount == 0 : 연출 없이 바로 반영 (SetCredit 등)
    void UpdateText(int amount)
    {
        int target = InventoryManager.Instance.CurCredit;
        sequence?.Kill(); // 연출 중이면 끊고 새로 시작

        if (amount == 0)
        {
            SetCreditText(target);
            addCreditText.alpha = 0f;
            return;
        }

        int from = displayCredit;
        addCreditText.text = amount > 0 ? $"+{amount}$" : $"{amount}$";
        addCreditText.alpha = 0f;
        addRect.anchoredPosition = new Vector2(addRect.anchoredPosition.x, baseY - 20f); // 아래에서 시작

        sequence = DOTween.Sequence()
            .Append(addRect.DOAnchorPosY(baseY, 0.2f).SetEase(Ease.OutCubic)) // 아래 -> 원위치
            .Join(addCreditText.DOFade(1f, 0.2f))
            .Append(DOTween.To(x => SetCreditText(Mathf.RoundToInt(x)), from, target, 0.4f)) // 숫자 카운트
            .Append(addRect.DOAnchorPosY(baseY + 20f, 0.4f).SetEase(Ease.OutCubic)) // 원위치 -> 위
            .Join(addCreditText.DOFade(0f, 0.4f));
    }

    void SetCreditText(int credit)
    {
        displayCredit = credit;
        creditText.text = $"{credit}$";
    }
}