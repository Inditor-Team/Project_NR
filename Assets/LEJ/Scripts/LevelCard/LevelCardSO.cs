using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "LevelCardSO", menuName = "LEJ/LevelCardSO")]
public class LevelCardSO : ScriptableObject
{
    public enum LevelCardType { None = -1, 
        BioreinforcementA = 0, //��ü����
        BioreinforcementB, //��ü����
        RecoveryAlgorithmA, //ȸ�� �˰�����
        RecoveryAlgorithmB, //ȸ�� �˰�����
        EvasionA, //ȸ��
        EvasionB, //ȸ��
        OverheatedMagazine, //���� źâ
        FullAutoProtocol, //���� ��������
        NeuralAccelerationA, //�Ű� ����
        NeuralAccelerationB, //�Ű� ����
        SubGear, //���� ���
        OverClock, //����Ŭ��
        InstableCore //�Ҿ��� �ھ�
        }
    public int Id;
    public Sprite CardIcon;
    public LevelCardType type;
    public LocalizedString CardName;
    public LocalizedString CardDescription;
    public LevelCardElement[] Elements;
    public float Weight;
}

[Serializable]
public class LevelCardElement
{
    public PlayerStat.Stat targetStat;
    public float upgradeAmount;
}
