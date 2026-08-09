using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 상점 테이블입니다. TriggerEnter 와 Interact 만 감지합니다.
/// </summary>
public class StoreTable : MonoBehaviour, IInteractable
{
    public event UnityAction<int> OnInteracted;
    public event UnityAction<int> OnTriggered;
    int myIndex;

    public void SetMyIndex(int index)
    {
        this.myIndex = index;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnTriggered?.Invoke(myIndex);
    }

    public void OnInteract()
    {
        OnInteracted?.Invoke(myIndex);
    }
}
