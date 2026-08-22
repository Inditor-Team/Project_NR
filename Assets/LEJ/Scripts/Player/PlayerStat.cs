using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStat : MonoBehaviour, IDamageable
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

        Count
    }

    Dictionary<Stat, float> statDic = new Dictionary<Stat, float>();
    public Dictionary<Stat, float> StatDic => statDic;

    public event UnityAction<Stat, float> OnUpdateStat;
    [SerializeField] LayerMask enemyLayer;
    PlayerController playerController;

    void Awake()
    {
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
            TakeDamage(100);
    }

    void SetDefaultStat()
    {
        UpdateStat(Stat.MoveSpeed, 3f);
        UpdateStat(Stat.RollSpeed, 10f);
        UpdateStat(Stat.RollDuration, 0.3f);
        UpdateStat(Stat.RollRate, 0.5f);
        UpdateStat(Stat.SwordSwingSpeed, 5f);
        UpdateStat(Stat.SwordDamage, 10f);
        UpdateStat(Stat.SwordSwingRate, 0.5f);
        UpdateStat(Stat.BulletSpeed, 30f);
        UpdateStat(Stat.BulletDamage, 1f);
        UpdateStat(Stat.BulletFireRate, 0.5f);
        UpdateStat(Stat.ProtocolDuration, 2.5f);
        UpdateStat(Stat.ProtocolRate, 10f);
        UpdateStat(Stat.Life, 5f);
        UpdateStat(Stat.MaxLife, 5f);
    }

    public void EarnLife(float amount)
    {
        UpdateStat(Stat.Life, amount);
    }

    public void TakeDamage(float damage)
    {
        UpdateStat(Stat.Life, -damage);
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(Sound_SFX.Player_Hit);
    }

    public void UpdateStat(Stat type, float value)
    {
        if (type == Stat.RollRate || type == Stat.SwordSwingRate || type == Stat.BulletFireRate || type == Stat.ProtocolRate)
        {
            //배율 증가 또는 감소
            statDic[type] *= value;
        }
        else
            statDic[type] += value;
        OnUpdateStat?.Invoke(type, value);
    }
}
