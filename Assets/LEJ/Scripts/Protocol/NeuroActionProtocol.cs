using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 빠르게 움직여 월드 시간을 느리게 보이게 하는 프로토콜
/// </summary>
public class NeuroActionProtocol : ProtocolBase
{
    [SerializeField] GameObject debug_effect; //임시 시각 효과

    [Header("임시 능력치")]
    [SerializeField] private float newTimeScale = 0.05f;
    [SerializeField] private float newTimeScaleDuration = 5f;

    new Dictionary<ProtocolCard.Buff, float> buffValues = new Dictionary<ProtocolCard.Buff, float>()
    {
        { ProtocolCard.Buff.LessCoolTime, 1f },
        { ProtocolCard.Buff.KillToCool, 1f },
        { ProtocolCard.Buff.DeadmanSwitch, 1f },
        { ProtocolCard.Buff.KillToExtend, 1f },
    };

    //프로토콜 발동 시 스펙트럼 이펙트
    private SpriteRenderer curSprite;
    private SpriteRenderer[] spectrumPool;

    [Header("잔상 이펙트")]
    [SerializeField] private int spectrumPoolSize = 30;
    [SerializeField] private float spectrumFadeDuration = 3f; //잔상 페이드 아웃 시간
    [SerializeField] private float spectrumInterval = 0.5f; //잔상 생성 간격
    private int index = 0;

    private Coroutine[] fadeCoroutines;

    private void Awake()
    {
        spectrumPool = new SpriteRenderer[spectrumPoolSize];
        fadeCoroutines = new Coroutine[spectrumPoolSize];

        for (int i = 0; i < spectrumPoolSize; i++)
        {
            spectrumPool[i] = new GameObject($"SpectrumEffect_{i}").AddComponent<SpriteRenderer>();
            spectrumPool[i].gameObject.SetActive(false);
        }
    }

    internal override void TryProtocol()
    {
        DoProtocol();
    }

    internal override void DoProtocol()
    {
        debug_effect.SetActive(true);
        isActive = true;

        Debug.Log("Player: NeuroAction Protocol! ");
        
        if (protocolRoutine == null)
            protocolRoutine = StartCoroutine(ProtocolTime());
    }

    Coroutine protocolRoutine;
    IEnumerator ProtocolTime()
    {
        GameTime.SetTimeScale(newTimeScale);

        float time = 0f;
        float nextSpectrumTime = 0f;

        while (time < newTimeScaleDuration)
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
        debug_effect.SetActive(false);

        GameTime.SetTimeScale(1f);
        isActive = false;
    }

    public override void UpgradeProtocol(ProtocolCard.Buff type, float level)
    {
        if (!buffValues.ContainsKey(type))
            return;

        buffValues[type] = level;
    }

    private void Effect()
    {
        curSprite = GameManager.Instance.Player.Model;

        SpriteRenderer spectrum = spectrumPool[index];

        // 기존 페이드 중이면 중지
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

        // 알파 초기화
        Color c = Color.white;
        c.a = 0.5f;
        spectrum.color = c;

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
            float alpha = Mathf.Lerp(0.5f, 0f, elapsed / spectrumFadeDuration);

            color.a = alpha;
            spectrum.color = color;

            elapsed += Time.deltaTime;
            yield return null;
        }

        spectrum.gameObject.SetActive(false);
        fadeCoroutines[poolIndex] = null;
    }
}
