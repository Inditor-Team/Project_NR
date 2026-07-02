using System.Collections.Generic;
using UnityEngine;

public abstract class ProtocolBase : MonoBehaviour
{
    internal Dictionary<ProtocolCard.Buff, float> buffValues = new Dictionary<ProtocolCard.Buff, float>();

    public abstract void UpgradeProtocol(ProtocolCard.Buff type, float level);

    internal abstract void TryProtocol();

    internal abstract void DoProtocol();

    internal abstract void EndProtocol();
}
