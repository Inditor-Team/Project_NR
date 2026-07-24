using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New ProtocolCard", menuName = "LEJ/ProtocolCard")]
public class ProtocolCard : ScriptableObject
{
    [Serializable]
    public enum Protocol
    {
        None,
        NeuroAction,
        Blader,
        Blitz,
        All
    }

    public enum Buff
    {
        None,
        LessCoolTime,
        KillToCool,
        DeadmanSwitch,
        KillToExtend,
        BloodLeak,
        LifeSteal,
        Count
    }

    public string CardID;
    public string CardName;
    public string Description;

    public Protocol TargetProtocol;
    public Buff BuffType;
    public float BuffValue;
    public float DebuffValue;
    public float DropWeight;
    public bool Overlap;
}
