using DG.Tweening;
using UnityEngine;

/// <summary>
/// 월드 상 아이템 오브젝트입니다.
/// </summary>
public class ItemObject : MonoBehaviour
{
    [SerializeField] SpriteRenderer sprite;

    //현재 오브젝트의 아이템 정보
    [SerializeField] ItemSO myItem;
    public ItemSO MyItem => myItem;

    #region DOTween
    private Tween floatingTween;
    private float moveDistance = 0.15f;
    private float moveDuration = 0.5f;

    private void OnEnable()
    {
        Vector3 targetPosition = transform.localPosition + Vector3.up * moveDistance;

        floatingTween = transform
            .DOLocalMove(targetPosition, moveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    private void OnDisable()
    {
        floatingTween?.Kill();
        floatingTween = null;
    }
    #endregion

    public void SetItem(ItemSO item)
    {
        myItem = item;
        sprite.sprite = item.Sprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IItemHolder itemHolder = collision.GetComponent<IItemHolder>();

        if (itemHolder == null)
            return;

        if (myItem != null)
        {
            itemHolder.HoldItem(this);
            ItemManager.Instance.DespawnItem(this);
            myItem = null;
        }
    }
}
