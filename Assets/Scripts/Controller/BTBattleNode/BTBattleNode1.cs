using BehaviorTree;
using System;
using System.Collections.Generic;

/// <summary>
/// boss克隆蘑人，策略为调整战线使得每次克隆的克隆蘑人数量最多
/// </summary>
public class BTBattleNode1 : BTBattleNode
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
            // new ActionNode(() => TryAdjustForward(frontLineIdx)),
            new ActionNode(() => TryCastComm15(frontLineIdx)),
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
		for (int i = FieldCapacity - 1; i > frontLineIdx + 1; i--)
		{
			// 单位数大于容量的一半时，将本战线血量低的单位往前推
			if (GetIsMoreThanHalf(i) && GetIsLineAvailable(i - 1))
			{
				Tuple<int, int> minHealthInfo = GetAvailableMinHealth(i);
				int minHealthPos = minHealthInfo.Item2;

				// 若存在可操作对象，则执行操作
				if (minHealthPos > -1)
				{
					return BTMove(i, minHealthPos, i - 1, 0);
				}
			}

			// 当单位数小于或等于容量的一半减一，且前一条战线单位数大于容量一半时，将前一条战线血量高的往后撤
			if (!GetIsMoreThanHalfMinusOne(i) && GetIsMoreThanHalf(i - 1))
			{
				Tuple<int, int> maxHealthInfo = GetAvailableMaxHealth(i - 1);
				int maxHealthPos = maxHealthInfo.Item2;

				if (maxHealthPos > -1)
				{
                    return BTMove(i - 1, maxHealthPos, i, 0);
				}
			}
		}
		return false;
	}
}

