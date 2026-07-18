using UnityEngine;

public class EXPManager : MonoBehaviour
{
    public static EXPManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    [SerializeField] LevelCardProvider levelCardProvider;
    
    private float curEXP;
    public float CurEXP => curEXP;

    float levelCardCycle = 20f; //юс╫ц

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
}
