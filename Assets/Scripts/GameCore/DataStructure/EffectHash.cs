using DataCore.BattleElements;
using DataCore.Cards;
using LogicCore;
using System;
using System.Collections;
using System.Collections.Generic;


namespace EventEffectModels
{

	internal delegate void BattleEventHandler(UnitElement source, BattleSystem system);

	/// <summary>
	/// 事件名字符串千万不能写错！！
	/// </summary>
	internal class EventTable
	{

		internal Hashtable eventTable;

		internal EventTable()
		{
			eventTable = new Hashtable();
		}
		/// <summary>
		/// 注册触发事件
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="handler"></param>
		internal void RegisterHandler(string eventName, BattleEventHandler handler)
		{
			if (!eventTable.ContainsKey(eventName))
			{
				eventTable.Add(eventName, null);
			}
			eventTable[eventName] = Delegate.Combine((BattleEventHandler)eventTable[eventName], handler);
		}
		/// <summary>
		/// 注册解除事件
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="handler"></param>
		internal void UnloadHandler(string eventName, BattleEventHandler handler)
		{
			if (eventTable.ContainsKey(eventName))
			{
				eventTable[eventName] = Delegate.Remove((BattleEventHandler)eventTable[eventName], handler);
			}
			else return;
		}
		/// <summary>
		/// broadcast method
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="source">事件的发布者</param>
		/// <exception cref="InvalidOperationException"></exception>
		internal void RaiseEvent(string eventName, UnitElement source, BattleSystem system)
		{
			if (!eventTable.ContainsKey(eventName))
			{
				eventTable.Add(eventName, null);
			}
			BattleEventHandler method = (BattleEventHandler)eventTable[eventName];
			method?.Invoke(source, system);

			//note: 未知委托签名方法时，可用如下方法Invoke
			//if (method != null)
			//{
			//	method?.DynamicInvoke(new object[] { null, args });
			//}
			//委托对象的DynamicInvoke方法可以动态地（后期绑定）调用当前委托所表示的方法，并返回该方法的返回值。
			//它接受一个对象数组作为参数，该数组包含要传递给该方法的参数。如果该方法没有参数，则可以传递一个空数组或者null。如果该方法有返回值，则可以通过返回值获取；如果没有返回值，则返回值为null。
			//使用DynamicInvoke方法可以在不知道委托实际类型或签名的情况下执行委托方法，但是它比直接使用Invoke方法要慢很多，因为它涉及到反射和装箱拆箱等操作。所以在知道委托类型或签名的情况下，尽量使用Invoke方法执行。
		}
	}

	internal class EffectsTable
	{
		internal Hashtable effectsTable;
		internal Hashtable argsTable;
		internal Hashtable buffer;

		//non-args effects
		internal void RecoverOperateCounter(UnitElement element, BattleSystem system)
		{
			element.operateCounter = 1;
		}
		//TODO
		internal void SetMoveRange(UnitElement element, BattleSystem system)
		{
			element.moveRange = 9;
		}
		internal void Cleave(UnitElement element, BattleSystem system)
		{
			element.cleave = true;
		}
		internal void Parry(UnitElement element, BattleSystem system)
		{
			element.immunity = true;
		}
		internal void Lurk(UnitElement element, BattleSystem system)
		{
			element.selectable = true;
		}
		//internal void Cleave(UnitElement element, BattleSystem system)
		//{
		//	element.cleave = true;
		//}
		//internal void Mock(UnitElement element, BattleSystem system)
		//{
		//	element.mocking = true;
		//}
		//internal void Thorn(UnitElement element, BattleSystem system)
		//{
		//	element.thorn = true;
		//}
		//internal void Unyield(UnitElement element, BattleSystem system)
		//{
		//	element.unyielding = true;
		//}


