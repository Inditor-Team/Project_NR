using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ ������ ���� �ð��� ������ ���̰� �ϴ� ��������
/// </summary>
public class NeuroActionProtocol : ProtocolBase
{
    [SerializeField] SpriteRenderer playerModel;

    [Header("�ӽ� �ɷ�ġ")]
    [SerializeField] private float newTimeScale = 0.05f;
    float duration;

    //�������� �ߵ� �� ����Ʈ�� ����Ʈ
    private SpriteRenderer curSprite;
    private SpriteRenderer[] spectrumPool;

    [Header("�ܻ� ����Ʈ")]
    [SerializeField] private int spectrumPoolSize = 30;
    [SerializeField] private float spectrumFadeDuration = 3f; //�ܻ� ���̵� �ƿ� �ð�
    [SerializeField] private float spectrumInterval = 0.5f; //�ܻ� ���� ����
    private int index = 0;

    private Coroutine[] fadeCoroutines;

    private void Start()
    {
        InitEffect();

        GameManager.Instance.OnProtocolChanged += InitEffect; //������
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnProtocolChanged -= InitEffect;
    }

    internal override void TryProtocol(float duration)
    {
        this.duration = duration;
        DoProtocol();
    }

    internal override void DoProtocol()
    {
        isActive = true;
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
    }

    public override void UpgradeProtocol(ProtocolCard.Buff type, float level)
    {
        if (!buffValues.ContainsKey(type))
            return;

        buffValues[type] = level;
    }

    void InitEffect()
    {
        if (spectrumPool != null)
            return;

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

        Color color = Color.limeGreen;

        color.a = 0.5f;
        spectrum.color = color;

        fadeCoroutines[index] = StartCoroutine(SpectrumFadeTime(index));

        index = (index + 1) % spectrumPoolSize;
    }

    /// <summary>
    /// ����Ʈ�� �ܻ� ���� ���̵� �ƿ� ȿ���� �����մϴ�
    /// </summary>
    /// <param name="spectrum"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator SpectrumFadeTime(int poolIndex)
    {
        SpriteRenderer spectrum = spectrumPool[poolIndex];

        float elapsed = 0f;

        while (elapsed < spectrumFadeDuration)
        {
            float alpha = Mathf.Lerp(
                0.5f,
                0f,
                elapsed / spectrumFadeDuration
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        spectrum.gameObject.SetActive(false);
        fadeCoroutines[poolIndex] = null;
    }
}
