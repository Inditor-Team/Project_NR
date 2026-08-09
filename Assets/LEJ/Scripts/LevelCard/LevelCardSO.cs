using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCardSO", menuName = "LEJ/LevelCardSO")]
public class LevelCardSO : ScriptableObject
{
    public enum LevelCardType { None, Attack, Shield, Move, Special, Count}
    public string Id;
    public LevelCardType type;
    public string CardName;
    public string CardDescription;
    public float Probability;
    public LevelCardElement[] Elements;
}

[Serializable]
public class LevelCardElement
{
    public PlayerStat.Stat targetStat;
    public float upgradeAmount;
}
