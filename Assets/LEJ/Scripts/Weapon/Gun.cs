using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Gun : WeaponBase
{
    [Tooltip("ÃÑ±¸ À§Ä¡")]
    [SerializeField] Transform firePoint; //ÃÑ±¸ À§Ä¡
    [SerializeField] SpriteRenderer model;
    public SpriteRenderer Model => model;
    //[SerializeField] LineRenderer lineRenderer;
    //[SerializeField] private float laserDuration = 1f;
    //private float disableTime;
    float maxDistance = 20f;

    float speed;
    float damage;

    [SerializeField] GameObject bulletPrefab; //ÃÑ¾Ë ÇÁ¸®ÆÕ
    private int bulletPoolSize = 20;
    BulletBase curBullet;

    [SerializeField] LayerMask hitLayer;
    [SerializeField] private ParticleSystem laserEffect;

    public UnityAction OnShoot;

    private void Awake()
    {
        //lineRenderer.positionCount = 2;
        //lineRenderer.enabled = false;
    }

    private void Start()
    {
        //MakeBulletPool();
    }

    private void Update()
    {
        //if (lineRenderer.enabled && Time.time >= disableTime)
        //    lineRenderer.enabled = false;
    }

    /*
    private void MakeBulletPool()
    {
        PoolManager.Instance.PoolInit(bulletPrefab, bulletPoolSize);
    }
    */

    public void TryAttack(float speed, float damage)
    {
        this.damage = damage;
        
        // ÃÑ¾Ë »ç¿ë
        this.speed = speed;

        //curBullet = PoolManager.Instance.Get(bulletPrefab).GetComponent<BulletBase>();
        //curBullet.transform.position = firePoint.position; //ÃÑ¾Ë À§Ä¡ ÃÊ±âÈ­

        Attack();
    }

    internal override void Attack()
    {
        OnShoot?.Invoke();

        /*ÃÑ¾Ë »ç¿ë
        if (curBullet == null)
            return;

        curBullet.OnFire(-firePoint.right, speed, damage, bulletPrefab);
        */

        Vector2 endPosition;

        Vector2 startPosition = firePoint.position;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction =
            (mouseWorldPos - startPosition).normalized;

        RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, maxDistance, hitLayer);

        if (hit.collider != null)
        {
            endPosition = hit.point;

            if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);

                Debug.Log($"{hit.collider.name}¿¡°Ô µ¥¹ÌÁö {damage}¸¦ °¡ÇÔ");
            }
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.OnInteract();
            }
        }
        else
            endPosition = startPosition + direction * maxDistance;

        PlayLaserEffect(firePoint.position, endPosition);

        /*
        if (lineRenderer != null)
            DrawLaser(startPosition, endPosition);
        */
    }
    
    public void PlayLaserEffect(Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
            return;

        float speed = distance / 0.06f;

        ParticleSystem.EmitParams emitParams =
            new ParticleSystem.EmitParams();

        emitParams.position = start;
        emitParams.velocity = direction.normalized * speed;
        emitParams.startLifetime = 0.06f;

        laserEffect.Emit(emitParams, 1);
    }

    /*
    private Coroutine laserCoroutine;

    private void DrawLaser(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        lineRenderer.enabled = true;
        disableTime = Time.time + laserDuration;

        if (laserCoroutine != null)
            StopCoroutine(laserCoroutine);

        laserCoroutine = StartCoroutine(FadeLaser());
    }

    private IEnumerator FadeLaser()
    {
        lineRenderer.enabled = true;

        float elapsed = 0f;

        while (elapsed < laserDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / laserDuration);
            SetFadeGradient(progress);

            yield return null;
        }

        lineRenderer.enabled = false;
        laserCoroutine = null;
    }

    private void SetFadeGradient(float progress)
    {
        float fadeEdge = Mathf.Clamp01(progress + 0.15f);

        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.green, 0f),
                new GradientColorKey(Color.green, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0f, progress),
                new GradientAlphaKey(1f, fadeEdge),
                new GradientAlphaKey(1f, 1f)
            }
        );

        lineRenderer.colorGradient = gradient;
    }
    */
}
