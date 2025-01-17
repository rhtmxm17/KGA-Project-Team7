using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아군 전원의 공격력을 올리는 스킬
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Skill/AllTargetAttackBuff")]
public class AllTargetAttackBuff : Skill
{
    [Header("공격버프 배율")]
    [SerializeField]
    float atkRate;
    [SerializeField]
    float duringTime;

    // 캐싱 데이터
    private WaitForSeconds waitPreDelay;
    private WaitForSeconds waitPostDelay;

    private void OnEnable()
    {
        // 런타임 진입시 필요한 데이터 캐싱
        waitPreDelay = new WaitForSeconds(preDelay);
        waitPostDelay = new WaitForSeconds(postDelay);
    }

    protected override IEnumerator SkillRoutineImplement(Combatable self, Combatable target)
    {
        yield return waitPreDelay;

        self.PlayAttckSnd();

        foreach (Combatable friendlyTarget in self.Group.CharList)
        {
            if (friendlyTarget != null && friendlyTarget.IsAlive)
            {
                friendlyTarget.StartCoroutine(StartBufftimeCO(friendlyTarget, (int)(atkRate * self.CurAttackPoint)));
            }
        }

        yield return waitPostDelay;

    }

    IEnumerator StartBufftimeCO(Combatable target, int amount)
    {
        yield return null;
        target.AddAtkBuff(amount);
        yield return new WaitForSeconds(duringTime);
        if(target != null && target.IsAlive)
            target.RemoveAtkBuff(amount);
    }
}
