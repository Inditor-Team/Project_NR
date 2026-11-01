using UnityEngine;

/// <summary>
/// Ä® Á¦ÇÑ UI
/// </summary>
public class SwordCountUI : MonoBehaviour
{
    [SerializeField] Sword sword;
    [SerializeField] GameObject[] swordIcons;
    int index = 0;

    private void Awake()
    {
        if (sword != null)
            sword.OnSwordLifeChanged += OnSwordLifeChanged;
    }

    private void OnDestroy()
    {
        if (sword != null)
            sword.OnSwordLifeChanged -= OnSwordLifeChanged;
    }

    void OnSwordLifeChanged()
    {
        if (swordIcons.Length <= index || swordIcons.Length == 0) //index ¿À·ù
            return;

        swordIcons[sword.SwordLife].gameObject.SetActive(false);
    }
}
