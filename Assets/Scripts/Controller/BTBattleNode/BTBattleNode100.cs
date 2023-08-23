using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 教程关
/// </summary>
public class BTBattleNode100 : BTBattleNode
{
    protected override void BuildBT()
    {
        return;
    }

    public override IEnumerator BehaviorTree()
    {
        switch(TurnNum)
        {
            // 第一回合空过
            case 1:
                yield return new WaitForSeconds(3f);
                BTSkip();
                break;
            // 第三回合空过，让两个孢子丛射击
            case 3:
                yield return new WaitForSeconds(6f);
                BTSkip();
                break;
            // 第五回合部署菇母
            case 5:
                yield return new WaitForSeconds(3f);
                BTDeploy(0);
                yield return new WaitForSeconds(1.5f);
                BTSkip();
                break;
            // 第七回合移动菇母和亮顶孢子
            case 7:
                yield return new WaitForSeconds(3f);
                BTMove(3, 0, 2, 0);
                yield return new WaitForSeconds(1f);
                BTMove(3, 0, 2, 0);
                yield return new WaitForSeconds(1.5f);
                BTSkip();
                break;
            // 空过让孢子丛攻击
            case 9:
                yield return new WaitForSeconds(3f);
                BTSkip();
                break;
            // 空过
            case 11:
                yield return new WaitForSeconds(1.5f);
                BTSkip();
                break;
            // 部署两个菇母
            case 13:
                yield return new WaitForSeconds(3f);
                BTDeploy(0);
                yield return new WaitForSeconds(1f);
                BTDeploy(0);
                yield return new WaitForSeconds(2f);
                BTSkip();
                break;
            // 蘑菇上前
            case 15:
                yield return new WaitForSeconds(3f);
                BTMove(3, 2, 2, 0);
                yield return new WaitForSeconds(1f);
                BTMove(3, 0, 2, 0);
                yield return new WaitForSeconds(1f);
                BTMove(3, 0, 2, 0);
                yield return new WaitForSeconds(2f);
                BTSkip();
                break;
            case 17:
                yield return new WaitForSeconds(3f);
                BTSkip();
                break;
            case 19:
                yield return new WaitForSeconds(1.5f);
                BTSkip();
                break;
            case 21:
                yield return new WaitForSeconds(1f);
                BTTargetCast(GetHandicapIdx("comm_mush_15"), 1, 0);
                yield return new WaitForSeconds(3f);
                BTTargetCast(GetHandicapIdx("comm_mush_15"), 1, 0);
                yield return new WaitForSeconds(1f);
                BTTargetCast(GetHandicapIdx("comm_mush_15"), 1, 0);
                yield return new WaitForSeconds(3f);
                BTSkip();
                break;
            case 23:
                yield return new WaitForSeconds(3f);
                BTSkip();
                break;
            case 25:
                yield return new WaitForSeconds(3f);
                BTSkip();
                break;
            case 27:
                yield return new WaitForSeconds(3f);
                BTSkip();
                break;
            case 29:
                yield return new WaitForSeconds(3f);
                BTSkip();
                break;
            default:
                yield return new WaitForSeconds(3f);
                BTSkip();
                break;
        }
        yield return new WaitForSeconds(3f);
    }


}

