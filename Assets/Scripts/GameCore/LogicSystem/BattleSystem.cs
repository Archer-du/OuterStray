using DataCore.BattleItems;
using DataCore.BattleElements;
using DataCore.TacticalItems;
using EventEffectModels;
using System.Collections;
using System.Collections.Generic;
using InputHandler;
using DisplayInterface;
using SystemEventHandler;
using System;
using DataCore.CultivateItems;
using DataCore.Cards;

namespace LogicCore
{

	/// <summary>
	/// note: 应尽量减少战线，手牌，牌堆对element的操作，以防element与过多其他类耦合和破坏封装性。尽量以平铺方式通过战斗系统直接操作element
	/// 逻辑判断主要交由系统和个体元素负责，所有事件仅可能由个体元素和系统发布，其触发参数也仅包含事件源元素和系统
	/// </summary>
	public class BattleSystem : IBattleSystemInput
	{
		//display-----------------------------------
		/// <summary>
		/// 
		/// </summary>
		internal IGameManagement gameManagement;

		/// <summary>
		/// 战斗场景渲染接口
		/// </summary>
		public IBattleSceneController controller;
		//display-----------------------------------


		//global
		/// <summary>
		/// 回合指示 0：我方回合 1：敌方回合
		/// </summary>
		public static int TURN;


		/// <summary>
		/// 战线
		/// </summary>
		internal List<BattleLine> battleLines;
		/// <summary>
		/// 战场总战线容量
		/// </summary>
		internal int linesCapacity;



		/// <summary>
		/// (TURN mapped)敌我事件系统
		/// </summary>
		internal EventTable[] eventTable;
		//dynamic(TURN)
		/// <summary>
		/// (TURN mapped)基地指针
		/// </summary>
		internal UnitElement[] bases;
		/// <summary>
		/// (TURN mapped)牌堆
		/// </summary>
		internal RandomCardStack[] stacks;
		/// <summary>
		/// (TURN mapped)手牌
		/// </summary>
		internal RedemptionZone[] handicaps;
		/// <summary>
		/// (TURN mapped)支援战线指针<br/>
		/// 战局内只读
		/// </summary>
		public int[] supportLines;
		/// <summary>
		/// (TURN mapped)前线指针
		/// </summary>
		public int[] frontLines;
		/// <summary>
		/// (TURN mapped)能量
		/// </summary>
		public int[] energy;
		/// <summary>
		/// (TURN mapped)能量补给
		/// </summary>
		public int[] energySupply;


		/// <summary>
		/// 部署队列
		/// </summary>
		internal List<UnitElement> deployQueue;

		internal Dictionary<string, List<UnitElement>> UnitIDDic;



		//data access (test)
		internal Pool pool;
		internal Deck humanDeck;
		internal Deck plantDeck;




		public BattleSystem(IBattleSceneController bsdspl)
		{
			//系统层渲染接口
			controller = bsdspl;
			//返回输入句柄
			controller.FieldInitialize(this);
			//TODO 之后由战术层参数化
			eventTable = new EventTable[2] { new EventTable(), new EventTable() };

			bases = new UnitElement[2];


			//test segment TODO------
			pool = new Pool();
			pool.LoadCardPool();
			humanDeck = new Deck(this);
			plantDeck = new Deck(this);
			humanDeck.LoadDeckFromPool(pool, "ally");
			plantDeck.LoadDeckFromPool(pool, "enemy");
			//-----------------------


			//初始回合为玩家
			TURN = 0;
			controller.UpdateTurn(TURN);

			//数据层初始化
			linesCapacity = 4;
			battleLines = new List<BattleLine>(linesCapacity);

			supportLines = new int[2] { 0, linesCapacity - 1 };
			stacks = new RandomCardStack[2];
			handicaps = new RedemptionZone[2];

			frontLines = new int[2] { 0, 1 };

			energy = new int[2] { 0, 0 };
			energySupply = new int[2] { 1, 1 };

			deployQueue = new List<UnitElement>();
			UnitIDDic = new Dictionary<string, List<UnitElement>>();



			//渲染控件初始化
			controller.UpdateEnergy(0, energy[0]);
			controller.UpdateEnergy(1, energy[1]);

			controller.UpdateEnergySupply(0, energySupply[0]);
			controller.UpdateEnergySupply(1, energySupply[1]);

			//TODO remove
			BuildBattleField(null, null, 4);
		}





