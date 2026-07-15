using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SwordAttacker : MonoBehaviour
{
    [SerializeField] Sword sword;

    RotateByAim rotateByAim;
    float swingStartRot = -45f;
    float swingEndRot = 45f;

    [Tooltip("플레이어 모델")]
    [SerializeField] SpriteRenderer characterModel;

    PlayerStat stat;

    #region HoldVariables
    //캐릭터가 왼쪽을 보고 있을 때 들고 있는 위치
    Vector3 originHoldPos_L = new Vector3(0.28f, -0f, 0f);
    Quaternion originHoldRot_L = Quaternion.Euler(0f, 0f, 30f);
    //캐릭터가 오른쪽을 보고 있을 때 들고 있는 위치
    Vector3 originHoldPos_R = new Vector3(-0.28f, -0f, 0f);
    Quaternion originHoldRot_R = Quaternion.Euler(0f, 180f, 30f);
    #endregion
    
    Coroutine swingRoutine;

    private void Awake()
    {
        rotateByAim = sword.GetComponent<RotateByAim>();

        if (rotateByAim != null)
            rotateByAim.enabled = false;
    }

    public void RegisterStat(PlayerStat stat)
    {
        this.stat = stat;
    }

    public void DoAttack()
    {
        if (sword == null || stat == null)
            return;

        if (swingRoutine != null)
            return;

        swingRoutine = StartCoroutine(SwingTime());
        sword.TryAttack(stat.StatDic[PlayerStat.Stat.SwordDamage], stat.StatDic[PlayerStat.Stat.SwordSwingRate]);
    }

    private void Update()
    {
        Hold();
    }

    /// <summary>
    /// 칼을 잡고 있는 상태
    /// </summary>
    void Hold()
    {
        if (swingRoutine != null) //스윙하고 있는 상태면 리턴
            return;

        //플레이어 캐릭터의 플립에 영향을 받음
        if (characterModel.flipX)
        {
            sword.transform.localPosition = originHoldPos_R;
            sword.transform.localRotation = originHoldRot_R;
        }
        else
        {
            sword.transform.localPosition = originHoldPos_L;
            sword.transform.localRotation = originHoldRot_L;
        }
    }

    /// <summary>
    /// 칼을 회전시킵니다
    /// </summary>
    /// <returns></returns>
    IEnumerator SwingTime()
    {
        //rotateByAim 에서 마우스 에임 기준 로테이션 정보를 가져옵니다
        yield return null;

        if (rotateByAim == null)
            yield break;

        var aim = rotateByAim.GetAimPos();
        sword.Attack();

        sword.transform.localPosition = Vector3.zero;
        //z가 90 ~ 260 일 땐 칼이 오른쪽에 있으므로 반전
        bool isFlip = (aim > 90 && aim < 260);
        float swingStartRot = aim + (isFlip ? -this.swingStartRot : this.swingStartRot);
        float swingEndRot = aim + (isFlip ? -this.swingEndRot : this.swingEndRot);
        sword.Model.flipY = !isFlip;

        rotateByAim.enabled = false;
        sword.transform.localRotation = Quaternion.Euler(0f, 0f, swingStartRot);

        float elapsedTime = 0f;
        float duration = 1f / stat.StatDic[PlayerStat.Stat.SwordSwingSpeed];

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);

            sword.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(swingStartRot, swingEndRot, t));
            yield return null;
        }

        sword.transform.localRotation = Quaternion.Euler(0f, 0f, swingEndRot);

        sword.EndAttack();
        swingRoutine = null;

        Hold();
    }
}
