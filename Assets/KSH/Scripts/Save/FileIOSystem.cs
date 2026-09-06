using System.IO;
using UnityEngine;
using Application = UnityEngine.Application;

// 파일 입출력 담당
public static class FileIOSystem
{
    private static readonly string SaveDirectoryName = "Saves";

    public static string GetSaveDirectoryPath() // 세이브 폴더 경로
    {
        // 저장 위치: 사용자디렉토리/AppData/LocalLow/Hyeonggwang/NeonRoute > Saves 폴더
        return Application.persistentDataPath + "/Saves";
    }

    public static string GetSlotFilePath(int slotIndex) // 특정 슬롯 경로
    {
        string path = GetSaveDirectoryPath();
        return Path.Combine(path, $"save_slot_{slotIndex}.json");
    }

    // 세이브 폴더가 존재 여부 확인, 세이브로드 시도 전에 1번 호출해서 확인하기
    public static void EnsureSaveDirectoryExists()
    {
        string path = GetSaveDirectoryPath();
        bool isExists = Directory.Exists(path); // 있으면 T 없으면 F

        if (!isExists) Directory.CreateDirectory(path); // 없으면 폴더 생성
    }
    
    public static bool SlotFileExists(string filePath) // 슬롯 파일이 있는 지 확인
    { 
        return File.Exists(filePath);
    }

    // 파일 저장, 성공 여부를 bool로 리턴. 원자적 쓰기(임시파일 -> rename) 방식 적용 예정
    public static bool WriteTextToFile(string filePath, string content)
    {
        string tempFilePath = filePath + ".tmp"; // 임시 파일에 먼저 쓰기, 세이브 파일 깨지는거 고려해서

        try
        {
            File.WriteAllText(tempFilePath, content); // 임시 파일에 쓰기

            // 기존 파일 있으면 임시 파일로 이름 변경하기
            if (File.Exists(filePath))
            {
                File.Replace(tempFilePath, filePath, null); // 세 번째 인자는 백업 경로 지정
            }
            else
            {
                // 기존 파일이 없으면 그냥 rename(move)
                File.Move(tempFilePath, filePath);
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileIOSystem] 파일 쓰기 실패: {filePath}\n{e}");

            // 실패 시 남아있을 수 있는 임시 파일 정리
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            return false;
        }
    }

    // 지정된 경로에서 json 로드, 파일이 없거나 읽기 실패 시 null 리턴
    public static string ReadTextFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[FileIOSystem] 파일이 존재하지 않음: {filePath}");
            return null;
        }

        try
        {
            return File.ReadAllText(filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileIOSystem] 파일 읽기 실패: {filePath}\n{e}");
            return null;
        }
    }

    // 지정된 경로의 파일을 삭제, 슬롯 삭제 시? 사용
    public static bool DeleteFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            // 파일 경로 없으면 원래 파일이 없던 것, 그대로 true 리턴
            return true;
        }

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileIOSystem] 파일 삭제 실패: {filePath}\n{e}");
            return false;
        }
    }
    
    // 세이브 시간 작성, 파일 최종 수정 시간 리턴하기
    public static System.DateTime GetLastWriteTime(string filePath)
    {
        return File.GetLastWriteTime(filePath);
    }
}