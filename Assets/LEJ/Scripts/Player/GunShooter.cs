using UnityEngine;

public class GunShooter : MonoBehaviour
{
    [SerializeField] Gun gun;
    public Gun Gun => gun;

    PlayerStat stat;

    public void RegisterStat(PlayerStat stat)
    {
        this.stat = stat;
    }

    public void DoAttack()
    {
        if (stat == null)
            return;

        gun.TryAttack(stat.StatDic[PlayerStat.Stat.BulletFireRate], stat.StatDic[PlayerStat.Stat.BulletSpeed], stat.StatDic[PlayerStat.Stat.BulletDamage]);
    }
}