		//TODO
		/// <summary>
		/// 由战术层指定参数构建战场
		/// </summary>
		/// <param name="deck"></param>
		/// <param name="enemyDeck"></param>
		internal void BuildBattleField(Deck deck, Deck enemyDeck, int lineCapacity)
		{
			BuildBattleLine(lineCapacity);

			BuildHumanStack(deck);
			BuildPlantStack(enemyDeck);

			InitializeHandicaps();

			TutorialInit();

			energy[TURN] += energySupply[TURN];
			controller.UpdateEnergy(energy[TURN]);
		}
		internal void UnloadBattleField()
		{

		}

		/// <summary>
		/// 构建战线
		/// </summary>
		private void BuildBattleLine(int capacity)
		{
			linesCapacity = capacity;

			for (int i = 0; i < linesCapacity; i++)
			{
				Random random = new Random();
				if (i == supportLines[TURN])
				{
					battleLines.Add(new BattleLine(5));//TODO config
					battleLines[i].ownerShip = 0;

					battleLines[i].controller = controller.InstantiateBattleLine(i, 5);
				}
				else if (i == supportLines[(TURN + 1)%2])
				{
					battleLines.Add(new BattleLine(5));
					battleLines[i].controller = controller.InstantiateBattleLine(i, 5);
				}
				else
				{
					//TODO temp
					switch (i)
					{
						case 1: 
							battleLines.Add(new BattleLine(4));
							battleLines[i].controller = controller.InstantiateBattleLine(i, 4);
							break;
						case 2:
							battleLines.Add(new BattleLine(3));
							battleLines[i].controller = controller.InstantiateBattleLine(i, 3);
							break;
					}
					//int len = random.Next(2, 7);
					//battleLines.Add(new BattleLine(len));
					//battleLines[i].controller = controller.InstantiateBattleLine(i,	len);
				}
				battleLines[i].index = i;

				battleLines[i].Init();
			}
			//初始只有自己的支援战线归属权为自己
		}
		private void UnloadBattleLine()
		{
			battleLines.Clear();
		}



		private void BuildHumanStack(Deck deck)
		{
			stacks[0] = new RandomCardStack();
			stacks[0].controller = controller.InstantiateCardStack(0);

			stacks[0].Fill(humanDeck);
		}
		private void UnloadHumanStack()
		{
			stacks[0].Clear();
		}


		//REVIEW
		//TODO 导入方式与human不同
		private void BuildPlantStack(Deck deck)
		{
			stacks[1] = new RandomCardStack();
			stacks[1].controller = controller.InstantiateCardStack(1);

			stacks[1].Fill(plantDeck);
		}
		private void UnloadPlantStack()
		{
			stacks[1].Clear();
		}



		private void InitializeHandicaps()
		{
			handicaps[0] = new RedemptionZone();
			handicaps[1] = new RedemptionZone();
			handicaps[0].controller = controller.InstantiateHandicap(0);
			handicaps[0].controller.Init();
			handicaps[1].controller = controller.InstantiateHandicap(1);
			handicaps[1].controller.Init();

			List<BattleElement>[] list;
			list = new List<BattleElement>[2];
			list[0] = new List<BattleElement>();
			list[1] = new List<BattleElement>();
			//初始化手牌
			for (int i = 0; i < 3; i++)
			{
				list[1].Add(stacks[1].Pop());
			}
			for(int i = 0; i < 1; i++)
			{
				list[0].Add(stacks[0].Pop());
			}
			handicaps[0].Fill(list[0]);
			handicaps[1].Fill(list[1]);
		}
		private void UnloadHandicaps()
		{
			handicaps[0].Clear();
			handicaps[1].Clear();
		}






