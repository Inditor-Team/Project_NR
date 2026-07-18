using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] PlayerStat stat;
    [SerializeField] Slider slider;
    float additiveValue = 0.2f;

    private void Awake()
    {
        stat.OnUpdateStat += OnUpdateStat;
    }
    
    void OnUpdateStat(PlayerStat.Stat type, float value)
    {
        if (type != PlayerStat.Stat.Life)
            return;

        UpdateHealthBar(value < 0);
    }

    void UpdateHealthBar(bool decrease)
    {
        if (decrease)
            slider.value += additiveValue;
        else
            slider.value -= additiveValue;
    }
}
