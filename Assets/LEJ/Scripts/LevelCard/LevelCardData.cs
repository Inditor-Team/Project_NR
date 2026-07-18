using UnityEngine;

/// <summary>
/// 레벨 카드에 대한 데이터 저장 프리팹(추후 시트 연결)
/// </summary>
public class LevelCardData : MonoBehaviour
{
    [SerializeField] LevelCard[] levelCards;
    public LevelCard[] LevelCards => levelCards;
}