		internal void FieldPreset()
		{

		}
		//tutorial temp function
		private void TutorialInit()
		{
			//TODO
			UnitCard card = new UnitCard("human_10000", 0, "基地车", "Construction", 0, 0, 30, 100000, "基地", -1, -1, "none");
			bases[0] = new ConstructionElement(card, this);
			bases[0].dynHealth = 3;
			bases[0].controller = controller.InstantiateUnitInBattleField(0, 0, 0);
			bases[0].UnitInit();
			bases[0].Deploy(this, battleLines[0], 0);
			bases[0].operateCounter = 1;
			bases[0].UpdateInfo();

			card = pool.GetCardByID("human_03") as UnitCard;
			LightArmorElement human03 = new LightArmorElement(card, this);
			human03.dynAttackCounter = 1;
            human03.controller = controller.InstantiateUnitInBattleField(0, 0, 0);
			human03.UnitInit();
			human03.Deploy(this, battleLines[0], 0);
			human03.operateCounter = 1;
			human03.UpdateInfo();

			card = pool.GetCardByID("mush_00") as UnitCard;
			LightArmorElement mush1 = new LightArmorElement(card, this);
			mush1.controller = controller.InstantiateUnitInBattleField(1, 1, 0);
			LightArmorElement mush2 = new LightArmorElement(card, this);
			mush2.controller = controller.InstantiateUnitInBattleField(1, 1, 0);
            mush1.UnitInit();
			mush2.UnitInit();
			mush1.Deploy(this, battleLines[1], 0);
			mush2.Deploy(this, battleLines[1], 0);
			mush1.operateCounter = 1;
			mush2.operateCounter = 1;
			mush1.UpdateInfo();
			mush2.UpdateInfo();

			card = pool.GetCardByID("mush_99_01") as UnitCard;
			ConstructionElement strangeMush = new ConstructionElement(card, this);
            strangeMush.controller = controller.InstantiateUnitInBattleField(1, 3, 0);
			strangeMush.UnitInit();
			strangeMush.Deploy(this, this.battleLines[3], 0);
			strangeMush.operateCounter = 1;
			strangeMush.UpdateInfo();

			UpdateFrontLine();
			UpdateAttackRange();
		}






		public void Deploy(int handicapIdx, int dstLineIdx, int dstPos)
		{
			//输入通常从显示层限制
			//如果不是单位卡
			if (handicaps[TURN][handicapIdx] is not UnitElement)
			{
				throw new InvalidOperationException("not unit");
			}
			//如果没有部署在支援战线
			if(dstLineIdx != supportLines[TURN])
			{
				throw new InvalidOperationException("not supportLine");
			}

			int cost = handicaps[TURN][handicapIdx].cost;
			//没有足够气矿
			if (cost > energy[TURN])
			{
				throw new InvalidOperationException("not enough energy!");
			}



			//无参事件
			eventTable[TURN].RaiseEvent("DeployUnit", null, this);


			//手牌区弹出卡牌
			UnitElement element = handicaps[TURN].Pop(handicapIdx) as UnitElement;
			//消耗费用
			energy[TURN] -= cost;
			//显示层更新
			controller.UpdateEnergy(energy[TURN]);

			element.Deploy(this, battleLines[dstLineIdx], dstPos);



			//更新前线指针
			UpdateFrontLine();
			UpdateAttackRange();
			//更新目标
			Settlement();




			if (dstLineIdx == frontLines[TURN])
			{
				eventTable[TURN].RaiseEvent("EnterFrontLine", element, this);
			}
			eventTable[TURN].RaiseEvent("UnitDeployed", element, this);

			eventTable[TURN].RaiseEvent("UpdateAura", null, this);
			eventTable[(TURN + 1) % 2].RaiseEvent("UpdateAura", null, this);


			controller.Settlement();
		}





