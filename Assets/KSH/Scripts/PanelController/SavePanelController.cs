using System;
using UnityEngine;

public class SavePanelController : MonoBehaviour
{
    [SerializeField] private GameObject savePanel;

    // 임시 테스트용 코드
    /*private void Start()
    {
        FileIOSystem.EnsureSaveDirectoryExists();
        string testPath = FileIOSystem.GetSlotFilePath(0);
        FileIOSystem.WriteTextToFile(testPath, "{ \"test\": 123 }");
        string readBack = FileIOSystem.ReadTextFromFile(testPath);
        Debug.Log(readBack);
    }
    */

    // TODO: 세이브 버튼 만들어서 연결하기
    public void ShowSavePanel()
    {
        if (savePanel == null) return;
        GameManager.Instance.RequestPause();// Pause(true);

        UIManager.Instance.Show(savePanel);
    }

    public void HideSavePanel()
    {
        UIManager.Instance.Hide(savePanel);
        GameManager.Instance.ReleasePause();// Pause(false);
    }
}
