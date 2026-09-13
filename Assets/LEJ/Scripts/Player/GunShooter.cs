using UnityEngine;
using UnityEngine.Events;

public class GunShooter : MonoBehaviour
{
    [SerializeField] Gun gun;
    public Gun Gun => gun;

    PlayerStat stat;
    float lastFireTime;

    public event UnityAction OnShoot;

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
        SoundManager.Instance.PlaySFX(Sound_SFX.Player_GunShoot);
        lastFireTime = Time.time;

        OnShoot?.Invoke();
    }

    public void ForceAttack()
    {
        gun.TryAttack(stat.StatDic[PlayerStat.Stat.BulletSpeed], stat.StatDic[PlayerStat.Stat.BulletDamage]);
        SoundManager.Instance.PlaySFX(Sound_SFX.Player_GunShoot);
    }

    public void ActiveGun(bool isActive)
    {
        gun.gameObject.SetActive(isActive);
    }
}
