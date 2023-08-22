using BehaviorTree;
using System;
using System.Collections.Generic;

/// <summary>
/// boss��¡Ģ�ˣ�����Ϊ����ս��ʹ��ÿ�ο�¡�Ŀ�¡Ģ���������
/// </summary>
public class BTBattleNode2 : BTBattleNode
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
	/// ս�ߵ�λ������ս������һ���򷵻�true
	/// </summary>
	/// <param name="battleLineIdx"></param>
	/// <returns></returns>
	private bool GetIsMoreThanHalf(int battleLineIdx)
	{
		return BattleLines[battleLineIdx].count > BattleLines[battleLineIdx].capacity / 2;
	}

	/// <summary>
	/// ս�ߵ�λ������ս������һ���һ�򷵻�true
	/// </summary>
	/// <param name="battleLineIdx"></param>
	/// <returns></returns>
	private bool GetIsMoreThanHalfMinusOne(int battleLineIdx)
	{
		return BattleLines[battleLineIdx].count > (BattleLines[battleLineIdx].capacity / 2 - 1);
	}

	/// <summary>
	/// ��ÿ��ս�ߵ�������λ��������ս������һ��
	/// </summary>
	private bool TryAdjustHalf()
	{
		for (int i = FieldCapacity - 1; i > frontLineIdx + 1; i--)
		{
			// ��λ������������һ��ʱ������ս��Ѫ���͵ĵ�λ��ǰ��
			if (GetIsMoreThanHalf(i) && GetIsLineAvailable(i - 1))
			{
				Tuple<int, int> minHealthInfo = GetAvailableMinHealth(i);
				int minHealthPos = minHealthInfo.Item2;

				// �����ڿɲ���������ִ�в���
				if (minHealthPos > -1)
				{
					BTMove(i, minHealthPos, i - 1, 0);
					return true;
				}
			}

			// ����λ��С�ڻ����������һ���һ����ǰһ��ս�ߵ�λ����������һ��ʱ����ǰһ��ս��Ѫ���ߵ�����
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
	/// ����������ͷѵ�ָ����
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

