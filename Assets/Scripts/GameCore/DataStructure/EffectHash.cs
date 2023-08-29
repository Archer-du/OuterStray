using DataCore.BattleElements;
using DataCore.BattleItems;
using DataCore.Cards;
using LogicCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.VirtualTexturing;


namespace EventEffectModels
{

	internal delegate void BattleEventHandler(BattleElement source, BattleSystem system);

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
				element.operateCounter = 1;
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
				element.moveRange = 9;
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


		internal void Thorn(BattleElement source, BattleSystem system)
		{
			UnitElement element = this.source as UnitElement;
			element.thorn = true;
		}
		internal void ThornOnEnable(BattleElement source, BattleSystem system)
		{
			UnitElement element = source as UnitElement;
			if (element.thorn)
			{
				element.attackSource.Damaged(element.dynAttackReader, "append");
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
		/// 无目标/自身生效
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
					system.handicaps[BattleSystem.TURN].Push(unit, "immediate", i + 1 - num);
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
					target = system.RandomTarget(this.source.ownership);
					break;
				case 1:
					target = system.RandomEnemyAtFrontLine(this.source.ownership);
					break;
			}

			target?.Damaged(damage, "append");
		}
		internal void RandomTerminate(BattleElement element, BattleSystem system)
		{
			int argsNum = 2;



			int range = ((List<int>)argsTable["RandomDamage"])[0];
			int damage = ((List<int>)argsTable["RandomDamage"])[1];

			UnitElement target = null;
			switch (range)
			{
				case 0:
					target = system.RandomTarget(this.source.ownership);
					break;
				case 1:
					target = system.RandomEnemyAtFrontLine(this.source.ownership);
					break;
			}

			target?.Terminate("append");
		}









		internal void GetExtraEnergy(BattleElement element, BattleSystem system)
		{
			int energy = ((List<int>)argsTable["GetExtraEnergy"])[0];

			system.energy[this.source.ownership] += energy;
		}
		internal void DrawExtraHandicaps(BattleElement element, BattleSystem system)
		{
			int extras = ((List<int>)argsTable["DrawExtraHandicaps"])[0];

			for(int i = 0; i < extras; i++)
			{
				BattleElement e = system.stacks[this.source.ownership].RandomPop();
				if (e != null)
				{
					system.presetHandicaps[this.source.ownership].Add(e);
				}
			}
		}
		internal void DecreaseHandicapsCost(BattleElement element, BattleSystem system)
		{
			int decrease = ((List<int>)argsTable["DecreaseHandicapsCost"])[0];

			int num = system.handicaps[this.source.ownership].count;
			for (int i = 0; i < num; i++)
			{
				BattleElement e = system.handicaps[this.source.ownership][i];
				if(e.cost != e.oriCost - decrease)
				{
					e.cost = e.oriCost - decrease;
					e.UpdateInfo();
				}
			}
		}
		internal void RecoverAdjacent(BattleElement element, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;

			int recover = ((List<int>)argsTable["RecoverAdjacent"])[0];

			if(publisher.inlineIdx + 1 < publisher.battleLine.count)
			{
				publisher.battleLine[publisher.inlineIdx + 1].Recover(recover, "append");
			}
			if(publisher.inlineIdx - 1 >= 0)
			{
				publisher.battleLine[publisher.inlineIdx - 1].Recover(recover, "append");
			}
		}




