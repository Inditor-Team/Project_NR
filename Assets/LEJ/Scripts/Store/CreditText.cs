using TMPro;
using UnityEngine;

public class CreditText : MonoBehaviour
{
    TMP_Text text;

    void Start()
    {
        text = GetComponent<TMP_Text>();
        InventoryManager.Instance.OnCreditChanged += UpdateText;

        UpdateText();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnCreditChanged -= UpdateText;
    }

    void UpdateText()
    {
        text.text = $"Credit {InventoryManager.Instance.CurCredit}$";
    }
}
