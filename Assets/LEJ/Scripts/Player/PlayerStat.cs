using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStat : MonoBehaviour, IDamageable, ISaveable
{
    public enum Stat
    {
        None,

        MoveSpeed, //이동 속도
        RollSpeed, //구르기 속도
        RollDuration, //구르는 시간
        RollRate, //구른 후 다시 구르기까지의 쿨타임

        SwordSwingSpeed, //칼 휘두르는 속도
        SwordDamage, //칼이 주는 데미지
        SwordSwingRate, //휘두른 후 다시 휘두르기까지의 쿨타임

        BulletSpeed, //총알의 속도 
        BulletDamage, //총알이 주는 데미지
        BulletFireRate, //발사 후 다시 발사까지의 쿨타임

        ProtocolDuration,
        ProtocolRate,

        Life, //생명
        MaxLife, //최대 생명

        AdditionalDamage, //데미지량 (카드 효과)

        Count
    }

    Dictionary<Stat, float> statDic = new Dictionary<Stat, float>();
    public Dictionary<Stat, float> StatDic => statDic;

    public event UnityAction<Stat, float> OnUpdateStat;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model; 
    bool isInvincible = false; //무적 상태
    public bool IsInvincible { get { return isInvincible; } set { isInvincible = value; } }

    void Awake()
    {
        for (int i = 1; i < (int)Stat.Count; i++)
            statDic.Add((Stat)i, 0f);

        SetDefaultStat();
    }

    void Start()
    {
        SetLifeByGameManager();

        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.Unregister(this);

        if (recoverProbability > 0f) //회복 알고리즘 카드가 있어 이벤트 등록이 됐었다면 해지
            SectorManager.Instance.OnDestroyedEnemy += RecoveryAlgorithm;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == enemyLayer)
            TakeDamage(100);
    }

    void SetDefaultStat()
    {
        SetStat(Stat.MoveSpeed, 3f);
        SetStat(Stat.RollSpeed, 10f);
        SetStat(Stat.RollDuration, 0.3f);
        SetStat(Stat.RollRate, 0.5f);

        SetStat(Stat.SwordSwingSpeed, 5f);
        SetStat(Stat.SwordDamage, 10f);
        SetStat(Stat.SwordSwingRate, 0.5f);

        SetStat(Stat.BulletSpeed, 30f);
        SetStat(Stat.BulletDamage, 1f);
        SetStat(Stat.BulletFireRate, 0.5f);

        SetStat(Stat.ProtocolDuration, 3f);
        SetStat(Stat.ProtocolRate, 10f);

        SetStat(Stat.Life, 5f);
        SetStat(Stat.MaxLife, 5f);

        SetStat(Stat.AdditionalDamage, 0f);
    }

    /// <summary>
    /// 씬이 바껴도 life 를 유지하고 싶을 때, 게임매니저에 저장된 life 를 계승합니다
    /// </summary>
    void SetLifeByGameManager()
    {
        SetStat(Stat.Life, GameManager.Instance.Life);
    }

    public void EarnLife(float amount)
    {
        AddStat(Stat.Life, amount);
    }

    public UnityAction OnDamaged;

    public void TakeDamage(float damage)
    {
        if (isInvincible)
            return;

        if (Random.value < evasionProbability) //카드 얻기 전 확률 0
        {
            //시각적 효과
            model.DOColor(Color.green, 0.2f).OnComplete(() =>
            {
                model.DOColor(Color.white, 0.2f);
            });

            return;
        }

        OnDamaged?.Invoke();

        //데미지 입는 효과
        model.DOColor(Color.red, 0.2f).OnComplete(() =>
        {
            model.DOColor(Color.white, 0.2f);
        });

        //additional damage 는 보통의 경우 0, 오버클럭 획득 시 +1
        AddStat(Stat.Life, -(damage + statDic[Stat.AdditionalDamage]));

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(Sound_SFX.Player_Hit);
    }

    private void SetStat(Stat type, float value)
    {
        statDic[type] = value;
    }

    /// <summary>
    /// 스탯에 값을 더합니다
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void AddStat(Stat type, float value)
    {
        statDic[type] += value;

        //최대 체력 이상으로 가질 수 없습니다
        if (type == Stat.Life && statDic[Stat.Life] > statDic[Stat.MaxLife])
            statDic[Stat.Life] = statDic[Stat.MaxLife];

        OnUpdateStat?.Invoke(type, value);
    }

    /// <summary>
    /// 스탯에 값 배율 증가
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void IncreaseStat(Stat type, float value, bool isDecrease = false)
    {
        if (!isDecrease)
            statDic[type] /= value;
        else
            statDic[type] *= value;

        OnUpdateStat?.Invoke(type, value);
    }

    #region Save
    // 세이브 관련
    public void SaveDataTo(SaveDataStruct data)
    {
        data.statDic = statDic;
        data.isStolen = false; // TODO: 상점 훔치는 지
    }

    public void LoadDataFrom(SaveDataStruct data)
    {
        statDic = data.statDic;
    }
    #endregion

    #region SpecialStat
    public void SpecialToggle(LevelCardSO.LevelCardType type)
    {
        bool isA = false;
        switch (type)
        {
            //회복 알고리즘의 경우 처치 시 체력 회복
            case LevelCardSO.LevelCardType.RecoveryAlgorithmA:
            case LevelCardSO.LevelCardType.RecoveryAlgorithmB:

                isA = (type == LevelCardSO.LevelCardType.RecoveryAlgorithmA);
                recoverProbability = isA ? 0.05f : 0.1f; //i 의 경우 5% ii 의 경우 10%

                SectorManager.Instance.OnDestroyedEnemy += RecoveryAlgorithm;

                break;

            //민첩 알고리즘의 경우 일정 확률로 적 공격 방어
            case LevelCardSO.LevelCardType.EvasionA:
            case LevelCardSO.LevelCardType.EvasionB:

                isA = (type == LevelCardSO.LevelCardType.EvasionA);
                evasionProbability = isA ? 0.3f : 0.5f;
                break;

            //불안정 코어의 경우 일정 확률 연사 및 이속 디버프
            case LevelCardSO.LevelCardType.InstableCore:
                instableCoreProbability = 0.05f; //PlayerController 에서 적용
                break;
        }
    }

    float recoverProbability = 0f;

    void RecoveryAlgorithm()
    {
        if (Random.value < recoverProbability)
        {
            //시각적 효과
            model.DOColor(Color.green, 0.2f).OnComplete(() =>
            {
                model.DOColor(Color.white, 0.2f);
            });

            AddStat(Stat.Life, 1);
        }
    }

    float evasionProbability = 0f;
    float instableCoreProbability = 0f;
    public float InstableCoreProbability => instableCoreProbability;
    #endregion
}
