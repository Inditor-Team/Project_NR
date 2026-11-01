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
        lastFireTime = Time.time;

        OnShoot?.Invoke();
        PlayGunShootSFX();
    }

    public void ForceAttack()
    {
        gun.TryAttack(stat.StatDic[PlayerStat.Stat.BulletSpeed], stat.StatDic[PlayerStat.Stat.BulletDamage]);
        PlayGunShootSFX();
    }

    public void ActiveGun(bool isActive)
    {
        gun.gameObject.SetActive(isActive);
    }


    int randValue;
    void PlayGunShootSFX()
    {
        randValue = Random.Range(0, 2);

        if (randValue == 0)
            SoundManager.Instance.PlaySFX(Sound_SFX.Player_GunShoot);
        else
            SoundManager.Instance.PlaySFX(Sound_SFX.Player_GunShootB);
    }
}
