using BehaviorTree;
using System.Collections.Generic;

/// <summary>
/// boss九号实验体，策略跟白菇类似
/// </summary>
public class BTBattleNode1 : BTBattleNode
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

