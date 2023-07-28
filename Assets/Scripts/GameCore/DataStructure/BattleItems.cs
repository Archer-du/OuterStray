//Author@Archer
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using DataCore.Cards;
using DataCore.BattleElements;
using DataCore.TacticalItems;

using DisplayInterface;


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



		/// <summary>
		/// 接收一个单位到该战线指定位置
		/// </summary>
		/// <param name="element"></param>
		/// <param name="pos"></param>
		/// <exception cref="InvalidOperationException"></exception>
		internal void Receive(UnitElement element, int pos)
		{
			// TODO throw event
			elementList.Insert(pos, element);
			this.ownerShip = element.ownership;
			//TODO check
			for(int i = 0; i < count; i++)
			{
				elementList[i].inlineIdx = i;
				elementList[i].battleLine = this;
			}

			//display
			//controller.Receive(element.controller, pos);
			InfoUpdate();
		}

		internal UnitElement Send(int Idx)
		{
			UnitElement element = elementList[Idx];

			elementList.RemoveAt(Idx);
			for (int i = 0; i < count; i++)
			{
				elementList[i].inlineIdx = i;
				elementList[i].battleLine = this;
			}

			InfoUpdate();

			return element;
		}

		internal void ElementDestroy(int idx)
		{
			elementList.RemoveAt(idx);
			for (int i = 0; i < count; i++)
			{
				elementList[i].inlineIdx = i;
				elementList[i].battleLine = this;
			}
		}

		internal void Init()
		{
			controller.Init(capacity, ownerShip);
		}
		internal void InfoUpdate()
		{
			//display
			controller.InfoUpdate(count, ownerShip);
		}

	}


	internal class RandomCardStack
	{
		public ICardStackController controller;

		/// <summary>
		/// 
		/// </summary>
		internal List<BattleElement> stack;
		/// <summary>
		/// 
		/// </summary>
		internal int count { get => stack.Count; }


		internal RandomCardStack()
		{
			stack = new List<BattleElement>(SystemConfig.stackCapacity);
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
					UnitElement element = deck[i] as UnitElement;
					element.controller = controller.InstantiateUnitElement(element.ownership);
					
				}
			}

		}
		internal void Push(BattleElement element)
		{
			stack.Add(element);
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
			return element;
		}
		internal void Clear()
		{
			stack.Clear();
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
			handicap = new List<BattleElement>();//TODO config
		}
		internal BattleElement this[int index]
		{
			get => handicap[index];
			set => handicap[index] = value;
		}
		/// <summary>
		/// 添加元素到手牌
		/// </summary>
		/// <param name="element"></param>
		internal void Push(BattleElement element)
		{
			//TODO if()
			
			handicap.Add(element);


			if (element is UnitElement)
			{
				//display
				UnitElement unit = element as UnitElement;
				controller.Push(unit.controller);
				unit.Init();
			}
			else throw new InvalidOperationException();//TODO display
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