		internal void RandomRecoverDamaged(BattleElement source, BattleSystem system)
		{
			int argsNum = 1;



			int recover = ((List<int>)argsTable["RandomRecoverDamaged"])[0];

			UnitElement unit = system.DamagedAlly(this.source.ownership);
			unit?.Recover(recover, "append");
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
				if (e.Damaged(damage, i == 0 ? "append" : "immediate") > 0)
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
			UnitElement element = this.source as UnitElement;
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
		internal void RecruitByCost(BattleElement source, BattleSystem system)
		{
			int argsNum = 3;
			if (!argsTable.ContainsKey("RecruitByCost"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			if (((List<int>)argsTable["RecruitByCost"]).Count != argsNum)
			{
				throw new InvalidOperationException("argsTable list length invalid");
			}
			int costValve = ((List<int>)argsTable["RecruitByCost"])[0];
			int position = ((List<int>)argsTable["RecruitByCost"])[1];
			int num = ((List<int>)argsTable["RecruitByCost"])[2];

			List<UnitElement> targets = new List<UnitElement>();
			int count = system.stacks[this.source.ownership].count;
			for (int i = 0; i < count; i++)
			{
				BattleElement e = system.stacks[this.source.ownership][i];
				if (e.cost < costValve && e is UnitElement)
				{
					targets.Add(e as UnitElement);
				}
			}
			if (targets.Count == 0) return;
			int n = num > targets.Count ? targets.Count : num;
			for (int i = 0; i < n; i++)
			{
				UnitElement unit;
				Random random = new Random();

				int index = random.Next(0, targets.Count);

				unit = targets[index];
				targets.RemoveAt(index);

				RecruitToPosition(system, position, unit);
			}
		}








		private UnitElement SummonToPosition(BattleElement element, BattleSystem system, int position, UnitCard card)
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
					UnitElement e = element as UnitElement;
					if (e.battleLine.Receiveable())
					{
						unit = new UnitElement(card, system, system.controller.InstantiateUnitInStack(element.ownership));
						unit.Deploy(e.battleLine, 0);
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
					case 2:
						card = system.pool.GetCardByID("mush_103") as UnitCard;
						break;
					default:
						break;
				}
				SummonToPosition(element, system, position, card);
			}
		}
		//单位卡专用
		internal void SummonTokenAndGain(BattleElement source, BattleSystem system)
		{
			UnitElement element = this.source as UnitElement;
			int argsNum = 5;
			if (!argsTable.ContainsKey("SummonTokenAndGain"))
			{
				throw new InvalidOperationException("argsTable fault");
			}
			if (((List<int>)argsTable["SummonTokenAndGain"]).Count != argsNum)
			{
				throw new InvalidOperationException("argsTable list length invalid");
			}
			//第一个参数是Token种类
			int category = ((List<int>)argsTable["SummonTokenAndGain"])[0];
			//第二个参数是Token位置
			int position = ((List<int>)argsTable["SummonTokenAndGain"])[1];
			//第三个参数是Token数量
			int num = ((List<int>)argsTable["SummonTokenAndGain"])[2];
			//
			int atkGain = ((List<int>)argsTable["SummonTokenAndGain"])[3];
			int mhpGain = ((List<int>)argsTable["SummonTokenAndGain"])[4];


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
				if(SummonToPosition(element, system, position, card) == null)
				{
					BattleLine line = system.battleLines[system.frontLines[this.source.ownership]];
					for(int j = 0; j < line.count; j++)
					{
						line[j].dynAttackWriter += atkGain;
						line[j].maxHealthWriter += mhpGain;
						line[j].UpdateInfo();
						line[j].UpdateHealth();
					}
				}
			}
		}






