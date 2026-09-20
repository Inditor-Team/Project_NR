using UnityEngine;

public class CurSectorDataRegister : MonoBehaviour
{
    public SectorSO mySectorSO;

    private void Start()
    {
        SectorManager.Instance.RegisterCurSector(mySectorSO);
    }
}
