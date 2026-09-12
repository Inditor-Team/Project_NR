using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레벨 카드에 대한 데이터 저장 프리팹(추후 시트 연결)
/// </summary>
public class LevelCardData : MonoBehaviour
{
    public static LevelCardData Instance;

    [SerializeField] LevelCardSO[] levelCards;
    public LevelCardSO[] LevelCards => levelCards;

    Dictionary<int, LevelCardSO> levelCardDic = new Dictionary<int, LevelCardSO>();
    public Dictionary<int, LevelCardSO> LevelCardDic => levelCardDic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        SetDictionary();
    }

    private void SetDictionary()
    {
        foreach (var levelCard in levelCards)
            levelCardDic.Add(levelCard.Id, levelCard);
    }
}
