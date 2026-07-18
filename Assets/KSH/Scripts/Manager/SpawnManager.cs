using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

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
    }
    
    // 적 관련 오브젝트등
    // [SerializeField] private GameObject enemyObj;
    // [SerializeField] private GameObject enemyHealtheBar;
    // [SerializeField] private Slider enemyHealthSlider;
    private int remainingCount = 6; // 맵에 남은 적
    
    public void DestroyedEnemy() // 적이 파괴되면 호출
    {
        remainingCount--;

        if (remainingCount <= 0)
        {
            GameManager.Instance.SectionClear();
        }
    }
}
