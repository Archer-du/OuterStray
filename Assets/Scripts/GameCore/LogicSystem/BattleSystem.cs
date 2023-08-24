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
	public enum BattleResult
	{
		normal,
		win,
		fail
	}

	/// <summary>
	/// note: 应尽量减少战线，手牌，牌堆对element的操作，以防element与过多其他类耦合和破坏封装性。尽量以平铺方式通过战斗系统直接操作element
	/// 逻辑判断主要交由系统和个体元素负责，所有事件仅可能由个体元素和系统发布，其触发参数也仅包含事件源元素和系统
	/// </summary>
	public class BattleSystem : IBattleSystemInput
	{

		internal static event Action UpdateWeight;
		internal static event Action ReParse;
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

		internal TacticalSystem tacticalSystem;

		//global
		/// <summary>
		/// 回合指示 0：我方回合 1：敌方回合
		/// </summary>
		public static int TURN;

		public bool tutorial
		{
			get => tacticalSystem.tutorial;
		}

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



		internal Pool pool;

		public BattleSystem(Pool pool, IBattleSceneController bsdspl)
		{
			this.pool = pool;
			//系统层渲染接口
			controller = bsdspl;

			eventTable = new EventTable[2] { new EventTable(), new EventTable() };

			bases = new UnitElement[2];

		}
		public void SetSceneController(IBattleSceneController bsdspl)
		{
			controller = bsdspl;
		}
		public void SetTacticalSystem(ITacticalSystemInput tacticalSystem)
		{
			this.tacticalSystem = tacticalSystem as TacticalSystem;
		}



		internal BattleResult result;
		internal bool final;

		internal List<BattleElement>[] presetHandicaps;
		/// <summary>
		/// 由战术层指定参数构建战场
		/// </summary>
		/// <param name="playerDeck"></param>
		/// <param name="enemyDeck"></param>
		internal void BuildBattleField(int BTindex, Deck playerDeck, Deck enemyDeck, int fieldCapacity, int[] battleLinesCapacity,
			int initialTurn, int initialHumanEnergy, int initialPlantEnergy, int initialHumanHandicaps, int initialPlantHandicaps,
			List<BattleNode.FieldPreset> fieldPresets, bool final)
		{
			//TODO
			this.final = final;
			eventTable = new EventTable[2] { new EventTable(), new EventTable() };

			result = BattleResult.normal;

			deployQueue = new List<UnitElement>();
			UnitIDDic = new Dictionary<string, List<UnitElement>>();

			stacks = new RandomCardStack[2];
			handicaps = new RedemptionZone[2];

			presetHandicaps = new List<BattleElement>[2];

			presetHandicaps[0] = new List<BattleElement>();
			presetHandicaps[1] = new List<BattleElement>();

			linesCapacity = fieldCapacity;

			controller.FieldInitialize(this, linesCapacity, BTindex);

			//数据层初始化
			TURN = initialTurn;

			if (fieldCapacity != 4)
			{
				throw new InvalidOperationException("暂不支持除4之外的战场容量");
			}
			battleLines = new List<BattleLine>(linesCapacity);

			supportLines = new int[2] { 0, linesCapacity - 1 };

			energy = new int[2] { initialHumanEnergy, initialPlantEnergy };
			energySupply = new int[2] { 1, 1 };

			frontLines = new int[2] { 0, 1 };

			BuildBattleLine(fieldCapacity, battleLinesCapacity);

			BuildHumanStack(playerDeck);
			BuildPlantStack(enemyDeck);


			FieldPreset(playerDeck.bases ,fieldPresets);

			BuildHandicaps(initialHumanHandicaps, initialPlantHandicaps, initialTurn);

			//渲染控件初始化
			controller.UpdateEnergy(0, energy[0]);
			controller.UpdateEnergy(1, energy[1]);

			controller.UpdateEnergySupply(0, energySupply[0]);
			controller.UpdateEnergySupply(1, energySupply[1]);

			energy[TURN] += energySupply[TURN];
			controller.UpdateEnergy(energy[TURN]);
		}
		internal void UnloadBattleField()
		{
			linesCapacity = 0;

			//display
			UnloadBattleLine();
			UnloadStack();
			UnloadHandicaps();
		}
		private void UnloadBattleLine()
		{
			battleLines = null;
		}
		private void UnloadStack()
		{
			stacks = null;
		}
		private void UnloadHandicaps()
		{
			handicaps = null;
		}



		/// <summary>
		/// 构建战线
		/// </summary>
		private void BuildBattleLine(int capacity, int[] battleLinesCapacity)
		{
			linesCapacity = capacity;
			if(battleLinesCapacity.Length != linesCapacity)
			{
				throw new InvalidOperationException("linesCapacity inconsistance");
			}

			for (int i = 0; i < linesCapacity; i++)
			{
				battleLines.Add(new BattleLine(battleLinesCapacity[i], 
					controller.InstantiateBattleLine(i, battleLinesCapacity[i])));

				battleLines[i].index = i;

			}
			//初始只有自己的支援战线归属权为自己
		}

		private void BuildHumanStack(Deck deck)
		{
			stacks[0] = new RandomCardStack(0, controller.InstantiateCardStack(0), this);

			stacks[0].Fill(deck);
		}


		//REVIEW
		private void BuildPlantStack(Deck deck)
		{
			stacks[1] = new RandomCardStack(1, controller.InstantiateCardStack(1), this);

			stacks[1].Fill(deck);
		}


		private void BuildHandicaps(int initialHumanHandicaps, int initialPlantHandicaps, int initialTurn)
		{
			handicaps[0] = new RedemptionZone(this);
			handicaps[1] = new RedemptionZone(this);
			handicaps[0].controller = controller.InstantiateHandicap(0);
			handicaps[0].controller.Init(0);
			handicaps[1].controller = controller.InstantiateHandicap(1);
			handicaps[1].controller.Init(1);


			//初始化手牌
			for(int i = 0; i < initialHumanHandicaps + (initialTurn == 0 ? 1 : 0); i++)
			{
				BattleElement element = stacks[0].RandomPop();
				if (element != null)
				{
					presetHandicaps[0].Add(element);
				}
			}
			for (int i = 0; i < initialPlantHandicaps + (initialTurn == 1 ? 1 : 0); i++)
			{
				BattleElement element = stacks[1].RandomPop();
				if (element != null)
				{
					presetHandicaps[1].Add(element);
				}
			}
			handicaps[0].Fill(presetHandicaps[0], initialTurn);
			handicaps[1].Fill(presetHandicaps[1], initialTurn);

			eventTable[0].RaiseEvent("HandicapPushed", null, this);
			eventTable[1].RaiseEvent("HandicapPushed", null, this);
		}

		internal void FieldPreset(UnitElement playerBase, List<BattleNode.FieldPreset> fieldPresets)
		{
			bases[0] = playerBase;
			bases[0].controller = controller.InstantiateUnitInBattleField(0, 0, 0);
			bases[0].Deploy(battleLines[0], 0);
			UpdateFrontLine();
			UpdateAttackRange();

			foreach (BattleNode.FieldPreset fieldPreset in fieldPresets)
			{
				UnitCard card = pool.GetCardByID(fieldPreset.cardPreset.ID) as UnitCard;
				UnitElement element = null;
				int lineIdx = card.ownership == 0 ? fieldPreset.position : linesCapacity - fieldPreset.position - 1;
				switch (card.category)
				{
					case "LightArmor":
						element = new LightArmorElement(card, this,
							controller.InstantiateUnitInBattleField(card.ownership, lineIdx, 0));
						break;
					case "Motorized":
						element = new MotorizedElement(card, this,
							controller.InstantiateUnitInBattleField(card.ownership, lineIdx, 0));
						break;
					case "Artillery":
						element = new ArtilleryElement(card, this,
							controller.InstantiateUnitInBattleField(card.ownership, lineIdx, 0));
						break;
					case "Guardian":
						element = new GuardianElement(card, this,
							controller.InstantiateUnitInBattleField(card.ownership, lineIdx, 0));
						break;
					case "Construction":
						element = new ConstructionElement(card, this,
							controller.InstantiateUnitInBattleField(card.ownership, lineIdx, 0));
						break;
				}
				if (fieldPreset.cardPreset.boss)
				{
					bases[1] = element;
				}
				element.dynHealth = fieldPreset.cardPreset.healthPreset;
				element.dynAttackWriter = fieldPreset.cardPreset.attackPreset;
				element.dynAttackCounter = fieldPreset.cardPreset.attackCounterPreset;
				//TODO
				element.PresetDeploy(battleLines[lineIdx], 0);

				UpdateFrontLine();
				UpdateAttackRange();
			}
			controller.InitBases(bases[0].controller, null);

			eventTable[0].RaiseEvent("BattleStart", null, this);
			eventTable[1].RaiseEvent("BattleStart", null, this);
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

			element.Deploy(battleLines[dstLineIdx], dstPos);

			UpdateFrontLine();
			UpdateAttackRange();
			Settlement();
            if (result != BattleResult.normal) { return; }


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

			element.Cast(battleLines[dstLineIdx][dstIdx]);
			if(element.dynDurability > 0)
			{
				stacks[TURN].Push(element);
			}

			UpdateFrontLine();
			UpdateAttackRange();
			Settlement();
            if (result != BattleResult.normal) { return; }


            eventTable[TURN].RaiseEvent("UpdateAura", null, this);
			eventTable[(TURN + 1) % 2].RaiseEvent("UpdateAura", null, this);

			controller.Settlement();
		}
		public void Cast(int handicapIdx)
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

			element.Cast(null);
			if (element.dynDurability > 0)
			{
				stacks[TURN].Push(element);
			}

			UpdateFrontLine();
			UpdateAttackRange();
			Settlement();
			if (result != BattleResult.normal) { return; }


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
            if (result != BattleResult.normal) { return; }


			eventTable[TURN].RaiseEvent("UpdateAura", null, this);
			eventTable[(TURN + 1) % 2].RaiseEvent("UpdateAura", null, this);

			controller.Settlement();
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
			Settlement();
            if (result != BattleResult.normal) { return; }


            eventTable[TURN].RaiseEvent("UpdateAura", null, this);
			eventTable[(TURN + 1) % 2].RaiseEvent("UpdateAura", null, this);
		}


		public void Skip()
		{
			eventTable[TURN].RaiseEvent("EndOfTurn", null, this);
			BroadCastEvent("EndOfTurn");

			energySupply[TURN] = energySupply[TURN] + 1 > 5 ? 5 : energySupply[TURN] + 1;
			controller.UpdateEnergySupply(energySupply[TURN]);


			RotateSettlement();
			if(result != BattleResult.normal) { return; }


			//新回合
			TURN = (TURN + 1) % 2;
			//发牌
			if (handicaps[TURN].count < handicaps[TURN].capacity)
			{
				BattleElement element = stacks[TURN].RandomPop();
				handicaps[TURN].Push(element, "append", 0);

				eventTable[TURN].RaiseEvent("HandicapPushed", null, this);
			}

			//TODO config
			energy[TURN] = energy[TURN] + energySupply[TURN] > 15 ? 15 : energy[TURN] + energySupply[TURN];
			controller.UpdateEnergy(TURN, energy[TURN]);

			eventTable[TURN].RaiseEvent("StartOfTurn", null, this);
			BroadCastEvent("StartOfTurn");

			//动画结算后回调
			controller.UpdateTurnWithSettlement();
		}
		/// <summary>
		/// 退出 返回作战结果：失败
		/// </summary>
		/// <returns></returns>
		public void Exit()
		{
			Surrender();
		}



		public void Surrender()
		{
			controller.Surrender();
			UnloadBattleField();
		}
		public void BattleFailed()
		{
			controller.BattleFailed();
			UnloadBattleField();
		}
		public void BattleWinned()
		{
			controller.BattleWinned();
		}
		public bool BattleOverChecked()
		{
			if (final)
			{
				tacticalSystem.Exit();
				return false;
			}
			tacticalSystem.playerDeck.InstantiateDeckTags();
			if(result == BattleResult.win)
			{
				tacticalSystem.BattleCampaignCompleted();
			}
			if(result == BattleResult.fail)
			{
				tacticalSystem.Exit();
				return false;
			}
			return true;
		}




		internal BattleElement[] rewards;
		public void GetReward()
		{
			rewards = new BattleElement[3];
			Random random = new Random();

			string[] IDs = new string[3];
			string[] names = new string[3];
			string[] categories = new string[3];
			int[] costs = new int[3];
			int[] attacks = new int[3];
			int[] healths = new int[3];
			int[] counters = new int[3];
			string[] descriptions = new string[3];

			//TODO config
			for(int i = 0; i < 3; i++)
			{
				int index = random.Next(0, pool.humanCardPool.Count);
				Card card = pool.humanCardPool[index];
				BattleElement reward = null;

				IDs[i] = card.backendID;
				names[i] = card.name;
				categories[i] = card.category;
				costs[i] = card.cost;
				descriptions[i] = card.description;

				switch (card.category)
				{
					case "LightArmor":
						reward = new LightArmorElement(card as UnitCard, this, null);
						counters[i] = (reward as UnitElement).oriAttackCounter;
						attacks[i] = (reward as UnitElement).oriAttack;
						healths[i] = (reward as UnitElement).oriHealth;
						break;
					case "Artillery":
						reward = new ArtilleryElement(card as UnitCard, this, null);
						counters[i] = (reward as UnitElement).oriAttackCounter;
						attacks[i] = (reward as UnitElement).oriAttack;
						healths[i] = (reward as UnitElement).oriHealth;
						break;
					case "Motorized":
						reward = new MotorizedElement(card as UnitCard, this, null);
						counters[i] = (reward as UnitElement).oriAttackCounter;
						attacks[i] = (reward as UnitElement).oriAttack;
						healths[i] = (reward as UnitElement).oriHealth;
						break;
					case "Guardian":
						reward = new GuardianElement(card as UnitCard, this, null);
						counters[i] = (reward as UnitElement).oriAttackCounter;
						attacks[i] = (reward as UnitElement).oriAttack;
						healths[i] = (reward as UnitElement).oriHealth;
						break;
					case "Construction":
						reward = new ConstructionElement(card as UnitCard, this, null);
						counters[i] = (reward as UnitElement).oriAttackCounter;
						attacks[i] = (reward as UnitElement).oriAttack;
						healths[i] = (reward as UnitElement).oriHealth;
						break;
					case "Command":
						reward = new CommandElement(card as CommandCard, this);
						counters[i] = (reward as CommandElement).oriDurability;
						break;
				}
				rewards[i] = reward;
			}
			controller.InstantiateReward(IDs, names, categories, costs, attacks, healths, counters, descriptions);
		}
		public void AddReward(int index)
		{
			tacticalSystem.playerDeck.WriteBack(rewards[index]);
			UnloadBattleField();
		}








		internal void BroadCastEvent(string eventName)
		{
			int num = deployQueue.Count;

            for (int i = 0; i < num; i++)
			{
				if (deployQueue[i].ownership == TURN && deployQueue[i].state == ElementState.inBattleLine)
				{
					deployQueue[i].eventTable.RaiseEvent(eventName, deployQueue[i], this);
				}
			}
		}





		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool FrontLineAlign()
		{
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
					controller.UpdateFrontLine(frontLines[0]);
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
			if(result == BattleResult.fail)
			{
				BattleFailed();
			}
			if(result == BattleResult.win)
			{
				BattleWinned();
			}
			//TODO
			UpdateWeight?.Invoke();
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
			if (result == BattleResult.fail)
			{
				BattleFailed();
			}
			if (result == BattleResult.win)
			{
				BattleWinned();
			}
			UpdateWeight?.Invoke();
		}









		public int allyNum;
		public int enemyNum;
		//一些战场查询方法
		internal void UpdateUnitNum(int ownership)
		{
			allyNum = 0;
			enemyNum = 0;
			for (int i = 0; i < deployQueue.Count; i++)
			{
				if (deployQueue[i].state == ElementState.inBattleLine)
				{
					allyNum += deployQueue[i].ownership == ownership ? 1 : 0;
					enemyNum += deployQueue[i].ownership == (ownership + 1) % 2 ? 1 : 0;
				}
			}
		}
		internal UnitElement LowestHealthTarget(int ownership)
		{
			int maxHealth = 100;
			UnitElement maxPointer = null;
			foreach(var item in deployQueue)
			{
				if(item.ownership != ownership && item.state == ElementState.inBattleLine)
				{
					if(item.dynHealth < maxHealth)
					{
						maxHealth = item.dynHealth;
						maxPointer = item;
					}
				}
			}
			return maxPointer;
		}
		internal UnitElement HighestHealthTarget(int ownership)
		{
			int minHealth = 0;
			UnitElement minPointer = null;
			foreach (var item in deployQueue)
			{
				if (item.ownership != ownership && item.state == ElementState.inBattleLine)
				{
					if (item.dynHealth > minHealth)
					{
						minHealth = item.dynHealth;
						minPointer = item;
					}
				}
			}
			return minPointer;
		}
		/// <summary>
		/// 查询敌方随机目标
		/// </summary>
		/// <returns></returns>
		internal UnitElement RandomTarget(int ownership)
		{
			UpdateUnitNum(ownership);
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
					UnitElement unit = battleLines[line][battleLines[line].count - 1 + counter];
					//TODO
					if(unit.ownership == ownership)
					{
						for(int i = 0; i < linesCapacity - 1; i++)
						{
							if (battleLines[i].count != 0 && battleLines[i].ownerShip != ownership)
							{
								return battleLines[0][0];
							}
						}
					}
                    return unit;
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

		internal UnitElement DamagedAlly(int ownership)
		{
			for (int i = 0; i < deployQueue.Count; i++)
			{
				if (deployQueue[i].state == ElementState.inBattleLine)
				{
					if (deployQueue[i].maxHealthReader > deployQueue[i].dynHealth && deployQueue[i].ownership == ownership)
					{
						return deployQueue[i];
					}
				}
			}
			return null;
		}
	}
}
