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
    [SerializeField] private GameObject enemyObj;
    // [SerializeField] private GameObject enemyHealtheBar;
    // [SerializeField] private Slider enemyHealthSlider;

    public void SpawnEnemy(int enemyCount)
    {
        
    }
}
