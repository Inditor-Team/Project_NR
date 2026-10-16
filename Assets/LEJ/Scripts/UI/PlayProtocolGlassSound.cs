using UnityEngine;

/// <summary>
/// 애니메이션 상에서 호출 됩니다
/// </summary>
public class PlayProtocolGlassSound : MonoBehaviour
{
    public void Play()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(Sound_SFX.ProtocolGlass);
    }    
}
