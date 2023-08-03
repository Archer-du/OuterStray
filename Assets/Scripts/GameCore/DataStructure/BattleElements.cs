using System;
using System.Collections;
using System.Collections.Generic;
using DataCore.BattleItems;
using DataCore.Cards;
using DataCore.StructClass;
using DisplayInterface;
using LogicCore;
using EventEffectModels;
using InputHandler;
using Codice.CM.Common;
using CodiceApp.EventTracking.Plastic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace DataCore.BattleElements
{
	/// <summary>
	/// battle element: actual operator during a battle
	/// </summary>
	internal abstract class BattleElement
	{


		/// <summary>
		/// 指向作战系统，获取全局信息
		/// </summary>
		internal BattleSystem battleSystem;
		/// <summary>
		/// 个体事件系统
		/// </summary>
		internal EventTable eventTable;
		/// <summary>
		/// 效果表(CRITICAL)
		/// </summary>
		protected EffectsTable effectsTable;



		internal ElementState state;
		/// <summary>
		/// 加载的静态卡牌信息
		/// </summary>
		internal Card card { get; private set; }

		/// <summary>
		/// 后端ID，用于索引渲染层信息
		/// </summary>
		internal string backendID { get; private set; }
		/// <summary>
		/// 卡牌名称
		/// </summary>
		internal string name { get; set; }
		/// <summary>
		/// 卡牌效果描述
		/// </summary>
		internal string description { get; set; }
		/// <summary>
		/// 费用
		/// </summary>
		internal int cost { get; set; }

		/// <summary>
		/// 绝对归属权
		/// </summary>
		internal int ownership;
		internal int stackIdx;


		internal BattleElement(Card __card, BattleSystem system)
		{
			battleSystem = system;
			//初始化事件系统和效果表
			eventTable = new EventTable();
			effectsTable = new EffectsTable();

			this.card = __card;
			this.backendID = __card.backendID;
			this.name = __card.name;
			this.description = __card.description;
			this.cost = __card.cost;
			this.ownership = __card.ownership;
			//TODO 维护
			this.stackIdx = -1;


			EffectsParse(__card.effects);
		}

		protected void EffectsParse(string effects)
		{
			if (effects != "none")
			{
				//复数效果分离
				string[] effect = effects.Split('|');

				foreach (string s in effect)
				{
					string prefabs = s;
					switch (prefabs)
					{
						case "Raid":
							prefabs = "<self:Initialize+Raid-0>/<self:AfterDeploy+RaidOnEnable-0>";
							break;
						case "Assault":
							prefabs = "<self:Initialize+Assault-0>/<self:AfterDeploy+AssaultOnEnable-0>";
							break;
						case "Cleave":
							prefabs = "<self:Initialize+Cleave-0>/<self:BeforeAttack+CleaveOnEnable-0>";
							break;
					}
					//将触发块与解除块分离
					string[] trigger = prefabs.Split('/');

					//遍历触发块与解除块
					foreach (string block in trigger)
					{
						//将事件与委托分离
						string[] tuple = block.Trim('<', '>').Split('+');

						if (tuple[0] == "none") break;

						//分离委托及其参数
						string[] triggerDelegate = tuple[1].Split('-');

						//检查委托注册状态
						BattleEventHandler handler = effectsTable.GetHandler(triggerDelegate[0]);

						//初始化委托参数列表
						List<int> argList = new List<int>();
						for (int i = 0; i < int.Parse(triggerDelegate[1]); i++)
						{
							argList.Add(int.Parse(triggerDelegate[i + 2]));
						}
						//注册委托参数
						effectsTable.RegisterArgs(triggerDelegate[0], argList);



						//解析分离 事件
						string[] triggerEvent = tuple[0].Split(':');

						//如果为自体触发事件
						if (triggerEvent[0] == "self")
						{
							//委托订阅对应事件
							eventTable.RegisterHandler(triggerEvent[1], handler);
						}
						//如果是全局事件
						else if (triggerEvent[0] == "global")
						{
							//敌方触发还是我方触发
							if (triggerEvent[1] == "ally")
							{
								//委托订阅对应事件
								battleSystem.eventTable[this.ownership].RegisterHandler(triggerEvent[2], handler);
							}
							else if (triggerEvent[1] == "enemy")
							{
								//委托订阅对应事件
								battleSystem.eventTable[(this.ownership + 1) % 2].RegisterHandler(triggerEvent[2], handler);
							}
							else if (triggerEvent[1] == "all")
							{
								battleSystem.eventTable[this.ownership].RegisterHandler(triggerEvent[2], handler);
								battleSystem.eventTable[(this.ownership + 1) % 2].RegisterHandler(triggerEvent[2], handler);
							}
							else
							{
								throw new Exception("invalid effects format");
							}
						}
						else
						{
							throw new Exception("invalid effects format");
						}

					}
				}
			}
		}
	}




	//CRITICAL!!
	internal class UnitElement : BattleElement, IUnitInput
	{

		/// <summary>
		/// 渲染层控件
		/// </summary>
		internal IUnitElementController controller;

		/// <summary>
		/// 战场中不会变化的唯一标识编号
		/// </summary>
		internal int battleID;
		/// <summary>
		/// 当前所在战线
		/// </summary>
		internal BattleLine battleLine;
		/// <summary>
		/// 所在战线指针
		/// </summary>
		internal int lineIdx;
		/// <summary>
		/// 战线内索引值
		/// </summary>
		internal int inlineIdx;




		/// <summary>
		/// 兵种
		/// </summary>
		internal string category { get; private set; }




		/// <summary>
		/// 原始攻击力
		/// </summary>
		internal int oriAttack { get; private set; }
		/// <summary>
		/// 动态攻击力
		/// </summary>
		internal int DynAttack;
		internal int dynAttack
		{
			get
			{
				int gainSum = 0;
				foreach (KeyValuePair<int, int> entry in attackGain)
				{
					gainSum += entry.Value;
				}
				return DynAttack + gainSum;
			}
			set { DynAttack = value; }
		}



		/// <summary>
		/// 原始攻击计数器最大值
		/// </summary>
		internal int oriAttackCounter { get; private set; }
		/// <summary>
		/// 动态攻击计数器
		/// </summary>
		private int DynAttackCounter;
		internal int dynAttackCounter
		{
			get { return DynAttackCounter; }
			set
			{
				DynAttackCounter = value;
				if(value < 0) DynAttackCounter = 0;
			}
		}




		/// <summary>
		/// 原始最大生命值
		/// </summary>
		internal int oriHealth { get; private set; }
		/// <summary>
		/// 动态生命值
		/// </summary>
		private int DynHealth;
		internal int dynHealth
		{
			get { return DynHealth; }
			set
			{
				DynHealth = value;
				if(value < 0) DynHealth = 0;
				if(value > MaxHealth) DynHealth = MaxHealth;
			}
		}
		/// <summary>
		/// 动态最大生命值
		/// </summary>
		private int MaxHealth;
		internal int maxHealth
		{
			get { return MaxHealth; }
			set
			{
				DynHealth += value - MaxHealth;
				MaxHealth = value;
			}
		}


		/// <summary>
		/// 记源攻击力增益
		/// </summary>
		internal Dictionary<int, int> attackGain;
		/// <summary>
		/// 记源最大生命值增益
		/// </summary>
		internal Dictionary<int, int> maxHealthGain;


		/// <summary>
		/// 治疗量寄存器
		/// </summary>
		internal int recover;
		/// <summary>
		/// 伤害量寄存器
		/// </summary>
		internal int damage;



		/// <summary>
		/// 攻击范围，长度为3
		/// </summary>
		internal UnitElement[] attackRange;
		/// <summary>
		/// 目标寄存器
		/// </summary>
		internal UnitElement target;
		/// <summary>
		/// 攻击范围目标索引值(0 ~ 2)
		/// </summary>
		internal int targetIdx;



		/// <summary>
		/// 操作计数器，为1时表示未被操作
		/// </summary>
		internal int OperateCounter;
		internal int operateCounter
		{
			get { return OperateCounter; }
			set
			{
				OperateCounter = value > 1 ? 1 : value;
			}
		}


		//legacy
		/// <summary>
		/// 嘲讽(重装)
		/// </summary>
		internal bool mocking;



		
		//附加属性
		/// <summary>
		/// 移动范围 default: 1
		/// </summary>
		internal int moveRange;



		//std状态
		/// <summary>
		/// 格挡状态
		/// </summary>
		internal bool parry;
		/// <summary>
		/// 顺劈状态
		/// </summary>
		internal bool cleave;
		internal bool assault;
		internal bool raid;
		internal int armor;
		/// <summary>
		/// 可选
		/// </summary>
		internal bool selectable;
		/// <summary>
		/// 反甲
		/// </summary>
		internal bool thorn;



		internal bool aura;
		internal int range;
		internal int type;
		internal int value;



		internal UnitElement(UnitCard __card, BattleSystem system) : base(__card, system)
		{
			//初始状态在卡组中
			state = ElementState.inDeck;

			//从卡牌读取原始数据
			this.category = __card.category;
			this.oriAttack = __card.attackPoint;
			this.oriHealth = __card.healthPoint;
			this.oriAttackCounter = __card.attackCounter;

			//初始化动态数据
			this.DynAttack = __card.attackPoint;
			this.MaxHealth = __card.healthPoint;
			this.DynHealth = __card.healthPoint;
			this.DynAttackCounter = __card.attackCounter;
			this.operateCounter = 1;

			//初始化攻击范围和攻击目标
			this.attackRange = new UnitElement[3] { null, null, null };
			this.targetIdx = -1;
			this.target = null;

			//寄存器初始化
			attackGain = new Dictionary<int, int>();
			maxHealthGain = new Dictionary<int, int>();

			this.recover = 0;
			this.damage = 0;



			//std attribute & status
			//内嵌逻辑特性
			this.mocking = false;
			this.moveRange = 1;

			parry = false;
			cleave = false;
			this.armor = 0;

			aura = false;

			eventTable.RaiseEvent("Initialize", this, null);
		}



		///// <summary>
		///// 嘲讽状态
		///// </summary>
		//internal bool mocking;
		///// <summary>
		///// 不屈状态
		///// </summary>
		//internal bool unyielding;
		///// <summary>
		///// 突击状态(可底层化)
		///// </summary>
		//internal bool assault;

		internal List<string> FXlist;

		/// <summary>
		/// 设置攻击范围（系统更新）
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="t2"></param>
		/// <param name="t3"></param>
		internal virtual void SetAttackRange(UnitElement t1, UnitElement t2, UnitElement t3)
		{
			attackRange[0] = t1;
			attackRange[1] = t2;
			attackRange[2] = t3;
			UpdateTarget();
		}
		/// <summary>
		/// 根据一定策略，更新即将攻击的目标（系统更新）
		/// </summary>
		/// <returns></returns>
		protected void UpdateTarget()
		{
			if (attackRange[1] != null)
			{
				target = attackRange[1];
				targetIdx = 1;
			}
			else
			{
				if (attackRange[0] == null)
				{
					if (attackRange[2] == null)
					{
						this.target = null;
						this.targetIdx = -1;
					}
                    else
                    {
						this.target = attackRange[2];
						this.targetIdx = 2;
					}
                }
				else
				{
					if (attackRange[2] == null)
					{
						this.target = attackRange[0];
						this.targetIdx = 0;
					}
					else
					{
						this.target = (attackRange[0].dynHealth < attackRange[2].dynHealth) ? attackRange[0] : attackRange[2];
						this.targetIdx = (attackRange[0].dynHealth < attackRange[2].dynHealth) ? 0 : 2;
					}
				}
			}

			//顺序不能变
			if (attackRange[0] != null && attackRange[0].category == "Guardian")
			{
				this.target = attackRange[0];
				this.targetIdx = 0;
			}
			if (attackRange[2] != null && attackRange[2].category == "Guardian")
			{
				this.target = attackRange[2];
				this.targetIdx = 2;
			}
			if (attackRange[1] != null && attackRange[1].category == "Guardian")
			{
				this.target = attackRange[1];
				this.targetIdx = 1;
			}


			controller.UpdateTarget(attackRange[0]?.controller, attackRange[1]?.controller, attackRange[2]?.controller, target?.controller, targetIdx);
			//if (attackRange[0] == null)
			//{
			//	if (attackRange[1] == null)
			//	{
			//		if (attackRange[2] == null)
			//		{
			//			this.target = null;
			//			this.targetIdx = -1;
			//		}
			//		else
			//		{
			//			this.target = attackRange[2];
			//			this.targetIdx = 2;
			//		}
			//	}
			//	else
			//	{
			//		if (attackRange[2] == null)
			//		{
			//			this.target = attackRange[1];
			//			this.targetIdx = 1;
			//		}
			//		else
			//		{
			//			this.target = (attackRange[1].dynHealth < attackRange[2].dynHealth) ? attackRange[1] : attackRange[2];
			//			this.targetIdx = (attackRange[1].dynHealth < attackRange[2].dynHealth) ? 1 : 2;
			//		}
			//	}
			//}
			//else
			//{
			//	if (attackRange[1] == null)
			//	{
			//		if (attackRange[2] == null)
			//		{
			//			this.target = attackRange[0];
			//			this.targetIdx = 0;
			//		}
			//		else
			//		{
			//			this.target = (attackRange[0].dynHealth < attackRange[2].dynHealth) ? attackRange[0] : attackRange[2];
			//			this.targetIdx = (attackRange[0].dynHealth < attackRange[2].dynHealth) ? 0 : 2;
			//		}
			//	}
			//	else
			//	{
			//		if (attackRange[2] == null)
			//		{
			//			this.target = (attackRange[0].dynHealth < attackRange[1].dynHealth) ? attackRange[0] : attackRange[1];
			//			this.targetIdx = (attackRange[0].dynHealth < attackRange[1].dynHealth) ? 0 : 1;
			//		}
			//		else
			//		{
			//			this.target = (attackRange[1].dynHealth < attackRange[2].dynHealth) ? attackRange[1] : attackRange[2];
			//			this.targetIdx = (attackRange[1].dynHealth < attackRange[2].dynHealth) ? 1 : 2;
			//			this.target = (attackRange[0].dynHealth < target.dynHealth) ? attackRange[0] : target;
			//			this.targetIdx = (attackRange[0].dynHealth < target.dynHealth) ? 0 : targetIdx;
			//		}
			//	}
			//}
			//controller.UpdateTarget(attackRange[0]?.controller, attackRange[1]?.controller, attackRange[2]?.controller, target?.controller, targetIdx);
		}
		/// <summary>
		/// 回合结束结算攻击，回复操作数（系统更新）
		/// </summary>
		internal void RotateSettlement()
		{
			eventTable.RaiseEvent("RotateSettlement", this, battleSystem);

			if(this.operateCounter > 0)
			{
				if(this.ownership == BattleSystem.TURN)
				{
					this.dynAttackCounter -= 1;
				}
			}
			//操作计数回复
			this.operateCounter++;

			//自动更新
			Settlement();
		}
		/// <summary>
		/// 操作中结算攻击，恢复操作数（系统更新）
		/// </summary>
		internal virtual void Settlement()
		{
			this.UpdateInfo();

			if (this.dynAttackCounter <= 0)
			{
				int result = -1;
				result = Attack();
				if (result > 0)
				{
					this.dynAttackCounter = this.oriAttackCounter;
				}
			}
		}







		/// <summary>
		/// 
		/// </summary>
		/// <param name="battleSystem"></param>
		internal void Deploy(BattleSystem battleSystem, BattleLine dstLine, int dstPos)
		{
			eventTable.RaiseEvent("BeforeDeploy", this, battleSystem);

			//加入部署队列
			this.battleID = battleSystem.deployQueue.Count;
			battleSystem.deployQueue.Add(this);
			//加入ID字典
			if (!battleSystem.UnitIDDic.ContainsKey(this.backendID))
			{
				battleSystem.UnitIDDic.Add(this.backendID, new List<UnitElement>());
			}
			battleSystem.UnitIDDic[this.backendID].Add(this);

			//战线接收
			dstLine.Receive(this, dstPos);
			this.battleSystem = battleSystem;
			this.operateCounter--;

			eventTable.RaiseEvent("AfterDeploy", this, battleSystem);

			UpdateInfo();
		}
		/// <summary>
		/// 主动移动，消耗行动次数
		/// </summary>
		internal virtual void Move(BattleLine resLine, BattleLine dstLine, int resIdx, int dstPos)
		{
			eventTable.RaiseEvent("BeforeMove", this, battleSystem);


			dstLine.Receive(resLine.Send(resIdx), dstPos);
			this.operateCounter--;

			UpdateInfo();

			eventTable.RaiseEvent("AfterMove", this, battleSystem);
		}
		/// <summary>
		/// 强制移动，不消耗移动次数
		/// </summary>
		internal void ForceMove(BattleLine dstLine)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// 攻击方法: 
		/// </summary>
		internal virtual int Attack()
		{
			if (target == null) return -1;


			eventTable.RaiseEvent("BeforeAttack", this, battleSystem);

			//顺劈状态会锁死默认攻击方式
			if (cleave) return 1;

			controller.AttackAnimationEvent(target.inlineIdx, target.battleLine.count);
			this.target.Attacked(this);


			eventTable.RaiseEvent("AfterAttack", this, battleSystem);

			return 1;

		}

		/// <summary>
		/// 有来源受击方法: 根据伤害源atk修改damage寄存值
		/// </summary>
		/// <param name="source"></param>
		internal void Attacked(UnitElement source)
		{
			this.damage = source.dynAttack;

			eventTable.RaiseEvent("BeforeAttacked", this, battleSystem);

			Damaged("append");

			eventTable.RaiseEvent("AfterAttacked", this, battleSystem);
		}
		/// <summary>
		/// 无来源受伤方法: 根据damage寄存值受伤
		/// </summary>
		internal void Damaged(string method)
		{
			eventTable.RaiseEvent("BeforeDamaged", this, battleSystem);
			if(this.dynHealth == this.maxHealth)
			{
				eventTable.RaiseEvent("Meticulous", this, battleSystem);
			}

			this.dynHealth -= this.damage;
			controller.DamageAnimationEvent(this.dynHealth, method);

			this.damage = 0;


			if (this.dynHealth <= 0)
			{
				Terminate();
			}

			eventTable.RaiseEvent("AfterDamaged", this, battleSystem);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="damage"></param>
		internal void Damaged(int damage, string method)
		{
			this.damage = damage;

			eventTable.RaiseEvent("BeforeDamaged", this, battleSystem);
			if (this.dynHealth == this.maxHealth)
			{
				eventTable.RaiseEvent("Meticulous", this, battleSystem);
			}

			this.dynHealth -= this.damage;
			controller.DamageAnimationEvent(this.dynHealth, method);

			this.damage = 0;

			if (this.dynHealth <= 0)
			{
				Terminate();
			}

			eventTable.RaiseEvent("AfterDamaged", this, battleSystem);
		}
		internal void Recover(int heal)
		{
			this.recover = heal;
			eventTable.RaiseEvent("BeforeRecover", this, battleSystem);
			battleSystem.eventTable[ownership].RaiseEvent("UnitHealed", this, battleSystem);

			this.dynHealth += recover;
			this.recover = 0;
		}
		/// <summary>
		/// 死亡
		/// </summary>
		internal void Terminate()
		{
			eventTable.RaiseEvent("BeforeTerminate", this, battleSystem);

			//由自己修改的状态
			this.state = ElementState.destroyed;
			UnloadEffects();


			battleLine.ElementRemove(inlineIdx);
			controller.TerminateAnimationEvent("append");


			//not likely
			eventTable.RaiseEvent("AfterTerminate", this, battleSystem);
		}
		/// <summary>
		/// 撤退回到手牌区,回复状态
		/// </summary>
		internal void Retreat(string method)
		{
			battleLine.ElementRemove(inlineIdx);

			battleLine.Send(this.inlineIdx);
			//TODO config
			this.dynHealth += 2;
			this.operateCounter = 1;
			UnloadEffects();


			controller.RetreatAnimationEvent(method);
		}





		//死亡和战斗结束时调用
		private void UnloadEffects()
		{
			//初始化攻击范围和攻击目标
			attackRange[0] = null;
			attackRange[1] = null;
			attackRange[2] = null;
			this.targetIdx = -1;
			this.target = null;

			//寄存器初始化
			attackGain.Clear();
			maxHealthGain.Clear();

			this.recover = 0;
			this.damage = 0;

			this.armor = 0;
			this.moveRange = 1;

			parry = false;
			cleave = false;
			mocking = false;

			aura = false;
		}



		internal void AuraGain(BattleElement element, BattleSystem system)
		{
			if (aura)
			{

			}
		}








		//arg: chip TODO
		internal void ChipImplant()
		{
			// modify effect table
		}







		/// <summary>
		/// 由手牌区初始化
		/// </summary>
		internal void Init()
		{
			controller.Init(backendID, ownership, name, category, description, this);
			UpdateInfo();
		}
		internal void UpdateInfo()
		{
			controller.UpdateInfo(cost, dynAttack, dynHealth, maxHealth, dynAttackCounter, operateCounter, 
				state, moveRange, aura);
		}






		public void UpdateManual()
		{
			UpdateInfo();
		}
		public void UpdateTargetManual()
		{
			battleSystem.UpdateAttackRange();
		}
	}









	internal sealed class LightArmorElement : UnitElement
	{
		internal LightArmorElement(UnitCard __card, BattleSystem system) : base(__card, system) { }
	}
	internal sealed class MotorizedElement : UnitElement
	{
		internal MotorizedElement(UnitCard __card, BattleSystem system) : base(__card, system) { }
		internal override void Move(BattleLine resLine, BattleLine dstLine, int resIdx, int dstPos)
		{
			eventTable.RaiseEvent("BeforeMove", this, battleSystem);


			dstLine.Receive(resLine.Send(resIdx), dstPos);

			this.dynAttackCounter -= 1;
			this.operateCounter--;

			UpdateInfo();

			eventTable.RaiseEvent("AfterMove", this, battleSystem);
		}
	}
	internal sealed class ArtilleryElement : UnitElement
	{
		internal UnitElement tmpTarget;
		internal ArtilleryElement(UnitCard __card, BattleSystem system) : base(__card, system) { }
		internal override void SetAttackRange(UnitElement t1, UnitElement t2, UnitElement t3)
		{
			attackRange[0] = null;
			attackRange[1] = null;
			attackRange[2] = null;
		}
		internal override void Settlement()
		{
			this.UpdateInfo();

			if (this.dynAttackCounter <= 0)
			{
				tmpTarget = battleSystem.RandomEnemy();
				int result = -1;
				result = Attack();
				if (result > 0)
				{
					this.dynAttackCounter = this.oriAttackCounter;
				}
			}
		}
		internal override int Attack()
		{
			if (tmpTarget == null) return -1;

			eventTable.RaiseEvent("BeforeAttack", this, battleSystem);


			controller.RandomAttackAnimationEvent(tmpTarget.controller);
			tmpTarget.Attacked(this);


			eventTable.RaiseEvent("AfterAttack", this, battleSystem);

			return 1;

		}
	}
	internal sealed class GuardianElement : UnitElement
	{
		internal GuardianElement(UnitCard __card, BattleSystem system) : base(__card, system) { }
	}
	internal sealed class ConstructionElement : UnitElement
	{
		internal ConstructionElement(UnitCard __card, BattleSystem system) : base(__card, system) { }
		internal override void Move(BattleLine resLine, BattleLine dstLine, int resIdx, int dstPos)
		{
			return;
		}
	}
	internal sealed class BehemothsElement : UnitElement
	{
		internal BehemothsElement(UnitCard __card, BattleSystem system) : base(__card, system)
		{
		}
	}








	internal sealed class CommandElement : BattleElement
	{
		internal ICommandElementController controller;

		internal int oriDurability { get; set; }
		private int DynDurability;
		internal int dynDurability
		{
			get { return DynDurability; }
			set
			{
				DynDurability = value;
				if (value < 0) DynDurability = 0;
			}
		}


		internal int tempBufferForCommMush07;

		internal CommandElement(CommandCard __card, BattleSystem system) : base(__card, system)
		{
			this.oriDurability = __card.maxDurability;
			this.DynDurability = __card.maxDurability;

			tempBufferForCommMush07 = 0;

			eventTable.RaiseEvent("Initialize", this, null);
		}


		internal void Cast(UnitElement target)
		{
			eventTable.RaiseEvent("Cast", target, null);

			dynDurability -= 1;


			controller.RetreatAnimationEvent("append");
		}
		internal void Recover(int heal)
		{
			dynDurability += heal;
		}

		/// <summary>
		/// 由手牌区初始化
		/// </summary>
		internal void Init()
		{
			controller.Init(backendID, ownership, name, description);
			UpdateInfo();
		}
		internal void UpdateInfo()
		{
			controller.UpdateInfo(cost, dynDurability);
		}
		public void UpdateManual()
		{
			UpdateInfo();
		}
	}





	/// <summary>
	/// realtime status of a battle element
	/// </summary>
	//internal class Status : IComparable<Status>
	//{
	//	internal double remainTime;
	//	internal int CompareTo(Status other)
	//	{
	//		if (other == null) return 1;
	//		return this.remainTime.CompareTo(other.remainTime);
	//	}
	//}




	public enum ElementState
	{
		inDeck,
		inStack,
		inHandicap,
		inBattleLine,
		destroyed
	}
}