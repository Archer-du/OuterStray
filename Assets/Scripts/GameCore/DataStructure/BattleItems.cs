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
		internal IBattleLineController controller;


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



		internal BattleLine(int maxElementNum)
		{
			this.capacity = maxElementNum;
			this.elementList = new List<UnitElement>(maxElementNum);
			//默认敌方
			this.ownerShip = 1;
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
			this.ownerShip = element.ownership;

			elementList.Insert(pos, element);

			UpdateElements();
			UpdateInfo();

			return 1;
		}

		internal UnitElement Send(int Idx)
		{
			if(count <= 0)
			{
				return null;
			}
			UnitElement element = elementList[Idx];

			elementList.RemoveAt(Idx);

			UpdateElements();
			UpdateInfo();

			return element;
		}

		internal void UpdateElements()
		{
			for (int i = 0; i < count; i++)
			{
				elementList[i].state = UnitState.inBattleLine;
				elementList[i].inlineIdx = i;
				elementList[i].battleLine = this;
			}
		}
		internal void ElementRemove(int idx)
		{
			elementList.RemoveAt(idx);
			UpdateElements();
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
		public ICardStackController controller;

		/// <summary>
		/// 
		/// </summary>
		internal List<BattleElement> stack;
		internal Dictionary<string, List<UnitElement>> UnitIDDic;

		internal List<LightArmorElement> lightArmors;
		internal List<MotorizedElement> motorizeds;
		internal List<ArtilleryElement> artillerys;
		internal List<GuardianElement> guardians;
		internal List<ConstructionElement> constructions;
		/// <summary>
		/// 
		/// </summary>
		internal int count { get => stack.Count; }


		internal RandomCardStack()
		{
			stack = new List<BattleElement>(SystemConfig.stackCapacity);
			UnitIDDic = new Dictionary<string, List<UnitElement>>();

		}

		/// <summary>
		/// fill stack with battle element reference in deck
		/// </summary>
		/// <param name="deck"></param>
		internal void Fill(Deck deck)
		{
			int num = deck.count;
			for (int i = 0; i < num; i++)
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
						element.state = UnitState.inStack;
						element.controller = controller.InstantiateUnitElementInBattle(element.ownership);
					}
					if (deck[i] is MotorizedElement)
					{
						MotorizedElement element = deck[i] as MotorizedElement;
						motorizeds.Add(element);
						element.state = UnitState.inStack;
						element.controller = controller.InstantiateUnitElementInBattle(element.ownership);
					}
					if (deck[i] is ArtilleryElement)
					{
						ArtilleryElement element = deck[i] as ArtilleryElement;
						artillerys.Add(element);
						element.state = UnitState.inStack;
						element.controller = controller.InstantiateUnitElementInBattle(element.ownership);
					}
					if (deck[i] is GuardianElement)
					{
						GuardianElement element = deck[i] as GuardianElement;
						guardians.Add(element);
						element.state = UnitState.inStack;
						element.controller = controller.InstantiateUnitElementInBattle(element.ownership);
					}
					if (deck[i] is ConstructionElement)
					{
						ConstructionElement element = deck[i] as ConstructionElement;
						constructions.Add(element);
						element.state = UnitState.inStack;
						element.controller = controller.InstantiateUnitElementInBattle(element.ownership);
					}
				}
				else
				{
					//TODO
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
			stack.Add(element);

			if (element is UnitElement)
			{
				UnitElement unit = element as UnitElement;
				unit.state = UnitState.inStack;
			}
			else
			{
				//TODO
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
		internal void Clear()
		{
			stack.Clear();
			UnitIDDic.Clear();
		}





		internal BattleElement PopElementByStackIdx(int stackIdx)
		{
			BattleElement element = stack[stackIdx];
			stack[stackIdx].stackIdx = -1;
			stack.RemoveAt(stackIdx);
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
			Random random = new Random();
			int idx = 0;
			switch(category)
			{
				case "LightArmor":
					idx = random.Next(0, lightArmors.Count);
					return lightArmors[idx];
				case "Motorized":
					idx = random.Next(0, motorizeds.Count);
					return motorizeds[idx];
				case "Artillery":
					idx = random.Next(0, artillerys.Count);
					return artillerys[idx];
				case "Guardian":
					idx = random.Next(0, guardians.Count);
					return guardians[idx];
				case "Construction":
					idx = random.Next(0, constructions.Count);
					return constructions[idx];
			}
			return null;
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
		internal void Fill(List<BattleElement> list)
		{
			List<IUnitElementController> controllerList = new List<IUnitElementController>();

			for(int i = 0; i < list.Count; i++)
			{
				handicap.Add(list[i]);
				
				if (list[i] is UnitElement)
				{
					//display
					UnitElement unit = list[i] as UnitElement;
					controllerList.Add(unit.controller);
					unit.Init();
				}
				else throw new InvalidOperationException();
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


			if (element is UnitElement)
			{
				//display
				UnitElement unit = element as UnitElement;
				controller.Push(unit.controller);
				unit.Init();
			}
			else throw new InvalidOperationException();
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