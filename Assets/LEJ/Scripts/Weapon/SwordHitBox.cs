using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class SwordHitBox : MonoBehaviour, IDamageable
{
    HashSet<GameObject> hitTargets = new(); //하나의 적이 여러 번 공격 인정 되는 것을 방지
    public UnityAction<GameObject> OnHit;

    private void OnEnable()
    {
        hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //콜리젼에 여러 번 들어오는 것을 방지하기 위한 중복 체크
        if (hitTargets.Contains(collision.gameObject))
            return;

        hitTargets.Add(collision.gameObject);
        OnHit?.Invoke(collision.gameObject);
    }

    public void TakeDamage(float damage)
    {
        //현재 enemyBullet 은 Player tag 와 IDamageable 이 적용되어있기 때문에
        //Sword 에 IDamageable 을 넣어 실제 데미지를 입진 않는 것으로 탄환을 막는 것을 구현했습니다
        Debug.Log("칼이 총알을 가름");
    }
}
