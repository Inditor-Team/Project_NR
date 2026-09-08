using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

// Newtonsoft.Json(Json.NET) 사용
// enum은 정수가 아닌 문자열(이름)로 직렬화되도록 StringEnumConverter를 적용
public class JsonSaveDataSerializer
{
    // JSON 직렬화 및 역직렬화 동작을 세밀하게 제어하기 위한 설정 클래스, 한 번만 사용해서 재사용하기 (매번 만들면 GC 부담) 
    private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter> { new StringEnumConverter() }, // enum을 문자열로 저장
        Formatting = Formatting.Indented, // 줄바꿈/들여쓰기 들어감
        NullValueHandling = NullValueHandling.Include, // null도 null로 작성, 기본값은 null 작성안함
        MissingMemberHandling = MissingMemberHandling.Ignore // 나중에 데이터 필드 추가되면 예외 무시하는지
    };

    public string Serialize(SaveDataStruct data)
    {
        try
        {
            return JsonConvert.SerializeObject(data, settings); // 위의 설정대로 세이브 데이터를 -> json 파일로 변환(직렬화)
        }
        catch (Exception e)
        {
            Debug.LogError($"Serialize 실패 Data: {data}\n{e}");
            return null;
        }
    }

    public SaveDataStruct Deserialize(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<SaveDataStruct>(json, settings); // json 파일 -> 세이브 데이터 (역직렬화)
        }
        catch (Exception e)
        {
            Debug.LogError($"Deserialize 실패 Data: {json}\n{e}");
            return null;
        }
    }
}
