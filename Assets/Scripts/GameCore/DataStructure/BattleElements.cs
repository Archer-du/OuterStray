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

namespace DataCore.BattleElements
{
	/// <summary>
	/// battle element: actual operator during a battle
	/// </summary>
	internal abstract class BattleElement
	{
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


		internal BattleElement(Card __card)
		{
			this.card = __card;
			this.backendID = __card.backendID;
			this.name = __card.name;
			this.description = __card.description;
			this.cost = __card.cost;
			this.ownership = __card.ownership;
		}
	}




	//CRITICAL!!
	internal sealed class UnitElement : BattleElement, IUnitInput
	{
		/// <summary>
		/// 渲染层控件
		/// </summary>
		internal IUnitElementController controller;

		/// <summary>
		/// 指向作战系统，获取全局信息
		/// </summary>
		internal BattleSystem battleSystem;
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
		/// 个体事件系统
		/// </summary>
		EventTable eventTable;
		/// <summary>
		/// 效果表(CRITICAL)
		/// </summary>
		EffectsTable effectsTable;

		internal UnitState state;

		//static info
		/// <summary>
		/// 兵种
		/// </summary>
		internal string category { get; private set; }
		/// <summary>
		/// 原始攻击力
		/// </summary>
		internal int oriAttack { get; private set; }
		/// <summary>
		/// 原始最大生命值
		/// </summary>
		internal int oriHealth { get; private set; }

		/// <summary>
		/// 原始攻击计数器最大值
		/// </summary>
		internal int oriAttackCounter { get; private set; }



		//dynamic info
		/// <summary>
		/// 动态攻击力
		/// </summary>
		internal int dynAttack { get; set; }
		/// <summary>
		/// 动态生命值
		/// </summary>
		internal int dynHealth { get; set; }
		/// <summary>
		/// 动态最大生命值
		/// </summary>
		internal int maxHealth { get; set; }
		/// <summary>
		/// 动态攻击计数器
		/// </summary>
		internal int dynAttackCounter;

		/// <summary>
		/// 攻击增益(伤害附加)
		/// </summary>
		internal int dynAttackGain;
		/// <summary>
		/// 最大生命值增益
		/// </summary>
		internal int maxHealthGain;


		/// <summary>
		/// 治疗量寄存器
		/// </summary>
		internal int recover;
		/// <summary>
		/// 伤害量寄存器
		/// </summary>
		internal int damage;
		/// <summary>
		/// 目标寄存器
		/// </summary>
		internal UnitElement target;
		/// <summary>
		/// 攻击范围目标索引值(0 ~ 2)
		/// </summary>
		internal int targetIdx;



		/// <summary>
		/// 攻击范围，长度为3
		/// </summary>
		internal UnitElement[] attackRange;

		/// <summary>
		/// (巨兽特有)身体组件
		/// </summary>
		internal UnitElement[] componentHash;


		/// <summary>
		/// 操作计数器，为1时表示未被操作
		/// </summary>
		internal int operateCounter;


		/// <summary>
		/// 移动时减少的攻击计数 default: 0
		/// </summary>
		internal int counterDecrease;
		/// <summary>
		/// 随机攻击
		/// </summary>
		internal bool randomAttack;
		/// <summary>
		/// 是否可以主动移动
		/// </summary>
		internal bool moveable;
		/// <summary>
		/// 嘲讽(重装)
		/// </summary>
		internal bool mocking;



		
		//附加属性
		/// <summary>
		/// 护甲值 default: 0
		/// </summary>
		internal int armor;
		/// <summary>
		/// 连击值 default: 1
		/// </summary>
		internal int batter;
		/// <summary>
		/// 移动范围 default: 1
		/// </summary>
		internal int moveRange;



		//std状态
		/// <summary>
		/// 无敌
		/// </summary>
		internal bool immunity;
		/// <summary>
		/// 可选
		/// </summary>
		internal bool selectable;
		/// <summary>
		/// 反甲
		/// </summary>
		internal bool thorn;



