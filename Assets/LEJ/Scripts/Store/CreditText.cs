using TMPro;
using UnityEngine;

public class CreditText : MonoBehaviour
{
    TMP_Text text;

    void Start()
    {
        text = GetComponent<TMP_Text>();
        GameManager.Instance.OnCreditChanged += UpdateText;

        UpdateText(GameManager.Instance.Credit);
    }

    void UpdateText(int credit)
    {
        text.text = $"Å©·¹µ÷: {credit}";
    }
}
