using DataCore.BattleElements;
using DataCore.BattleItems;
using DataCore.Cards;
using LogicCore;
using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;


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
		internal void UnloadAllHandler()
		{
			eventTable = new Hashtable(); 
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
		internal BattleElement source;

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
				element.controller.AttackAnimationEvent(element.target.inlineIdx, element.target.battleLine.count);
				element.target.Attacked(element);
				for (int i = 0; i < 3; i++)
				{
					if (i != element.targetIdx && element.attackRange[i] != null)
					{
						element.attackRange[i].Damaged(element.dynAttackReader, "immediate");
					}
				}
				system.UpdateAttackRange();
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
					//TODO
					BattleElement unit = system.stacks[BattleSystem.TURN].RandomPop();
					system.handicaps[BattleSystem.TURN].Push(unit);
				}
			}
		}
		internal void RandomDamage(BattleElement element, BattleSystem system)
		{
			int argsNum = 2;



			int range = ((List<int>)argsTable["RandomDamage"])[0];
			int damage = ((List<int>)argsTable["RandomDamage"])[1];

			UnitElement target = null;
			switch (range)
			{
				case 0:
					target = system.RandomEnemy(this.source.ownership);
					break;
				case 1:
					target = system.RandomEnemyAtFrontLine(this.source.ownership);
					break;
			}

			target?.Damaged(damage, "append");
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

			int i = 0; int j = 0;
			int num = system.battleLines[system.frontLines[(this.source.ownership + 1) % 2]].count;
			while (i < num)
			{
				UnitElement e = system.battleLines[system.frontLines[(this.source.ownership + 1) % 2]][j];
				if (e.Damaged(damage, "immediate") > 0)
				{
					j++;
				}
				i++;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="system"></param>
		internal void TokenGain(BattleElement target, BattleSystem system)
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

			foreach (UnitElement unit in system.UnitIDDic[ID])
			{
				if (unit.state == ElementState.inBattleLine)
				{
					unit.dynAttackWriter += atkGain;
					unit.maxHealthWriter += maxhealthGain;
				}
                unit.UpdateInfo();
                unit.UpdateHealth();
            }
		}



		private void RecruitToPosition(BattleSystem system, int position, UnitElement unit)
		{
			switch (position)
			{
				//0：支援战线
				case 0:
					if (system.battleLines[system.supportLines[this.source.ownership]].Receiveable())
					{
						//unit.Init();
						system.stacks[this.source.ownership].PopElementByStackIdx(unit.stackIdx);
						unit.Deploy(system.battleLines[system.supportLines[this.source.ownership]], 0);
					}
					break;
				//1：当前战线
				case 1:
					UnitElement element = this.source as UnitElement;
					if (element.battleLine.Receiveable())
					{
						//unit.Init();
						system.stacks[this.source.ownership].PopElementByStackIdx(unit.stackIdx);
						unit.Deploy(element.battleLine, 0);
					}
					break;
				case 2:
					if (system.battleLines[system.frontLines[this.source.ownership]].Receiveable())
					{
						//unit.Init();
						system.stacks[this.source.ownership].PopElementByStackIdx(unit.stackIdx);
						unit.Deploy(system.battleLines[system.frontLines[this.source.ownership]], 0);
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

				RecruitToPosition(system, position, unit);
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
			UnitElement element = this.source as UnitElement;
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
						unit = system.stacks[this.source.ownership].RandomFindUnitByCategory("LightArmor");
						break;
					case 1:
						unit = system.stacks[this.source.ownership].RandomFindUnitByCategory("Motorized");
						break;
					case 2:
						unit = system.stacks[this.source.ownership].RandomFindUnitByCategory("Artillery");
						break;
					case 3:
						unit = system.stacks[this.source.ownership].RandomFindUnitByCategory("Guardian");
						break;
					case 4:
						unit = system.stacks[this.source.ownership].RandomFindUnitByCategory("Construction");
						break;
				}
				if (unit == null) break;

				RecruitToPosition(system, position, unit);
			}
		}




		private UnitElement SummonToPosition(UnitElement element, BattleSystem system, int position, UnitCard card)
		{
			UnitElement unit = null;

			switch (position)
			{
				//0：支援战线
				case 0:
					if (system.battleLines[system.supportLines[this.source.ownership]].Receiveable())
					{
						unit = new UnitElement(card, system, system.controller.InstantiateUnitInStack(this.source.ownership));
						unit.Deploy(system.battleLines[system.supportLines[this.source.ownership]], 0);
					}
					break;
				//1：当前战线
				case 1:
					if (element.battleLine.Receiveable())
					{
						unit = new UnitElement(card, system, system.controller.InstantiateUnitInStack(element.ownership));
						unit.Deploy(element.battleLine, 0);
					}
					break;
				//2: 前线
				case 2:
					if (system.battleLines[system.frontLines[this.source.ownership]].Receiveable())
					{
						unit = new UnitElement(card, system, system.controller.InstantiateUnitInStack(this.source.ownership));
						unit.Deploy(system.battleLines[system.frontLines[this.source.ownership]], 0);
					}
					break;
			}

			return unit;
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
				SummonToPosition(element, system, position, card);
			}
		}

		internal void TargetDamage(BattleElement source, BattleSystem system)
		{
			UnitElement target = source as UnitElement;
			int argsNum = 1;



			int damage = ((List<int>)argsTable["TargetDamage"])[0];
			target.Damaged(damage, "immediate");
		}

		internal void AOEDamage(BattleElement source, BattleSystem system)
		{
			int argsNum = 2;



			int position = ((List<int>)argsTable["AOEDamage"])[0];
			int damage = ((List<int>)argsTable["AOEDamage"])[1];


			switch (position)
			{
				//0：支援战线
				case 0:
					break;
				//1：当前战线
				case 1:
					break;
				//2: 前线
				case 2:
					int i = 0; int j = 0;
					int num = system.battleLines[system.frontLines[(BattleSystem.TURN + 1) % 2]].count;
                    while (i < num)
					{
						if(system.battleLines[system.frontLines[(BattleSystem.TURN + 1) % 2]][j].Damaged(damage, "immediate") > 0)
						{
							j++;
						}
						i++;
					}
					break;
			}
		}
		internal void AOERetreat(BattleElement source, BattleSystem system)
		{
			int argsNum = 1;

			int position = ((List<int>)argsTable["AOERetreat"])[0];

			switch (position)
			{
				case 0:
					break;
				case 1:
					break;
				case 2:
					int num = system.battleLines[system.frontLines[(BattleSystem.TURN + 1) % 2]].count;
					for (int i = 0; i < num; i++)
					{
						UnitElement unit = system.battleLines[system.frontLines[(BattleSystem.TURN + 1) % 2]][0];
						system.stacks[(BattleSystem.TURN + 1) % 2].Push(unit);
						unit.Retreat("immediate");
					}
					break;
			}
		}
		internal void RecoverBase(BattleElement source, BattleSystem system)
		{
			int argsNum = 1;

			int value = ((List<int>)argsTable["RecoverBase"])[0];

			system.bases[BattleSystem.TURN].Recover(value);
		}
		







		internal void Aura(BattleElement source, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;

			publisher.aura = true;
		}
		internal void AuraUnload(BattleElement source, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;

			publisher.aura = false;

			for(int i = 0; i < system.deployQueue.Count; i++)
			{
				if (system.deployQueue[i].attackGain.ContainsKey(publisher.battleID))
				{
					system.deployQueue[i].attackGain.Remove(publisher.battleID);
				}
				if (system.deployQueue[i].maxHealthGain.ContainsKey(publisher.battleID))
				{
					system.deployQueue[i].maxHealthGain.Remove(publisher.battleID);
					if(system.deployQueue[i].dynHealth > system.deployQueue[i].maxHealthReader)
					{
						system.deployQueue[i].dynHealth = system.deployQueue[i].maxHealthReader;
					}
				}
				system.deployQueue[i].UpdateInfo();
				system.deployQueue[i].UpdateHealth();
			}
		}
		internal void AuraDisable(BattleElement target, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			UnitElement element = target as UnitElement;

			if (element.attackGain.ContainsKey(publisher.battleID))
			{
				element.attackGain.Remove(publisher.battleID);
			}
			if (element.maxHealthGain.ContainsKey(publisher.battleID))
			{
				element.maxHealthGain.Remove(publisher.battleID);
				if (element.dynHealth > element.maxHealthReader)
				{
					element.dynHealth = element.maxHealthReader;
				}
			}
			element.UpdateInfo();
			element.UpdateHealth();
		}

		internal void AuraRandomDamage(BattleElement element, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			if (!publisher.aura)
			{
				return;
			}
			int argsNum = 2;



			int range = ((List<int>)argsTable["AuraRandomDamage"])[0];
			int damage = ((List<int>)argsTable["AuraRandomDamage"])[1];

			UnitElement target = null;
			switch (range)
			{
				case 0:
					target = system.RandomEnemy(this.source.ownership);
					break;
				case 1:
					target = system.RandomEnemyAtFrontLine(this.source.ownership);
					break;
			}

			target?.Damaged(damage, "append");
		}
		internal void AuraAttackCounterDecrease(BattleElement element, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			if (!publisher.aura)
			{
				return;
			}
			int argsNum = 1;
			if (!argsTable.ContainsKey("AuraAttackCounterDecrease"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			if (((List<int>)argsTable["AuraAttackCounterDecrease"]).Count != argsNum)
			{
				throw new InvalidOperationException("argsTable list length invalid");
			}

			int decrease = ((List<int>)argsTable["AuraAttackCounterDecrease"])[0];

			publisher.dynAttackCounter -= decrease;

			publisher.UpdateInfo();
		}
		internal void AuraUnitGain(BattleElement target, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			if (!publisher.aura)
			{
				return;
			}
			int argsNum = 2;


			int atkGain = ((List<int>)argsTable["AuraUnitGain"])[0];
			int mhpGain = ((List<int>)argsTable["AuraUnitGain"])[1];

			UnitElement element = target as UnitElement;

			if(element != this.source)
			{
				element.attackGain.Add(publisher.battleID, atkGain);
				element.maxHealthGain.Add(publisher.battleID, mhpGain);
				element.dynHealth += mhpGain;
			}
			element.UpdateInfo();
			element.UpdateHealth();
		}
		internal void AuraConstantUnitGain(BattleElement target, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			if (!publisher.aura)
			{
				return;
			}
			int argsNum = 2;


			int atkGain = ((List<int>)argsTable["AuraUnitGain"])[0];
			int mhpGain = ((List<int>)argsTable["AuraUnitGain"])[1];

			for(int i = 0; i < system.deployQueue.Count; i++)
			{
				UnitElement element = system.deployQueue[i];
				if(element.ownership == publisher.ownership && element.state == ElementState.inBattleLine && element is not ConstructionElement)
				{
					element.attackGain.Add(publisher.battleID, atkGain);
					element.maxHealthGain.Add(publisher.battleID, mhpGain);
					element.dynHealth += mhpGain;
				}
				element.UpdateInfo();
				element.UpdateHealth();
			}
		}
		internal void AuraDoubleRecover(BattleElement target, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			if (!publisher.aura)
			{
				return;
			}


			UnitElement element = target as UnitElement;
			element.recover *= 2;
		}
		internal void AuraBaseImmunity(BattleElement target, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			if (!publisher.aura)
			{
				return;
			}

			UnitElement element = target as UnitElement;
			if(element.dynHealth - element.damage <= 0)
			{
				element.dynHealth = 1;
				element.damage = 0;
				element.UpdateHealth();
			}
		}








		//temp
		internal void DrawCommandCardsRandomAndRecover(BattleElement source, BattleSystem system)
		{
			int argsNum = 2;



			int num = ((List<int>)argsTable["DrawCommandCardsRandomAndRecover"])[0];
			int recover = ((List<int>)argsTable["DrawCommandCardsRandomAndRecover"])[1];

			for (int i = 0; i < num; i++)
			{
				if (system.handicaps[BattleSystem.TURN].count < system.handicaps[BattleSystem.TURN].capacity)
				{
					CommandElement comm = system.stacks[BattleSystem.TURN].PopCommand();

					comm.dynDurability += 2;

					system.handicaps[BattleSystem.TURN].Push(comm);
				}
			}
		}







		internal void Comm_Mush_07(BattleElement source, BattleSystem system)
		{
			CommandElement publisher = this.source as CommandElement;


            int category = 0;
			int num = 2;
			//解析完成， 逻辑处理
			for (int i = 0; i < num + publisher.tempBufferForCommMush07; i++)
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
				UnitElement unit = new UnitElement(card, system,
					system.stacks[1].controller.InstantiateUnitElementInBattle());

				if (system.handicaps[BattleSystem.TURN].count < system.handicaps[BattleSystem.TURN].capacity)
				{
					system.handicaps[BattleSystem.TURN].Push(unit);
				}
			}

			publisher.tempBufferForCommMush07++;
		}
		internal void Comm_Mush_08(BattleElement source, BattleSystem system)
		{
			int count = 0;
			for (int i = 0; i < system.linesCapacity; i++)
			{
				for (int j = 0; j < system.battleLines[i].count; j++)
				{
					if(system.battleLines[i][j].backendID == "mush_00")
					{
						count++;
					}
				}
			}
			for(int i = 0; i < count + 1; i++)
			{
				if (system.handicaps[BattleSystem.TURN].count < system.handicaps[BattleSystem.TURN].capacity)
				{
					BattleElement unit = system.stacks[BattleSystem.TURN].RandomPop();
					system.handicaps[BattleSystem.TURN].Push(unit);
				}
			}
		}
		internal void Comm_Mush_13(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			for (int i = 0; i < 3; i++)
			{
				if (system.handicaps[BattleSystem.TURN].count < system.handicaps[BattleSystem.TURN].capacity)
				{
					BattleElement e = system.stacks[BattleSystem.TURN].RandomPop();
					system.handicaps[BattleSystem.TURN].Push(e);
				}
			}

			UnitCard card = null;
			card = system.pool.GetCardByID("mush_00") as UnitCard;

			UnitElement unit = SummonToPosition(element, system, 2, card);
			if (unit == null) return;

			unit.dynAttackWriter = system.handicaps[element.ownership].count > 0 ? system.handicaps[element.ownership].count : 1;
			unit.maxHealthWriter = system.handicaps[element.ownership].count > 0 ? system.handicaps[element.ownership].count : 1;

			unit.UpdateInfo();
			unit.UpdateHealth();
		}
		internal void Comm_Mush_18(BattleElement source, BattleSystem system)
		{
			UnitElement target = source as UnitElement;

			target.Damaged(5, "immediate");
			
			for(int i = 0; i < target.battleLine.count; i++)
			{
				if (i == 0)
				{
					target.battleLine[i].Damaged(2, "append");
					continue;
				}
				target.battleLine[i].Damaged(2, "immediate");
			}
		}
		/// <summary>
		/// boss: mush_99_01特效
		/// </summary>
		/// <param name="element"></param>
		/// <param name="system"></param>
		internal void StrangeGrowth(BattleElement element, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;

			publisher.dynAttackWriter += 2;
			publisher.maxHealthWriter += 2;
			publisher.UpdateHealth();

			if (publisher.dynAttackWriter >= 20)
			{
				BattleLine line = publisher.battleLine;
				int resIdx = publisher.inlineIdx;
				publisher.Terminate("immediate");
				UnitCard card = system.pool.GetCardByID("mush_99_00") as UnitCard;
				UnitElement unit = new GuardianElement(card, system,
					system.controller.InstantiateUnitInBattleField(element.ownership, line.index, resIdx));
				unit.Deploy(line, resIdx);
			}
		}
		internal void StrangeGrowthTutorial(BattleElement element, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;

			publisher.dynAttackWriter += 2;
			publisher.maxHealthWriter += 2;
			publisher.UpdateHealth();

			if (publisher.dynAttackWriter >= 15)
			{
				BattleLine line = publisher.battleLine;
				int resIdx = publisher.inlineIdx;
				publisher.Terminate("immediate");
				UnitCard card = system.pool.GetCardByID("mush_100_00") as UnitCard;
				UnitElement unit = new GuardianElement(card, system,
					system.controller.InstantiateUnitInBattleField(element.ownership, line.index, resIdx));
				system.bases[1] = unit;
				unit.Deploy(line, resIdx);
			}
		}

		internal EffectsTable(BattleElement source)
		{
			this.source	= source;
			//新效果方法在这里注册
			effectsTable = new Hashtable()
			{
				//basic
				{"Assault", (BattleEventHandler)Assault },
				{"AssaultOnEnable", (BattleEventHandler)AssaultOnEnable },
				{"Raid", (BattleEventHandler)Raid },
				{"RaidOnEnable", (BattleEventHandler)RaidOnEnable },
				{"Cleave", (BattleEventHandler)Cleave },
				{"CleaveOnEnable", (BattleEventHandler)CleaveOnEnable },
				{"Armor", (BattleEventHandler)Armor },
				{"ArmorOnEnable", (BattleEventHandler)ArmorOnEnable },
				{"Parry", (BattleEventHandler)Parry },
				{"ParryOnEnable", (BattleEventHandler)ParryOnEnable },
				{"ParryUnload", (BattleEventHandler)ParryUnload },
				{"AttackCounterDecrease", (BattleEventHandler)AttackCounterDecrease },
				{"DrawCardsRandom", (BattleEventHandler)DrawCardsRandom },
				{"RandomDamage", (BattleEventHandler)RandomDamage },
				{"RandomRecoverDamaged", (BattleEventHandler)RandomRecoverDamaged },
				{"DamageAdjacent", (BattleEventHandler)DamageAdjacent },
				{"RecruitByID", (BattleEventHandler)RecruitByID },
				{"RecruitByCategory", (BattleEventHandler)RecruitByCategory },
				{"SummonToken", (BattleEventHandler)SummonToken },
				{"TokenGain", (BattleEventHandler)TokenGain },
				{"TargetDamage", (BattleEventHandler)TargetDamage },
				{"AOEDamage", (BattleEventHandler)AOEDamage },
				{"AOERetreat", (BattleEventHandler)AOERetreat },
				{"RecoverBase", (BattleEventHandler)RecoverBase },
				{"DrawCommandCardsRandomAndRecover", (BattleEventHandler)DrawCommandCardsRandomAndRecover },
				{"StrangeGrowth", (BattleEventHandler)StrangeGrowth },
				{"StrangeGrowthTutorial", (BattleEventHandler)StrangeGrowthTutorial },
				{"AuraConstantUnitGain", (BattleEventHandler)AuraConstantUnitGain },

				//aura
				{"Aura", (BattleEventHandler)Aura },
				{"AuraUnload", (BattleEventHandler)AuraUnload },
				{"AuraDisable", (BattleEventHandler)AuraDisable },

				{"AuraRandomDamage", (BattleEventHandler)AuraRandomDamage },
				{"AuraAttackCounterDecrease", (BattleEventHandler)AuraAttackCounterDecrease },
				{"AuraUnitGain", (BattleEventHandler)AuraUnitGain },
				{"AuraDoubleRecover", (BattleEventHandler)AuraDoubleRecover },
				{"AuraBaseImmunity", (BattleEventHandler)AuraBaseImmunity },

				//command
				{"Comm_Mush_07", (BattleEventHandler)Comm_Mush_07 },
				{"Comm_Mush_08", (BattleEventHandler)Comm_Mush_08 },
				{"Comm_Mush_13", (BattleEventHandler)Comm_Mush_13 },
				{"Comm_Mush_18", (BattleEventHandler)Comm_Mush_18 },
			};

			//如果需要参数，请在这里注册
			argsTable = new Hashtable()
			{
				{"Assault", null },
				{"AssaultOnEnable", null },
				{"Raid", null },
				{"RaidOnEnable", null },
				{"Cleave", null },
				{"CleaveOnEnable",	null },
				{"Armor", null },
				{"ArmorOnEnable", null },
				{"Parry", null },
				{"ParryOnEnable", null },
				{"ParryUnload", null },
				{"AttackCounterDecrease", null },
				{"DrawCardsRandom", null },
				{"RandomDamage", null },
				{"RandomRecoverDamaged", null },
				{"DamageAdjacent", null },
				{"RecruitByID", null },
				{"RecruitByCategory", null },
				{"SummonToken", null },
				{"TokenGain", null },
				{"TargetDamage", null },
				{"AOEDamage", null },
				{"AOERetreat", null },
				{"DrawCommandCardsRandomAndRecover", null },
				{"RecoverBase", null },
				{"StrangeGrowth", null },
				{"StrangeGrowthTutorial", null },
				{"AuraConstantUnitGain", null },

				{"Aura", null },
				{"AuraUnload", null },
				{"AuraDisable", null },
				{"AuraRandomDamage", null },
				{"AuraAttackCounterDecrease", null },
				{"AuraUnitGain", null },
				{"AuraDoubleRecover", null },
				{"AuraBaseImmunity", null },

				{"Comm_Mush_07", null },
				{"Comm_Mush_08", null },
				{"Comm_Mush_13", null },
				{"Comm_Mush_18", null },
			};

			//TODO ignore
			buffer = new Hashtable()
			{
				{"StrangeGrowth", new List<int>(1) {0} }
			};
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