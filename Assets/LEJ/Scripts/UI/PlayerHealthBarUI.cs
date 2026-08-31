using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    PlayerStat stat;
    [SerializeField] Slider slider;

    private void Start()
    {
        stat = GameManager.Instance.Player.GetComponent<PlayerController>().Stat;
        stat.OnUpdateStat += OnUpdateStat;

        OnUpdateStat(PlayerStat.Stat.Life, stat.StatDic[PlayerStat.Stat.Life]);
    }
    
    void OnUpdateStat(PlayerStat.Stat type, float value)
    {
        if (type != PlayerStat.Stat.Life)
            return;

        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        slider.value = stat.StatDic[PlayerStat.Stat.Life];
    }
}
