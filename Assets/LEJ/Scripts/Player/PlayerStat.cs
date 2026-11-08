using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PlayerStat : MonoBehaviour, IDamageable
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

        //특수 카드 효과 (확률). 새 스탯은 반드시 Count 바로 앞에 추가 (세이브 호환)
        RecoverProbability, //회복 알고리즘
        EvasionProbability, //민첩 알고리즘
        InstableCoreProbability, //불안정 코어

        Count // 스탯 항목 수 
    }

    Dictionary<Stat, float> statDic = new Dictionary<Stat, float>();
    public Dictionary<Stat, float> StatDic => statDic;

    public event UnityAction<Stat, float> OnUpdateStat; //(스탯, 변화량 또는 배율) 기존 이벤트
    public event UnityAction<Stat, float> OnStatChanged; //(스탯, 바뀐 뒤 값) 스탯 UI 용
    public event UnityAction OnStatsRefreshed; //전체 스탯이 새로 설정, 스탯 UI 용

    [SerializeField] LayerMask enemyLayer;
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model; 
    bool isInvincible = false; //무적 상태
    public bool IsInvincible { get { return isInvincible; } set { isInvincible = value; } }

    private Coroutine damagedCoreCoroutine;
    private readonly Dictionary<Stat, float> damagedCoreBonus = new();
    private float damagedCoreAmount;

    //손상된 코어, 높을수록 좋아지는 능력치
    private static readonly Stat[] DamagedCoreIncreaseStats =
    {
        Stat.MoveSpeed,

        Stat.SwordSwingSpeed,
        Stat.SwordDamage,

        Stat.BulletSpeed,
        Stat.BulletDamage,

        Stat.ProtocolDuration
    };

    //손상된 코어, 낮을수록 좋아지는 쿨타임 계열
    private static readonly Stat[] DamagedCoreRateStats =
    {
        Stat.RollRate,
        Stat.SwordSwingRate,
        Stat.BulletFireRate,
        Stat.ProtocolRate
    };

    void Awake()
    {
        for (int i = 1; i < (int)Stat.Count; i++)
            statDic.Add((Stat)i, 0f);

        StatDataManager.Instance.Register(this); // 스탯 이어받기
    }

    private void OnDestroy()
    {
        if (RecoverProbability > 0f && SectorManager.Instance != null) //회복 알고리즘 카드가 있어 이벤트 등록이 됐었다면 해지
            SectorManager.Instance.OnDestroyedEnemy -= RecoveryAlgorithm;

        StatDataManager.Instance.Unregister(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((enemyLayer.value & (1 << collision.gameObject.layer)) != 0)
            TakeDamage(100);
    }

    // 전체 스탯 설정
    public void ApplyStats(Dictionary<Stat, float> stats)
    {
        for (int i = 1; i < (int)Stat.Count; i++)
        {
            Stat type = (Stat)i;
            if (stats.TryGetValue(type, out float value))
                statDic[type] = value;
        }

        RefreshRecoverySubscription();
        OnStatsRefreshed?.Invoke();
    }
    
    // 저장, 씬 이동용 스탯. 일시 버프 제외
    public Dictionary<Stat, float> GetPersistentSnapshot()
    {
        var snapshot = new Dictionary<Stat, float>(statDic);

        foreach (var bonus in damagedCoreBonus)
            snapshot[bonus.Key] -= bonus.Value;

        return snapshot;
    }

    public void EarnLife(float amount)
    {
        AddStat(Stat.Life, amount);
        SoundManager.Instance.PlaySFX(Sound_SFX.HP_UpA);

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

        if (Random.value < EvasionProbability) //카드 얻기 전 확률 0
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
        SoundManager.Instance.PlaySFX(Sound_SFX.Player_Hit);
    }

    /// <summary>
    /// 스탯에 값을 더합니다
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void AddStat(Stat type, float value)
    {
        statDic[type] += value;

        AfterStatChanged(type, value);
    }

    /// <summary>
    /// 스탯에 값 배율 증가
    /// 손상된 코어 버프 중이라면 버프를 뺀 값에 배율을 적용, 버프는 새 값 기준으로 다시 계산
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value">배율</param>
    /// <param name="playSound">카드 효과 해제처럼 스탯이 내려가는 경우 false</param>
    public void IncreaseStat(Stat type, float value, bool playSound = true)
    {
        bool hasBuff = damagedCoreBonus.TryGetValue(type, out float oldBonus);

        float newBase = (statDic[type] - (hasBuff ? oldBonus : 0f)) * value;
        float newBonus = 0f;

        if (hasBuff)
        {
            newBonus = CalcDamagedCoreBonus(type, newBase);
            damagedCoreBonus[type] = newBonus;
        }

        Debug.Log($"{type} 이 {statDic[type]} 에서 {newBase + newBonus} 로 변경 됨");
        statDic[type] = newBase + newBonus;

        AfterStatChanged(type, value);

        if (playSound)
            SoundManager.Instance.PlaySFX(Sound_SFX.HP_UpB); //스탯 업에 대한 사운드
    }

    // 스탯 변경 후 체력 보정과 이벤트 호출
    private void AfterStatChanged(Stat type, float eventValue)
    {
        //최대 체력 이상으로 가질 수 없음
        float lifeDelta = 0f;
        if (statDic[Stat.Life] > statDic[Stat.MaxLife])
        {
            lifeDelta = statDic[Stat.MaxLife] - statDic[Stat.Life];
            statDic[Stat.Life] = statDic[Stat.MaxLife];
        }

        OnUpdateStat?.Invoke(type, eventValue);
        OnStatChanged?.Invoke(type, statDic[type]);

        if (lifeDelta != 0f && type != Stat.Life)
        {
            OnUpdateStat?.Invoke(Stat.Life, lifeDelta);
            OnStatChanged?.Invoke(Stat.Life, statDic[Stat.Life]);
        }
    }

    #region DamagedCore
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
        damagedCoreAmount = amount;

        foreach (Stat stat in DamagedCoreIncreaseStats)
            AddDamagedCoreBonus(stat);

        foreach (Stat stat in DamagedCoreRateStats)
            AddDamagedCoreBonus(stat);

        // 연출
        model.DOColor(Color.yellow, 0.2f);

        yield return new WaitForSeconds(duration);

        RemoveDamagedCoreBuff();

        model.DOColor(Color.white, 0.2f);

        damagedCoreCoroutine = null;
    }

    private void AddDamagedCoreBonus(Stat stat)
    {
        float bonus = CalcDamagedCoreBonus(stat, statDic[stat]);

        damagedCoreBonus.Add(stat, bonus);
        AddStat(stat, bonus);
    }

    // 버프를 뺀 원래 값 기준으로 손상된 코어 보너스를 계산
    private float CalcDamagedCoreBonus(Stat stat, float baseValue)
    {
        // 낮을수록 좋아지는 쿨타임 계열 : 50% 강화라면 1 / 1.5배
        if (System.Array.IndexOf(DamagedCoreRateStats, stat) >= 0)
            return baseValue / (1f + damagedCoreAmount) - baseValue;

        // 높을수록 좋아지는 능력치
        return baseValue * damagedCoreAmount;
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
    #endregion

    #region SpecialStat
    float RecoverProbability => statDic[Stat.RecoverProbability];
    float EvasionProbability => statDic[Stat.EvasionProbability];
    public float InstableCoreProbability => statDic[Stat.InstableCoreProbability];

    // 기존 호출 형태 유지용. 카드 효과는 StatDataManager.ApplyCardEffect 에서 처리
    public void SpecialToggle(LevelCardSO.LevelCardType type, float amount)
    {
        switch (type)
        {
            //회복 알고리즘의 경우 처치 시 체력 회복
            case LevelCardSO.LevelCardType.RecoveryAlgorithmA:
            case LevelCardSO.LevelCardType.RecoveryAlgorithmB:
                SetSpecialProbability(Stat.RecoverProbability, amount);
                break;

            //민첩 알고리즘의 경우 일정 확률로 적 공격 방어
            case LevelCardSO.LevelCardType.EvasionA:
            case LevelCardSO.LevelCardType.EvasionB:
                SetSpecialProbability(Stat.EvasionProbability, amount);
                break;

            //불안정 코어의 경우 일정 확률 연사 및 이속 디버프
            case LevelCardSO.LevelCardType.InstableCore:
                SetSpecialProbability(Stat.InstableCoreProbability, amount);
                break;
        }
    }

    // 특수 카드 확률을 설정, 0 이면 효과 해제
    public void SetSpecialProbability(Stat type, float value)
    {
        statDic[type] = value;

        if (type == Stat.RecoverProbability)
            RefreshRecoverySubscription();

        OnStatChanged?.Invoke(type, value);
    }

    // 회복 확률에 맞춰 적 처치 이벤트 등록
    void RefreshRecoverySubscription()
    {
        if (SectorManager.Instance == null) //로비 등 섹터가 없는 씬
            return;

        SectorManager.Instance.OnDestroyedEnemy -= RecoveryAlgorithm;

        if (RecoverProbability > 0f)
            SectorManager.Instance.OnDestroyedEnemy += RecoveryAlgorithm;
    }

    void RecoveryAlgorithm()
    {
        if (Random.value < RecoverProbability)
        {
            //시각적 효과
            model.DOColor(Color.green, 0.2f).OnComplete(() =>
            {
                model.DOColor(Color.white, 0.2f);
            });

            AddStat(Stat.Life, 1);
        }
    }
    #endregion
}