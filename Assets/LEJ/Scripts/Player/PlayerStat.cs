using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

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
    [SerializeField] SpriteRenderer model;
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
        SetStat(Stat.ProtocolRate, 1f);

        SetStat(Stat.Life, 5f);
        SetStat(Stat.MaxLife, 5f);
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
        UpdateStat(Stat.Life, amount);
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible)
            return;

        model.DOColor(Color.red, 0.2f).OnComplete(() =>
        {
            model.DOColor(Color.white, 0.2f);
        });

        UpdateStat(Stat.Life, -damage);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(Sound_SFX.Player_Hit);
    }

    private void SetStat(Stat type, float value)
    {
        statDic[type] = value;
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
