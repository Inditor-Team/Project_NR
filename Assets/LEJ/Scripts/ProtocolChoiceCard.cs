using UnityEngine;

public class ProtocolChoiceCard : MonoBehaviour
{
    public ProtocolCard.Protocol protocol;
    public void SetProtocol()
    {
        GameManager.Instance.SetProtocol(protocol);
    }
}
