using DG.Tweening;
using LogicCore;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BehaviorTree
{
    /// <summary>
    /// 战斗节点的行为树
    /// </summary>
    public abstract class BTBattleNode
    {
        protected BattleSceneManager SceneManager
        {
            get => GameManager.GetInstance().battleSceneManager;
        }
        protected int TurnNum
        {
            get => SceneManager.turnNum;
        }
        protected HandicapController AIHandicap
        {
            get => SceneManager.handicapController[1];
        }
        protected int FieldCapacity
        {
            get => SceneManager.fieldCapacity;
        }
        protected BattleLineController[] BattleLines
        {
            get => SceneManager.battleLines;
        }
        protected int Energy
        {
            get => SceneManager.energy[1];
        }
        protected float SequenceTime
        {
            get => SceneManager.sequenceTime;
        }

        // 战线索引 & 战线
        protected int AISupportLineIdx;
        protected int frontLineIdx;
        protected int AIAdjacentLineIdx;
        protected BattleLineController AISupportLine;
        protected BattleLineController AIAdjacentLine;

        protected static float waitTime = 1f;

        // 教程
        public bool guideRunning = true;
        public void EndGuideRunning()
        {
            guideRunning = false;
        }


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
            yield return new WaitForSeconds(waitTime * 3);

            // 循环，直到行为树根节点返回false
            while (loopTimes != 0 && rootNode.Execute())
            {
                loopTimes--;
                Debug.Log($"loopTimes:{loopTimes} SequenceTime:{SequenceTime}");
                yield return new WaitForSeconds(SequenceTime + waitTime);
            }

            yield return new WaitForSeconds(SequenceTime + waitTime);
            TrySkip();
        }


        // 行为树可执行的基本操作
        protected void BTDeploy(int handicapIdx, int dstLineIdx = 3, int dstPos = 0)
        {
            if (AIHandicap[handicapIdx] is UnitElementController && AIHandicap[handicapIdx].cost <= Energy && BattleLines[dstLineIdx].count < BattleLines[dstLineIdx].capacity)
            {
                SceneManager.AIDeploy(handicapIdx, dstLineIdx, dstPos);
            }
        }
        // TODO
        protected void BTTargetCast(int handicapIdx, int dstLineIdx, int dstPos)
        {
            if (AIHandicap[handicapIdx] is CommandElementController && AIHandicap[handicapIdx].cost <= Energy && BattleLines[dstLineIdx][dstPos] != null)
            {
                SceneManager.AITargetCast(handicapIdx, dstLineIdx, dstPos);
            }
        }
        protected void BTNoneTargetCast(int handicapIdx)
        {
            if (AIHandicap[handicapIdx] is CommandElementController && AIHandicap[handicapIdx].cost <= Energy)
            {
                SceneManager.AINoneTargetCast(handicapIdx);
            }
        }
        protected void BTSkip()
        {
            SceneManager.AISkip();
        }
        protected void BTMove(int resLineIdx, int resPos, int dstLineIdx, int dstPos)
        {
            if (BattleLines[resLineIdx][resPos].ownership == 1 && BattleLines[resLineIdx][resPos].operateCounter == 1 && BattleLines[resLineIdx][resPos].category != "Construction" && BattleLines[dstLineIdx].count < BattleLines[dstLineIdx].capacity)
            {
                SceneManager.AIMove(resLineIdx, resPos, dstLineIdx, dstPos);
            }
        }
        protected void BTRetreat(int resLineIdx, int resPos)
        {
            if (resLineIdx == FieldCapacity - 1 && BattleLines[resLineIdx][resPos].operateCounter == 1)
            {
                SceneManager.AIRetreat(resLineIdx, resPos);
            }
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

		/// <summary>
		/// 获取某条战线上可操作的单位中生命值最大的单位的生命值和索引
		/// </summary>
		/// <returns>返回一个元组，包含生命值最大单位的生命值和索引</returns>
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

        /// <summary>
        /// 获取手牌中费用最高（低于上界）的指令卡
        /// </summary>
        /// <param name="Upperbound">费用上界，不取等</param>
        /// <returns>返回手牌索引</returns>
        protected int GetMaxCostCommandIndex(int Upperbound = 16)
        {
            int maxCost = 0;
            int maxCostCommandIndex = -1;
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].cost > maxCost && AIHandicap[i].cost < Upperbound)
                {
                    maxCost = AIHandicap[i].cost;
                    maxCostCommandIndex = i;
                }
            }
            return maxCost;
        }

        /// <summary>
        /// 获取手牌中费用最高（低于上界）的单位卡
        /// </summary>
        /// <param name="Upperbound">费用上界，不取等</param>
        /// <returns>返回手牌索引</returns>
        protected int GetMaxCostUnitIndex(int Upperbound = 16)
        {
            int maxCost = 0;
            int maxCostUnitIndex = -1;
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].cost > maxCost && AIHandicap[i].cost < Upperbound)
                {
                    maxCost = AIHandicap[i].cost;
                    maxCostUnitIndex = i;
                }
            }
            return maxCost;
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
        protected int GetMinCostUnitPointer(int lowerBound = -1)
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

        protected bool GetExistUnavailable(int battleLineIdx)
        {
            BattleLineController battleLine = BattleLines[battleLineIdx];
            for (int i = 0; i < battleLine.count; i++)
            {
                if (battleLine[i].operateCounter == 0)
                {
                    return true;
                }
            }
            return false;
        }

        protected int GetHandicapIndexByCardID(string ID)
        {
            for(int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].ID == ID) { return i; }
            }
            return -1;
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

        protected bool TryCastComm15(int frontLineIdx)
        {
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].ID == "comm_mush_15" && Energy >= AIHandicap[i].cost)
                {
                    int dstLineIdx;
                    int dstPos;
					(_, dstLineIdx, dstPos) = GetFieldMaxHealth(frontLineIdx);

                    BTTargetCast(i, dstLineIdx, dstPos);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 整体向前调整战线
        /// </summary>
        /// <param name="frontLineIdx"></param>
        /// <returns>若调整过任意卡，则返回true</returns>
        protected bool TryAdjustForward(int frontLineIdx)
        {
            for (int j = FieldCapacity - 1; j > frontLineIdx + 1; j--)
            {
                if (TryMoveForward(j))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 将战线上推进优先级最高的卡向前推，同优先级攻击力高者优先
        /// </summary>
        /// <param name="battleLineIdx"></param>
        /// <returns></returns>
        protected bool TryMoveForward(int battleLineIdx)
        {
            // 前一条战线已满，直接返回false
			if (!GetIsLineAvailable(battleLineIdx - 1))
			{
				return false;
			}

            BattleLineController battleLine = BattleLines[battleLineIdx];
			int highestPriority = -1;
			int priority;
            int highestPriorityPos = 0;

			// 遍历该战线，给每张卡赋优先级
			for (int i = 0; i < battleLine.count; i++)
			{
				// 规则上不能移动的卡（轰击boss || 建筑 || 移动次数为0）优先级为-1
				if (battleLine[i].ID == "mush_102" || battleLine[i].category == "Construction" || battleLine[i].operateCounter == 0)
				{
                    continue;
				}
                // 对轰击卡，若战线已满且所有卡均可操作
                else if (battleLine[i].category == "Artillery")
                {
                    priority = !GetIsLineAvailable(battleLineIdx) && !GetExistUnavailable(battleLineIdx) ? 1 : 0;
                }
                // 攻击计数器为0的优先级为4
				else if (battleLine[i].attackCounter == 0)
				{
					priority = 4;
				}
                // 机动卡优先级为3
				else if (battleLine[i].category == "Motorized")
                {
                    priority = 3;
                }
                // 其余卡优先级为2
                else
                {
                    priority = 2;
                }

                // 更新最大优先级以及对应卡
                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    highestPriorityPos = i;
                }
                else if (priority == highestPriority)
                {
                    highestPriorityPos = battleLine[i].attackPoint > battleLine[highestPriorityPos].attackPoint ? i : highestPriorityPos;
				}
			}

            // 有优先级大于0的才移动
            if (highestPriority > 0)
            {
                BTMove(battleLineIdx, highestPriorityPos, battleLineIdx - 1, 0);
                return true;
            }
            else
            {
				return false;
			}
		}

        /// <summary>
        /// 撤退某些单位
        /// </summary>
        /// <param name="AISupportLineIdx"></param>
        /// <returns></returns>
        protected bool TryRetreatUnits(int AISupportLineIdx)
        {
            BattleLineController battleLine = BattleLines[AISupportLineIdx];
            int highestPriority = -1;
            int priority;
            int highestPriorityPos = 0;

            // 遍历该战线，给每张卡赋优先级
            for (int i = 0; i < battleLine.count; i++)
            {
                if (battleLine[i].ID == "mush_04" || battleLine[i].ID == "mush_10" || battleLine[i].ID == "mush_11" || battleLine[i].ID == "mush_13")
                {
                    priority = 2;
                }
                else if (battleLine[i].ID == "mush_04" && frontLineIdx == AISupportLineIdx - 1)
                {
                    priority = 5;
                }
                else
                {
                    priority = -1;
                }

                // 更新最大优先级以及对应卡
                if (priority > highestPriority)
                {
                    highestPriority = priority;
                    highestPriorityPos = i;
                }
            }

            if (highestPriority > 0)
            {
                BTRetreat(AISupportLineIdx, highestPriorityPos);
                return true;
            }
            else
            {
                return false;
            }
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

        /// <summary>
        /// 按卡名来释放手牌中的指令卡
        /// </summary>
        /// <param name="cardID"></param>
        /// <returns></returns>
        protected bool TryCast(string cardID)
        {
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].ID == cardID && Energy >= AIHandicap[i].cost)
                {
                    BTNoneTargetCast(i);
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