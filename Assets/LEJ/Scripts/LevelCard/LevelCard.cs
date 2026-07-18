using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCard", menuName = "LEJ/LevelCard")]
public class LevelCard : ScriptableObject
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
