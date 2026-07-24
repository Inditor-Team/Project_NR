using UnityEngine;

public class Title : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKeyDown)
            gameObject.SetActive(false);
    }
}
