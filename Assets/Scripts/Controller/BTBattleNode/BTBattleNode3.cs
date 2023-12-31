using BehaviorTree;
using System;
using System.Collections.Generic;

/// <summary>
/// TODO
/// </summary>
public class BTBattleNode3 : BTBattleNode
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void BuildBT()
    {
        rootNode = new SelectorNode(new List<BTNode>()
        {
            new ActionNode(() => TryAdjustHalf()),
            new ActionNode(() => TryCastLowCost()),
        });
    }

    /// <summary>
    /// 战线单位数大于战线容量一半则返回true
    /// </summary>
    /// <param name="battleLineIdx"></param>
    /// <returns></returns>
    private bool GetIsMoreThanHalf(int battleLineIdx)
    {
        return BattleLines[battleLineIdx].count > BattleLines[battleLineIdx].capacity / 2;
    }

    /// <summary>
    /// 战线单位数大于战线容量一半减一则返回true
    /// </summary>
    /// <param name="battleLineIdx"></param>
    /// <returns></returns>
    private bool GetIsMoreThanHalfMinusOne(int battleLineIdx)
    {
        return BattleLines[battleLineIdx].count > (BattleLines[battleLineIdx].capacity / 2 - 1);
    }

    /// <summary>
    /// 将每条战线调整至单位数不大于战线容量一半
    /// </summary>
    private bool TryAdjustHalf()
    {
        for (int i = FieldCapacity - 1; i > FrontLineIdx + 1; i--)
        {
            // 单位数大于容量的一半时，将本战线血量低的单位往前推
            if (GetIsMoreThanHalf(i))
            {
                Tuple<int, int> minHealthInfo = GetAvailableMinHealth(i);
                int minHealthPos = minHealthInfo.Item2;

                // 若存在可操作对象，则执行操作
                if (minHealthPos > -1)
                {
                    BTMove(i, minHealthPos, i + 1, 0);
                    return true;
                }
            }

            // 当单位数小于或等于容量的一半减一，且前一条战线单位数大于容量一半时，将前一条战线血量高的往后撤
            if (!GetIsMoreThanHalfMinusOne(i) && GetIsMoreThanHalf(i - 1))
            {
                Tuple<int, int> maxHealthInfo = GetAvailableMaxHealth(i - 1);
                int maxHealthPos = maxHealthInfo.Item2;

                if (maxHealthPos > -1)
                {
                    BTMove(i - 1, maxHealthPos, i, 0);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 将手牌中最低费的指令打出
    /// </summary>
    /// <returns></returns>
    private bool TryCastLowCost()
    {
        int minCostPointer = GetMinCostCommPointer();
        if (minCostPointer < 0)
        {
            return false;
        }
        else
        {
            BTNoneTargetCast(minCostPointer);
            return true;
        }
    }
}

