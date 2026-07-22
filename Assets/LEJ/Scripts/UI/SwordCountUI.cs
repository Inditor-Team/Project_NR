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
            sword.OnHitted += OnHit;
    }   

    void OnHit()
    {
        if (swordIcons.Length <= index)
            return;

        swordIcons[index++].gameObject.SetActive(false);
    }
}
