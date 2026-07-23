using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class HowToUI : MonoBehaviour
{
    [SerializeField] GameObject[] howToTexts;
    [SerializeField] float[] activeRate;

    private void OnEnable()
    {
        foreach (var element in howToTexts)
            element.SetActive(false);

        StartCoroutine(TextActiveRoutine());
    }

    IEnumerator TextActiveRoutine()
    {
        for (int i = 0; i < howToTexts.Length; i++)
        {
            howToTexts[i].SetActive(true);
            yield return new WaitForSeconds(activeRate[i]);
        }
    }
}
