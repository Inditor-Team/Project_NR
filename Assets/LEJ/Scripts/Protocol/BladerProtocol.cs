using System;
using System.Collections;
using UnityEngine;

public class BladerProtocol : ProtocolBase
{
    [SerializeField] SwordAttacker swordAttacker;
    [SerializeField] SpriteRenderer swordModel;
    [SerializeField] Gun gun;

    float duration;
    float damage;

    //«¡∑Œ≈‰ƒ› πﬂµø Ω√ Ω∫∆Â∆Æ∑≥ ¿Ã∆Â∆Æ
    private SpriteRenderer[] spectrumPool;

    [Header("¿‹ªÛ ¿Ã∆Â∆Æ")]
    [SerializeField] private int spectrumPoolSize = 30;
    [SerializeField] private float spectrumInterval = 0.1f; //¿‹ªÛ ª˝º∫ ∞£∞›

    private int index = 0;

    private void Start()
    {
        InitEffect();
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
        float elapsed = 0f;
        swordAttacker.CircleSwing();

        while (elapsed < duration)
        {
            Effect();

            yield return new WaitForSeconds(spectrumInterval);

            elapsed += spectrumInterval;
        }

        EndProtocol();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        var enemyBullet = collision.GetComponent<EnemyBullet>();

        if (enemyBullet == null)
            return;

        gun.ReflectAttack(enemyBullet.transform.position, -enemyBullet.velocity.normalized, enemyBullet.velocity.magnitude, damage);
        enemyBullet.DestroyBullet();
    }

    internal override void EndProtocol()
    {
        EndEffect();

        isActive = false;
        protocolRoutine = null;
    }

    private void InitEffect()
    {
        if (GameManager.Instance.CurProtocol != ProtocolCard.Protocol.Blader)
            return;

        spectrumPool = new SpriteRenderer[spectrumPoolSize];

        for (int i = 0; i < spectrumPoolSize; i++)
        {
            GameObject obj = new GameObject($"blader effect {i}");

            obj.transform.SetParent(transform);

            spectrumPool[i] = obj.AddComponent<SpriteRenderer>();
            obj.SetActive(false);
        }
    }

    Color color;
    float colorTime = 0f;
    float colorSpeed = 8f;

    private void Effect()
    {
        if (index >= spectrumPoolSize)
            return;

        SpriteRenderer spectrum = spectrumPool[index];

        spectrum.gameObject.SetActive(true);

        spectrum.sprite = swordModel.sprite;
        spectrum.flipX = swordModel.flipX;
        spectrum.flipY = swordModel.flipY;

        spectrum.transform.position = swordModel.transform.position;
        spectrum.transform.rotation = swordModel.transform.rotation;
        spectrum.transform.localScale = swordModel.transform.lossyScale;

        spectrum.sortingLayerID = swordModel.sortingLayerID;
        spectrum.sortingOrder = swordModel.sortingOrder - 3;

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

        index++;
    }


    private void EndEffect()
    {
        for (int i = 0; i < spectrumPoolSize; i++)
            spectrumPool[i].gameObject.SetActive(false);

        index = 0;
        colorTime = 0f;
    }
}
