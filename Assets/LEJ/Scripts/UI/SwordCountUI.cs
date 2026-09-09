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
            sword.OnSwing += OnSwing;
    }

    private void OnDestroy()
    {
        if (sword != null)
            sword.OnSwing -= OnSwing;
    }

    void OnSwing()
    {
        if (swordIcons.Length <= index)
            return;

        swordIcons[index++].gameObject.SetActive(false);
    }
}
