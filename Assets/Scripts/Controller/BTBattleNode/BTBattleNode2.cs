using BehaviorTree;
using System.Collections.Generic;

/// <summary>
/// boss九号实验体，策略跟白菇类似
/// </summary>
public class BTBattleNode2 : BTBattleNode
{
    protected override void BuildBT()
    {
        rootNode = new SelectorNode(new List<BTNode>()
        {
            new ActionNode(() => TryCastComm2()),
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => AIHandicap.count < AIHandicap.capacity - 2),
                new ActionNode(() => TryCast("comm_mush_07")),
            }),
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => !GetIsLineAvailable(AISupportLineIdx) || FrontLineIdx == AISupportLineIdx - 1),
                new ActionNode(() => TryRetreatUnits(AISupportLineIdx)),
            }),
            new ActionNode(() => TryAdjustForward(FrontLineIdx)),
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => Energy > 8),
                new ActionNode(() => TryDeployHighCostUnit(AISupportLineIdx)),
            }),
            new ActionNode(() => TryDeployLowCostUnit(AISupportLineIdx)),
            new SequenceNode(new List<BTNode>
            {
                new ConditionNode(() => AISupportLine.count < AISupportLine.capacity - 1),
                new ActionNode(() => TryCast("comm_mush_01")),
            }),
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => GetIsLineAvailable(AIAdjacentLineIdx)),
                new ActionNode(() => TryCast("comm_mush_13")),
            }),
            new ActionNode(() => TryCast("comm_mush_08")),
        });
    }
}


