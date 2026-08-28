using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 빠르게 움직여 월드 시간을 느리게 보이게 하는 프로토콜
/// </summary>
public class NeuroActionProtocol : ProtocolBase
{
    [SerializeField] SpriteRenderer playerModel;

    [Header("임시 능력치")]
    [SerializeField] private float newTimeScale = 0.05f;
    float duration;

    //프로토콜 발동 시 스펙트럼 이펙트
    private SpriteRenderer curSprite;
    private SpriteRenderer[] spectrumPool;

    [Header("잔상 이펙트")]
    [SerializeField] private int spectrumPoolSize = 30;
    [SerializeField] private float spectrumFadeDuration = 3f; //잔상 페이드 아웃 시간
    [SerializeField] private float spectrumInterval = 0.5f; //잔상 생성 간격
    private int index = 0;

    private Coroutine[] fadeCoroutines;

    private void Start()
    {
        InitEffect();
    }

    internal override void TryProtocol(float duration)
    {
        this.duration = duration;
        DoProtocol();
    }

    internal override void DoProtocol()
    {
        isActive = true;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(Sound_SFX.Player_NeuroAction);

        if (protocolRoutine == null)
            protocolRoutine = StartCoroutine(ProtocolTime());
    }

    Coroutine protocolRoutine;
    IEnumerator ProtocolTime()
    {
        GameTime.SetTimeScale(newTimeScale);

        float time = 0f;
        float nextSpectrumTime = 0f;

        while (time < duration)
        {
            if (time >= nextSpectrumTime)
            {
                Effect();
                nextSpectrumTime += spectrumInterval;
            }

            yield return null;
            time += Time.deltaTime;
        }

        protocolRoutine = null;
        EndProtocol();
    }

    internal override void EndProtocol()
    {
        GameTime.SetTimeScale(1f);
        isActive = false;
        colorTime = 0f;
    }

    public override void UpgradeProtocol(ProtocolCard.Buff type, float level)
    {
        if (!buffValues.ContainsKey(type))
            return;

        buffValues[type] = level;
    }

    void InitEffect()
    {
        if (GameManager.Instance.CurProtocol != ProtocolCard.Protocol.NeuroAction)
            return;

        spectrumPool = new SpriteRenderer[spectrumPoolSize];
        fadeCoroutines = new Coroutine[spectrumPoolSize];

        for (int i = 0; i < spectrumPoolSize; i++)
        {
            spectrumPool[i] = new GameObject($"neuro action effect {i}").AddComponent<SpriteRenderer>();
            spectrumPool[i].gameObject.SetActive(false);
        }
    }

    float colorTime = 0f;
    [SerializeField] float colorSpeed = 0.02f;

    private void Effect()
    {
        curSprite = playerModel;

        SpriteRenderer spectrum = spectrumPool[index];

        if (fadeCoroutines[index] != null)
            StopCoroutine(fadeCoroutines[index]);

        spectrum.gameObject.SetActive(true);

        spectrum.sprite = curSprite.sprite;
        spectrum.flipX = curSprite.flipX;
        spectrum.flipY = curSprite.flipY;

        spectrum.transform.position = curSprite.transform.position;
        spectrum.transform.rotation = curSprite.transform.rotation;
        spectrum.transform.localScale = curSprite.transform.lossyScale;

        spectrum.sortingLayerID = curSprite.sortingLayerID;
        spectrum.sortingOrder = curSprite.sortingOrder - 1;

        Color magenta = new Color(1f, 0f, 1f);
        Color lime = new Color(0.5f, 1f, 0f);
        Color cyan = new Color(0f, 1f, 1f);

        colorTime += spectrumInterval * colorSpeed;

        float t = colorTime % 3f;

        Color color;

        if (t < 1f)
            color = Color.Lerp(magenta, lime, t);
        else if (t < 2f)
            color = Color.Lerp(lime, cyan, t - 1f);
        else
            color = Color.Lerp(cyan, magenta, t - 2f);

        color.a = 0.5f;
        spectrum.color = color;

        fadeCoroutines[index] = StartCoroutine(SpectrumFadeTime(index));

        index = (index + 1) % spectrumPoolSize;
    }

    /// <summary>
    /// 스펙트럼 잔상에 대해 페이드 아웃 효과를 적용합니다
    /// </summary>
    /// <param name="spectrum"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator SpectrumFadeTime(int poolIndex)
    {
        SpriteRenderer spectrum = spectrumPool[poolIndex];

        float elapsed = 0f;

        Color color = spectrum.color;

        while (elapsed < spectrumFadeDuration)
        {
            float alpha = Mathf.Lerp(
                0.5f,
                0f,
                elapsed / spectrumFadeDuration
            );

            color.a = alpha;
            spectrum.color = color;

            elapsed += Time.deltaTime;

            yield return null;
        }

        spectrum.gameObject.SetActive(false);
        fadeCoroutines[poolIndex] = null;
    }
}