		public void Cast(int handicapIdx, int dstLineIdx, int dstIdx)
		{
			if (handicaps[TURN][handicapIdx] is CommandElement)
			{
				//throw event TODO
				eventTable[TURN].RaiseEvent("", null, this);
			}

			int cost = handicaps[TURN][handicapIdx].cost;
			//手牌区弹出卡牌
			CommandElement element = handicaps[TURN].Pop(handicapIdx) as CommandElement;
			//消耗费用
			energy[TURN] -= cost;
			//显示层更新
			controller.UpdateEnergy(energy[TURN]);

			if(element.type != "Target")
			{
				element.Cast(null);
			}
			else
			{
				element.Cast(battleLines[dstLineIdx][dstIdx]);
			}
			if(element.dynDurability > 0)
			{
				stacks[TURN].Push(element);
			}

			//更新前线指针
			UpdateFrontLine();
			UpdateAttackRange();
			//更新目标
			Settlement();


			eventTable[TURN].RaiseEvent("UpdateAura", null, this);
			eventTable[(TURN + 1) % 2].RaiseEvent("UpdateAura", null, this);

			controller.Settlement();
		}





		public void Move(int resLineIdx, int resIdx, int dstLineIdx, int dstPos)
		{
			UnitElement element = battleLines[resLineIdx][resIdx];

			//移动范围超出限制
			if (Math.Abs(dstLineIdx - resLineIdx) > element.moveRange)
			{
				throw new InvalidOperationException();
			}
			//单位剩余可操作次数为0
			if (element.operateCounter <= 0)
			{
				throw new InvalidOperationException();
			}
			//战线容量不足
			if (battleLines[dstLineIdx].count >= battleLines[dstLineIdx].capacity)
			{
				throw new InvalidOperationException("battleLine overflow");
			}
			//目标位置不合法
			if (dstPos > battleLines[dstLineIdx].count)
			{
				throw new InvalidOperationException("battleLine out of range");
			}
			//目标战线归属权不一致并且被占有
			if (battleLines[dstLineIdx].ownerShip != element.ownership && battleLines[dstLineIdx].count > 0)
			{
				throw new InvalidOperationException("battleLine receive denied");
			}



			eventTable[TURN].RaiseEvent("MoveUnit", null, this);

			element.Move(battleLines[resLineIdx], battleLines[dstLineIdx], resIdx, dstPos);

			//先更新前线再更新目标
			UpdateFrontLine();
			UpdateAttackRange();
			Settlement();





			if (dstLineIdx == frontLines[TURN])
			{
				eventTable[TURN].RaiseEvent("EnterFrontLine", null, this);
			}
			eventTable[TURN].RaiseEvent("UnitMoved", null, this);

			eventTable[TURN].RaiseEvent("UpdateAura", null, this);
			eventTable[(TURN + 1) % 2].RaiseEvent("UpdateAura", null, this);

			controller.Settlement();
		}



		//legacy
		public void VerticalMove(int resLineIdx, int resIdx, int dstPos)
		{
			if (dstPos >= battleLines[resLineIdx].count)
			{
				throw new InvalidOperationException("battleLine out of range");
			}

			eventTable[TURN].RaiseEvent("MoveUnit", null, this);


			UnitElement element = battleLines[resLineIdx][resIdx];
			battleLines[resLineIdx].Receive(battleLines[resLineIdx].Send(resIdx), dstPos);

			//先更新前线再更新目标
			UpdateFrontLine();
			UpdateAttackRange();

			//element.Move();


			eventTable[TURN].RaiseEvent("UnitMoved", null, this);

			eventTable[TURN].RaiseEvent("UpdateAura", null, this);
			eventTable[(TURN + 1) % 2].RaiseEvent("UpdateAura", null, this);
		}




		public void Retreat(int resLineIdx, int resIdx)
		{
			UnitElement element = battleLines[resLineIdx][resIdx];

			//单位剩余可操作次数为0
			if (element.operateCounter <= 0)
			{
				//显示层限制输入
				throw new InvalidOperationException();
			}
			if(element.battleLine.index != supportLines[TURN])
			{
				throw new InvalidOperationException();
			}

			stacks[TURN].Push(element);
			element.Retreat("append");

			UpdateFrontLine();
			UpdateAttackRange();

			eventTable[TURN].RaiseEvent("UpdateAura", null, this);
			eventTable[(TURN + 1) % 2].RaiseEvent("UpdateAura", null, this);
		}
		//public void PullOut(int resLineIdx, int resIdx)
		//{
		//	UnitElement element = battleLines[resLineIdx][resIdx];
		//}