		//args effects
		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="system"></param>
		/// <exception cref="InvalidOperationException"></exception>
		internal void RecruitByID(UnitElement element, BattleSystem system)
		{
			int argsNum = 3;
			if (!argsTable.ContainsKey("RecruitByID"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			if (((List<int>)argsTable["RecruitByID"]).Count != argsNum)
			{
				throw new InvalidOperationException("argsTable list length invalid");
			}

			//第一个参数是招募对象ID数值域
			string ID = ((List<int>)argsTable["RecruitByID"])[0] < 10 
				? "0" + ((List<int>)argsTable["RecruitByID"])[0].ToString() : ((List<int>)argsTable["RecruitByID"])[0].ToString();
			//TODO
			ID = element.ownership == 0 ? "human_" + ID : "mush_" + ID;
			//第二个参数是招募位置
			int position = ((List<int>)argsTable["RecruitByID"])[1];
			//第三个参数是招募数量
			int num = ((List<int>)argsTable["RecruitByID"])[2];

			for(int i = 0; i < num; i++)
			{
				UnitElement unit = system.stacks[element.ownership].FindElementByID(ID) as UnitElement;
				if (unit == null) break;
				switch (position)
				{
					case 0:
						if(system.battleLines[system.supportLines[element.ownership]].Receive(unit, 0) > 0)
						{
							system.stacks[element.ownership].PopElementByID(ID);
						}
						break;
					case 1:
						break;
				}
			}
		}
		internal void RecruitByCategory(UnitElement element, BattleSystem system)
		{
			int argsNum = 3;
			if (!argsTable.ContainsKey("RecruitByCategory"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			if (((List<int>)argsTable["RecruitByCategory"]).Count != argsNum)
			{
				throw new InvalidOperationException("argsTable list length invalid");
			}
			int category = ((List<int>)argsTable["RecruitByCategory"])[0];
			int position = ((List<int>)argsTable["RecruitByCategory"])[1];
			int num = ((List<int>)argsTable["RecruitByCategory"])[2];
		}
		/// <summary>
		/// 根据参数在指定位置召唤指定类型的Token(事件必须有源)
		/// </summary>
		/// <param name="element"></param>
		/// <param name="system"></param>
		/// <exception cref="InvalidOperationException"></exception>
		internal void SummonToken(UnitElement element, BattleSystem system)
		{
			int argsNum = 3;
			if (!argsTable.ContainsKey("SummonToken"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			if(((List<int>)argsTable["SummonToken"]).Count != argsNum)
			{
				throw new InvalidOperationException("argsTable list length invalid");
			}
			//第一个参数是Token种类
			int category = ((List<int>)argsTable["SummonToken"])[0];
			//第二个参数是Token位置
			int position = ((List<int>)argsTable["SummonToken"])[1];
			//第三个参数是Token数量
			int num = ((List<int>)argsTable["SummonToken"])[2];


			//解析完成， 逻辑处理
			for (int i = 0; i < num; i++)
			{
				UnitCard card = null;
				switch (category)
				{
					//召唤亮顶孢子
					case 0: 
						card = system.pool.GetCardByID("mush_00") as UnitCard;
						break;
					case 1:
						break;
					default:
						break;
				}
				UnitElement unit = new UnitElement(card);

				switch (position)
				{
					//召唤至支援战线
					case 0:
						system.battleLines[system.supportLines[element.ownership]].Receive(unit, 0);
						break;
					//召唤至当前战线
					case 1:
						element.battleLine.Receive(unit, 0);
						break;
				}
			}
		}
		internal void AOEDamage(UnitElement element, BattleSystem system)
		{
			int argsNum = 2;

		}
		internal void SummonTokenInline(UnitElement element, BattleSystem system)
		{

		}
		internal void Armor(UnitElement element, BattleSystem system)
		{
			if (!argsTable.ContainsKey("armor"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			element.armor = ((List<int>)argsTable["armor"])[0];
		}

		internal void Batter(UnitElement element, BattleSystem system)
		{
			if (!argsTable.ContainsKey("batter"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			element.batter = ((List<int>)argsTable["batter"])[0];
		}




		internal EffectsTable()
		{
			//新效果方法在这里注册
			effectsTable = new Hashtable()
			{
				//non args
				{"RecoverOperateCounter", (BattleEventHandler)RecoverOperateCounter },

				//args
				{"SummonToken", (BattleEventHandler)SummonToken },
				{"Parry", (BattleEventHandler)Parry },
				{"Lurk", (BattleEventHandler)Lurk },
				//{"CLeave", (BattleEventHandler)Cleave },
				//{"Mock",(BattleEventHandler)Mock },
				//{"Thorn", (BattleEventHandler)Thorn },
				//{"Unyield", (BattleEventHandler)Unyield },
				{"Armor",(BattleEventHandler)Armor }
			};

			//如果需要参数，请在这里注册
			argsTable = new Hashtable()
			{
				{"SummonToken", null },
				{"armor", null },
				{"batter", null }
			};

			//TODO ignore
			buffer = new Hashtable() { };
		}

		internal BattleEventHandler GetHandler(string effectsName)
		{
			if (!effectsTable.ContainsKey(effectsName))
			{
				throw new InvalidOperationException("unregister effect name");
			}
			return (BattleEventHandler)effectsTable[effectsName];
		}
		internal void RegisterArgs(string effectsName, List<int> argList)
		{
			if (!argsTable.ContainsKey(effectsName))
			{
				throw new InvalidOperationException("unregister effect name");
			}
			argsTable[effectsName] = argList;
		}
	}

	internal class CommandTable
	{
		internal Hashtable commandTable;
		internal Hashtable argsTable;
		internal Hashtable buffer;

		internal CommandTable()
		{
			commandTable = new Hashtable()
			{

			};
			argsTable = new Hashtable()
			{

			};
			buffer = new Hashtable() { };
		}
	}
}