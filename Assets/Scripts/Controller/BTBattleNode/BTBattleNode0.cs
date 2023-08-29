using BehaviorTree;
using System.Collections.Generic;

/// <summary>
/// boss白菇，策略为狙击加治疗
/// </summary>
public class BTBattleNode0 : BTBattleNode
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
            new ActionNode(() => TryCastComm15()),
            new SequenceNode(new List<BTNode>()
            {
                new ConditionNode(() => GetIsLineAvailable(AIAdjacentLineIdx)),
                new ActionNode(() => TryCast("comm_mush_13")),
            }),
            new ActionNode(() => TryCast("comm_mush_10")),
            new ActionNode(() => TryCast("comm_mush_08")),
        });
    }
}

