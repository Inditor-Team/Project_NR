using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCardSO", menuName = "LEJ/LevelCardSO")]
public class LevelCardSO : ScriptableObject
{
    public enum LevelCardType { None = -1, 
        BioreinforcementA = 0, //생체보강
        BioreinforcementB, //생체보강
        RecoveryAlgorithmA, //회복 알고리즘
        RecoveryAlgorithmB, //회복 알고리즘
        EvasionA, //회피
        EvasionB, //회피
        OverheatedMagazine, //과열 탄창
        FullAutoProtocol, //연사 프로토콜
        NeuralAccelerationA, //신경 가속
        NeuralAccelerationB, //신경 가속
        SubGear, //보조 기어
        OverClock, //오버클럭
        InstableCore //불안정 코어
        }
    public int Id;
    public Sprite CardIcon;
    public LevelCardType type;
    public string CardName;
    public string CardDescription;
    public LevelCardElement[] Elements;
    public float Weight;
}

[Serializable]
public class LevelCardElement
{
    public PlayerStat.Stat targetStat;
    public float upgradeAmount;
}
