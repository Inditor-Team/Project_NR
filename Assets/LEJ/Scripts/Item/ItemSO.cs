using UnityEngine;
using UnityEngine.Localization;

public enum ItemId
{
    None = 0,
    Credit,
    EmergencyPatch,
    RepairKit,
    WasteProtocol,
    DamagedCore,
    BlankBullet
}

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

[CreateAssetMenu(fileName = "ItemSO", menuName = "LEJ/ItemSO")]
public class ItemSO : ScriptableObject
{
    public ItemId Id;
    public LocalizedString Name;
    public LocalizedString Description;
    public Sprite Sprite;
    public int Price;
    public int weight;
    public ItemType Type;
    public float Amount; //체력 회복 양, 공포탄 범위, 손상된 코어 스탯 상승 값에 쓰임
    public float SubAmount; //손상된 코어 스탯 상승 시간에 쓰임
}
