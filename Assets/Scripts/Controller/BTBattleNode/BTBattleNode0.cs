using BehaviorTree;
using LogicCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

/*namespace BehaviorTree
{
    public class BTBattleNode0 : BTBattleNode
    {


        public override void Init()
        {
            sceneManager = FindObjectOfType<BattleSceneManager>();

            *//*            AIHandicap = sceneManager.handicapController[1];
                        energy = sceneManager.energy[1];
                        fieldCapacity = sceneManager.fieldCapacity;*//*

            AISupportLineIdx = sceneManager.fieldCapacity - 1;
            AISupportLine = sceneManager.battleLineControllers[AISupportLineIdx];

            frontLineIdx = GetFrontLineIdx();
            AIAdjacentLineIdx = frontLineIdx + 1;
            AIAdjacentLine = sceneManager.battleLineControllers[AIAdjacentLineIdx];

            // 构造行为树
            BTNode TryCastComm07Node = new ActionNode(() => TryCast("comm_mush_07"));
            BTNode TryRetreatSomeUnitsNode = new ActionNode(() => TryRetreatSomeUnits(AISupportLineIdx));
            BTNode TryAdjustForwardNode = new ActionNode(() => TryAdjustForward(frontLineIdx));
            BTNode TryDeployLowCostUnitNode = new ActionNode(() => TryDeployLowCostUnit(AISupportLineIdx));
            BTNode TryCastComm01Node = new ActionNode(() => TryCast("comm_mush_01"));
            BTNode TryCastComm13Node = new ActionNode(() => TryCast("comm_mush_13"));
            BTNode TryCastComm08Node = new ActionNode(() => TryCast("comm_mush_08"));
            BTNode TryCastComm18Node = new ActionNode(() => TryCastComm18(frontLineIdx));
            BTNode TrySkipNode = new ActionNode(() => TrySkip());

            BTNode rootSelector = new SelectorNode(new List<BTNode>()
            {
                TryCastComm07Node, TryRetreatSomeUnitsNode, TryAdjustForwardNode, TryDeployLowCostUnitNode, TryCastComm01Node, TryCastComm13Node, TryCastComm08Node, TryCastComm18Node, TrySkipNode
            });
            rootNode = new CoroutineNode(this, rootSelector);
        }

        public bool TryCastComm18(int frontLineIdx)
        {
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].ID == "comm_mush_18" && energy >= 3)
                {
                    Tuple<int, int, int> MaxHealthInfo = GetFieldMaxHealth(frontLineIdx);
                    BTCast(i, MaxHealthInfo.Item2, MaxHealthInfo.Item3);
                    Debug.Log("BT tried cast 'comm_mush_18'");
                    return true;
                }
            }
            return false;
        }

        public bool TryCastComm13(BattleLineController AIAdjacentLine)
        {
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].ID == "comm_mush_13" && energy >= 6 && AIAdjacentLine.count < AIAdjacentLine.count)
                {
                    BTCast(i, 0, 0);
                    Debug.Log("BT tried cast 'comm_mush_13'");
                    return true;
                }
            }
            return false;
        }

        public bool TryAdjustForward(int frontLineIdx)
        {
            for (int i = fieldCapacity - 1; i > frontLineIdx + 1; i--)
            {
                BattleLineController battleLine = battleLineControllers[i];

                // 前一条线有空位则尝试前移
                if (GetIsLineAvailable(i - 1))
                {
                    int artilleryIdx = -1;

                    // 遍历该战线
                    for (int j = 0; j < battleLine.count; j++)
                    {
                        // 判断规则上能移动的卡（轰击boss || 建筑 || 移动次数为0）
                        if (battleLine[j].ID != "mush_102" && battleLine[j].category != "Construction" && battleLine[j].operateCounter == 1)
                        {
                            // 若该战线有空，则不会移动轰击
                            if (GetIsLineAvailable(i))
                            {
                                if (battleLine[j].category != "Artillery")
                                {
                                    BTMove(i, j, i - 1, 0);
                                    Debug.Log($"AIMove({i}, {j}, {i - 1}, 0)");
                                    return true;
                                }
                                else
                                {
                                    artilleryIdx = j;
                                }
                            }

                            //TODO 若战线没有空，则可移动轰击，但移动轰击的优先级最低
                            else
                            {
                                BTMove(i, j, i - 1, 0);
                                Debug.Log($"AIMove({i}, {j}, {i - 1}, 0)");
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool TryRetreatSomeUnits(int AISupportLineIdx)
        {
            BattleLineController battleLine = battleLineControllers[AISupportLineIdx];
            if (battleLine.count == battleLine.capacity)
            {
                for (int i = 0; i < battleLine.count; i++)
                {
                    if (battleLine[i].operateCounter == 1)
                    {
                        if (battleLine[i].ID == "mush_04" || battleLine[i].ID == "mush_10" || battleLine[i].ID == "mush_11" || battleLine[i].ID == "mush_13")
                        {
                            BTRetreat(AISupportLineIdx, i);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool TryDeployLowCostUnit(int AISupportLineIdx)
        {
            if (AISupportLine.count < AISupportLine.capacity)
            {
                // 若支援战线的建筑数小于战线容量减二，则建筑也可部署
                if (GetConstructionNum(AISupportLineIdx) < AISupportLine.capacity - 2)
                {
                    int idx = GetMinCostUnitPointer();
                    if (idx >= 0)
                    {
                        BTDeploy(idx);
                        Debug.Log("Deploy");
                        return true;
                    }
                }
                // 若支援战线的建筑数大于等于战线容量减二，则不部署建筑
                else
                {
                    int idx = GetMinCostUnitPointerExcConstr();
                    if (idx >= 0)
                    {
                        BTDeploy(idx);
                        Debug.Log("Deploy");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
*/

