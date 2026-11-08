using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Stat = PlayerStat.Stat;

/// <summary>
/// 씬을 넘어 유지되는 플레이어 스탯을 관리하고 세이브/로드를 담당
/// - entryStats   : 스테이지 입장 시 스냅샷 (클리어 전 저장 값, 실패 시 복귀 기준)
/// - carriedStats : 다음 씬의 PlayerStat 이 이어받을 값
/// 저장 규칙
/// - 스테이지 클리어 전 : entryStats
/// - 클리어 후 / 스테이지가 아닌 씬 : 현재 플레이어의 영구 스탯 (일시 버프 제외)
/// - 플레이어가 없는 씬 (맵 분기) : carriedStats
/// </summary>
[DefaultExecutionOrder(-100)]
public class StatDataManager : MonoBehaviour, ISaveable
{
    public static StatDataManager Instance { get; private set; }

    private readonly Dictionary<Stat, float> defaultStats = new();
    private readonly Dictionary<Stat, float> entryStats = new();
    private readonly Dictionary<Stat, float> carriedStats = new();

    //현재 씬의 플레이어. 맵 분기 씬처럼 플레이어가 없는 씬이나 로드 직후에는 null
    private PlayerStat livePlayer;

    /// <summary>
    /// 플레이어가 없는 씬에서 carriedStats 가 바뀌었을 때 (스탯, 바뀐 뒤 값)
    /// 플레이어가 있는 씬에서는 PlayerStat.OnStatChanged 를 사용
    /// </summary>
    public event UnityAction<Stat, float> OnCarriedStatChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetDefaultStats(defaultStats);
        CopyStats(defaultStats, entryStats);
        CopyStats(defaultStats, carriedStats);
    }

    private void Start()
    {
        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;

        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.Unregister(this);
    }

    #region Default
    private static void SetDefaultStats(Dictionary<Stat, float> stats)
    {
        stats.Clear();
        for (int i = 1; i < (int)Stat.Count; i++)
            stats.Add((Stat)i, 0f);

        stats[Stat.MoveSpeed] = 3f;
        stats[Stat.RollSpeed] = 3.5f;
        stats[Stat.RollDuration] = 0.3f;
        stats[Stat.RollRate] = 0.5f;

        stats[Stat.SwordSwingSpeed] = 5f;
        stats[Stat.SwordDamage] = 5f;
        stats[Stat.SwordSwingRate] = 0.5f;

        stats[Stat.BulletSpeed] = 30f;
        stats[Stat.BulletDamage] = 1f;
        stats[Stat.BulletFireRate] = 0.5f;

        stats[Stat.ProtocolDuration] = 3f;
        stats[Stat.ProtocolRate] = 10f;

        stats[Stat.Life] = 5f;
        stats[Stat.MaxLife] = 5f;

        stats[Stat.AdditionalDamage] = 0f;
        
        stats[Stat.RecoverProbability] = 0f;
        stats[Stat.EvasionProbability] = 0f;
        stats[Stat.InstableCoreProbability] = 0f;
    }

    private static void CopyStats(Dictionary<Stat, float> from, Dictionary<Stat, float> to)
    {
        to.Clear();
        foreach (var pair in from)
            to.Add(pair.Key, pair.Value);
    }

    /// 새 게임 시작 시 호출. 모든 스탯을 기본값으로
    public void ResetToDefault()
    {
        CopyStats(defaultStats, entryStats);
        CopyStats(defaultStats, carriedStats);

        if (livePlayer != null)
            livePlayer.ApplyStats(carriedStats);
    }
    #endregion

    #region State
    
    // 스테이지 씬이면서 아직 클리어하지 않은 상태인지
    public static bool IsUncommittedStage()
    {
        if (SceneController.Instance == null)
            return false;

        SceneController.Scene scene = SceneController.Instance.curScene;
        if (!SceneController.IsStageScene(scene))
            return false;

        bool cleared = GameManager.Instance.ClearedSector.TryGetValue(scene, out bool value) && value;
        return !cleared;
    }

    // PlayerStat.Awake 에서 호출. 이어받을 스탯으로 플레이어를 초기화합니다
    public void Register(PlayerStat player)
    {
        livePlayer = player;
        livePlayer.ApplyStats(carriedStats);
    }

    public void Unregister(PlayerStat player)
    {
        if (livePlayer == player)
            livePlayer = null;
    }

    // 스테이지 입장 직전에 호출 (SceneController.ChangeScene)
    public void CaptureStageEntryState()
    {
        CopyStats(carriedStats, entryStats);
    }

    // 씬을 나가기 직전, curScene 이 바뀌기 전에 호출 (SceneController.ChangeScene)
    public void OnLeaveScene()
    {
        //플레이어가 없는 씬은 carriedStats 를 직접 수정했으므로 처리 없음
        //로드 직후에도 연결이 끊겨 있어 로드한 값이 보존됨
        if (livePlayer == null)
            return;

        if (IsUncommittedStage())
        {
            //클리어 전 이탈 (실패 포함) : 입장 시 상태로 복귀
            CopyStats(entryStats, carriedStats);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.RestoreStageEntryState();
        }
        else
        {
            //클리어 후 / 스테이지가 아닌 씬 : 현재 스탯 확정
            CopyStats(livePlayer.GetPersistentSnapshot(), carriedStats);
        }
    }
    #endregion

    #region Stat
    // 현재 스탯 값
    public float GetStat(Stat type)
    {
        if (livePlayer != null)
            return livePlayer.StatDic[type];

        return carriedStats[type];
    }

    public void AddStat(Stat type, float value)
    {
        if (livePlayer != null)
        {
            livePlayer.AddStat(type, value);
            return;
        }

        carriedStats[type] += value;
        NotifyCarriedChanged(type);
    }

    public void IncreaseStat(Stat type, float multiplier, bool playSound = true)
    {
        if (livePlayer != null)
        {
            livePlayer.IncreaseStat(type, multiplier, playSound);
            return;
        }

        carriedStats[type] *= multiplier;
        NotifyCarriedChanged(type);

        if (playSound)
            SoundManager.Instance.PlaySFX(Sound_SFX.HP_UpB);
    }

    public void SetSpecialProbability(Stat type, float value)
    {
        if (livePlayer != null)
        {
            livePlayer.SetSpecialProbability(type, value);
            return;
        }

        carriedStats[type] = value;
        NotifyCarriedChanged(type);
    }

    private void NotifyCarriedChanged(Stat type)
    {
        //최대 체력 이상으로 가질 수 없음
        bool lifeClamped = false;
        if (carriedStats[Stat.Life] > carriedStats[Stat.MaxLife])
        {
            carriedStats[Stat.Life] = carriedStats[Stat.MaxLife];
            lifeClamped = true;
        }

        OnCarriedStatChanged?.Invoke(type, carriedStats[type]);

        if (lifeClamped && type != Stat.Life)
            OnCarriedStatChanged?.Invoke(Stat.Life, carriedStats[Stat.Life]);
    }
    #endregion

    #region Card Effect
    // 카드 효과를 적용
    public void ApplyCardEffect(int cardId) => ProcessCardEffect(cardId, true);

    // 슬롯에서 밀려난 카드의 효과를 되돌림
    public void RevertCardEffect(int cardId) => ProcessCardEffect(cardId, false);

    private void ProcessCardEffect(int cardId, bool apply)
    {
        if (cardId < 0)
            return;

        var cardDic = LevelCardData.Instance.LevelCardDic;
        if (!cardDic.TryGetValue(cardId, out var card))
        {
            Debug.LogWarning($"[StatDataManager] 카드 데이터를 찾을 수 없습니다 : {cardId}");
            return;
        }

        var cardType = (LevelCardSO.LevelCardType)cardId;

        // 특수 카드는 첫 번째 요소의 upgradeAmount 를 확률로 사용
        float specialAmount = 0f;
        foreach (var element in card.Elements)
        {
            if (element == null)
                continue;

            specialAmount = element.upgradeAmount;
            break;
        }

        switch (cardType)
        {
            // 배율 카드 (적용 시 곱하기, 해제 시 같은 배율로 나누기)
            case LevelCardSO.LevelCardType.OverheatedMagazine:
            case LevelCardSO.LevelCardType.OverClock:
            case LevelCardSO.LevelCardType.FullAutoProtocol:
            case LevelCardSO.LevelCardType.SubGear:
                foreach (var element in card.Elements)
                {
                    if (element == null)
                        continue;

                    if (apply)
                        IncreaseStat(element.targetStat, element.upgradeAmount);
                    else if (!Mathf.Approximately(element.upgradeAmount, 0f))
                        IncreaseStat(element.targetStat, 1f / element.upgradeAmount, playSound: false);
                }
                break;

            // 회복 알고리즘 (처치 시 일정 확률로 체력 회복)
            case LevelCardSO.LevelCardType.RecoveryAlgorithmA:
            case LevelCardSO.LevelCardType.RecoveryAlgorithmB:
                SetSpecialProbability(Stat.RecoverProbability, apply ? specialAmount : 0f);
                break;

            // 민첩 알고리즘 (일정 확률로 적 공격 방어)
            case LevelCardSO.LevelCardType.EvasionA:
            case LevelCardSO.LevelCardType.EvasionB:
                SetSpecialProbability(Stat.EvasionProbability, apply ? specialAmount : 0f);
                break;

            // 불안정 코어 (일정 확률 연사 및 이속 디버프)
            case LevelCardSO.LevelCardType.InstableCore:
                SetSpecialProbability(Stat.InstableCoreProbability, apply ? specialAmount : 0f);
                break;

            case LevelCardSO.LevelCardType.BioreinforcementA:
            case LevelCardSO.LevelCardType.BioreinforcementB:
                break;

            case LevelCardSO.LevelCardType.NeuralAccelerationA:
            case LevelCardSO.LevelCardType.NeuralAccelerationB:
                break;
        }

        Debug.Log($"{cardType} 효과 {(apply ? "적용" : "해제")}");
    }
    #endregion

    #region Save
    public void SaveDataTo(SaveDataStruct data)
    {
        Dictionary<Stat, float> source;

        if (IsUncommittedStage())
            source = entryStats;
        else if (livePlayer != null)
            source = livePlayer.GetPersistentSnapshot();
        else
            source = carriedStats;

        if (data.statDic == null)
            data.statDic = new Dictionary<Stat, float>();

        foreach (var pair in source)
            data.statDic[pair.Key] = pair.Value;

        data.statDic[Stat.None] = 1f;
    }

    public void LoadDataFrom(SaveDataStruct data)
    {
        //세이브에 없는 키는 기본값으로
        CopyStats(defaultStats, carriedStats);

        if (data.statDic != null)
        {
            foreach (var pair in data.statDic)
            {
                if (carriedStats.ContainsKey(pair.Key))
                    carriedStats[pair.Key] = pair.Value;
            }
        }

        CopyStats(carriedStats, entryStats);

        //로드 후 ChangeScene 에서 이전 씬 플레이어의 값이 덮어쓰지 않도록 연결 해제
        livePlayer = null;
    }
    #endregion
}