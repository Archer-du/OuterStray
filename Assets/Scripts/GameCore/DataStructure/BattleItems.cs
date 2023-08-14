//Author@Archer
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using DataCore.Cards;
using DataCore.BattleElements;
using DataCore.TacticalItems;

using DisplayInterface;
using System.Xml.Linq;
using System.Data;

namespace DataCore.BattleItems
{

	/// <summary>
	/// battle front
	/// </summary>
	internal class BattleLine
	{
		//display
		private IBattleLineController Controller;
		internal IBattleLineController controller
		{
			get => Controller;
			set
			{
				if (value != null)
				{
					Controller = value;
					Init();
				}
				else { Controller = null; }
			}
		}


		//data
		/// <summary>
		/// 战线上元素
		/// </summary>
		private List<UnitElement> elementList;
		/// <summary>
		/// 战线当前容量
		/// </summary>
		internal int count { get => elementList.Count; }
		/// <summary>
		/// 战线总容量
		/// </summary>
		internal int capacity;
		/// <summary>
		/// 归属权 0为敌方
		/// </summary>
		internal int ownerShip;
		/// <summary>
		/// 战线编号
		/// </summary>
		internal int index;



		internal BattleLine(int maxElementNum, IBattleLineController controller)
		{
			this.capacity = maxElementNum;
			this.elementList = new List<UnitElement>(maxElementNum);
			//默认敌方
			this.ownerShip = 1;

			this.controller = controller;
		}
		internal UnitElement this[int index]
		{
			get => elementList[index];
			set => elementList[index] = value;
		}


		internal bool Receiveable()
		{
			return count < capacity;
		}
		internal bool Sendable()
		{
			return count > 0;
		}
		/// <summary>
		/// 接收一个单位到该战线指定位置
		/// </summary>
		/// <param name="element"></param>
		/// <param name="pos"></param>
		internal int Receive(UnitElement element, int pos)
		{
			if(count >= capacity)
			{
				return -1;
			}
			controller.Receive(element.controller, pos);

			this.ownerShip = element.ownership;

			elementList.Insert(pos, element);

			UpdateElements();
			UpdateInfo();

			return 1;
		}

		internal UnitElement Send(int idx)
		{
			if(count <= 0)
			{
				return null;
			}
			controller.Send(idx);

			UnitElement element = elementList[idx];

			elementList.RemoveAt(idx);

			UpdateElements();
			UpdateInfo();

			return element;
		}

		internal void UpdateElements()
		{
			for (int i = 0; i < count; i++)
			{
				elementList[i].state = ElementState.inBattleLine;
				elementList[i].inlineIdx = i;
				elementList[i].battleLine = this;
			}
		}
		internal void ElementRemove(int idx)
		{
			elementList.RemoveAt(idx);
			UpdateElements();

			List<IUnitElementController> controllerList = new List<IUnitElementController>();

			for (int i = 0; i < count; i++)
			{
				//display
				UnitElement unit = elementList[i];
				controllerList.Add(unit.controller);
				//unit.Init();
			}
			controller.UpdateElementLogicPosition(controllerList);
		}

		internal void Init()
		{
			controller.Init(capacity, ownerShip);
		}
		internal void UpdateInfo()
		{
			//display
			controller.UpdateInfo(count, ownerShip);
		}

	}





