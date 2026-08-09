using UnityEngine;

/// <summary>
/// 아이템을 가질 수 있습니다.
/// </summary>
public interface IItemHolder
{
    public void HoldItem(ItemObject itemObject);
    /// <summary>
    /// 아이템을 사용합니다.
    /// </summary>
    public void UseItem();
}
