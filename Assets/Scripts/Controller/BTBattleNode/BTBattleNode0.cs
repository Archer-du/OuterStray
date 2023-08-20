using BehaviorTree;
using System.Collections.Generic;

/// <summary>
/// boss白菇，策略为大量部署亮顶孢子
/// </summary>
public class BTBattleNode0 : BTBattleNode
{
    protected override void BuildBT()
    {
        rootNode = new SelectorNode(new List<BTNode>()
        {
            new ActionNode(() => TryCast("comm_mush_07")),
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => !GetIsLineAvailable(AISupportLineIdx)),
                new ActionNode(() => TryRetreatSomeUnits(AISupportLineIdx)),
            }),
            new ActionNode(() => TryAdjustForward(frontLineIdx)),
            new ActionNode(() => TryDeployLowCostUnit(AISupportLineIdx)),
            new ActionNode(() => TryCast("comm_mush_01")),
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => GetIsLineAvailable(AIAdjacentLineIdx)),
                new ActionNode(() => TryCast("comm_mush_13")),
            }),
            new ActionNode(() => TryCast("comm_mush_08")),
            new ActionNode(() => TryCastComm18(frontLineIdx)),
            new ActionNode(() => TrySkip()),
        });
    }
}

