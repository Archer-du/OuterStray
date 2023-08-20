using DG.Tweening;
using LogicCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BehaviorTree
{
    /// <summary>
    /// 战斗节点的行为树
    /// </summary>
    public abstract class BTBattleNode
    {
        protected BattleSceneManager sceneManager;

        protected HandicapController AIHandicap
        {
            get => sceneManager.handicapController[1];
        }
        protected int FieldCapacity
        {
            get => sceneManager.fieldCapacity;
        }
        protected BattleLineController[] BattleLines
        {
            get => sceneManager.battleLines;
        }
        protected int Energy
        {
            get => sceneManager.energy[1];
        }
        protected float SequenceTime
        {
            get => sceneManager.sequenceTime;
        }

        // 战线索引 & 战线
        protected int AISupportLineIdx;
        protected int frontLineIdx;
        protected int AIAdjacentLineIdx;
        protected BattleLineController AISupportLine;
        protected BattleLineController AIAdjacentLine;

        protected static float waitTime = 1f;

        /// <summary>
        /// 循环执行行为树的最大次数
        /// </summary>
        protected int loopTimes;

        /// <summary>
        /// 行为树根节点
        /// </summary>
        protected BTNode rootNode;

        protected virtual void Init()
        {
            sceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();

            AISupportLineIdx = FieldCapacity - 1;
            AISupportLine = BattleLines[AISupportLineIdx];

            frontLineIdx = GetFrontLineIdx();
            AIAdjacentLineIdx = frontLineIdx + 1;
            AIAdjacentLine = BattleLines[AIAdjacentLineIdx];

            loopTimes = 15;

            BuildBT();
        }

        /// <summary>
        /// 构造行为树
        /// </summary>
        protected abstract void BuildBT();

        /// <summary>
        /// 循环执行行为树的协程
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator BehaviorTree()
        {
            // 初始化
            Init();
            yield return new WaitForSeconds(waitTime);

            // 循环，直到行为树根节点返回false
            while (loopTimes != 0 && rootNode.Execute())
            {
                loopTimes--;
                Debug.Log($"loopTimes:{loopTimes}\nSequenceTime:{SequenceTime}");
                yield return new WaitForSeconds(SequenceTime + waitTime);
            }
        }


        // 行为树可执行的基本操作
        protected void BTDeploy(int handicapIdx)
        {
            sceneManager.AIDeploy(handicapIdx);
        }
        protected void BTCast(int handicapIdx, int dstLineIdx, int dstPos)
        {
            sceneManager.AICast(handicapIdx, dstLineIdx, dstPos);
        }
        protected void BTSkip()
        {
            sceneManager.AISkip();
        }
        protected void BTMove(int resLineIdx, int resIdx, int dstLineIdx, int dstPos)
        {
            sceneManager.AIMove(resLineIdx, resIdx, dstLineIdx, dstPos);
        }
        protected void BTRetreat(int resLineIdx, int resPos)
        {
            sceneManager.AIRetreat(resLineIdx, resPos);
        }


        protected int GetConstructionNum(int idx)
        {
            int num = 0;
            BattleLineController battleLine = BattleLines[idx];
            for (int i = 0; i < battleLine.count; i++)
            {
                if (battleLine[i].category == "Construction")
                {
                    num++;
                }
            }
            return num;
        }

        protected bool GetIsLineAvailable(int idx)
        {
            return BattleLines[idx].count < BattleLines[idx].capacity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>返回可操作战线的索引-1</returns>
        protected int GetFrontLineIdx()
        {
            int idx = FieldCapacity - 1;
            while (BattleLines[idx].ownership == 1 || BattleLines[idx].count == 0)
            {
                idx--;
            }
            return idx;
        }

        /// <summary>
        /// 获取某条战线上可操作的单位中生命值最小的单位的生命值和索引
        /// </summary>
        /// <returns>返回一个元组，包含生命值最小单位的生命值和索引</returns>
        protected Tuple<int, int> GetAvailableMinHealth(int battleLineIdx)
        {
            BattleLineController battleLine = BattleLines[battleLineIdx];
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
        protected Tuple<int, int> GetAvailableMaxHealth(int battleLineIdx)
        {
            BattleLineController battleLine = BattleLines[battleLineIdx];
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

        /// <summary>
        /// 获取战线上最高生命值的单位信息
        /// </summary>
        /// <param name="battleLineIdx"></param>
        /// <returns></returns>
        protected Tuple<int, int> GetLineMaxHealth(int battleLineIdx)
        {
            BattleLineController battleLine = BattleLines[battleLineIdx];
            int lineMaxHealth = 0;
            int maxHealthPos = -1;
            for (int i = 0; i < battleLine.count; i++)
            {
                if (battleLine[i].healthPoint > lineMaxHealth)
                {
                    lineMaxHealth = battleLine[i].healthPoint;
                    maxHealthPos = i;
                }
            }
            return Tuple.Create(lineMaxHealth, maxHealthPos);
        }

        /// <summary>
        /// 获取human侧生命值最高的单位信息
        /// </summary>
        /// <param name="frontLineIdx">human前线索引</param>
        /// <returns>返回human侧血量最高单位的血量、战线索引、战线位置</returns>
        protected Tuple<int, int, int> GetFieldMaxHealth(int frontLineIdx)
        {
            Tuple<int, int> lineMaxHealth;
            int fieldMaxHealth = 0;
            int lineIdx = 0;
            int pos = 0;
            for (int j = 0; j < frontLineIdx + 1; j++)
            {
                lineMaxHealth = GetLineMaxHealth(j);
                if (fieldMaxHealth < lineMaxHealth.Item1)
                {
                    fieldMaxHealth = lineMaxHealth.Item1;
                    lineIdx = j;
                    pos = lineMaxHealth.Item2;
                }
            }
            return Tuple.Create(fieldMaxHealth, lineIdx, pos);
        }

        protected int GetMaxCost()
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

        protected int GetMinCost()
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

        protected int GetMaxCostUnitPointer()
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
            if (AIHandicap[maxPointer].cost > Energy)
            {
                return -1;
            }
            return maxPointer;
        }

        /// <summary>
        /// 获取手牌中最低费用的单位卡索引
        /// </summary>
        /// <param name="lowerBound">费用下界，返回的索引对应卡费用不会低于这个值</param>
        /// <returns>无单位卡或没有足够的能源则返回-1</returns>
        protected int GetMinCostUnitPointer(int lowerBound = 0)
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
            if (AIHandicap[minPointer].cost > Energy)
            {
                return -1;
            }
            return minPointer;
        }

        /// <summary>
        /// 获取手牌中最低费用的指令卡索引
        /// </summary>
        /// <param name="lowerBound">费用下界，返回的索引对应卡费用不会低于这个值</param>
        /// <returns>无指令卡或没有足够能源则返回-1</returns>
        protected int GetMinCostCommPointer(int lowerBound = 0)
        {
            int minCost = 10000;
            int minPointer = -1;
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (lowerBound < AIHandicap[i].cost && AIHandicap[i].cost < minCost && AIHandicap[i] is CommandElementController)
                {
                    minCost = AIHandicap[i].cost;
                    minPointer = i;
                }
            }
            if (minPointer == -1)
            {
                return -1;
            }
            if (GetIsEnergyEnough(AIHandicap[minPointer].cost))
            {
                return -1;
            }
            return minPointer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cost"></param>
        /// <returns>若能源大于等于费用则返回true</returns>
        protected bool GetIsEnergyEnough(int cost)
        {
            return Energy >= cost;
        }

        protected int GetMinCostUnitPointerExcConstr()
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
            if (AIHandicap[minPointer].cost > Energy)
            {
                return -1;
            }
            return minPointer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>若没有合适的操作目标则返回-1</returns>
        protected int GetOperatorPointerInSupportLine()
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

        protected bool TryCastComm18(int frontLineIdx)
        {
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].ID == "comm_mush_18" && Energy >= 3)
                {

                    int dstLineIdx = 0;
                    int dstPos = 0;
                    int maxHealth = 0;
                    Tuple<int, int> lineMaxHealth = Tuple.Create(0, 0);
                    for (int j = 0; j < frontLineIdx + 1; j++)
                    {
                        lineMaxHealth = GetLineMaxHealth(j);
                        if (maxHealth > lineMaxHealth.Item1)
                        {
                            dstLineIdx = j;
                            dstPos = lineMaxHealth.Item2;
                        }
                    }
                    BTCast(i, dstLineIdx, dstPos);
                    return true;
                }
            }
            return false;
        }

        protected bool TryAdjustForward(int frontLineIdx)
        {
            for (int i = FieldCapacity - 1; i > frontLineIdx + 1; i--)
            {
                BattleLineController battleLine = BattleLines[i];

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
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        protected bool TryRetreatSomeUnits(int AISupportLineIdx)
        {
            BattleLineController battleLine = BattleLines[AISupportLineIdx];
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

        protected bool TryDeployLowCostUnit(int AISupportLineIdx)
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
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool TryCast(string cardID)
        {
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].ID == cardID && Energy >= AIHandicap[i].cost)
                {
                    BTCast(i, 0, 0);
                    return true;
                }
            }
            return false;
        }

        protected bool TrySkip()
        {
            BTSkip();
            return false;
        }
    }


    /// <summary>
    /// 行为树节点的抽象类
    /// </summary>
    public abstract class BTNode
    {
        public abstract bool Execute();
    }
    /// <summary>
    /// 选择器节点，依次运行子节点，当子节点返回true则立即返回true
    /// </summary>
    public class SelectorNode : BTNode
    {
        private List<BTNode> children;

        public SelectorNode(List<BTNode> children)
        {
            this.children = children;
        }

        public override bool Execute()
        {
            foreach (BTNode child in children)
            {
                if (child.Execute())
                {
                    return true;
                }
            }
            return false;
        }
    }
    /// <summary>
    /// 序列器节点，依次运行子节点，当子节点返回false则立即返回false
    /// </summary>
    public class SequenceNode : BTNode
    {
        private List<BTNode> children;

        public SequenceNode(List<BTNode> children)
        {
            this.children = children;
        }

        public override bool Execute()
        {
            foreach (BTNode child in children)
            {
                if (!child.Execute())
                {
                    return false;
                }
            }
            return true;
        }
    }
    /// <summary>
    /// 条件节点
    /// </summary>
    public class ConditionNode : BTNode
    {
        private Func<bool> condition;

        public ConditionNode(Func<bool> condition)
        {
            this.condition = condition;
        }

        public override bool Execute()
        {
            return condition();
        }
    }
    /// <summary>
    /// 行动节点
    /// </summary>
    public class ActionNode : BTNode
    {
        private Func<bool> action;
        public ActionNode(Func<bool> action)
        {
            this.action = action;
        }

        public override bool Execute()
        {
            return action();
        }
    }
}