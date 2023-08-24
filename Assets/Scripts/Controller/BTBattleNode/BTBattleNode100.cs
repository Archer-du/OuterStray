using BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 教程关
/// </summary>
public class BTBattleNode100 : BTBattleNode
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void BuildBT()
    {
        return;
    }

    public override IEnumerator BehaviorTree()
    {
        Init();

        if (TurnNum == 21)
        {
            Tuple<int, int> Info;
            yield return new WaitForSeconds(1f);

            Info = GetInfoByCardID("tutorial_27");
            if (Info.Item1 > -1 && Info.Item2 > -1)
            {
                BTTargetCast(GetHandicapIndexByCardID("comm_mush_15"), Info.Item1, Info.Item2);
            }
            yield return new WaitForSeconds(3f);

            Info = GetInfoByCardID("tutorial_27");
            if (Info.Item1 > -1 && Info.Item2 > -1)
            {
                BTTargetCast(GetHandicapIndexByCardID("comm_mush_15"), Info.Item1, Info.Item2);
            }
            yield return new WaitForSeconds(1f);

            Info = GetInfoByCardID("tutorial_27");
            if (Info.Item1 > -1 && Info.Item2 > -1)
            {
                BTTargetCast(GetHandicapIndexByCardID("comm_mush_15"), Info.Item1, Info.Item2);
            }
            yield return new WaitForSeconds(1f);

            Info = GetInfoByCardID("tutorial_27");
            if (Info.Item1 > -1 && Info.Item2 > -1)
            {
                BTTargetCast(GetHandicapIndexByCardID("comm_mush_15"), Info.Item1, Info.Item2);
            }
            yield return new WaitForSeconds(5f);
        }
        else if(guideRunning)
        {
            switch (TurnNum)
            {
                // 第一回合空过
                case 1:
                    yield return new WaitForSeconds(6f);
                    break;
                // 第三回合空过，让两个孢子丛射击
                case 3:
                    yield return new WaitForSeconds(8f);
                    break;
                // 第五回合部署菇母
                case 5:
                    yield return new WaitForSeconds(3f);
                    BTDeploy(GetHandicapIndexByCardID("mush_02"));

                    yield return new WaitForSeconds(2f);
                    break;
                // 第七回合移动菇母和亮顶孢子
                case 7:
                    yield return new WaitForSeconds(3f);
                    BTMove(3, 0, 2, 0);

                    yield return new WaitForSeconds(1f);
                    BTMove(3, 0, 2, 0);

                    yield return new WaitForSeconds(1.5f);
                    break;
                // 空过让孢子丛攻击
                case 9:
                    yield return new WaitForSeconds(3f);
                    break;
                // 空过
                case 11:
                    yield return new WaitForSeconds(1.5f);
                    break;
                // 部署两个菇母
                case 13:
                    yield return new WaitForSeconds(3f);
                    BTDeploy(GetHandicapIndexByCardID("mush_02"));

                    yield return new WaitForSeconds(1f);
                    BTDeploy(GetHandicapIndexByCardID("mush_02"));

                    yield return new WaitForSeconds(1.5f);
                    break;
                // 蘑菇上前
                case 15:
                    yield return new WaitForSeconds(3f);
                    BTMove(3, 2, 2, 0);

                    yield return new WaitForSeconds(1f);
                    BTMove(3, 0, 2, 0);

                    yield return new WaitForSeconds(1f);
                    BTMove(3, 0, 2, 0);

                    yield return new WaitForSeconds(1.5f);
                    break;
                case 17:
                    yield return new WaitForSeconds(3f);
                    break;
                case 19:
                    yield return new WaitForSeconds(1.5f);
                    break;
                default:
                    yield return new WaitForSeconds(3f);
                    break;
            }
        }
        else
        {
            yield return new WaitForSeconds(1.5f + SequenceTime);
            if (TryDeployLowCostUnit(AISupportLineIdx))
            {
                yield return new WaitForSeconds(1.5f + SequenceTime);
            }
            if (TryDeployLowCostUnit(AISupportLineIdx))
            {
                yield return new WaitForSeconds(1.5f + SequenceTime);
            }
            if (TryMoveForward(AISupportLineIdx))
            {
                yield return new WaitForSeconds(1.5f + SequenceTime);
            }
            if (TryMoveForward(AISupportLineIdx))
            {
                yield return new WaitForSeconds(1.5f + SequenceTime);
            }

        }
        yield return new WaitForSeconds(1.5f + SequenceTime);
        BTSkip(); 
    }



    public Tuple<int, int> GetInfoByCardID(string CardID)
    {
        int dstLineIdx = -1;
        int dstPos = -1;
        BattleLineController battleLine;
        for (int i = 0; i < FieldCapacity; i++)
        {
            battleLine = BattleLines[i];
            for (int j = 0; j < battleLine.count; j++)
            {
                if (battleLine[j].ID == CardID)
                {
                    dstLineIdx = i;
                    dstPos = j;
                }
            }
        }
        return Tuple.Create(dstLineIdx, dstPos);
    }
}

