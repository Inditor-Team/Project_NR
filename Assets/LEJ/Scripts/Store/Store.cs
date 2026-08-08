using TMPro;
using UnityEngine;

/// <summary>
/// 가판대(Table) 과 판매 아이템 데이터, 판매 기능을 담당합니다
/// </summary>
public class Store : MonoBehaviour
{
    [SerializeField] LayerMask playerLayer;

    [SerializeField] StoreTable[] tables;
    [SerializeField] ItemSO[] saleItems;
    
    [SerializeField] GameObject bubble;
    [SerializeField] TMP_Text bubbleText;

    GameObject[] saleItemObjects;
    bool isPlayerNear = false;

    private void Start()
    {
        saleItemObjects = new GameObject[tables.Length];

        InitTables();
    }

    private void Update()
    {
        bubble.SetActive(isPlayerNear);
    }

    void InitTables()
    {
        for (int i = 0; i < tables.Length; i++)
        {
            tables[i].SetMyIndex(i);
            
            saleItemObjects[i] = ItemManager.Instance.SpawnItem(saleItems[i], tables[i].transform); //테이블에 팔 아이템을 스폰합니다
            saleItemObjects[i].GetComponent<Collider2D>().enabled = false; //플레이어가 가져가지 못 하게 콜라이더를 끕니다

            tables[i].OnTriggered += OnLook;
            tables[i].OnInteracted += OnSold;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((playerLayer.value & (1 << collision.gameObject.layer)) == 0)
            return;

        isPlayerNear = true;

        bubbleText.text = "반가워요 \n저는 상인이에용";
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((playerLayer.value & (1 << collision.gameObject.layer)) == 0)
            return;

        isPlayerNear = false;
    }

    /// <summary>
    /// n번째 테이블의 상품에 플레이어가 다가갔을 때 말풍선을 세팅합니다
    /// </summary>
    /// <param name="tableIndex"></param>
    void OnLook(int tableIndex)
    {
        bubbleText.text = $"{saleItems[tableIndex].Name}(은)는 {saleItems[tableIndex].Price} 크레딧 입니다.";
    }

    /// <summary>
    /// n번째 테이블의 상품에 플레이어가 인터랙션 했을 때 물건을 팝니다
    /// </summary>
    /// <param name="tableIndex"></param>
    void OnSold(int tableIndex)
    {
        GameManager.Instance.Credit -= saleItems[tableIndex].Price;

        //아이템 오브젝트의 콜라이더를 켜 플레이어가 가져갈 수 있게 합니다
        saleItemObjects[tableIndex].GetComponent<Collider2D>().enabled = true;
        saleItemObjects[tableIndex].GetComponent<ItemObject>().OnInteract();

        //상품이 판매 된 테이블은 테이블 기능을 상실합니다
        tables[tableIndex].OnTriggered += OnLook;
        tables[tableIndex].OnInteracted += OnSold;

        tables[tableIndex].GetComponent<Collider2D>().enabled = false;
        tables[tableIndex].GetComponent<StoreTable>().enabled = false; 
    }
}