	internal class RandomCardStack
	{
		private ICardStackController Controller;
		internal ICardStackController controller
		{
			get => Controller;
			set
			{
				if (value != null)
				{
					Controller = value;
					Init();
				}
				else { Controller = null; }
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal List<BattleElement> stack;
		internal List<UnitElement> unitStack;
		internal List<CommandElement> commandStack;


		internal Dictionary<string, List<UnitElement>> UnitIDDic;
		internal Dictionary<string, List<CommandElement>> CommIDDic;

		internal List<LightArmorElement> lightArmors;
		internal List<MotorizedElement> motorizeds;
		internal List<ArtilleryElement> artillerys;
		internal List<GuardianElement> guardians;
		internal List<ConstructionElement> constructions;
		/// <summary>
		/// 
		/// </summary>
		internal int count { get => stack.Count; }

		internal int ownership;


		internal RandomCardStack(int ownership, ICardStackController controller)
		{
			stack = new List<BattleElement>(SystemConfig.stackCapacity);

			UnitIDDic = new Dictionary<string, List<UnitElement>>();
			CommIDDic = new Dictionary<string, List<CommandElement>>();


			lightArmors = new List<LightArmorElement>();
			motorizeds = new List<MotorizedElement>();
			artillerys = new List<ArtilleryElement>();
			guardians = new List<GuardianElement>();
			constructions = new List<ConstructionElement>();

			this.ownership = ownership;

			this.controller = controller;
		}

		/// <summary>
		/// fill stack with battle element reference in deck
		/// </summary>
		/// <param name="deck"></param>
		internal void Fill(Deck deck)
		{
			int num = deck.count;
			//TODO
			for (int i = 1; i < num; i++)
			{
				stack.Add(deck[i]);

				if (deck[i] is UnitElement)
				{
					if (!UnitIDDic.ContainsKey(deck[i].backendID))
					{
						UnitIDDic.Add(deck[i].backendID, new List<UnitElement>());
					}
					UnitIDDic[deck[i].backendID].Add(deck[i] as UnitElement);


					if (deck[i] is LightArmorElement)
					{
						LightArmorElement element = deck[i] as LightArmorElement;
						lightArmors.Add(element);
						element.controller = controller.InstantiateUnitElementInBattle();
						element.state = ElementState.inStack;
					}
					if (deck[i] is MotorizedElement)
					{
						MotorizedElement element = deck[i] as MotorizedElement;
						motorizeds.Add(element);
						element.controller = controller.InstantiateUnitElementInBattle();
						element.state = ElementState.inStack;
					}
					if (deck[i] is ArtilleryElement)
					{
						ArtilleryElement element = deck[i] as ArtilleryElement;
						artillerys.Add(element);
						element.controller = controller.InstantiateUnitElementInBattle();
						element.state = ElementState.inStack;
					}
					if (deck[i] is GuardianElement)
					{
						GuardianElement element = deck[i] as GuardianElement;
						guardians.Add(element);
						element.controller = controller.InstantiateUnitElementInBattle();
						element.state = ElementState.inStack;
					}
					if (deck[i] is ConstructionElement)
					{
						ConstructionElement element = deck[i] as ConstructionElement;
						constructions.Add(element);
						element.controller = controller.InstantiateUnitElementInBattle();
						element.state = ElementState.inStack;
					}
				}
				else
				{
					if (!CommIDDic.ContainsKey(deck[i].backendID))
					{
						CommIDDic.Add(deck[i].backendID, new List<CommandElement>());
					}
					CommIDDic[deck[i].backendID].Add(deck[i] as CommandElement);

					CommandElement element = deck[i] as CommandElement;
					element.controller = controller.InstantiateCommandElementInBattle();
					element.state = ElementState.inStack;
				}
			}

			UpdateStackIdx();
		}
		internal void UpdateStackIdx()
		{
			for (int i = 0; i < stack.Count; i++)
			{
				stack[i].stackIdx = i;
			}
		}
		internal void Push(BattleElement element)
		{
			stack.Insert(count - 1, element);

			if (element is UnitElement)
			{
				UnitElement unit = element as UnitElement;
				unit.state = ElementState.inStack;
			}
			else
			{
				CommandElement comm = element as CommandElement;
				comm.state = ElementState.inStack;
			}

			UpdateStackIdx();
		}
		internal BattleElement RandomPop()
		{
			if(stack.Count == 0)
			{
				return null; //TODO
			}
			Random random = new Random();
			int index = random.Next(0, stack.Count);
			BattleElement element = stack[index];
			stack.RemoveAt(index);

			UpdateStackIdx();


			return element;
		}
		//test tutorial TODO remove
		internal BattleElement Pop()
		{
			if (stack.Count == 0)
			{
				return null;
			}
			BattleElement element = stack[0];
			stack.RemoveAt(0);

			UpdateStackIdx();

			return element;
		}
		internal void Clear()
		{
			stack.Clear();
			UnitIDDic.Clear();
		}
		




		internal CommandElement PopCommand()
		{
			if (stack.Count == 0) { return null; }
			for(int i = 0; i < stack.Count; i++)
			{
				if (stack[i] is CommandElement)
				{
					CommandElement element = stack[i] as CommandElement;
					return element;
				}
			}
			return null;
		}
		internal BattleElement PopElementByStackIdx(int stackIdx)
		{
			BattleElement element = stack[stackIdx];
			stack[stackIdx].stackIdx = -1;
			stack.RemoveAt(stackIdx);

			UpdateStackIdx();

			return element;
		}

		internal void UpdateDics()
		{
			foreach(KeyValuePair<string, List<UnitElement>> pair in UnitIDDic)
			{
				int i = 0;
				while(i < pair.Value.Count)
				{
					if (pair.Value[i].stackIdx == -1)
					{
						pair.Value.RemoveAt(i);
					}
					else { i++; }
				}
			}
		}
		internal UnitElement RandomFindUnitByID(string id)
		{
			UpdateDics();

			if (!UnitIDDic.ContainsKey(id))
			{
				return null;
			}
			Random random = new Random();
			int idx = random.Next(0, UnitIDDic[id].Count);
			UnitElement element = UnitIDDic[id][idx];
			UnitIDDic.Remove(id);

			return element;
		}
		internal UnitElement RandomFindUnitByCategory(string category)
		{
			for(int i = stack.Count - 1; i >= 0; i--)
			{
				UnitElement element = stack[i] as UnitElement;
				if (element != null && element.category == category)
				{
					return element;
				}
			}
			return null;
			//Random random = new Random();
			//int idx = 0;
			//switch(category)
			//{
			//	case "LightArmor":
			//		idx = random.Next(0, lightArmors.Count);
			//		return lightArmors[idx];
			//	case "Motorized":
			//		idx = random.Next(0, motorizeds.Count);
			//		return motorizeds[idx];
			//	case "Artillery":
			//		idx = random.Next(0, artillerys.Count);
			//		return artillerys[idx];
			//	case "Guardian":
			//		idx = random.Next(0, guardians.Count);
			//		return guardians[idx];
			//	case "Construction":
			//		idx = random.Next(0, constructions.Count);
			//		return constructions[idx];
			//}
			//return null;
		}

		internal void Init()
		{
			controller.Init(ownership);
		}
	}





	internal class RedemptionZone
	{
		public IHandicapController controller;

		private List<BattleElement> handicap;


		internal int capacity = 8;//TODO config
		internal int count { get => handicap.Count; }
		internal RedemptionZone()
		{
			handicap = new List<BattleElement>();
		}
		internal BattleElement this[int index]
		{
			get => handicap[index];
			set => handicap[index] = value;
		}
		/// <summary>
		/// 初始化手牌
		/// </summary>
		/// <param name="list"></param>
		/// <exception cref="Exception"></exception>
		internal void Fill(List<BattleElement> list)
		{
			List<IBattleElementController> controllerList = new List<IBattleElementController>();

			for(int i = 0; i < list.Count; i++)
			{
				handicap.Add(list[i]);
				list[i].state = ElementState.inHandicap;

				if (list[i] is UnitElement)
				{
					//display
					UnitElement unit = list[i] as UnitElement;
					controllerList.Add(unit.controller);
					//unit.Init();
				}
				else
				{
					CommandElement comm = list[i] as CommandElement;
					controllerList.Add(comm.controller);
					//comm.Init();
				}
			}
			if(controllerList.Count <= 0) { throw new Exception("list"); }
			controller.Fill(controllerList);
		}
		/// <summary>
		/// 添加元素到手牌
		/// </summary>
		/// <param name="element"></param>
		internal void Push(BattleElement element)
		{
			if (element == null) { return; }
			handicap.Add(element);
			element.state = ElementState.inHandicap;


			if (element is UnitElement)
			{
				//display
				UnitElement unit = element as UnitElement;
				//unit.Init();
				controller.Push(unit.controller);
			}
			else
			{
				CommandElement comm = element as CommandElement;
				//comm.Init();
				controller.Push(comm.controller);
			}
		}
		/// <summary>
		/// 根据索引从手牌中移除一个元素(弃牌，被弃牌)
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal BattleElement Pop(int index)
		{
			BattleElement element = handicap[index];
			handicap.RemoveAt(index);

			return element;
		}
		internal void Clear()
		{
			handicap.Clear();
		}
	}















	internal enum BattleLineCategories
	{
		support,
		battleLine,
		FrontLine
	}
	internal enum TerrainCategories
	{

	}
	internal enum NodeCategories
	{
		resNode,
		dstNode,
		battle,
		outpost,
		airDrop,
		medical,
		legacy,
		randomEvent
	}
}