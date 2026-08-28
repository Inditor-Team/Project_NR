using System.Collections;
using UnityEngine;

public class BlitzProtocol : ProtocolBase
{
    [SerializeField] GameObject player;
    [SerializeField] SpriteRenderer playerModel;
    [SerializeField] SwordAttacker swordAttacker;
    [Header("임시 스탯")]
    [SerializeField] float dashSpeed = 50f;
    [SerializeField] float detectRadius = 8f;
    [SerializeField] float swordSpeed = 5f;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] int attackCount = 3;
    int killCount = 0;
    int spectrumIndex = 0;
    float attackDistance = 2f;


    //프로토콜 발동 시 스펙트럼 이펙트
    private SpriteRenderer curSprite;
    private SpriteRenderer[] spectrumPool;

    [Header("잔상 이펙트")]
    [SerializeField] private int spectrumPoolSize = 30;
    [SerializeField] private float spectrumFadeDuration = 3f; //잔상 페이드 아웃 시간
    [SerializeField] private float spectrumInterval = 0.005f; //잔상 생성 간격

    private Coroutine[] fadeCoroutines;

    private void Start()
    {
        InitEffect();
    }

    public override void UpgradeProtocol(ProtocolCard.Buff type, float level)
    {
    }

    internal override void TryProtocol(float duration)
    {
        DoProtocol();
    }

    internal override void DoProtocol()
    {
        if (protocolRoutine != null)
            return;

        protocolRoutine = StartCoroutine(ProtocolTime());
    }

    Coroutine protocolRoutine;
    IEnumerator ProtocolTime()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            if (killCount >= attackCount)
                break;

            float intervalTime = 0f;

            while (enemy != null && Vector2.Distance(transform.position, enemy.transform.position) > attackDistance)
            {
                player.transform.position = Vector2.MoveTowards(transform.position,enemy.transform.position,dashSpeed * Time.deltaTime);
                
                intervalTime += Time.deltaTime;

                if (intervalTime >= spectrumInterval)
                {
                    Effect();
                    intervalTime = 0f;
                }

                yield return null;
            }

            swordAttacker.Swing();
            enemy.GetComponent<IDamageable>().TakeDamage(100);
            killCount++;

            yield return new WaitForSeconds(0.05f);
        }

        EndProtocol();
        yield break;
    }

    internal override void EndProtocol()
    {
        colorTime = 0f;
        killCount = 0;
        protocolRoutine = null;
    }


    void InitEffect()
    {
        if (GameManager.Instance.CurProtocol != ProtocolCard.Protocol.Blitz)
            return;

        spectrumPool = new SpriteRenderer[spectrumPoolSize];
        fadeCoroutines = new Coroutine[spectrumPoolSize];

        for (int i = 0; i < spectrumPoolSize; i++)
        {
            spectrumPool[i] = new GameObject($"blitz effect {i}").AddComponent<SpriteRenderer>();
            spectrumPool[i].gameObject.SetActive(false);
        }
    }

    float colorTime = 0f;
    [SerializeField] float colorSpeed = 2f;

    private void Effect()
    {
        curSprite = playerModel;

        SpriteRenderer spectrum = spectrumPool[spectrumIndex];

        if (fadeCoroutines[spectrumIndex] != null)
            StopCoroutine(fadeCoroutines[spectrumIndex]);

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


        fadeCoroutines[spectrumIndex] =
            StartCoroutine(SpectrumFadeTime(spectrumIndex));

        spectrumIndex = (spectrumIndex + 1) % spectrumPoolSize;
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
