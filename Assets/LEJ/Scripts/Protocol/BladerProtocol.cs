using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class BladerProtocol : ProtocolBase
{
    [SerializeField] SpriteRenderer playerModel;
    [SerializeField] Gun gun;

    GameObject curBullet;
    float duration;
    float damage;

    //프로토콜 발동 시 스펙트럼 이펙트
    private SpriteRenderer curSprite;
    private SpriteRenderer[] spectrumPool;

    [Header("잔상 이펙트")]
    [SerializeField] private int spectrumPoolSize = 30;
    [SerializeField] private float spectrumFadeDuration = 3f; //잔상 페이드 아웃 시간
    [SerializeField] private float spectrumInterval = 0.2f; //잔상 생성 간격
    private int index = 0;

    private Coroutine[] fadeCoroutines;

    new Dictionary<ProtocolCard.Buff, float> buffValues = new Dictionary<ProtocolCard.Buff, float>()
    {
        { ProtocolCard.Buff.LessCoolTime, 1f },
        { ProtocolCard.Buff.KillToCool, 1f },
        { ProtocolCard.Buff.DeadmanSwitch, 1f },
        { ProtocolCard.Buff.BloodLeak, 1f },
    };

    private void Start()
    {
        GameManager.Instance.OnProtocolChanged += InitEffect;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnProtocolChanged -= InitEffect;
    }

    public override void UpgradeProtocol(ProtocolCard.Buff type, float level)
    {
        if (!buffValues.ContainsKey(type))
            return;

        buffValues[type] = level;
    }

    internal override void TryProtocol(float duration)
    {
        this.duration = duration;
        DoProtocol();
    }

    internal override void DoProtocol()
    {
        isActive = true;
        Debug.Log("Player: Blader Protocol! ");
        
        if (protocolRoutine == null)
            protocolRoutine = StartCoroutine(ProtocolTime());
    }

    Coroutine protocolRoutine;

    IEnumerator ProtocolTime()
    {
        float elapsedTime = 0f;

        while (true)
        {
            yield return new WaitForSeconds(spectrumInterval);
            elapsedTime += spectrumInterval;
            
            Effect();

            if (elapsedTime > duration)
                break;
        }
        
        EndProtocol();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        var enemyBullet = collision.GetComponent<EnemyBullet>();

        if (enemyBullet == null || curBullet == enemyBullet.gameObject)
            return;

        gun.FireReflectBullet(enemyBullet.transform.position, -enemyBullet.velocity.normalized, enemyBullet.velocity.magnitude, damage);
    }

    internal override void EndProtocol()
    {
        isActive = false;
    }

    void InitEffect()
    {
        if (GameManager.Instance.CurProtocol != ProtocolCard.Protocol.NeuroAction)
            return;

        spectrumPool = new SpriteRenderer[spectrumPoolSize];
        fadeCoroutines = new Coroutine[spectrumPoolSize];

        for (int i = 0; i < spectrumPoolSize; i++)
        {
            spectrumPool[i] = new GameObject($"SpectrumEffect_{i}").AddComponent<SpriteRenderer>();
            spectrumPool[i].gameObject.SetActive(false);
        }
    }

    private void Effect()
    {
        curSprite = playerModel;

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
