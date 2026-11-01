using UnityEngine;

/// <summary>
/// �ִϸ��̼� �󿡼� ȣ�� �˴ϴ�
/// </summary>
public class PlayProtocolGlassSound : MonoBehaviour
{
    public void Play()
    {
        SoundManager.Instance.PlaySFX(Sound_SFX.ProtocolGlass);
    }    
}
