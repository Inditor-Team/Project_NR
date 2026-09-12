using System;
using System.Collections.Generic;
using UnityEngine;

public static class DialogueEventDispatcher
{
    static Dictionary<string, Action<string[]>> eventHandlers = new();
    static Dictionary<string, Func<string[], bool>> conditionHandlers = new();

    public static void RegisterEvent(string key, Action<string[]> handler) => eventHandlers[key] = handler;
    public static void RegisterCondition(string key, Func<string[], bool> handler) => conditionHandlers[key] = handler;

    public static void FireEvent(string command)
    {
        var parts = command.Split(':');
        if (eventHandlers.TryGetValue(parts[0], out var handler))
            handler.Invoke(parts[1..]);
        else
            Debug.LogWarning($"등록되지 않은 대화 이벤트: {parts[0]}");
    }

    public static bool CheckCondition(string command)
    {
        var parts = command.Split(':');
        if (conditionHandlers.TryGetValue(parts[0], out var handler))
            return handler.Invoke(parts[1..]);

        Debug.LogWarning($"등록되지 않은 대화 조건: {parts[0]}, false");
        return false; // 일단 실패 처리
    }
}
