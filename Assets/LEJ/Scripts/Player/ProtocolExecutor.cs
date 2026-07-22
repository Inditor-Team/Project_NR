using UnityEngine;
using UnityEngine.Events;
using static PlayerStat;

public class ProtocolExecutor : MonoBehaviour
{
    [SerializeField] ProtocolBase protocol;
    PlayerStat stat;
    float coolTime = 0;
    public float CoolTime => coolTime;

    public UnityAction OnTryProtocol;

    private void Update()
    {
        if (stat == null)
            return;

        if (coolTime < 1)
            coolTime += Time.deltaTime / stat.StatDic[PlayerStat.Stat.ProtocolRate];
    }

    public void RegisterStat(PlayerStat stat)
    {
        this.stat = stat;
    }

    public void TryProtocol()
    {
        if (coolTime < 1)
            return;

        if (protocol == null)
            return;

        DoProtocol();
        OnTryProtocol?.Invoke();

        coolTime = 0f;
    }

    void DoProtocol()
    {
        protocol.TryProtocol(stat.StatDic[Stat.ProtocolDuration]);
    }
}
