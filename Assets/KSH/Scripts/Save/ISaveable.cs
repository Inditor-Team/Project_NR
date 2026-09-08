// 세이브 대상에게 사용
public interface ISaveable
{
    public void SaveDataTo(SaveDataStruct data);
    public void LoadDataFrom(SaveDataStruct data);
}