public class BTBattleNode0 : BattleSceneManager
{
    //行为树
    HandicapController AIHandicap;
    BattleLineController AISupportLine;
    BattleLineController AIAdjacentLine;

    IEnumerator BehaviorTree()
    {
        float waitTime = 1f;
        yield return new WaitForSeconds(waitTime);
        AIHandicap = handicapController[1];

        int AISupportLineIdx = fieldCapacity - 1;
        AISupportLine = battleLines[AISupportLineIdx];

        int frontLineIdx = GetFrontLineIdx();
        AIAdjacentLine = battleLines[frontLineIdx + 1];


        int whileCounter = 30;
    startwhile:
        whileCounter--;
        Debug.Log(whileCounter);
        while (energy[Turn] > 2 && whileCounter != 0)
        {
            // 优先使用“蔓延”，补充手牌
            if (TryCast("comm_mush_07"))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 撤退一些带有部署效果的卡片
            if (TryRetreatSomeUnits(AISupportLineIdx))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 调整战线
            if (TryAdjustForward(frontLineIdx))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 部署低费卡
            if (TryDeployLowCostUnit(AISupportLineIdx))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 菇军奋战，铺场
            if (TryCast("comm_mush_01"))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 散播孢子，扩大场面
            if (TryCastComm13(AIAdjacentLine))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 增殖，赚卡
            if (TryCast("comm_mush_08"))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 腐蚀，攻击血量高的单位
            if (TryCastComm18(frontLineIdx))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            break;
        }

        yield return new WaitForSeconds(waitTime);
        Skip();
    }

