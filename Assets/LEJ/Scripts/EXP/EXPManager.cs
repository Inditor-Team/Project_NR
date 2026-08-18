using UnityEngine;

public class EXPManager : MonoBehaviour
{
    //public static EXPManager Instance { get; private set; }

    //private void Awake()
    //{
    //    if (Instance != null && Instance != this)
    //    {
    //        Destroy(this.gameObject);
    //    }
    //    else
    //    {
    //        Instance = this;
    //        DontDestroyOnLoad(this.gameObject);
    //    }

    //}

    private void Start()
    {
        //현재 섹터 하나 클리어 시 경험치 카드 제공
        SectorManager.Instance.OnSectorClear += ProvideLevelCard;
    }

    private void OnDestroy()
    {
        if (SectorManager.Instance != null)
            SectorManager.Instance.OnSectorClear -= ProvideLevelCard;
    }

    [SerializeField] LevelCardProvider levelCardProvider;
    
    private float curEXP;
    public float CurEXP => curEXP;

    float levelCardCycle = 20f; //임시

    public void SetEXP(float value)
    {
        curEXP += value;

        if (value % levelCardCycle == 0f) 
            ProvideLevelCard();
    }

    void ProvideLevelCard()
    {
        if (levelCardProvider == null)
            return;

        levelCardProvider.ProvideByUI();
    }

    void ProvideLevelCard(SectorSO.SectorType type)
    {
        if (levelCardProvider == null)
            return;

        levelCardProvider.ProvideByUI();
    }
}
