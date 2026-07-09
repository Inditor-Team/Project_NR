using System.Collections.Generic;
using UnityEngine;

public abstract class ProtocolBase : MonoBehaviour
{
    /// <summary>
    /// 프로토콜이 실행 중인지 여부
    /// </summary>
    internal bool isActive = false;
    public bool IsActive => isActive;

    internal Dictionary<ProtocolCard.Buff, float> buffValues = new Dictionary<ProtocolCard.Buff, float>();
    internal float speedMultiplier = 1f; //캐릭터의 이속 증가가 있을 때
    public float SpeedMultiplier => speedMultiplier;
    internal bool isInvincible = false; //캐릭터의 무적 상태가 있을 때
    public bool IsInvincible => isInvincible;

    public abstract void UpgradeProtocol(ProtocolCard.Buff type, float level);

    internal abstract void TryProtocol();

    internal abstract void DoProtocol();

    internal abstract void EndProtocol();
}
