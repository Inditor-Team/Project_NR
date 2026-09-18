using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

/// <summary>
/// 플레이어의 인벤토리 입니다.
/// </summary>
public class PlayerInventory : MonoBehaviour, IItemHolder
{
    [SerializeField] ItemSlotUI ui;
    [SerializeField] PlayerStat playerStat; //추후 매니저에서 Player 참조 시 그쪽으로 연결
    [SerializeField] LevelCardProvider levelCardProvider; //추후 맵매니저에서 참조하기

    [Header("Blank Bullet")]
    [SerializeField] private float blankBulletRadius = 10f;
    [SerializeField] private LayerMask enemyBulletLayer;
    [SerializeField] private BlankBulletEffect blankBulletEffectPrefab;
    
    [Header("Damaged Core")]
    [SerializeField] private float damagedCoreDuration = 10f;

    private ItemSO curItem;
    public ItemSO CurItem => curItem;

    /// <summary>
    /// 획득 할 때 플레이어의 슬롯이 비어있지 않다면, 기존 아이템을 땅에 떨구고 새로운 아이템을 슬롯에 장착합니다.
    /// </summary>
    /// <param name="itemObject"></param>
    public void HoldItem(ItemObject itemObject)
    {
        if (itemObject.MyItem == null)
        {
            Debug.Log($"{itemObject.gameObject.name} : item object 의 my item 이 null");
            return;
        }

        //만약 재화 아이템이라면 획득 시 바로 사용
        if (itemObject.MyItem.Type == ItemSO.ItemType.GetCredit)
        {
            GameManager.Instance.Credit += (int)itemObject.MyItem.Amount;
            return;
        }

        //이미 아이템을 들고 있다면, 획득하려는 아이템과 교체
        if (curItem != null)
        {
            //현재 아이템을 월드 내 스폰 해 뱉어내기
            ItemSpawner.Instance.SpawnItem(curItem, itemObject.transform);
            curItem = itemObject.MyItem; //월드에 있던 아이템을 슬롯에 장착
        }
        else //아이템을 들고 있지 않다면 그대로 슬롯에 장착
            curItem = itemObject.MyItem; //슬롯에 장착 

        if (ui != null)
            ui.UpdateUI(curItem);
    }

    /// <summary>
    /// 인벤토리매니저를 통해 이전 씬에서 들고있던 아이템을 바로 장착합니다
    /// </summary>
    /// <param name="itemSO"></param>
    public void HoldItem(ItemSO itemSO)
    {
        curItem = itemSO;

        if (ui != null)
            ui.UpdateUI(curItem);
    }

    public void UseItem()
    {
        if (curItem == null)
            return;

        switch (curItem.Type)
        {
            case ItemSO.ItemType.DamagedCore:
                playerStat.UseDamagedCore(curItem.Amount, damagedCoreDuration);
                break;
            case ItemSO.ItemType.GetCard:
                levelCardProvider.ProvideByUI();
                break;
            case ItemSO.ItemType.GetCredit:
                GameManager.Instance.Credit += (int)curItem.Amount;
                break;
            case ItemSO.ItemType.GetHP:
                playerStat.EarnLife(curItem.Amount);
                break;
            case ItemSO.ItemType.BlankBullet:
                UseBlankBullet();
                break;

        }

        curItem = null;

        if (ui != null)
            ui.UpdateUI(curItem);
    }

    /// <summary>
    /// 플레이어 주변의 적 탄환을 제거합니다.
    /// </summary>
    private void UseBlankBullet()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            blankBulletRadius,
            enemyBulletLayer
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out EnemyBullet bullet))
                bullet.Expire();
        }

        BlankBulletEffect effect = Instantiate(
            blankBulletEffectPrefab,
            transform.position,
            Quaternion.identity
        );

        effect.Play(blankBulletRadius);
    }

}
