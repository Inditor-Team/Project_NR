using UnityEngine;
using UnityEngine.UI;

public class ProtocolCoolTimeUI : MonoBehaviour
{
    [SerializeField] ProtocolExecutor protocolExecutor;
    [SerializeField] Image coolTimeFill;
    float rainbowSpeed = 1.0f;
    private float hue;
    [SerializeField] Color originColor;
    bool doRainbow = false;

    private void Awake()
    {
        coolTimeFill.color = originColor;

        if (protocolExecutor != null)
            protocolExecutor.OnTryProtocol += Clear;
    }
    private void Update()
    {
        if (protocolExecutor == null)
            return;

        Fill();
        Rainbow();

        if (coolTimeFill.fillAmount >= 1.0f && !doRainbow)
            doRainbow = true;

        if (coolTimeFill.fillAmount < 1.0f && doRainbow)
        {
            coolTimeFill.color = originColor;
            doRainbow = false;
        }
    }

    void Fill()
    {
        if (coolTimeFill == null || protocolExecutor == null)
            return;

        coolTimeFill.fillAmount = protocolExecutor.CoolTime;
    }
    
    void Clear()
    {
        if (coolTimeFill == null)
            return;

        coolTimeFill.fillAmount = 0;
    }

    void Rainbow()
    {
        if (!doRainbow)
            return;

        hue += Time.deltaTime * rainbowSpeed;
        hue %= 1f;

        coolTimeFill.color = Color.HSVToRGB(hue, 1f, 1f);
    }
}
