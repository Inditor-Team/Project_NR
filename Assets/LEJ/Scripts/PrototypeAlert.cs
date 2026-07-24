using UnityEngine;

public class PrototypeAlert : MonoBehaviour
{
    void OnEnable()
    {
        gameObject.SetActive(GameManager.Instance.IsSetionOneClear);
    }

}
