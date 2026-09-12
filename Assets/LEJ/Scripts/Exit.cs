using Unity.Multiplayer.Center.Common;
using UnityEngine;

public class Exit : MonoBehaviour
{
    public SceneController.Scene targetScene;

    [SerializeField] LevelCardProvider levelCardProvider;
    [SerializeField] GameObject myModel;
    Animator anim;

    public bool isEventScene = false;
    bool isAppear = false;

    private void Start()
    {
        anim = GetComponent<Animator>();

        //이벤트 맵의 경우 바로 나갈 수 있게 출입구가 열립니다
        if (isEventScene)
            isAppear = true;
        else //전투 맵의 경우 SectorClear 시에 모습을 드러내고 씬을 나갈 수 있게 됩니다
            SectorManager.Instance.OnSectorClear += Appear;

        myModel.SetActive(isAppear);
        anim.enabled = isAppear;
    }

    private void OnDestroy()
    {
        if (SectorManager.Instance != null)
            SectorManager.Instance.OnSectorClear -= Appear;
    }

    void Appear(SectorSO.SectorType type)
    {
        isAppear = true;
        myModel.SetActive(isAppear);
        anim.enabled = isAppear;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "Player")
            return;

        if (isAppear)
        {
            if (!isEventScene && levelCardProvider != null)
                levelCardProvider.ProvideByUI();
            else
            {
                SectorManager.Instance.SectorClear();
                SceneController.Instance.ChangeScene(targetScene);
            }
        }
    }
}
