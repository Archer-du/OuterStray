using DataCore.BattleElements;
using DataCore.Cards;
using LogicCore;
using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections;
using System.Collections.Generic;


namespace EventEffectModels
{

	internal delegate void BattleEventHandler(BattleElement source, BattleSystem system);

	internal delegate void NonTargetCommandHandler(BattleSystem system);
	internal delegate void TargetedCommandHandler(UnitElement target, BattleSystem system);

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
		/// 将委托注册到事件
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
		/// 将委托从事件中解除
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
		internal void RaiseEvent(string eventName, BattleElement source, BattleSystem system)
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

		//non-args effects---------------------------------------------
		internal void Assault(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			element.assault = true;
		}
		internal void AssaultOnEnable(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			if (element.assault)
			{
				element.moveRange = 9;
			}
		}
		internal void Raid(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			element.raid = true;
		}
		internal void RaidOnEnable(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			if (element.raid)
			{
				element.operateCounter = 1;
			}
		}
		internal void Cleave(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			element.cleave = true;
		}
		//必须订阅自体攻击前事件
		internal void CleaveOnEnable(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			if (element.cleave)
			{
				element.controller.CleaveAttackAnimationEvent(element.target.inlineIdx, element.target.battleLine.count);
				element.target.Attacked(element);
				for (int i = 0; i < 3; i++)
				{
					if (i != element.targetIdx)
					{
						element.attackRange[i]?.Damaged(element.dynAttack, "immediate");
					}
				}
			}
		}
		internal void Armor(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			int argsNum = 1;
			if (!argsTable.ContainsKey("Armor"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			if (((List<int>)argsTable["Armor"]).Count != argsNum)
			{
				throw new InvalidOperationException("argsTable list length invalid");
			}

			element.armor = ((List<int>)argsTable["Armor"])[0];
		}
		internal void ArmorOnEnable(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			if (element.armor > 0)
			{
				element.damage = element.damage - element.armor < 0 ? 0 : element.damage - element.armor;
			}
		}
		internal void Parry(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			element.parry = true;
		}
		//必须订阅自体受击前事件
		internal void ParryOnEnable(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			if (element.parry)
			{
				element.damage = 0;
			}
		}
		internal void ParryUnload(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			if (element.parry)
			{
				element.parry = false;
			}
		}





		//args effects------------------------------------------------------

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="system"></param>
		/// <exception cref="InvalidOperationException"></exception>
		internal void AttackCounterDecrease(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			int argsNum = 1;
			if (!argsTable.ContainsKey("AttackCounterDecrease"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			if (((List<int>)argsTable["AttackCounterDecrease"]).Count != argsNum)
			{
				throw new InvalidOperationException("argsTable list length invalid");
			}

			int decrease = ((List<int>)argsTable["AttackCounterDecrease"])[0];

			element.dynAttackCounter -= decrease;

			element.UpdateInfo();
		}
		internal void DrawCardsRandom(BattleElement source, BattleSystem system)
		{
			int argsNum = 1;



			int num = ((List<int>)argsTable["DrawCardsRandom"])[0];
			for(int i = 0; i < num; i++)
			{
				if (system.handicaps[BattleSystem.TURN].count < system.handicaps[BattleSystem.TURN].capacity)
				{
					BattleElement unit = system.stacks[BattleSystem.TURN].RandomPop();
					system.handicaps[BattleSystem.TURN].Push(unit);
				}
			}
		}
		internal void RandomDamage(BattleElement source, BattleSystem system)
		{
			int argsNum = 2;



			int range = ((List<int>)argsTable["RandomDamage"])[0];
			int damage = ((List<int>)argsTable["RandomDamage"])[1];

			UnitElement target = null;
			switch (range)
			{
				case 0:
					target = system.RandomEnemy();
					break;
				case 1:
					target = system.RandomEnemyAtFrontLine();
					break;
			}

			target?.Damaged(damage, "immediate");
		}
		internal void DoubleRecovery(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			element.recover *= 2;
		}
		internal void UnitGain(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			int argsNum = 2;


			element.DynAttack += ((List<int>)argsTable["UnitGain"])[0];
			element.maxHealth += ((List<int>)argsTable["UnitGain"])[1];
		}

		internal void RandomRecoverDamaged(BattleElement source, BattleSystem system)
		{
			int argsNum = 1;



			int recover = ((List<int>)argsTable["RandomRecoverDamaged"])[0];

			UnitElement unit = system.DamagedAlly();
			unit?.Recover(recover);
		}
		internal void DamageAdjacent(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			int argsNum = 1;



			int damage = ((List<int>)argsTable["DamageAdjacent"])[0];

			if (element.battleLine.index != system.frontLines[element.ownership]) return;

			for(int i = 0; i < system.battleLines[system.frontLines[element.ownership]].count; i++)
			{
				system.battleLines[system.frontLines[element.ownership]][i].Damaged(damage, "immediate");
			}
		}




		private void RecruitToPosition(UnitElement element, BattleSystem system, int position, UnitElement unit)
		{
			switch (position)
			{
				//0：支援战线
				case 0:
					if (system.battleLines[system.supportLines[element.ownership]].Receiveable())
					{
						unit.Deploy(system, system.battleLines[system.supportLines[element.ownership]], 0);
						system.stacks[element.ownership].PopElementByStackIdx(unit.stackIdx);
					}
					break;
				//1：当前战线
				case 1:
					if (element.battleLine.Receiveable())
					{
						unit.Deploy(system, element.battleLine, 0);
						system.stacks[element.ownership].PopElementByStackIdx(unit.stackIdx);
					}
					break;
				case 2:
					if (system.battleLines[system.frontLines[element.ownership]].Receiveable())
					{
						unit.Deploy(system, system.battleLines[system.frontLines[element.ownership]], 0);
						system.stacks[element.ownership].PopElementByStackIdx(unit.stackIdx);
					}
					break;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="system"></param>
		/// <exception cref="InvalidOperationException"></exception>
		internal void RecruitByID(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
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
				UnitElement unit = system.stacks[element.ownership].RandomFindUnitByID(ID);
				if (unit == null) break;

				RecruitToPosition(element, system, position, unit);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="system"></param>
		/// <exception cref="InvalidOperationException"></exception>
		internal void RecruitByCategory(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
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

			for(int i = 0; i < num; i++)
			{
				UnitElement unit = null;
				switch (category)
				{
					case 0:
						unit = system.stacks[element.ownership].RandomFindUnitByCategory("LightArmor");
						break;
					case 1:
						unit = system.stacks[element.ownership].RandomFindUnitByCategory("Motorized");
						break;
					case 2:
						unit = system.stacks[element.ownership].RandomFindUnitByCategory("Artillery");
						break;
					case 3:
						unit = system.stacks[element.ownership].RandomFindUnitByCategory("Guardian");
						break;
					case 4:
						unit = system.stacks[element.ownership].RandomFindUnitByCategory("Construction");
						break;
				}
				if (unit == null) break;

				RecruitToPosition(element, system, position, unit);
			}
		}




		private void SummonToPosition(UnitElement element, BattleSystem system, int position, UnitElement unit)
		{
			switch (position)
			{
				//0：支援战线
				case 0:
					if (system.battleLines[system.supportLines[element.ownership]].Receiveable())
					{
						unit.Deploy(system, system.battleLines[system.supportLines[element.ownership]], 0);
					}
					break;
				//1：当前战线
				case 1:
					if (element.battleLine.Receiveable())
					{
						unit.Deploy(system, element.battleLine, 0);
					}
					break;
				case 2:
					if (system.battleLines[system.frontLines[element.ownership]].Receiveable())
					{
						unit.Deploy(system, system.battleLines[system.frontLines[element.ownership]], 0);
					}
					break;
			}
		}
		/// <summary>
		/// 根据参数在指定位置召唤指定类型的Token(事件必须有源)
		/// </summary>
		/// <param name="element"></param>
		/// <param name="system"></param>
		/// <exception cref="InvalidOperationException"></exception>
		internal void SummonToken(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
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
				UnitElement unit = new UnitElement(card, system);

				SummonToPosition(element, system, position, unit);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="system"></param>
		internal void TokenGain(BattleElement source, BattleSystem system)
		{
			int argNum = 3;



			int category = ((List<int>)argsTable["TokenGain"])[0];
			int atkGain = ((List<int>)argsTable["TokenGain"])[1];
			int maxhealthGain = ((List<int>)argsTable["TokenGain"])[2];

			string ID = null;
			switch (category)
			{
				case 0:
					ID = "mush_00";
					break;
			}

			if (!system.UnitIDDic.ContainsKey(ID)) return;
			if (system.UnitIDDic[ID].Count == 0) return;

			foreach(UnitElement unit in system.UnitIDDic[ID])
			{
				if(unit.state == ElementState.inBattleLine)
				{
					unit.DynAttack += atkGain;
					unit.maxHealth += maxhealthGain;
				}
			}
		}


		internal void AOEDamage(BattleElement source, BattleSystem system)
		{
			int argsNum = 2;

		}





		internal void Aura(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			int argsNum = 3;
			if (!argsTable.ContainsKey("Aura"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			if (((List<int>)argsTable["Aura"]).Count != argsNum)
			{
				throw new InvalidOperationException("argsTable list length invalid");
			}

			element.aura = true;
			system.eventTable[0].RegisterHandler("UpdateAura", element.AuraGain);
			system.eventTable[1].RegisterHandler("UpdateAura", element.AuraGain);


			//作用范围
			int range = ((List<int>)argsTable["Aura"])[0];
			//作用效果
			int type = ((List<int>)argsTable["Aura"])[1];
			//作用数值
			int value = ((List<int>)argsTable["Aura"])[2];

		}





		internal EffectsTable()
		{
			//新效果方法在这里注册
			effectsTable = new Hashtable()
			{
				//non args

				//args
				{"SummonToken", (BattleEventHandler)SummonToken },
				{"Parry", (BattleEventHandler)Parry },
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

}