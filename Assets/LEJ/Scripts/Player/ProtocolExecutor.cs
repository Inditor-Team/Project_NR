using UnityEngine;
using UnityEngine.Events;
using static PlayerStat;

public class ProtocolExecutor : MonoBehaviour
{
    [SerializeField] NeuroActionProtocol neuroAction;
    [SerializeField] BlitzProtocol blitz;
    [SerializeField] BladerProtocol blader;
    private ProtocolBase curProtocol = null;
    PlayerStat stat;
    float coolTime = 0;
    public float CoolTime => coolTime;

    public UnityAction OnTryProtocol;

    private void Start()
    {
        if (GameManager.Instance.CurProtocol != ProtocolCard.Protocol.None)
            SetProtocol(GameManager.Instance.CurProtocol);

        GameManager.Instance.OnProtocolChanged += SetProtocol;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnProtocolChanged -= SetProtocol;
    }

    private void Update()
    {
        if (stat == null)
            return;

        if (coolTime < 1)
        {
            if(GameTime.WorldTimeScale > 0f) // 게임 시간이 정지되어 있으면 쿨타임도 정지  
                coolTime += Time.deltaTime / stat.StatDic[PlayerStat.Stat.ProtocolRate];
        }
    }

    public void RegisterStat(PlayerStat stat)
    {
        this.stat = stat;
    }

    public void SetProtocol(ProtocolCard.Protocol protocol)
    {
        switch (protocol)
        {
            case ProtocolCard.Protocol.NeuroAction:
                curProtocol = neuroAction;
                break;
                case ProtocolCard.Protocol.Blitz:
                curProtocol = blitz;
                break;
            case ProtocolCard.Protocol.Blader:
                curProtocol = blader;
                break;
        }
    }

    public void SetProtocol()
    {
        switch (GameManager.Instance.CurProtocol)
        {
            case ProtocolCard.Protocol.NeuroAction:
                curProtocol = neuroAction;
                break;
            case ProtocolCard.Protocol.Blitz:
                curProtocol = blitz;
                break;
            case ProtocolCard.Protocol.Blader:
                curProtocol = blader;
                break;
        }
    }

    public void TryProtocol()
    {
        if (coolTime < 1)
            return;

        if (curProtocol == null)
            return;

        DoProtocol();
        OnTryProtocol?.Invoke();

        coolTime = 0f;
    }

    void DoProtocol()
    {
        curProtocol.TryProtocol(stat.StatDic[Stat.ProtocolDuration]);
    }
}
