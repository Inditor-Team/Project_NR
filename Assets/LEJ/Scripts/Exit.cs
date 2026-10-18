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

        //�̺�Ʈ ���� ��� �ٷ� ���� �� �ְ� ���Ա��� �����ϴ�
        if (isEventScene)
            isAppear = true;
        else //���� ���� ��� SectorClear �ÿ� ����� �巯���� ���� ���� �� �ְ� �˴ϴ�
            SectorManager.Instance.OnSectorClear += Appear;

        myModel.SetActive(isAppear);
        anim.enabled = isAppear;
    }

    private void OnDestroy()
    {
        if (SectorManager.Instance != null)
            SectorManager.Instance.OnSectorClear -= Appear;
    }

    void Appear(SceneController.Scene type)
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
            GameObject playerObj = GameManager.Instance.Player;
            
            playerObj.transform.position = Vector3.Lerp(playerObj.transform.position, transform.position, Time.deltaTime * 10f);


            if (!isEventScene && levelCardProvider != null)
                levelCardProvider.ProvideByUI();
            else
            {
                SectorManager.Instance.SectorClear(); // ?? 흐름 개선 필요
                SceneController.Instance.ChangeScene(targetScene);
            }
        }
    }
}
