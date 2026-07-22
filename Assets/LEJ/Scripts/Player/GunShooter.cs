using UnityEngine;

public class GunShooter : MonoBehaviour
{
    [SerializeField] Gun gun;
    public Gun Gun => gun;

    PlayerStat stat;
    float lastFireTime;

    public void RegisterStat(PlayerStat stat)
    {
        this.stat = stat;
    }

    public void DoAttack()
    {
        if (stat == null)
            return;

        if (Time.time - lastFireTime < stat.StatDic[PlayerStat.Stat.BulletFireRate])
            return;

        gun.TryAttack(stat.StatDic[PlayerStat.Stat.BulletSpeed], stat.StatDic[PlayerStat.Stat.BulletDamage]);
        lastFireTime = Time.time;
    }
}
