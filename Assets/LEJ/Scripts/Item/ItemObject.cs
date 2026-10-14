
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 월드 상 아이템 오브젝트
/// </summary>
public class ItemObject : MonoBehaviour
{
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] ItemSO myItem;

    public ItemSO MyItem => myItem;

    [Header("Magnet")]
    [SerializeField] float magneticMoveSpeed = 5f;
    [SerializeField] float magneticAcceleration = 2f;
    [SerializeField] float pickupDistance = 0.1f;

    private PlayerInventory targetInventory;
    private bool isMagnetActive = false;
    private bool allowReplacement = false;
    private float timer = 0f;

    #region DOTween

    private Tween floatingTween;
    private Vector3 originalLocalPosition;

    private float moveDistance = 0.15f;
    private float moveDuration = 0.5f;

    private void Awake()
    {
        originalLocalPosition = sprite.transform.localPosition;
    }

    private void DoFloatAnim()
    {
        StopFloatAnim();

        Vector3 targetPosition =
            originalLocalPosition + Vector3.up * moveDistance;

        floatingTween = sprite.transform
            .DOLocalMove(targetPosition, moveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopFloatAnim()
    {
        floatingTween?.Kill();
        floatingTween = null;

        if (sprite != null)
            sprite.transform.localPosition = originalLocalPosition;
    }

    #endregion

    private void OnDisable()
    {
        StopFloatAnim();

        myItem = null;
        targetInventory = null;
        isMagnetActive = false;
        allowReplacement = false;
        timer = 0f;
    }

    public void SetItem(ItemSO item)
    {
        myItem = item;
        sprite.sprite = item.Sprite;

        targetInventory = null;
        isMagnetActive = false;
        allowReplacement = false;
        timer = 0f;

        DoFloatAnim();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryAutoPickup(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryAutoPickup(collision);
    }

    /// <summary>
    /// 슬롯이 비어 있으면 자동으로 자석 이동 시작
    /// </summary>
    private void TryAutoPickup(Collider2D collision)
    {
        if (myItem == null || isMagnetActive)
            return;

        PlayerInventory inventory =
            collision.GetComponentInParent<PlayerInventory>();

        if (inventory == null)
            return;

        // 재화는 언제나 자동 획득
        if (myItem.Type == ItemType.GetCredit)
        {
            StartMagnet(inventory, false);
            return;
        }

        // 슬롯이 비어 있을 때만 자동 획득
        if (inventory.CurItem == null)
        {
            StartMagnet(inventory, false);
        }
    }

    /// <summary>
    /// E키 상호작용으로 아이템 교체 요청
    /// </summary>
    public void PickUp(PlayerInventory inventory)
    {
        if (inventory == null || myItem == null)
            return;

        StartMagnet(inventory, true);
    }

    /// <summary>
    /// 자석 이동 시작
    /// </summary>
    private void StartMagnet(
        PlayerInventory inventory,
        bool canReplace)
    {
        if (isMagnetActive || myItem == null)
            return;

        // 자동 획득은 기존 아이템을 교체하지 않음
        if (!canReplace &&
            inventory.CurItem != null &&
            myItem.Type != ItemType.GetCredit)
            return;

        targetInventory = inventory;
        allowReplacement = canReplace;
        isMagnetActive = true;
        timer = 0f;

        StopFloatAnim();
    }

    private void Update()
    {
        if (!isMagnetActive)
            return;

        if (targetInventory == null || myItem == null)
        {
            CancelMagnet();
            return;
        }

        // 자동 획득 중 슬롯이 차면 교체하지 않고 중단
        if (!allowReplacement &&
            targetInventory.CurItem != null &&
            myItem.Type != ItemType.GetCredit)
        {
            CancelMagnet();
            return;
        }

        timer += Time.deltaTime * magneticAcceleration;

        float moveSpeed = magneticMoveSpeed + timer;

        Vector3 targetPosition =
            targetInventory.transform.position;

        // 플레이어를 향해 부드럽게 이동
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // 충분히 가까워졌을 때 실제 획득
        if (Vector2.Distance(
            transform.position,
            targetPosition) <= pickupDistance)
        {
            CompletePickup();
        }
    }

    private void CompletePickup()
    {
        if (targetInventory == null || myItem == null)
            return;

        PlayerInventory inventory = targetInventory;

        isMagnetActive = false;
        targetInventory = null;

        // 인벤토리 적용
        inventory.HoldItem(this);

        // 월드 아이템 제거
        ItemSpawner.Instance.DespawnItem(gameObject);
    }

    private void CancelMagnet()
    {
        isMagnetActive = false;
        allowReplacement = false;
        targetInventory = null;
        timer = 0f;

        if (myItem != null && isActiveAndEnabled)
            DoFloatAnim();
    }
}
