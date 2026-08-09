using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.WSA;

/// <summary>
/// 월드 상 아이템 오브젝트입니다.
/// </summary>
public class ItemObject : MonoBehaviour, IInteractable
{
    [SerializeField] SpriteRenderer sprite;

    //현재 오브젝트의 아이템 정보
    [SerializeField] ItemSO myItem;
    public ItemSO MyItem => myItem;

    float magneticMoveSpeed = 5f;
    bool isInteracted = false;

    #region DOTween
    private Tween floatingTween;
    private float moveDistance = 0.15f;
    private float moveDuration = 0.5f;

    private void OnDisable()
    {
        StopFloatAnim();

        isInteracted = false;
    }

    private void DoFloatAnim()
    {
        Vector3 targetPosition = transform.position + Vector3.up * moveDistance;

        floatingTween = transform
            .DOMove(targetPosition, moveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopFloatAnim()
    {
        floatingTween?.Kill();
        floatingTween = null;
    }
    #endregion

    public void SetItem(ItemSO item)
    {
        myItem = item;
        sprite.sprite = item.Sprite;
        DoFloatAnim();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        IItemHolder holder = collision.GetComponent<IItemHolder>();

        if (holder == null)
            return;

        if (!isInteracted) //플레이어에 의해 상호작용 됐을 때 주워짐
            return;

        StopFloatAnim();
        transform.position = Vector2.MoveTowards(transform.position, collision.transform.position, magneticMoveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, collision.transform.position) < 0.1f)
        {
            holder.HoldItem(this);
            myItem = null;
            ItemManager.Instance.DespawnItem(gameObject);
        }
    }

    public void OnInteract()
    {
        isInteracted = true;
    }
}
