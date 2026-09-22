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

    void UpdateText()
    {
        text.text = $"Credit {InventoryManager.Instance.CurCredit}$";
    }
}
