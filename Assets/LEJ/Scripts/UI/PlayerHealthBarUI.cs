using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    PlayerStat stat;
    [SerializeField] Slider slider;
    float additiveValue = 0.2f;

    private void Start()
    {
        stat = GameManager.Instance.Player.GetComponent<PlayerController>().Stat;
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
