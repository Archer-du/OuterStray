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
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => AIHandicap.count < AIHandicap.capacity - 2),
                new ActionNode(() => TryCast("comm_mush_07")),
            }),
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => !GetIsLineAvailable(AISupportLineIdx)),
                new ActionNode(() => TryRetreatSomeUnits(AISupportLineIdx)),
            }),
            new ActionNode(() => TryAdjustForward(frontLineIdx)),
            new ActionNode(() => TryDeployLowCostUnit(AISupportLineIdx)),
            new SequenceNode(new List<BTNode>
            {
                new ConditionNode(() => AISupportLine.count < AISupportLine.capacity - 1),
                new ActionNode(() => TryCast("comm_mush_01")),
            }),
            new ActionNode(() => TryCastComm15(frontLineIdx)),
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => GetIsLineAvailable(AIAdjacentLineIdx)),
                new ActionNode(() => TryCast("comm_mush_13")),
            }),
            new ActionNode(() => TryCast("comm_mush_08")),
        });
    }
}

