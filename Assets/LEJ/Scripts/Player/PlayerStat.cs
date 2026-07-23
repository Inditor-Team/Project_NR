using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

public class PlayerStat : MonoBehaviour, IDamageable
{
    public enum Stat
    {
        None,

        MoveSpeed, //움직임 속도
        RollSpeed, //구르는 속도
        RollDuration, //구르는 기간
        RollRate, //다시 구르기까지의 간격

        SwordSwingSpeed, //칼을 휘두르는 속도
        SwordDamage, //칼 데미지
        SwordSwingRate, //칼을 휘두르고 난 후 다시 휘두를 때까지의 기간

        BulletSpeed, //총알 속도
        BulletDamage, //총알 데미지
        BulletFireRate, //총을 쏜 후 다시 쏠 때까지의 기간

        ProtocolDuration,
        ProtocolRate,

        Life, //생명

        Count
    }

    Dictionary<Stat, float> statDic = new Dictionary<Stat, float>();
    public Dictionary<Stat, float> StatDic => statDic;

    public UnityAction<Stat, float> OnUpdateStat;
    [SerializeField] LayerMask enemyLayer;
    PlayerController playerController;

    void Awake()
    {
        //딕셔너리 초기화
        for (int i = 1; i < (int)Stat.Count; i++)
            statDic.Add((Stat)i, 0f);

        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        SetDefaultStat();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == enemyLayer)
            statDic[Stat.Life] += 0;
    }

    void SetDefaultStat()
    {
        UpdateStat(Stat.MoveSpeed, 3f);
        UpdateStat(Stat.RollSpeed, 6f);
        UpdateStat(Stat.RollDuration, 0.25f);
        UpdateStat(Stat.RollRate, 0.5f);
        UpdateStat(Stat.SwordSwingSpeed, 10f);
        UpdateStat(Stat.SwordDamage, 10f);
        UpdateStat(Stat.SwordSwingRate, 1f);
        UpdateStat(Stat.BulletSpeed, 30f);
        UpdateStat(Stat.BulletDamage, 1f);
        UpdateStat(Stat.BulletFireRate, 0.5f);
        UpdateStat(Stat.ProtocolDuration, 2.5f);
        UpdateStat(Stat.ProtocolRate, 10f);
        UpdateStat(Stat.Life, 5f);
    }

    public void TakeDamage(float damage)
    {
        UpdateStat(Stat.Life, -damage);
    }

    //스탯을 업데이트
    public void UpdateStat(Stat type, float value)
    {
        statDic[type] += value;
        OnUpdateStat?.Invoke(type, value);
    }
}
