using DG.Tweening;
using LogicCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BehaviorTree
{
/*    public class AIBehavior : MonoBehaviour
    {
        public BTBattleNode0 BT;
        public int turn1;
        public int turn2;

        private void Awake()
        {
            BT = gameObject.AddComponent<BTBattleNode0>();
            turn1 = BattleSceneManager.Turn;
        }

        private void Update()
        {
            turn2 = BattleSceneManager.Turn;
            if(turn1 != turn2 && turn2 == 1)
            {
                BT.rootNode.Execute();
                turn1 = turn2;
            }
        }
    }*/

    public abstract class BTNode
    {
        public abstract bool Execute();
    }

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

    // 协程节点，用于每次执行行为树后的延时
    public class CoroutineNode : BTNode
    {
        BTNode child;

        private MonoBehaviour monoBehaviour;
        private bool isRunning;


        public CoroutineNode(MonoBehaviour monoBehaviour, BTNode child)
        {
            this.monoBehaviour = monoBehaviour;
            this.child = child;
            isRunning = true;
        }

        public override bool Execute()
        {
            while (isRunning)
            {
                monoBehaviour.StartCoroutine(Coroutine());

            }
            return isRunning;
        }

        private IEnumerator Coroutine()
        {
            float waitTime = 1f;
            yield return new WaitForSeconds(waitTime);
            isRunning = child.Execute();
        }
    }

    public abstract class BTBattleNode : MonoBehaviour
    {
        public BattleSceneManager sceneManager;
        // public BattleSystem battleSystem;

/*        public HandicapController AIHandicap;
        public BattleLineController[] battleLineControllers;
        public int energy;
        public int fieldCapacity;*/

        // 手牌
        public HandicapController AIHandicap
        {
            get => sceneManager.handicapController[1];
        }

        // 战线
        public BattleLineController[] battleLineControllers
        {
            get => sceneManager.battleLines;
        }

        // 能量
        public int energy
        {
            get => sceneManager.energy[1];
        }

        // 战场容量
        public int fieldCapacity
        {
            get => sceneManager.fieldCapacity;
        }

        // 战线索引 & 战线
        public int AISupportLineIdx;
        public int frontLineIdx;
        public int AIAdjacentLineIdx;
        public BattleLineController AISupportLine;
        public BattleLineController AIAdjacentLine;

        // 动画序列
        public Sequence rotateSequence;
        public float sequenceTime = 0;

        // 行为树根节点
        public BTNode rootNode;



        // AI的可执行操作
        public void BTDeploy(int handicapIdx)
        {
            sceneManager.AIDeploy(handicapIdx);
        }

        public void BTCast(int handicapIdx, int dstLineIdx, int dstPos)
        {
            sceneManager.AICast(handicapIdx, dstLineIdx, dstPos);
        }

        public void BTSkip()
        {
            sceneManager.AISkip();
        }

        public void BTMove(int resLineIdx, int resIdx, int dstLineIdx, int dstPos)
        {
            sceneManager.AIMove(resLineIdx, resIdx, dstLineIdx, dstPos);
        }

        public void BTRetreat(int resLineIdx, int resPos)
        {
            sceneManager.AIRetreat(resLineIdx, resPos);
        }

        public int GetConstructionNum(int idx)
        {
            int num = 0;
            BattleLineController battleLine = battleLineControllers[idx];
            for (int i = 0; i < battleLine.count; i++)
            {
                if (battleLine[i].category == "Construction")
                {
                    num++;
                }
            }
            return num;
        }


        public bool GetIsLineAvailable(int idx)
        {
            return battleLineControllers[idx].count < battleLineControllers[idx].capacity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>返回可操作战线的索引-1</returns>
        public int GetFrontLineIdx()
        {
            int idx = fieldCapacity - 1;
            while (battleLineControllers[idx].ownership == 1 || battleLineControllers[idx].count == 0)
            {
                idx--;
            }
            return idx;
        }

        /// <summary>
        /// 获取某条战线上可操作的单位中生命值最小的单位的生命值和索引
        /// </summary>
        /// <returns>返回一个元组，包含生命值最小单位的生命值和索引</returns>
        public Tuple<int, int> GetAvailableMinHealth(int battleLineIdx)
        {
            BattleLineController battleLine = battleLineControllers[battleLineIdx];
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

        public Tuple<int, int> GetAvailableMaxHealth(int battleLineIdx)
        {
            BattleLineController battleLine = battleLineControllers[battleLineIdx];
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

        public Tuple<int, int> GetLineMaxHealth(int battleLineIdx)
        {
            BattleLineController battleLine = battleLineControllers[battleLineIdx];
            int lineMaxHealth = 0;
            int maxHealthPointer = -1;
            for (int i = 0; i < battleLine.count; i++)
            {
                if (battleLine[i].healthPoint > lineMaxHealth)
                {
                    lineMaxHealth = battleLine[i].healthPoint;
                    maxHealthPointer = i;
                }
            }
            return Tuple.Create(lineMaxHealth, maxHealthPointer);
        }

        /// <summary>
        /// 获取human侧生命值最高的单位信息
        /// </summary>
        /// <param name="frontLineIdx">human前线索引</param>
        /// <returns>返回human侧血量最高单位的血量、战线索引、战线位置</returns>
        public Tuple<int, int, int> GetFieldMaxHealth(int frontLineIdx)
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

        public int GetMaxCost()
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
        public int GetMinCost()
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
        public int GetMaxCostUnitPointer()
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
            if (AIHandicap[maxPointer].cost > energy)
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
        public int GetMinCostUnitPointer(int lowerBound = 0)
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
            if (AIHandicap[minPointer].cost > energy)
            {
                return -1;
            }
            return minPointer;
        }
        public int GetMinCostUnitPointerExcConstr()
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
            if (AIHandicap[minPointer].cost > energy)
            {
                return -1;
            }
            return minPointer;
        }
        /// <summary>
        /// 若没有合适的操作目标返回-1
        /// </summary>
        /// <returns></returns>
        public int GetOperatorPointerInSupportLine()
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

        public bool TryCast(string cardID)
        {
            for (int i = 0; i < AIHandicap.count; i++)
            {
                if (AIHandicap[i].ID == cardID && energy >= AIHandicap[i].cost)
                {
                    BTCast(i, 0, 0);
                    Debug.Log($"BT tried cast '{cardID}'");
                    return true;
                }
            }
            return false;
        }

        public bool TrySkip()
        {
            BTSkip();
            return false;
        }

        public abstract void Init();

        [SerializeField]public int turn;
        private static bool isReady = false;

        private IEnumerator ExecuteRootNode()
        {
            rootNode.Execute();
            yield return null;
        }

        private void Update()
        {
            turn = BattleSceneManager.Turn;
            if (turn == 1 && !isReady)
            {
                Init();
                StartCoroutine(ExecuteRootNode());
                isReady = true;
            }
            else if (turn == 0)
            {
                isReady = false;
            }
        }

    }

    


    /*    public class Coroutinenode : BTNode
    {
        private MonoBehaviour monoBehaviour;
        private IEnumerator coroutine;
        private bool isRunning;
        private float waitTime;


        public Coroutinenode(MonoBehaviour monoBehaviour, IEnumerator coroutine)
        {
            monoBehaviour = monoBehaviour;
            coroutine = coroutine;
            isRunning = false;
            waitTime = 1f;
        }

        public override bool Execute()
        {
            if (!isRunning)
            {
                isRunning = true;
                monoBehaviour.StartCoroutine(RunCoroutine());
            }
            return isRunning;
        }

        private IEnumerator RunCoroutine()
        {
            yield return monoBehaviour.StartCoroutine(coroutine);
            yield return new WaitForSeconds(waitTime);
            isRunning = false;
        }
    }*/
}