		internal UnitElement(UnitCard __card) : base(__card)
		{
			//初始化事件系统和效果表
			eventTable = new EventTable();
			effectsTable = new EffectsTable();

			//初始状态在卡组中
			state = UnitState.inDeck;

			//从卡牌读取原始数据
			this.category = __card.category;
			this.oriAttack = __card.attackPoint;
			this.oriHealth = __card.healthPoint;
			this.oriAttackCounter = __card.attackCounter;

			//初始化动态数据
			this.dynAttack = __card.attackPoint;
			this.dynHealth = __card.healthPoint;
			this.maxHealth = __card.healthPoint;
			this.dynAttackCounter = __card.attackCounter;
			this.operateCounter = 1;

			//初始化攻击范围和攻击目标
			this.attackRange = new UnitElement[3] { null, null, null };
			this.targetIdx = -1;
			this.target = null;

			//寄存器初始化
			this.dynAttackGain = 0;
			this.maxHealthGain = 0;
			this.recover = 0;
			this.damage = 0;


			//兵种特性加载
			this.counterDecrease = __card.counterDecrease;
			this.randomAttack = __card.randomAttack;
			this.moveable = __card.moveable;
			this.mocking = __card.mocking;
			//TODO
			this.componentHash = new UnitElement[5];


			//std attribute & status
			//内嵌逻辑特性
			this.armor = 0;
			this.batter = 1;
			this.moveRange = 1;



			//效果形式化解析
			string effects = __card.effects;
			if (effects != "none")
			{
				//复数效果分离
				string[] effect = effects.Split('|');
				foreach (string s in effect)
				{
					//将触发块与解除块分离
					string[] trigger = s.Split('/');

					//遍历触发块与解除块
					for(int j = 0; j < 2; j++)
					{
						//将事件与委托分离
						string[] tuple = trigger[j].Trim('<', '>').Split('+');

						//若无事件(通常只可能没有解除事件)
						if (tuple[0] == "none") break;//TODO 由Unload 统一处理

						//分离委托及其参数
						string[] triggerDelegate = tuple[1].Split('-');

						//检查委托注册状态
						BattleEventHandler handler = effectsTable.GetHandler(triggerDelegate[0]);

						//初始化委托参数列表
						List<int> argList = new List<int>();
						for(int i = 0; i < int.Parse(triggerDelegate[1]); i++)
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

		//TODO
		///// <summary>
		///// 顺劈状态
		///// </summary>
		//internal bool cleave;
		///// <summary>
		///// 嘲讽状态
		///// </summary>
		//internal bool mocking;
		///// <summary>
		///// 荆棘状态
		///// </summary>
		//internal bool thorn;
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
		internal void SetAttackRange(UnitElement t1, UnitElement t2, UnitElement t3)
		{
			//炮兵没有攻击范围
			if (randomAttack)
			{
				attackRange[0] = null;
				attackRange[1] = null;
				attackRange[2] = null;
				return;
			}
			attackRange[0] = t1;
			attackRange[1] = t2;
			attackRange[2] = t3;
			UpdateTarget();
			
		}
		/// <summary>
		/// 根据一定策略，更新即将攻击的目标（系统更新）
		/// </summary>
		/// <returns></returns>
		private void UpdateTarget()
		{
			if (attackRange[0] == null)
			{
				if (attackRange[1] == null)
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
						this.target = attackRange[1];
						this.targetIdx = 1;
					}
					else
					{
						this.target = (attackRange[1].dynHealth < attackRange[2].dynHealth) ? attackRange[1] : attackRange[2];
						this.targetIdx = (attackRange[1].dynHealth < attackRange[2].dynHealth) ? 1 : 2;
					}
				}
			}
			else
			{
				if (attackRange[1] == null)
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
				else
				{
					if (attackRange[2] == null)
					{
						this.target = (attackRange[0].dynHealth < attackRange[1].dynHealth) ? attackRange[0] : attackRange[1];
						this.targetIdx = (attackRange[0].dynHealth < attackRange[1].dynHealth) ? 0 : 1;
					}
					else
					{
						this.target = (attackRange[1].dynHealth < attackRange[2].dynHealth) ? attackRange[1] : attackRange[2];
						this.targetIdx = (attackRange[1].dynHealth < attackRange[2].dynHealth) ? 1 : 2;
						this.target = (attackRange[0].dynHealth < target.dynHealth) ? attackRange[0] : target;
						this.targetIdx = (attackRange[0].dynHealth < target.dynHealth) ? 0 : targetIdx;
					}
				}
			}
			controller.UpdateTarget(attackRange[0]?.controller, attackRange[1]?.controller, attackRange[2]?.controller, target?.controller, targetIdx);
			//if (attackRange[0] != null && attackRange[1] != null)
			//{
			//	this.targetIdx = (attackRange[0].dynHealth < attackRange[1].dynHealth) ? 0 : 1;
			//	this.target = (attackRange[0].dynHealth < attackRange[1].dynHealth) ? attackRange[0] : attackRange[1];
			//}
			//else
			//{
			//	this.targetIdx = (attackRange[0] == null) ? 1 : 0;
			//	this.target = (attackRange[0] == null) ? attackRange[1] : attackRange[0];
			//}

			//if (target == null)
			//{
			//	this.targetIdx = 2;
			//	this.target = attackRange[2];
			//}
			//else if (attackRange[2] != null)
			//{
			//	this.targetIdx = (target.dynHealth < attackRange[2].dynHealth) ? targetIdx : 2;
			//	this.target = (target.dynHealth < attackRange[2].dynHealth) ? target : attackRange[2];
			//}
			//else 
			//{
			//	this.targetIdx = -1;
			//	this.target = null;
			//}
		}
		/// <summary>
		/// 回合结束结算攻击，回复操作数（系统更新）
		/// </summary>
		internal void RotateSettlement()
		{
			this.dynAttackCounter = (this.dynAttackCounter - this.operateCounter) < 0 ? 0 : this.dynAttackCounter - this.operateCounter;
			//操作计数回复
			this.operateCounter = 1;

			//自动更新
			this.UpdateInfo();

			if (this.dynAttackCounter <= 0)
			{
				if (randomAttack)
				{
					UnitElement tmpTarget = battleSystem.RandomTarget();
					int result = -1;
					result = Attack(tmpTarget);
					if(result > 0)
					{
						this.dynAttackCounter = this.oriAttackCounter;
					}
				}
				else
				{
					int result = -1;
					result = Attack();
					if (result > 0)
					{
						this.dynAttackCounter = this.oriAttackCounter;
					}
				}
			}
		}
		internal void Settlement()
		{
			this.UpdateInfo();

			if (this.dynAttackCounter <= 0)
			{
				if (randomAttack)
				{
					UnitElement tmpTarget = battleSystem.RandomTarget();
					int result = -1;
					result = Attack(tmpTarget);
					if (result > 0)
					{
						this.dynAttackCounter = this.oriAttackCounter;
					}
				}
				else
				{
					int result = -1;
					result = Attack();
					if (result > 0)
					{
						this.dynAttackCounter = this.oriAttackCounter;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="battleSystem"></param>
		internal void Deploy(BattleSystem battleSystem)
		{
			eventTable.RaiseEvent("BeforeDeploy", this, battleSystem);
			//巨兽， 需要拓展

			this.battleSystem = battleSystem;
			this.operateCounter = 0;

			UpdateInfo();

			eventTable.RaiseEvent("AfterDeploy", this, battleSystem);
		}
		/// <summary>
		/// 攻击方法: 
		/// </summary>
		internal int Attack()
		{
			eventTable.RaiseEvent("BeforeAttack", this, battleSystem);

			if (target == null) return -1;

			this.target.Attacked(this);

			controller.AttackAnimation(target.inlineIdx, target.battleLine.count);

			eventTable.RaiseEvent("AfterAttack", this, battleSystem);

			return 1;

		}
		internal int Attack(UnitElement tmpTarget)
		{
			eventTable.RaiseEvent("BeforeAttack", this, battleSystem);

			if (tmpTarget == null) return -1;

			tmpTarget.Attacked(this);

			controller.RandomAttackAnimation(tmpTarget.controller);

			eventTable.RaiseEvent("AfterAttack", this, battleSystem);

			return 1;

		}
		/// <summary>
		/// 有来源受击方法: 根据伤害源atk修改damage寄存值
		/// </summary>
		/// <param name="source"></param>
		internal void Attacked(UnitElement source)
		{
			eventTable.RaiseEvent("BeforeAttacked", this, battleSystem);

			this.damage = source.dynAttack;
			Damaged();

			eventTable.RaiseEvent("AfterAttacked", this, battleSystem);
		}
		/// <summary>
		/// 无来源受伤方法: 根据damage寄存值受伤
		/// </summary>
		internal void Damaged()
		{
			eventTable.RaiseEvent("BeforeDamaged", this, battleSystem);
			if(this.dynHealth == this.maxHealth)
			{
				eventTable.RaiseEvent("Meticulous", this, battleSystem);
			}

			this.dynHealth = this.dynHealth - this.damage < 0 ? 0 : this.dynHealth - this.damage;
			this.damage = 0;

			if (this.dynHealth <= 0)
			{
				Terminate();
			}

			eventTable.RaiseEvent("AfterDamaged", this, battleSystem);
		}
		internal void Damaged(int damage)
		{
			eventTable.RaiseEvent("BeforeDamaged", this, battleSystem);
			if (this.dynHealth == this.maxHealth)
			{
				eventTable.RaiseEvent("Meticulous", this, battleSystem);
			}

			this.dynHealth = this.dynHealth - damage < 0 ? 0 : this.dynHealth - damage;

			if (this.dynHealth <= 0)
			{
				Terminate();
			}

			eventTable.RaiseEvent("AfterDamaged", this, battleSystem);
		}
		/// <summary>
		/// 主动移动，消耗行动次数
		/// </summary>
		internal void Move()
		{
			if (!moveable)
			{
				return;//TODO
			}

			eventTable.RaiseEvent("BeforeMove", this, battleSystem);

			//骑兵特供 TODO 
			this.dynAttackCounter = this.dynAttackCounter - this.counterDecrease < 0 ? 0 : this.dynAttackCounter - this.counterDecrease;
			//if (this.dynAttackCounter <= 0 && target != null)
			//{
			//	this.dynAttackCounter = this.oriAttackCounter;
			//	Attack();
			//}

			this.operateCounter = 0;

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
		/// 死亡
		/// </summary>
		internal void Terminate()
		{
			eventTable.RaiseEvent("BeforeTerminate", this, battleSystem);

			//由自己修改的状态
			this.state = UnitState.destroyed;

			battleLine.ElementRemove(inlineIdx);


			//not likely
			eventTable.RaiseEvent("AfterTerminate", this, battleSystem);
		}
		/// <summary>
		/// 撤退回到手牌区回复状态
		/// </summary>
		internal void Retreat()
		{
			battleLine.ElementRemove(inlineIdx);

			//TODO config
			this.dynHealth = dynHealth + 2 > oriHealth ? oriHealth : dynHealth + 2;
			this.operateCounter = 1;
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
			UpdateInfo();
			controller.Init(backendID, ownership, this);
		}
		internal void UpdateInfo()
		{
			controller.UpdateInfo(name, category, cost, dynAttack, dynHealth, maxHealth, dynAttackCounter, operateCounter, state);
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











	internal sealed class CommandElement : BattleElement
	{
		//global info
		/// <summary>
		/// 指向作战系统，获取全局信息
		/// </summary>
		internal BattleSystem battleSystem;

		internal int durability { get; set; }
		internal int targetNum { get; set; }
		internal CommandElement(CommandCard __card) : base(__card)
		{
			this.durability = __card.maxDurability;
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




	public enum UnitState
	{
		inDeck,
		inStack,
		inHandicap,
		inBattleLine,
		destroyed
	}
}