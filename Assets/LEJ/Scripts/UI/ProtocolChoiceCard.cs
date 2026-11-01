using UnityEngine;

public class ProtocolChoiceCard : MonoBehaviour
{
    public ProtocolCard.Protocol protocol;
    public void SetProtocol()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.UISelect_Positive);
        GameManager.Instance.SetProtocol(protocol);
    }
}