		public void Skip()
		{
			eventTable[TURN].RaiseEvent("EndOfTurn", null, this);
			BroadCastEvent("EndOfTurn");

			energySupply[TURN] = energySupply[TURN] + 1 > 5 ? 5 : energySupply[TURN] + 1;
			controller.UpdateEnergySupply(energySupply[TURN]);

			//发牌
			if (handicaps[TURN].count < handicaps[TURN].capacity)
			{
				BattleElement element = stacks[TURN].Pop();
				handicaps[TURN].Push(element);
			}

			RotateSettlement();

			TURN = (TURN + 1) % 2;

			energy[TURN] = energy[TURN] + energySupply[TURN] > 15 ? 15 : energy[TURN] + energySupply[TURN];
			controller.UpdateEnergy(TURN, energy[TURN]);

			eventTable[TURN].RaiseEvent("StartOfTurn", null, this);
			BroadCastEvent("StartOfTurn");

			//动画结算后回调
			controller.UpdateTurnWithSettlement();
		}

		public void Exit()
		{
			throw new System.NotImplementedException("exit");
		}



		internal void BroadCastEvent(string eventName)
		{
			for (int i = 0; i < deployQueue.Count; i++)
			{
				if (deployQueue[i].ownership == TURN && deployQueue[i].state == ElementState.inBattleLine)
				{
					deployQueue[i].eventTable.RaiseEvent(eventName, deployQueue[i], this);
				}
			}
		}




