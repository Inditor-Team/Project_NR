using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PlayerStat : MonoBehaviour, IDamageable, ISaveable
{
    public enum Stat
    {
        None,

        MoveSpeed, //이동 속도
        RollSpeed, //구르기 속도 (이속에서 n배 증가)
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

    private Coroutine damagedCoreCoroutine;
    private readonly Dictionary<Stat, float> damagedCoreBonus = new();

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

        if (recoverProbability > 0f && SectorManager.Instance != null) //회복 알고리즘 카드가 있어 이벤트 등록이 됐었다면 해지
            SectorManager.Instance.OnDestroyedEnemy -= RecoveryAlgorithm;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == enemyLayer)
            TakeDamage(100);
    }

    void SetDefaultStat()
    {
        SetStat(Stat.MoveSpeed, 3f);
        SetStat(Stat.RollSpeed, 3.5f);
        SetStat(Stat.RollDuration, 0.3f);
        SetStat(Stat.RollRate, 0.5f);

        SetStat(Stat.SwordSwingSpeed, 5f);
        SetStat(Stat.SwordDamage, 5f);
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

        //시각적 효과
        model.DOColor(Color.green, 0.2f).OnComplete(() =>
        {
            model.DOColor(Color.white, 0.2f);
        });
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
    public void IncreaseStat(Stat type, float value)
    {
        Debug.Log($"{type} 이 {statDic[type]} 에서 {statDic[type] * value} 로 변경 됨");
        statDic[type] *= value;

        OnUpdateStat?.Invoke(type, value);
    }

    /// <summary>
    /// 손상된 코어를 사용해 일정 시간 동안 전투 능력치를 강화합니다.
    /// amount : 증가 비율 (0.5 = 50%)
    /// duration : 지속 시간
    /// </summary>
    public void UseDamagedCore(float amount, float duration)
    {
        // 이미 사용 중이라면 기존 버프를 먼저 제거
        if (damagedCoreCoroutine != null)
        {
            StopCoroutine(damagedCoreCoroutine);
            RemoveDamagedCoreBuff();
        }

        damagedCoreCoroutine = StartCoroutine(
            DamagedCoreCoroutine(amount, duration)
        );
    }

    private IEnumerator DamagedCoreCoroutine(float amount, float duration)
    {
        damagedCoreBonus.Clear();

        Stat[] increaseStats =
        {
            Stat.MoveSpeed,
    
            Stat.SwordSwingSpeed,
            Stat.SwordDamage,
    
            Stat.BulletSpeed,
            Stat.BulletDamage,
    
            Stat.ProtocolDuration
        };

        // 높을수록 좋아지는 능력치
        foreach (Stat stat in increaseStats)
        {
            float bonus = statDic[stat] * amount;

            damagedCoreBonus.Add(stat, bonus);
            AddStat(stat, bonus);
        }

        Stat[] rateStats =
        {
            Stat.RollRate,
            Stat.SwordSwingRate,
            Stat.BulletFireRate,
            Stat.ProtocolRate
        };

        // 낮을수록 좋아지는 쿨타임 계열
        foreach (Stat stat in rateStats)
        {
            // 50% 강화라면 1 / 1.5배
            float newValue = statDic[stat] / (1f + amount);
            float bonus = newValue - statDic[stat];

            damagedCoreBonus.Add(stat, bonus);
            AddStat(stat, bonus);
        }

        // 연출
        model.DOColor(Color.yellow, 0.2f);

        yield return new WaitForSeconds(duration);

        RemoveDamagedCoreBuff();

        model.DOColor(Color.white, 0.2f);

        damagedCoreCoroutine = null;
    }

    /// <summary>
    /// 손상된 코어로 변경했던 값만 되돌립니다.
    /// 버프 도중 획득한 카드 효과는 유지됩니다.
    /// </summary>
    private void RemoveDamagedCoreBuff()
    {
        foreach (var bonus in damagedCoreBonus)
            AddStat(bonus.Key, -bonus.Value);

        damagedCoreBonus.Clear();
    }

    #region Save
    // 세이브 관련
    public void SaveDataTo(SaveDataStruct data)
    {
        data.statDic = statDic;
    }

    public void LoadDataFrom(SaveDataStruct data)
    {
        statDic = data.statDic;
    }
    #endregion

    #region SpecialStat
    public void SpecialToggle(LevelCardSO.LevelCardType type, float amount)
    {
        bool isA = false;
        switch (type)
        {
            //회복 알고리즘의 경우 처치 시 체력 회복
            case LevelCardSO.LevelCardType.RecoveryAlgorithmA:
            case LevelCardSO.LevelCardType.RecoveryAlgorithmB:
                recoverProbability = amount;
                SectorManager.Instance.OnDestroyedEnemy -= RecoveryAlgorithm;
                SectorManager.Instance.OnDestroyedEnemy += RecoveryAlgorithm;

                break;

            //민첩 알고리즘의 경우 일정 확률로 적 공격 방어
            case LevelCardSO.LevelCardType.EvasionA:
            case LevelCardSO.LevelCardType.EvasionB:
                evasionProbability = amount;
                break;

            //불안정 코어의 경우 일정 확률 연사 및 이속 디버프
            case LevelCardSO.LevelCardType.InstableCore:
                instableCoreProbability = amount;
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
