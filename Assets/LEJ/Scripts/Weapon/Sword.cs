using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sword : WeaponBase
{
    [SerializeField] SpriteRenderer model;
    [SerializeField] float damage;

    [SerializeField] float attackRate; //검 휘두르는 것에 대한 쿨타임
    float lastAttackTime;

    Collider2D col;
    HashSet<IDamageable> hitTargets = new(); //하나의 적이 여러 번 공격 인정 되는 것을 방지

    GameObject owner;

    [SerializeField] RotateByAim rotateByAim;
    [SerializeField] float swingStartRot = -45f;
    [SerializeField] float swingEndRot = 45f;

    [SerializeField] SpriteRenderer characterModel;
    bool isAttacking = false;

    //캐릭터가 왼쪽을 보고 있을 때 들고 있는 위치
    Vector3 originHoldPos_L = new Vector3(0.28f, -0.45f, 0f);
    Quaternion originHoldRot_L = Quaternion.Euler(0f, 0f, 30f);
    //캐릭터가 오른쪽을 보고 있을 때 들고 있는 위치
    Vector3 originHoldPos_R = new Vector3(-0.28f, -0.45f, 0f);
    Quaternion originHoldRot_R = Quaternion.Euler(0f, 180f, 30f);

    float charactorCenterY = -0.4f; //캐릭터 기준 중심 y 를 기준으로 회전합니다
    float curSpeed = 1f;

    private void Awake()
    {
        if (model != null)
            col = model.GetComponent<Collider2D>();

        rotateByAim.enabled = false;
    }

    private void Update()
    {
        Hold();
    }

    public override void SetOwner(GameObject owner)
    {
        this.owner = owner;
        lastAttackTime = -attackRate; //처음에 바로 공격할 수 있도록 초기화
    }

    void Hold()
    {
        if (isAttacking) //공격 상태가 아닌 그냥 들고 있을 때만 실행
            return;

        //플레이어 캐릭터의 플립에 영향을 받음
        if (characterModel.flipX)
        {
            transform.localPosition = originHoldPos_R;
            transform.localRotation = originHoldRot_R;
        }
        else
        {
            transform.localPosition = originHoldPos_L;
            transform.localRotation = originHoldRot_L;
        }
    }

    public override void TryAttack(float speed)
    {
        if (Time.time - lastAttackTime < attackRate)
            return;

        curSpeed = speed;
        Attack();

        lastAttackTime = Time.time;
    }

    internal override void Attack()
    {
        isAttacking = true;
        StartCoroutine(SwingRoutine());
    }

    void EndAttack()
    {
        //검 공격 중단 후 콜라이더를 끕니다
        isAttacking = false;
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();

        //데미지를 안 받는 오브젝트거나 공격자 자신이면 무시
        if (target == null || collision.gameObject == owner)
            return;
        
        //콜리젼에 여러 번 들어오는 것을 방지하기 위한 중복 체크
        if (hitTargets.Contains(target))
            return;

        hitTargets.Add(target);
        target.TakeDamage(damage);
    }

    /// <summary>
    /// 칼을 회전시킵니다
    /// </summary>
    /// <returns></returns>
    IEnumerator SwingRoutine()
    {
        //rotateByAim 에서 마우스 에임 기준 로테이션 정보를 가져옵니다
        yield return null;

        if (rotateByAim == null)
            yield break;

        var aim = rotateByAim.GetAimPos();
        col.enabled = true;

        transform.localPosition = new Vector3(0f, charactorCenterY, 0f);
        //z가 90 ~ 260 일 땐 칼이 오른쪽에 있으므로 반전
        bool isFlip = (aim > 90 && aim < 260);
        float swingStartRot = aim + (isFlip ? -this.swingStartRot : this.swingStartRot);
        float swingEndRot = aim + (isFlip ? -this.swingEndRot : this.swingEndRot);
        model.flipY = !isFlip;

        rotateByAim.enabled = false;
        transform.localRotation = Quaternion.Euler(0f, 0f, swingStartRot);

        float elapsedTime = 0f;
        float duration = 1f / curSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);

            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(swingStartRot, swingEndRot, t));
            yield return null;
        }

        transform.localRotation = Quaternion.Euler(0f, 0f, swingEndRot);

        EndAttack();
    }
}