		private bool ExistFrontLine()
		{
			for (int i = 0; i < linesCapacity - 1; i++)
			{
				if (battleLines[i].ownerShip != battleLines[i + 1].ownerShip)
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool FrontLineAlign()
		{
			if (!ExistFrontLine())
			{
				return false;
			}
			return (battleLines[frontLines[0]].count + battleLines[frontLines[1]].count) % 2 == 0;
		}
		/// <summary>
		/// 更新前线信息
		/// </summary>
		private void UpdateFrontLine()
		{
			for (int i = 0; i < linesCapacity - 1; i++)
			{
				if (battleLines[i].ownerShip != battleLines[i + 1].ownerShip)
				{
					frontLines[0] = i;
					frontLines[1] = i + 1;
				}
			}
		}

		//CRITICAL ALGORITHM
		/// <summary>
		/// 更新全场所有单位的目标
		/// </summary>
		internal void UpdateAttackRange()
		{
			for (int i = 0; i < linesCapacity; i++)
			{
				for (int j = 0; j < battleLines[i].count; j++)
				{
					battleLines[i][j].SetAttackRange(null, null, null);
				}
			}
			if (ExistFrontLine())
			{
				if (FrontLineAlign())
				{
					//我方
					for (int j = 0; j < battleLines[frontLines[0]].count; j++)
					{
						int idx2 = j + (battleLines[frontLines[1]].count - battleLines[frontLines[0]].count) / 2;
						int idx1 = idx2 - 1;
						int idx3 = idx2 + 1;
						UnitElement element = battleLines[frontLines[0]][j];
						UnitElement target1 = idx1 < 0 || idx1 >= battleLines[frontLines[1]].count ? null : battleLines[frontLines[1]][idx1];
						UnitElement target2 = idx2 < 0 || idx2 >= battleLines[frontLines[1]].count ? null : battleLines[frontLines[1]][idx2];
						UnitElement target3 = idx3 < 0 || idx3 >= battleLines[frontLines[1]].count ? null : battleLines[frontLines[1]][idx3];
						element.SetAttackRange(target1, target2, target3);
					}
					for (int j = 0; j < battleLines[frontLines[1]].count; j++)
					{
						int idx2 = j + (battleLines[frontLines[0]].count - battleLines[frontLines[1]].count) / 2;
						int idx1 = idx2 - 1;
						int idx3 = idx2 + 1;
						UnitElement element = battleLines[frontLines[1]][j];
						UnitElement target1 = idx1 < 0 || idx1 >= battleLines[frontLines[0]].count ? null : battleLines[frontLines[0]][idx1];
						UnitElement target2 = idx2 < 0 || idx2 >= battleLines[frontLines[0]].count ? null : battleLines[frontLines[0]][idx2];
						UnitElement target3 = idx3 < 0 || idx3 >= battleLines[frontLines[0]].count ? null : battleLines[frontLines[0]][idx3];
						element.SetAttackRange(target1, target2, target3);
					}
				}
				else
				{
					for (int j = 0; j < battleLines[frontLines[0]].count; j++)
					{
						int idx1 = j + (int)Math.Floor((battleLines[frontLines[1]].count - battleLines[frontLines[0]].count) / 2f);
						int idx3 = idx1 + 1;
						UnitElement element = battleLines[frontLines[0]][j];
						UnitElement target1 = idx1 < 0 || idx1 >= battleLines[frontLines[1]].count ? null : battleLines[frontLines[1]][idx1];
						UnitElement target3 = idx3 < 0 || idx3 >= battleLines[frontLines[1]].count ? null : battleLines[frontLines[1]][idx3];
						element.SetAttackRange(target1, null, target3);
					}
					for (int j = 0; j < battleLines[frontLines[1]].count; j++)
					{
						int idx1 = j + (int)Math.Floor((battleLines[frontLines[0]].count - battleLines[frontLines[1]].count) / 2f);
						int idx3 = idx1 + 1;
						UnitElement element = battleLines[frontLines[1]][j];
						UnitElement target1 = idx1 < 0 || idx1 >= battleLines[frontLines[0]].count ? null : battleLines[frontLines[0]][idx1];
						UnitElement target3 = idx3 < 0 || idx3 >= battleLines[frontLines[0]].count ? null : battleLines[frontLines[0]][idx3];
						element.SetAttackRange(target1, null, target3);
					}
				}
			}
		}



		private void RotateSettlement()
		{
			for (int i = 0; i < deployQueue.Count; i++)
			{
				if (deployQueue[i].state == ElementState.inBattleLine)
				{
					deployQueue[i].RotateSettlement();
					UpdateAttackRange();
				}
			}
		}
		private void Settlement()
		{
			for (int i = 0; i < deployQueue.Count; i++)
			{
				if (deployQueue[i].state == ElementState.inBattleLine)
				{
					deployQueue[i].Settlement();
					UpdateAttackRange();
				}
			}
		}







		public int allyNum;
		public int enemyNum;
		//一些战场查询方法
		internal void UpdateUnitNum()
		{
			allyNum = 0;
			enemyNum = 0;
			for (int i = 0; i < deployQueue.Count; i++)
			{
				if (deployQueue[i].state == ElementState.inBattleLine)
				{
					allyNum += deployQueue[i].ownership == TURN ? 1 : 0;
					enemyNum += deployQueue[i].ownership == (TURN + 1) % 2 ? 1 : 0;
				}
			}
		}
		/// <summary>
		/// 查询敌方随机目标 TODO
		/// </summary>
		/// <returns></returns>
		internal UnitElement RandomEnemy(int ownership)
		{
			UpdateUnitNum();
			if (enemyNum == 0) return null;
			
			Random random = new Random();
			int counter = random.Next(1, enemyNum + 1);
			//从敌方战线开始
			int line = ownership == 0 ? linesCapacity - 1 : 0;
			while (counter > 0)
			{
				counter -= battleLines[line].count;
				if(counter <= 0)
				{
					return battleLines[line][battleLines[line].count - 1 + counter];
				}
				if(ownership == 0) { line--; }
				else { line++; }
			}
			return null;
		}
		internal UnitElement RandomEnemyAtFrontLine(int ownership)
		{
			Random random = new Random();
			if (battleLines[frontLines[(ownership + 1) % 2]].count == 0) return null;

			int counter = random.Next(0, battleLines[frontLines[(ownership + 1) % 2]].count);
			return battleLines[frontLines[(ownership + 1) % 2]][counter];
		}

		internal UnitElement DamagedAlly()
		{
			for (int i = 0; i < deployQueue.Count; i++)
			{
				if (deployQueue[i].state == ElementState.inBattleLine)
				{
					if (deployQueue[i].maxHealthReader > deployQueue[i].dynHealth)
					{
						return deployQueue[i];
					}
				}
			}
			return null;
		}
















	}
}
