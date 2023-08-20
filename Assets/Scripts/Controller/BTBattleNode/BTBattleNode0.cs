using BehaviorTree;
using System.Collections.Generic;

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