using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레벨 카드에 대한 데이터 저장 프리팹(추후 시트 연결)
/// </summary>
public class LevelCardData : MonoBehaviour
{
    [SerializeField] LevelCardSO[] levelCards;
    public LevelCardSO[] LevelCards => levelCards;

    Dictionary<string, LevelCardSO> levelCardDic = new Dictionary<string, LevelCardSO>();
    public Dictionary<string, LevelCardSO> LevelCardDic => levelCardDic;

    private void Awake()
    {
        SetDictionary();
    }

    private void SetDictionary()
    {
        foreach (var levelCard in levelCards)
            levelCardDic.Add(levelCard.Id, levelCard);
    }
}
