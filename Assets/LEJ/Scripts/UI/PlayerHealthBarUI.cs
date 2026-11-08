using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    PlayerStat stat;
    [SerializeField] Image sliderOutline;
    [SerializeField] Slider slider;

    private void Start()
    {
        stat = GameManager.Instance.Player.GetComponent<PlayerController>().Stat;
        stat.OnUpdateStat += OnUpdateStat;

        //���� ���� �� ���� �� ���� ������ ����
        OnUpdateStat(PlayerStat.Stat.Life, StatDataManager.Instance.GetStat(PlayerStat.Stat.Life));
    }

    private void OnDestroy()
    {
        stat.OnUpdateStat -= OnUpdateStat;
    }

    bool doOnceAtUpdate = false;
    private void Update()
    {
        if (!doOnceAtUpdate) //Start ����Ŭ Ÿ�̹� �������� update ���� �� �� ����
        {
            SetHealthBarSize();
            UpdateHealthBar();
            doOnceAtUpdate = true;
        }
    }

    void OnUpdateStat(PlayerStat.Stat type, float value)
    {
        if (type == PlayerStat.Stat.Life)
            UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        slider.value = stat.StatDic[PlayerStat.Stat.Life];
        Debug.Log($"PlayerHealthBarUI: �÷��̾� ü���� {stat.StatDic[PlayerStat.Stat.Life]} slider.value �� {slider.value}");
    }

    float increaseAmount = 50f;
    float defaultSliderSize = 160;
    float defaultSliderOutlineSize = 250;
    public void SetHealthBarSize()
    {
        //�⺻ ü�� 5 ���� �ִ�ü�� ������ value
        int value = (int)stat.StatDic[PlayerStat.Stat.MaxLife] - 5;

        // �����̴� UI ������ �� maxValue ����
        var sliderSize = slider.GetComponent<RectTransform>();
        sliderSize.sizeDelta = new Vector2(defaultSliderSize + (increaseAmount * value), sliderSize.sizeDelta.y);

        var sliderOutlineSize = sliderOutline.GetComponent<RectTransform>();
        sliderOutlineSize.sizeDelta = new Vector2(defaultSliderOutlineSize + (increaseAmount * value), sliderOutlineSize.sizeDelta.y);

        slider.maxValue = stat.StatDic[PlayerStat.Stat.MaxLife];

        Debug.Log($"���� �ִ� ü�� {(int)stat.StatDic[PlayerStat.Stat.MaxLife]} �� slider maxValue �� {slider.maxValue}");
    }
}
