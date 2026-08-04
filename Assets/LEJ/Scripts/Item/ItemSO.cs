using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "LEJ/ItemSO")]
public class ItemSO : ScriptableObject
{
    public enum ItemType 
    { 
        None, 
        GetCredit, //재화
        GetHP, //체력 회복
        GetCard, //카드 획득
        DamagedCore, //손상된 코어
        BlankBullet, //공포탄
        Count 
    }
    public string Name;
    public Sprite Sprite;
    public int Price;
    public ItemType Type;
    public float Amount; //체력 회복 양, 공포탄 범위, 손상된 코어 스탯 상승 값에 쓰임
    public float SubAmount; //손상된 코어 스탯 상승 시간에 쓰임
}
