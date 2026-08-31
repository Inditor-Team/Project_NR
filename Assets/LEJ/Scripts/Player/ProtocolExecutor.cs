using System.Collections;
using Unity.VisualScripting;
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

    public event UnityAction OnTryProtocol;

    private void Start()
    {
        coolTime = 0;

        if (GameManager.Instance.CurProtocol != ProtocolCard.Protocol.None)
            SetProtocol(GameManager.Instance.CurProtocol);

        GameManager.Instance.OnProtocolChanged += SetProtocol;
    }

    private void Update()
    {
        if (stat == null)
            return;

        if (coolTime < 1 && GameTime.WorldTimeScale > 0f)
            coolTime += Time.deltaTime / stat.StatDic[PlayerStat.Stat.ProtocolRate];
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
        if (coolTime < 1 || curProtocol == null)
            return;

        DoProtocol();
        OnTryProtocol?.Invoke();

        coolTime = 0f;
    }

    void DoProtocol()
    {
        curProtocol.TryProtocol(stat.StatDic[Stat.ProtocolDuration]);

        //프로토콜이 블레이더일 땐 무적 모드가 됩니다
        if (GameManager.Instance.CurProtocol == ProtocolCard.Protocol.Blader)
            InvincibleMode();
    }

    /// <summary>
    /// 프로토콜 활성화일 때 플레이어가 무적이 됩니다
    /// </summary>
    void InvincibleMode()
    {
        if (InvincibleRoutine != null)
            InvincibleRoutine = null;

        InvincibleRoutine = StartCoroutine(InvincibleTime());
    }

    Coroutine InvincibleRoutine;
    IEnumerator InvincibleTime()
    {
        stat.IsInvincible = true;
        yield return new WaitForSeconds(stat.StatDic[Stat.ProtocolDuration]);
        stat.IsInvincible = false;
        InvincibleRoutine = null;
    }
}
