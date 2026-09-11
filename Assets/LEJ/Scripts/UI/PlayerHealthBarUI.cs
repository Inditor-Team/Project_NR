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

        //섹터 종료 후 저장 된 생명 값으로 설정
        OnUpdateStat(PlayerStat.Stat.Life, GameManager.Instance.Life);
    }

    bool doOnceAtUpdate = false;
    private void Update()
    {
        if (!doOnceAtUpdate) //Start 사이클 타이밍 엇나가서 update 에서 한 번 실행
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
    }

    float increaseAmount = 50f;
    float defaultSliderSize = 160;
    float defaultSliderOutlineSize = 250;
    public void SetHealthBarSize()
    {
        //기본 체력 5 기준 최대체력 변동량 value
        int value = (int)stat.StatDic[PlayerStat.Stat.MaxLife] - 5;

        // 슬라이더 UI 사이즈 및 maxValue 조정
        var sliderSize = slider.GetComponent<RectTransform>();
        sliderSize.sizeDelta = new Vector2(defaultSliderSize + (increaseAmount * value), sliderSize.sizeDelta.y);

        var sliderOutlineSize = sliderOutline.GetComponent<RectTransform>();
        sliderOutlineSize.sizeDelta = new Vector2(defaultSliderOutlineSize + (increaseAmount * value), sliderOutlineSize.sizeDelta.y);

        slider.maxValue = stat.StatDic[PlayerStat.Stat.MaxLife];

        Debug.Log($"현재 최대 체력 {(int)stat.StatDic[PlayerStat.Stat.MaxLife]} 및 slider maxValue 는 {slider.maxValue}");
    }
}
