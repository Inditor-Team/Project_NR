using UnityEngine;
using UnityEngine.Rendering;

public class GameStartButton : MonoBehaviour
{
    public GameObject protocolChoiceUI;

    public void OnClick()
    {
        if (GameManager.Instance.CurProtocol == ProtocolCard.Protocol.None)
        {
            protocolChoiceUI.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            SceneController.Instance.ChangeScene(SceneController.Scene.Map);
        }
    }
}