		internal void TargetDamage(BattleElement source, BattleSystem system)
		{
			UnitElement target = source as UnitElement;
			int argsNum = 1;



			int damage = ((List<int>)argsTable["TargetDamage"])[0];
			target.Damaged(damage, "immediate");
		}
		internal void TargetRecover(BattleElement source, BattleSystem system)
		{
			UnitElement target = source as UnitElement;


			int recover = ((List<int>)argsTable["TargetRecover"])[0];

			target.Recover(recover, "immediate");
		}
		internal void TargetRetreat(BattleElement source, BattleSystem system)
		{
			UnitElement target = source as UnitElement;

			system.stacks[target.ownership].Push(target);
			target.Retreat("immediate");
		}
		internal void TargetTerminateAlly(BattleElement source, BattleSystem system)
		{
			UnitElement target = source as UnitElement;

			if(target.ownership != this.source.ownership)
			{
				return;
			}

			target.Terminate("immediate");
		}
		internal void TargetGain(BattleElement source, BattleSystem system)
		{
			int atkGain = ((List<int>)argsTable["TargetGain"])[0];
			int mhpGain = ((List<int>)argsTable["TargetGain"])[1];

			for (int i = 0; i < system.deployQueue.Count; i++)
			{
				//UnitElement element = system.deployQueue[i];
				//if (element.ownership == publisher.ownership && element.state == ElementState.inBattleLine)
				//{
				//	element.attackGain.Add(publisher.battleID, atkGain);
				//	element.maxHealthGain.Add(publisher.battleID, mhpGain);
				//	element.dynHealth += mhpGain;
				//}
				//element.UpdateInfo();
				//element.UpdateHealth();
			}
		}
		internal void TargetSetParry(BattleElement source, BattleSystem system)
		{
			UnitElement target = source as UnitElement;

			target.parry = true;
			target.EffectsParse("<self:BeforeDamaged+ParryOnEnable-0>/<self:AfterDamaged+ParryUnload-0>");

			target.UpdateInfo();
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
				//3：全图
				case 3:
					foreach (UnitElement unit in system.deployQueue)
					{
						if(unit.ownership != this.source.ownership && unit.state == ElementState.inBattleLine)
						{
							unit.Damaged(damage, "immediate");
						}
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
					int num = system.battleLines[system.frontLines[(this.source.ownership + 1) % 2]].count;
					for (int i = 0; i < num; i++)
					{
						UnitElement unit = system.battleLines[system.frontLines[(this.source.ownership + 1) % 2]][0];
						system.stacks[(this.source.ownership + 1) % 2].Push(unit);
						unit.Retreat("immediate");
					}
					break;
			}
		}
		internal void AOERecover(BattleElement source, BattleSystem system)
		{
			int position = ((List<int>)argsTable["AOERecover"])[0];

			int heal = ((List<int>)argsTable["AOERecover"])[1];

			switch (position)
			{
				case 0:
					break;
				case 1:
					break;
				case 2:
					foreach (UnitElement unit in system.deployQueue)
					{
						if (unit.ownership == this.source.ownership && unit.state == ElementState.inBattleLine && unit.battleLine.index == system.frontLines[this.source.ownership])
						{
							unit.Recover(heal, "immediate");
						}
					}
					break;
				//全图
				case 3:
					foreach (UnitElement unit in system.deployQueue)
					{
						if (unit.ownership == this.source.ownership && unit.state == ElementState.inBattleLine)
						{
							unit.Recover(heal, "immediate");
						}
					}
					break;

			}
		}
		internal void AOEStifle(BattleElement source, BattleSystem system)
		{
			int position = ((List<int>)argsTable["AOEStifle"])[0];

			switch (position)
			{
				case 0:
					break;
				case 1:
					break;
				case 2:
					int num = system.battleLines[system.frontLines[(this.source.ownership + 1) % 2]].count;
					for (int i = 0; i < num; i++)
					{
						UnitElement unit = system.battleLines[system.frontLines[(this.source.ownership + 1) % 2]][i];
						unit.Stifle();
					}
					break;
				case 3:
					foreach (UnitElement unit in system.deployQueue)
					{
						if (unit.ownership != this.source.ownership && unit.state == ElementState.inBattleLine)
						{
							unit.Stifle();
						}
					}
					break;
			}
		}
		internal void AOEAttackCounterDecrease(BattleElement source, BattleSystem system)
		{
			int argsNum = 2;



			int position = ((List<int>)argsTable["AOEAttackCounterDecrease"])[0];
			int decrease = ((List<int>)argsTable["AOEAttackCounterDecrease"])[1];


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
					foreach (UnitElement unit in system.deployQueue)
					{
						if (unit.ownership == this.source.ownership && unit.state == ElementState.inBattleLine && unit.battleLine.index == system.frontLines[this.source.ownership])
						{
							unit.dynAttackCounter -= decrease;
							unit.UpdateInfo();
						}
					}
					break;
				//3：全图
				case 3:
					break;
			}
		}
		internal void AOEResetAttackCounter(BattleElement source, BattleSystem system)
		{
			int position = ((List<int>)argsTable["AOEResetAttackCounter"])[0];

			BattleLine line;
			switch (position)
			{
				//0：支援战线
				case 0:
					line = system.battleLines[system.supportLines[this.source.ownership]];
					for (int i = 0; i < line.count; i++)
					{
						line[i].dynAttackCounter = line[i].oriAttackCounter;
						line[i].UpdateInfo();
					}
					break;
				//1：当前战线
				case 1:
					UnitElement element = this.source as UnitElement;

					for (int i = 0; i < element.battleLine.count; i++)
					{
						element.battleLine[i].dynAttackCounter = element.battleLine[i].oriAttackCounter;
						element.battleLine[i].UpdateInfo();
					}
					break;
				case 2:
					line = system.battleLines[system.frontLines[this.source.ownership]];
					for (int i = 0; i < line.count; i++)
					{
						line[i].dynAttackCounter = line[i].oriAttackCounter;
						line[i].UpdateInfo();
					}
					break;
			}
		}










		internal void DamageAll(BattleElement source, BattleSystem system)
		{
			int damage = ((List<int>)argsTable["DamageAll"])[0];
			bool first = true;
			foreach (UnitElement unit in system.deployQueue)
			{
				if(unit.state == ElementState.inBattleLine)
				{
					unit.Damaged(damage, first ? "append" : "immediate");
					first = false;
				}
			}
		}
		internal void RecoverAll(BattleElement source, BattleSystem system)
		{
			int heal = ((List<int>)argsTable["RecoverAll"])[0];
			foreach (UnitElement unit in system.deployQueue)
			{
				if(unit.state == ElementState.inBattleLine)
				{
					unit.Recover(heal, "immediate");
				}
			}
		}
		internal void RecoverBase(BattleElement source, BattleSystem system)
		{
			int argsNum = 1;

			int value = ((List<int>)argsTable["RecoverBase"])[0];

			system.bases[BattleSystem.TURN].Recover(value, "append");
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
					target = system.RandomTarget(this.source.ownership);
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


			int atkGain = ((List<int>)argsTable["AuraConstantUnitGain"])[0];
			int mhpGain = ((List<int>)argsTable["AuraConstantUnitGain"])[1];

			for(int i = 0; i < system.deployQueue.Count; i++)
			{
				UnitElement element = system.deployQueue[i];
				if(element.ownership == publisher.ownership && element.state == ElementState.inBattleLine)
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
		internal void AuraDamageDeployed(BattleElement target, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			if (!publisher.aura)
			{
				return;
			}


			int damage = ((List<int>)argsTable["AuraDamageDeployed"])[0];

			if (target.ownership != publisher.ownership)
			{
				UnitElement unit = target as UnitElement;
				unit.Damaged(damage, "append");
			}
		}
		internal void AuraSelfAttackGainByID(BattleElement target, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			if (!publisher.aura)
			{
				return;
			}

			//第一个参数是对象ID数值域
			string ID = ((List<int>)argsTable["AuraSelfAttackGainByID"])[0] < 10
				? "0" + ((List<int>)argsTable["AuraSelfAttackGainByID"])[0].ToString() : ((List<int>)argsTable["AuraSelfAttackGainByID"])[0].ToString();
			//TODO
			ID = publisher.ownership == 0 ? "human_" + ID : "mush_" + ID;
			//第二个参数是增益数值
			int atkGain = ((List<int>)argsTable["AuraSelfAttackGainByID"])[1];

			if (system.UnitIDDic.ContainsKey(ID))
			{
				int num = 0;
				for (int i = 0; i < system.UnitIDDic[ID].Count; i++)
				{
					if (system.UnitIDDic[ID][i].state == ElementState.inBattleLine) num++;
				}
				if (!publisher.attackGain.ContainsKey(publisher.battleID))
				{
					publisher.attackGain.Add(publisher.battleID, atkGain * num);
				}
				else
				{
					publisher.attackGain[publisher.battleID] = atkGain * num;
				}
				publisher.UpdateInfo();
			}

		}
		internal void AuraConstantSelfAttackGainByID(BattleElement target, BattleSystem system)
		{
			UnitElement publisher = this.source as UnitElement;
			if (!publisher.aura)
			{
				return;
			}

			//第一个参数是对象ID数值域
			string ID = ((List<int>)argsTable["AuraConstantSelfAttackGainByID"])[0] < 10
				? "0" + ((List<int>)argsTable["AuraConstantSelfAttackGainByID"])[0].ToString() : ((List<int>)argsTable["AuraConstantSelfAttackGainByID"])[0].ToString();
			//TODO
			ID = publisher.ownership == 0 ? "human_" + ID : "mush_" + ID;
			//第二个参数是增益数值
			int atkGain = ((List<int>)argsTable["AuraConstantSelfAttackGainByID"])[1];

			if (system.UnitIDDic.ContainsKey(ID))
			{
				int num = 0;
				for(int i = 0; i < system.UnitIDDic[ID].Count; i++)
				{
					if (system.UnitIDDic[ID][i].state == ElementState.inBattleLine) num++;
				}
				if (!publisher.attackGain.ContainsKey(publisher.battleID))
				{
					publisher.attackGain.Add(publisher.battleID, atkGain * num);
				}
				else
				{
					publisher.attackGain[publisher.battleID] = atkGain * num;
				}
				publisher.UpdateInfo();
			}
		}
        internal void AuraSelfGainByID(BattleElement target, BattleSystem system)
        {
            UnitElement publisher = this.source as UnitElement;
            if (!publisher.aura)
            {
                return;
            }

            //第一个参数是对象ID数值域
            string ID = ((List<int>)argsTable["AuraSelfGainByID"])[0] < 10
                ? "0" + ((List<int>)argsTable["AuraSelfGainByID"])[0].ToString() : ((List<int>)argsTable["AuraSelfGainByID"])[0].ToString();
            //TODO
            ID = publisher.ownership == 0 ? "human_" + ID : "mush_" + ID;
            //第二个参数是增益数值
            int atkGain = ((List<int>)argsTable["AuraSelfGainByID"])[1];
			int mhpGain = ((List<int>)argsTable["AuraSelfGainByID"])[2];

            if (system.UnitIDDic.ContainsKey(ID))
            {
                int num = 0;
                for (int i = 0; i < system.UnitIDDic[ID].Count; i++)
                {
                    if (system.UnitIDDic[ID][i].state == ElementState.inBattleLine) num++;
                }
                if (!publisher.attackGain.ContainsKey(publisher.battleID))
                {
                    publisher.attackGain.Add(publisher.battleID, atkGain * num);
                }
                else
                {
                    publisher.attackGain[publisher.battleID] = atkGain * num;
                }
				if (!publisher.maxHealthGain.ContainsKey(publisher.battleID))
				{
					publisher.maxHealthGain.Add(publisher.battleID, mhpGain * num);
                    publisher.dynHealth += mhpGain * num;
                }
				else
				{
                    publisher.maxHealthGain[publisher.battleID] = mhpGain * num;
                    publisher.dynHealth += mhpGain * num;
                }
                publisher.UpdateInfo();
				publisher.UpdateHealth();
            }

        }





		internal void CheckOutRemoteTarget(BattleElement source, BattleSystem system)
		{
			ArtilleryElement publisher = this.source as ArtilleryElement;


			int method = ((List<int>)argsTable["CheckOutRemoteTarget"])[0];

			switch (method)
			{
				//生命值最高的单位
				case 0:
					publisher.remoteTarget = system.HighestHealthTarget(publisher.ownership);
					break;
				//生命值最低的单位
				case 1:
					publisher.remoteTarget = system.LowestHealthTarget(publisher.ownership);
					break;
			}
		}





		internal void AttackAndStifle(BattleElement source, BattleSystem system)
		{
			ArtilleryElement publisher = this.source as ArtilleryElement;

			if(publisher.remoteTarget != null)
			{
				publisher.remoteTarget.Stifle();
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

					if (comm == null) return;
					comm.dynDurability += recover;
					comm.UpdateInfo();

					system.handicaps[BattleSystem.TURN].Push(comm, "immediate", i + 1 - num);
				}
			}
		}












		internal void Comm_Mush_07(BattleElement source, BattleSystem system)
		{
			CommandElement publisher = this.source as CommandElement;

            int category = 0;
			int num = 3;
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
				UnitElement unit = new UnitElement(card, system,
					system.stacks[1].controller.InstantiateUnitElementInBattle());

				if (system.handicaps[BattleSystem.TURN].count < system.handicaps[BattleSystem.TURN].capacity)
				{
					system.handicaps[BattleSystem.TURN].Push(unit, "immediate", 0);
				}
			}
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
					system.handicaps[BattleSystem.TURN].Push(unit, "immediate", 0);
				}
			}
		}
		internal void Comm_Mush_13(BattleElement nullRefer, BattleSystem system)
		{
			CommandElement element = this.source as CommandElement;
			for (int i = 0; i < 3; i++)
			{
				if (system.handicaps[BattleSystem.TURN].count < system.handicaps[BattleSystem.TURN].capacity)
				{
					BattleElement e = system.stacks[BattleSystem.TURN].RandomPop();
					system.handicaps[BattleSystem.TURN].Push(e, "immediate", 0);
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
				UnitCard card = system.pool.GetCardByID("mush_100") as UnitCard;
				UnitElement unit = new GuardianElement(card, system,
					system.controller.InstantiateUnitInBattleField(element.ownership, line.index, resIdx));
				system.bases[1] = unit;
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
				{"Thorn", (BattleEventHandler)Thorn },
				{"ThornOnEnable", (BattleEventHandler)ThornOnEnable },
				{"Parry", (BattleEventHandler)Parry },
				{"ParryOnEnable", (BattleEventHandler)ParryOnEnable },
				{"ParryUnload", (BattleEventHandler)ParryUnload },

				{"AttackCounterDecrease", (BattleEventHandler)AttackCounterDecrease },
				{"DrawCardsRandom", (BattleEventHandler)DrawCardsRandom },
				{"RandomDamage", (BattleEventHandler)RandomDamage },
				{"RandomRecoverDamaged", (BattleEventHandler)RandomRecoverDamaged },
				{"RecoverAdjacent", (BattleEventHandler)RecoverAdjacent },
				{"DamageAdjacent", (BattleEventHandler)DamageAdjacent },
				{"RecruitByID", (BattleEventHandler)RecruitByID },
				{"RecruitByCategory", (BattleEventHandler)RecruitByCategory },
				{"RecruitByCost", (BattleEventHandler)RecruitByCost },
				{"SummonToken", (BattleEventHandler)SummonToken },

				{"TokenGain", (BattleEventHandler)TokenGain },
				{"TargetDamage", (BattleEventHandler)TargetDamage },
				{"TargetRetreat", (BattleEventHandler)TargetRetreat },
				{"TargetRecover", (BattleEventHandler)TargetRecover },
				{"TargetGain", (BattleEventHandler)TargetGain },
				{"TargetSetParry", (BattleEventHandler)TargetSetParry },
				{"TargetTerminateAlly", (BattleEventHandler)TargetTerminateAlly },

				{"AOEDamage", (BattleEventHandler)AOEDamage },
				{"AOERetreat", (BattleEventHandler)AOERetreat },
				{"AOERecover", (BattleEventHandler)AOERecover },
				{"AOEStifle", (BattleEventHandler)AOEStifle },
				{"AOEAttackCounterDecrease", (BattleEventHandler)AOEAttackCounterDecrease },
				{"AOEResetAttackCounter", (BattleEventHandler)AOEResetAttackCounter },

				{"RecoverBase", (BattleEventHandler)RecoverBase },

				{"DamageAll", (BattleEventHandler)DamageAll },
				{"RecoverAll", (BattleEventHandler)RecoverAll },
				{"DrawCommandCardsRandomAndRecover", (BattleEventHandler)DrawCommandCardsRandomAndRecover },
				{"AuraConstantUnitGain", (BattleEventHandler)AuraConstantUnitGain },
				{"AuraConstantSelfAttackGainByID", (BattleEventHandler)AuraConstantSelfAttackGainByID },
				{"AuraSelfGainByID", (BattleEventHandler)AuraSelfGainByID },
				{"SummonTokenAndGain", (BattleEventHandler)SummonTokenAndGain },

				{"StrangeGrowth", (BattleEventHandler)StrangeGrowth },
				{"StrangeGrowthTutorial", (BattleEventHandler)StrangeGrowthTutorial },

				{"GetExtraEnergy", (BattleEventHandler)GetExtraEnergy },
				{"DrawExtraHandicaps", (BattleEventHandler)DrawExtraHandicaps },
				{"DecreaseHandicapsCost", (BattleEventHandler)DecreaseHandicapsCost },

				{"CheckOutRemoteTarget", (BattleEventHandler)CheckOutRemoteTarget },

				{"AttackAndStifle", (BattleEventHandler)AttackAndStifle },

				//aura
				{"Aura", (BattleEventHandler)Aura },
				{"AuraUnload", (BattleEventHandler)AuraUnload },
				{"AuraDisable", (BattleEventHandler)AuraDisable },

				{"AuraRandomDamage", (BattleEventHandler)AuraRandomDamage },
				{"AuraAttackCounterDecrease", (BattleEventHandler)AuraAttackCounterDecrease },
				{"AuraUnitGain", (BattleEventHandler)AuraUnitGain },
				{"AuraDoubleRecover", (BattleEventHandler)AuraDoubleRecover },
				{"AuraBaseImmunity", (BattleEventHandler)AuraBaseImmunity },
				{"AuraSelfAttackGainByID", (BattleEventHandler)AuraSelfAttackGainByID },
				{"AuraDamageDeployed", (BattleEventHandler)AuraDamageDeployed },

				//command
				{"Comm_Mush_07", (BattleEventHandler)Comm_Mush_07 },
				{"Comm_Mush_08", (BattleEventHandler)Comm_Mush_08 },
				{"Comm_Mush_13", (BattleEventHandler)Comm_Mush_13 },
				{"Comm_Mush_18", (BattleEventHandler)Comm_Mush_18 },
			};

			//如果需要参数，请在这里注册
			argsTable = new Hashtable()
			{
				//basic
				{"Assault", null },
				{"AssaultOnEnable", null },
				{"Raid", null },
				{"RaidOnEnable", null },
				{"Cleave", null },
				{"CleaveOnEnable", null },
				{"Armor", null },
				{"ArmorOnEnable", null },
				{"Thorn", null },
				{"ThornOnEnable", null },
				{"Parry", null },
				{"ParryOnEnable", null },
				{"ParryUnload", null },

				{"AttackCounterDecrease", null },
				{"DrawCardsRandom", null },
				{"RandomDamage", null },
				{"RandomRecoverDamaged", null },
				{"RecoverAdjacent", null },
				{"DamageAdjacent", null },
				{"RecruitByID", null },
				{"RecruitByCategory", null },
				{"RecruitByCost", null },
				{"SummonToken", null },

				{"TokenGain", null },
				{"TargetDamage", null },
				{"TargetRetreat", null },
				{"TargetRecover", null },
				{"TargetGain", null },
				{"TargetSetParry", null },
				{"TargetTerminateAlly", null },

				{"AOEDamage", null },
				{"AOERetreat", null },
				{"AOERecover", null },
				{"AOEStifle", null },
				{"AOEAttackCounterDecrease", null },
				{"AOEResetAttackCounter", null },

				{"RecoverBase", null },

				{"DamageAll", null },
				{"RecoverAll", null },
				{"DrawCommandCardsRandomAndRecover", null },
				{"AuraConstantUnitGain", null },
				{"AuraConstantSelfAttackGainByID", null },
				{"AuraSelfGainByID", null },
				{"SummonTokenAndGain", null },

				{"StrangeGrowth", null },
				{"StrangeGrowthTutorial", null },

				{"GetExtraEnergy", null },
				{"DrawExtraHandicaps", null },
				{"DecreaseHandicapsCost", null },

				{"CheckOutRemoteTarget", null },

				{"AttackAndStifle", null },

				//aura
				{"Aura", null },
				{"AuraUnload", null },
				{"AuraDisable", null },

				{"AuraRandomDamage", null },
				{"AuraAttackCounterDecrease", null },
				{"AuraUnitGain", null },
				{"AuraDoubleRecover", null },
				{"AuraBaseImmunity", null },
				{"AuraSelfAttackGainByID", null },
				{"AuraDamageDeployed", null },

				//command
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