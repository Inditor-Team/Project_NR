using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStat : MonoBehaviour, IDamageable
{
    public enum Stat
    {
        None,

        MoveSpeed, //������ �ӵ�
        RollSpeed, //������ �ӵ�
        RollDuration, //������ �Ⱓ
        RollRate, //�ٽ� ����������� ����

        SwordSwingSpeed, //Į�� �ֵθ��� �ӵ�
        SwordDamage, //Į ������
        SwordSwingRate, //Į�� �ֵθ��� �� �� �ٽ� �ֵθ� �������� �Ⱓ

        BulletSpeed, //�Ѿ� �ӵ�
        BulletDamage, //�Ѿ� ������
        BulletFireRate, //���� �� �� �ٽ� �� �������� �Ⱓ

        Life, //����

        Count
    }

    Dictionary<Stat, float> statDic = new Dictionary<Stat, float>();
    public Dictionary<Stat, float> StatDic => statDic;

    public UnityAction<Stat, float> OnUpdateStat;

    void Awake()
    {
        //��ųʸ� �ʱ�ȭ
        for (int i = 1; i < (int)Stat.Count; i++)
            statDic.Add((Stat)i, 0f);
    }

    void Start()
    {
        SetDefaultStat();
    }

    void SetDefaultStat()
    {
        UpdateStat(Stat.MoveSpeed, 3f);
        UpdateStat(Stat.RollSpeed, 6f);
        UpdateStat(Stat.RollDuration, 0.25f);
        UpdateStat(Stat.RollRate, 0.5f);
        UpdateStat(Stat.SwordSwingSpeed, 5f);
        UpdateStat(Stat.SwordDamage, 1f);
        UpdateStat(Stat.SwordSwingRate, 1f);
        UpdateStat(Stat.BulletSpeed, 30f);
        UpdateStat(Stat.BulletDamage, 1f);
        UpdateStat(Stat.BulletFireRate, 0.5f);
        UpdateStat(Stat.Life, 5f);
    }

    public void TakeDamage(float damage)
    {
        UpdateStat(Stat.Life, -damage);
        SoundManager.Instance.PlaySFX(Sound_SFX.Player_Hit);

        if (statDic[Stat.Life] <= 0)
            Die();
    }

    void Die()
    {

    }

    //������ ������Ʈ
    public void UpdateStat(Stat type, float value)
    {
        statDic[type] += value;
        OnUpdateStat?.Invoke(type, value);
    }
}