    private bool TryCast(string cardID)
    {
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].ID == cardID && energy[Turn] >= AIHandicap[i].cost)
            {
                AICast(i, 0, 0);
                Debug.Log($"Cast '{cardID}'");
                return true;
            }
        }
        return false;
    }

    private bool TryCastComm18(int frontLineIdx)
    {
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].ID == "comm_mush_18" && energy[Turn] >= 3)
            {

                int dstLineIdx = 0;
                int dstPos = 0;
                int maxHealth = 0;
                Tuple<int, int> lineMaxHealth = Tuple.Create(0, 0);
                for (int j = 0; j < frontLineIdx + 1; j++)
                {
                    lineMaxHealth = GetMaxHealth(j);
                    if (maxHealth > lineMaxHealth.Item1)
                    {
                        dstLineIdx = j;
                        dstPos = lineMaxHealth.Item2;
                    }
                }
                AICast(i, dstLineIdx, dstPos);
                Debug.Log("Cast 'comm_mush_18'");
                return true;
            }
        }
        return false;
    }

    private bool TryCastComm13(BattleLineController AIAdjacentLine)
    {
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].ID == "comm_mush_13" && energy[Turn] >= 6 && AIAdjacentLine.count < AIAdjacentLine.count)
            {
                AICast(i, 0, 0);
                Debug.Log("Cast 'comm_mush_13'");
                return true;
            }
        }
        return false;
    }

    private bool TryAdjustForward(int frontLineIdx)
    {
        for (int i = fieldCapacity - 1; i > frontLineIdx + 1; i--)
        {
            BattleLineController battleLine = battleLines[i];

            // 前一条线有空位则尝试前移
            if (GetIsLineAvailable(i - 1))
            {
                int artilleryIdx = -1;

                // 遍历该战线
                for (int j = 0; j < battleLine.count; j++)
                {
                    // 判断规则上能移动的卡（轰击boss || 建筑 || 移动次数为0）
                    if (battleLine[j].ID != "mush_102" && battleLine[j].category != "Construction" && battleLine[j].operateCounter == 1)
                    {
                        // 若该战线有空，则不会移动轰击
                        if (GetIsLineAvailable(i))
                        {
                            if (battleLine[j].category != "Artillery")
                            {
                                AIMove(i, j, i - 1, 0);
                                Debug.Log($"AIMove({i}, {j}, {i - 1}, 0)");
                                return true;
                            }
                            else
                            {
                                artilleryIdx = j;
                            }
                        }

                        //TODO 若战线没有空，则可移动轰击，但移动轰击的优先级最低
                        else
                        {
                            AIMove(i, j, i - 1, 0);
                            Debug.Log($"AIMove({i}, {j}, {i - 1}, 0)");
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool TryRetreatSomeUnits(int AISupportLineIdx)
    {
        BattleLineController battleLine = battleLines[AISupportLineIdx];
        if (battleLine.count == battleLine.capacity)
        {
            for (int i = 0; i < battleLine.count; i++)
            {
                if (battleLine[i].operateCounter == 1)
                {
                    if (battleLine[i].ID == "mush_04" || battleLine[i].ID == "mush_10" || battleLine[i].ID == "mush_11" || battleLine[i].ID == "mush_13")
                    {
                        AIRetreat(AISupportLineIdx, i);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool TryDeployLowCostUnit(int AISupportLineIdx)
    {
        if (AISupportLine.count < AISupportLine.capacity)
        {
            // 若支援战线的建筑数小于战线容量减二，则建筑也可部署
            if (GetConstructionNum(AISupportLineIdx) < AISupportLine.capacity - 2)
            {
                int idx = GetMinCostUnitPointer();
                if (idx >= 0)
                {
                    AIDeploy(idx);
                    Debug.Log("Deploy");
                    return true;
                }
            }
            // 若支援战线的建筑数大于等于战线容量减二，则不部署建筑
            else
            {
                int idx = GetMinCostUnitPointerExcConstr();
                if (idx >= 0)
                {
                    AIDeploy(idx);
                    Debug.Log("Deploy");
                    return true;
                }
            }
        }
        return false;
    }

    /*IEnumerator AIBehaviourNode3()
	{
        HandicapController handicap = handicapController[1];

        float waitTime = 0.9f;
        yield return new WaitForSeconds(waitTime);

        int movetimes = 1;
		int frontLineIdx = GetFrontLineIdx();

        // 移动策略：调整各蘑人站位，血量少的往前推，血量多的往后撤，使回合结束增殖时收益最大
		// 从支援战线开始遍历一次，调整站位
        for (int i = battleLineControllers.Length - 1; i > frontLineIdx; i--)
        {
            int halfCapacity = battleLineControllers[i].capacity / 2;

            // 单位数大于容量的一半时，将本战线血量低的单位往前推
            while (battleLineControllers[i].count > halfCapacity && i > frontLineIdx + 1)
            {
                Tuple<int, int> minHealthInfo = GetAvailableMinHealth(i);
                int minHealthPointer = minHealthInfo.Item2;

				// 若存在可操作对象，则执行操作
				if (minHealthPointer > -1)
				{
                    AIMove(i, minHealthPointer, i + 1, 0);
                    movetimes++;
                }
            }

            // 当单位数小于或等于容量的一半减一，且前一条战线单位数大于容量一半时，将前一条战线血量高的往后撤
            while (battleLineControllers[i].count <= halfCapacity - 1 && battleLineControllers[i - 1].count > battleLineControllers[i - 1].capacity / 2 && i > frontLineIdx + 1)
            {
                Tuple<int, int> maxHealthInfo = GetAvailableMaxHealth(i - 1);
                int maxHealthPointer = maxHealthInfo.Item2;

				if(maxHealthPointer > -1)
				{
                    AIMove(i - 1, maxHealthPointer, i, 0);
                    movetimes++;
                }
            }
        }
		// 反向遍历一次，使站位更合理
		for (int i = frontLineIdx + 1; i < battleLineControllers.Length; i++)
		{
			int halfCapacity = battleLineControllers[i].capacity / 2;

			// 当单位数大于容量一半，且后一条战线单位数小于容量一半时，将本战线血量高的往后撤
			while (battleLineControllers[i].count > halfCapacity && battleLineControllers[i + 1].count <= battleLineControllers[i + 1].capacity / 2 && i < fieldCapacity - 1)
            {
				Tuple<int, int> maxHealthInfo = GetAvailableMaxHealth(i);
				int maxHealthPointer = maxHealthInfo.Item2;

				if (maxHealthPointer > -1)
				{
                    AIMove(i, maxHealthPointer, i + 1, 0);
                    movetimes++;
                }
			}

			// 当单位数小于或等于容量一半加一,且后一条战线单位数大于或等于容量一半时，将后一条战线血量低的往前推
			while (battleLineControllers[i].count <= halfCapacity + 1 && battleLineControllers[i + 1].count > battleLineControllers[i + 1].capacity / 2 && i < fieldCapacity - 1)
			{
				Tuple<int, int> minHealthInfo = GetAvailableMinHealth(i + 1);
				int minHealthPointer = minHealthInfo.Item2;

				if(minHealthPointer > -1)
				{
                    AIMove(i + 1, minHealthPointer, i, 0);
                    movetimes++;
                }
			}
		}

		// 指令卡策略：费用够就出
		while (energy[Turn] > 3 && handicap.count > 0)
		{
			AICast(0, 0, 0);
		}

		Skip();
    }*/

    /// <summary>
    /// 获取某条战线的建筑数量
    /// </summary>
    /// <param name="idx">战线索引</param>
    /// <returns></returns>
    private int GetConstructionNum(int idx)
    {
        int num = 0;
        BattleLineController battleLine = battleLines[idx];
        for (int i = 0; i < battleLine.count; i++)
        {
            if (battleLine[i].category == "Construction")
            {
                num++;
            }
        }
        return num;
    }


    private bool GetIsLineAvailable(int idx)
    {
        return battleLines[idx].count < battleLines[idx].capacity;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>返回可操作战线的索引-1</returns>
    private int GetFrontLineIdx()
    {
        int idx = fieldCapacity - 1;
        while (battleLines[idx].ownership == 1 || battleLines[idx].count == 0)
        {
            idx--;
        }
        return idx;
    }

    /// <summary>
    /// 获取某条战线上可操作的单位中生命值最小的单位的生命值和索引
    /// </summary>
    /// <returns>返回一个元组，包含生命值最小单位的生命值和索引</returns>
    private Tuple<int, int> GetAvailableMinHealth(int battleLineIdx)
    {
        BattleLineController battleLine = battleLines[battleLineIdx];
        int minHealth = 100;
        int minHealthPointer = -1;
        for (int i = 0; i < battleLine.count; i++)
        {
            if (battleLine[i].healthPoint < minHealth && battleLine[i].operateCounter == 1)
            {
                minHealth = battleLine[i].healthPoint;
                minHealthPointer = i;
            }
        }
        return Tuple.Create(minHealth, minHealthPointer);
    }

    private Tuple<int, int> GetAvailableMaxHealth(int battleLineIdx)
    {
        BattleLineController battleLine = battleLines[battleLineIdx];
        int maxHealth = 0;
        int maxHealthPointer = -1;
        for (int i = 0; i < battleLine.count; i++)
        {
            if (battleLine[i].healthPoint > maxHealth && battleLine[i].operateCounter == 1)
            {
                maxHealth = battleLine[i].healthPoint;
                maxHealthPointer = i;
            }
        }
        return Tuple.Create(maxHealth, maxHealthPointer);
    }

    private Tuple<int, int> GetMaxHealth(int battleLineIdx)
    {
        BattleLineController battleLine = battleLines[battleLineIdx];
        int maxHealth = 0;
        int maxHealthPointer = -1;
        for (int i = 0; i < battleLine.count; i++)
        {
            if (battleLine[i].healthPoint > maxHealth)
            {
                maxHealth = battleLine[i].healthPoint;
                maxHealthPointer = i;
            }
        }
        return Tuple.Create(maxHealth, maxHealthPointer);
    }

    private int GetMaxCost()
    {
        int maxCost = 0;
        int maxPointer = -1;
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].cost > maxCost)
            {
                maxCost = AIHandicap[i].cost;
                maxPointer = i;
            }
        }
        return maxCost;
    }
    private int GetMinCost()
    {
        int minCost = 100;
        int minPointer = -1;
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].cost < minCost)
            {
                minCost = AIHandicap[i].cost;
                minPointer = i;
            }
        }
        return minCost;
    }
    private int GetMaxCostUnitPointer()
    {
        int maxCost = 0;
        int maxPointer = -1;
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].cost >= maxCost && AIHandicap[i] is UnitElementController)
            {
                maxCost = AIHandicap[i].cost;
                maxPointer = i;
            }
        }
        if (AIHandicap[maxPointer].cost > energy[1])
        {
            return -1;
        }
        return maxPointer;
    }
    /// <summary>
    /// 只有在费用不足时才会返回-1
    /// </summary>
    /// <param name="lowerBound">下界，返回的索引对应卡费用不会低于这个值</param>
    /// <returns></returns>
    private int GetMinCostUnitPointer(int lowerBound = 0)
    {
        int minCost = 10000;
        int minPointer = -1;
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (lowerBound < AIHandicap[i].cost && AIHandicap[i].cost < minCost && AIHandicap[i] is UnitElementController)
            {
                minCost = AIHandicap[i].cost;
                minPointer = i;
            }
        }
        if (minPointer == -1)
        {
            return -1;
        }
        if (AIHandicap[minPointer].cost > energy[1])
        {
            return -1;
        }
        return minPointer;
    }
    private int GetMinCostUnitPointerExcConstr()
    {
        int minCost = 10000;
        int minPointer = -1;
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].category != "Construction" && AIHandicap[i].cost < minCost && AIHandicap[i] is UnitElementController)
            {
                minCost = AIHandicap[i].cost;
                minPointer = i;
            }
        }
        if (minPointer == -1)
        {
            return -1;
        }
        if (AIHandicap[minPointer].cost > energy[1])
        {
            return -1;
        }
        return minPointer;
    }
    /// <summary>
    /// 若没有合适的操作目标返回-1
    /// </summary>
    /// <returns></returns>
    private int GetOperatorPointerInSupportLine()
    {
        for (int i = 0; i < AISupportLine.count; i++)
        {
            if (AISupportLine[i].operateCounter == 1 && (AISupportLine[i].category != "Construction" && AISupportLine[i].category != "Artillery"))
            {
                return i;
            }
        }
        return -1;
    }
}