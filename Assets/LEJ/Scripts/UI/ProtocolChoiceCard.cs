using UnityEngine;

public class ProtocolChoiceCard : MonoBehaviour
{
    public ProtocolCard.Protocol protocol;
    public void SetProtocol()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(Sound_SFX.UISelect_Positive);

        GameManager.Instance.SetProtocol(protocol);
    }
}
