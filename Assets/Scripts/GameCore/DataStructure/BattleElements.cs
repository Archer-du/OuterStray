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
using WeightUpdaterHash;

namespace DataCore.BattleElements
{
	/// <summary>
	/// battle element: actual operator during a battle
	/// </summary>
	internal abstract class BattleElement : IComparable<BattleElement>
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

		protected WeightUpdaterTable updaterTable;


		private ElementState State;
		internal ElementState state
		{
			get => State;
			set
			{
				State = value;
				if(State != ElementState.inDeck && State != ElementState.lost)
				{
					UpdateState();
				}
			}
		}
		/// <summary>
		/// 加载的静态卡牌信息
		/// </summary>
		internal Card card { get; private set; }

		/// <summary>
		/// 后端ID，用于索引渲染层信息
		/// </summary>
		internal string backendID { get; private set; }
		/// <summary>
		/// 战场中不会变化的唯一标识编号
		/// </summary>
		internal int battleID;
		/// <summary>
		/// 在卡组中同类卡集的编号
		/// </summary>
		internal int deckID;
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
		/// 气矿消耗
		/// </summary>
		internal int gasMineCost;
		/// <summary>
		/// 兵种
		/// </summary>
		internal string category { get; private set; }

		/// <summary>
		/// 绝对归属权
		/// </summary>
		internal int ownership;
		internal int stackIdx;

		internal int weight;

		internal string effects;

		internal BattleElement(Card card, BattleSystem system)
		{
			BattleSystem.UpdateWeight += UpdateWeight;

			battleSystem = system;
			//初始化事件系统和效果表
			eventTable = new EventTable();
			effectsTable = new EffectsTable(this);

			this.card = card;
			this.backendID = card.backendID;
			this.name = card.name;
			this.description = card.description;
			this.cost = card.cost;
			this.category = card.category;
			this.ownership = card.ownership;
			//TODO 维护
			this.stackIdx = -1;
			this.gasMineCost = card.gasMineCost;

			effects = card.effects;
			//EffectsParse(card.effects);

			BattleSystem.ReParse += EffectsReParse;
		}

		/// <summary>
		/// 将效果解析并加载到单位
		/// </summary>
		/// <param name="effects"></param>
		/// <exception cref="Exception"></exception>
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
						//如果是光环事件
						else if (triggerEvent[0] == "Aura")
						{
							eventTable.RegisterHandler("AfterDeploy", effectsTable.GetHandler("Aura"));

							eventTable.RegisterHandler("BeforeTerminate", effectsTable.GetHandler("AuraUnload"));
							eventTable.RegisterHandler("BeforeRetreat", effectsTable.GetHandler("AuraUnload"));

							battleSystem.eventTable[0].RegisterHandler("UnitTerminated", effectsTable.GetHandler("AuraDisable"));
							battleSystem.eventTable[1].RegisterHandler("UnitTerminated", effectsTable.GetHandler("AuraDisable"));
							battleSystem.eventTable[0].RegisterHandler("UnitRetreated", effectsTable.GetHandler("AuraDisable"));
							battleSystem.eventTable[1].RegisterHandler("UnitRetreated", effectsTable.GetHandler("AuraDisable"));


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

		internal virtual void UpdateInfo() { }
		internal virtual void UpdateState() { }
		internal virtual void UpdateWeight() { }

		public int CompareTo(BattleElement other)
		{
			if(category == other.category)
			{
				return cost.CompareTo(other.cost);
			}
			else return category.CompareTo(other.category);
		}

		internal void EffectsReParse()
		{
			EffectsParse(effects);
			eventTable.RaiseEvent("Initialize", this, null);
		}
	}






	//CRITICAL!!
	internal class UnitElement : BattleElement, IUnitInput
	{

		/// <summary>
		/// 渲染层控件
		/// </summary>
		private IUnitElementController Controller;
		internal IUnitElementController controller
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
		/// 当前所在战线
		/// </summary>
		internal BattleLine battleLine;
		/// <summary>
		/// 战线内索引值
		/// </summary>
		internal int inlineIdx;



		/// <summary>
		/// 原始攻击力
		/// </summary>
		internal int oriAttack { get; private set; }
		/// <summary>
		/// 动态攻击力
		/// </summary>
		private int DynAttack;
		internal int dynAttackWriter
		{
			get { return DynAttack; }
			set { DynAttack = value; }
		}
		internal int dynAttackReader
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
				if(value > maxHealthReader) DynHealth = maxHealthReader;
			}
		}
		/// <summary>
		/// 动态最大生命值
		/// </summary>
		private int MaxHealth;
		internal int maxHealthWriter
		{
			get { return MaxHealth; }
			set
			{
				DynHealth += value - MaxHealth;
				MaxHealth = value;
			}
		}
		internal int maxHealthReader
		{
			get
			{
				int gainSum = 0;
				foreach (KeyValuePair<int, int> entry in maxHealthGain)
				{
					gainSum += entry.Value;
				}
				return MaxHealth + gainSum;
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



		internal UnitElement(UnitCard __card, BattleSystem system, IUnitElementController controller) : base(__card, system)
		{

			//从卡牌读取原始数据
			this.oriAttack = __card.attackPoint;
			this.oriHealth = __card.healthPoint;
			this.oriAttackCounter = __card.attackCounter;

			//初始化动态数据
			this.DynAttack = __card.attackPoint;
			this.maxHealthWriter = __card.healthPoint;
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

			//eventTable.RaiseEvent("Initialize", this, null);

			this.controller = controller;
			//初始状态在卡组中
			state = ElementState.lost;
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

			this.mocking = false;
			//顺序不能变
			if (attackRange[0] != null && attackRange[0].category == "Guardian")
			{
				this.mocking = true;
				this.target = attackRange[0];
				this.targetIdx = 0;
			}
			if (attackRange[2] != null && attackRange[2].category == "Guardian")
			{
				this.mocking = true;
				this.target = attackRange[2];
				this.targetIdx = 2;
			}
			if (attackRange[1] != null && attackRange[1].category == "Guardian")
			{
				this.mocking = true;
				this.target = attackRange[1];
				this.targetIdx = 1;
			}

			int t1 = attackRange[0] == null ? 0 : 1;
			int t2 = attackRange[1] == null ? 0 : 1;
			int t3 = attackRange[2] == null ? 0 : 1;

			controller.UpdateTarget(t1, t2, t3, target?.controller, targetIdx, mocking, cleave);
		}
		/// <summary>
		/// 回合结束结算攻击，回复操作数（系统更新）
		/// </summary>
		internal void RotateSettlement()
		{
			eventTable.RaiseEvent("RotateSettlement", this, battleSystem);

			if(this.ownership == BattleSystem.TURN)
			{
				if(this.operateCounter > 0)
				{
					this.dynAttackCounter -= 1;
				}
				this.operateCounter++;
			}
			//操作计数回复

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
		internal void Deploy(BattleLine dstLine, int dstPos)
		{
			//部署前解析效果
			EffectsReParse();

			eventTable.RaiseEvent("BeforeDeploy", this, battleSystem);

			//加入部署队列
			battleSystem.deployQueue.Add(this);
			//加入ID字典
			if (!battleSystem.UnitIDDic.ContainsKey(this.backendID))
			{
				battleSystem.UnitIDDic.Add(this.backendID, new List<UnitElement>());
			}
			battleSystem.UnitIDDic[this.backendID].Add(this);

			UpdateHealth();
			UpdateTarget();
			controller.DeployAnimationEvent();
			//战线接收
			dstLine.Receive(this, dstPos);
			this.operateCounter--;



			eventTable.RaiseEvent("AfterDeploy", this, battleSystem);
			battleSystem.eventTable[ownership].RaiseEvent("UnitDeployed", this, battleSystem);
			if (dstLine.index == battleSystem.frontLines[ownership])
			{
				battleSystem.eventTable[ownership].RaiseEvent("EnterFrontLine", this, battleSystem);
				eventTable.RaiseEvent("EnterFrontLine", this, battleSystem);
			}
			UpdateInfo();
		}
		/// <summary>
		/// 主动移动，消耗行动次数
		/// </summary>
		internal virtual void Move(BattleLine resLine, BattleLine dstLine, int resIdx, int dstPos)
		{
			eventTable.RaiseEvent("BeforeMove", this, battleSystem);

			controller.MoveAnimationEvent();


			dstLine.Receive(resLine.Send(resIdx), dstPos);
			this.operateCounter--;


			eventTable.RaiseEvent("AfterMove", this, battleSystem);
			if (dstLine.index == battleSystem.frontLines[ownership])
			{
				battleSystem.eventTable[ownership].RaiseEvent("EnterFrontLine", this, battleSystem);
				eventTable.RaiseEvent("EnterFrontLine", this, battleSystem);
			}
			battleSystem.eventTable[ownership].RaiseEvent("UnitMoved", this, battleSystem);
			UpdateInfo();
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
			this.damage = source.dynAttackReader;

			eventTable.RaiseEvent("BeforeAttacked", this, battleSystem);

			Damaged("append");

			eventTable.RaiseEvent("AfterAttacked", this, battleSystem);
		}
		/// <summary>
		/// 无来源受伤方法: 根据damage寄存值受伤
		/// </summary>
		internal int Damaged(string method)
		{
			eventTable.RaiseEvent("BeforeDamaged", this, battleSystem);
			if(this.dynHealth == this.maxHealthReader)
			{
				eventTable.RaiseEvent("Meticulous", this, battleSystem);
			}
			if(this == battleSystem.bases[ownership])
			{
				battleSystem.eventTable[ownership].RaiseEvent("BaseAttacked", this, battleSystem);
			}

			this.dynHealth -= this.damage;
			controller.DamageAnimationEvent(this.dynHealth, method);

			this.damage = 0;


            if (this.dynHealth <= 0)
            {
                if (this == battleSystem.bases[0])
                {
                    battleSystem.result = BattleResult.fail;
                }
                if (this == battleSystem.bases[1])
                {
                    battleSystem.result = BattleResult.win;
                }
                Terminate(method);
                return -1;
            }

            eventTable.RaiseEvent("AfterDamaged", this, battleSystem);
			return 1;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="damage"></param>
		internal int Damaged(int damage, string method)
		{
			this.damage = damage;

			eventTable.RaiseEvent("BeforeDamaged", this, battleSystem);
			if (this.dynHealth == this.maxHealthReader)
			{
				eventTable.RaiseEvent("Meticulous", this, battleSystem);
			}

			this.dynHealth -= this.damage;
			controller.DamageAnimationEvent(this.dynHealth, method);

			this.damage = 0;

			if (this.dynHealth <= 0)
			{
				if (this == battleSystem.bases[0])
				{
					battleSystem.result = BattleResult.fail;
				}
				if (this == battleSystem.bases[1])
				{
					battleSystem.result = BattleResult.win;
				}
				Terminate(method);
				return -1;
			}

			eventTable.RaiseEvent("AfterDamaged", this, battleSystem);
			return 1;
		}
		internal void Recover(int heal)
		{
			this.recover = heal;
			eventTable.RaiseEvent("BeforeRecover", this, battleSystem);
			battleSystem.eventTable[ownership].RaiseEvent("UnitHealed", this, battleSystem);

			this.dynHealth += recover;
			controller.RecoverAnimationEvent(this.dynHealth, "append");

			this.recover = 0;

			if (this.dynHealth <= 0)
			{
				if (this == battleSystem.bases[0])
				{
					battleSystem.result = BattleResult.fail;
				}
				if (this == battleSystem.bases[1])
				{
					battleSystem.result = BattleResult.win;
				}
				Terminate("append");
			}
		}
		/// <summary>
		/// 死亡
		/// </summary>
		internal void Terminate(string method)
		{
			eventTable.RaiseEvent("BeforeTerminate", this, battleSystem);


			//由自己修改的状态
			this.state = ElementState.destroyed;
			UnloadEffects();


			battleLine.ElementRemove(inlineIdx);
			controller.TerminateAnimationEvent(method);

			//not likely
			battleSystem.eventTable[ownership].RaiseEvent("UnitTerminated", this, battleSystem);
			eventTable.RaiseEvent("AfterTerminate", this, battleSystem);
		}
		/// <summary>
		/// 撤退回到手牌区,回复状态
		/// </summary>
		internal void Retreat(string method)
		{
			if (this == battleSystem.bases[ownership])
			{
				return;
			}

			eventTable.RaiseEvent("BeforeRetreat", this, battleSystem);

			battleLine.Send(this.inlineIdx);
			//TODO config
			this.dynHealth += 2;
			this.operateCounter = 1;
			UnloadEffects();


			battleSystem.eventTable[ownership].RaiseEvent("UnitRetreated", this, battleSystem);
			controller.RetreatAnimationEvent(method);
		}





		//死亡和战斗结束时调用
		internal void UnloadEffects()
		{
			eventTable = new EventTable();
			effectsTable = new EffectsTable(this);
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

			parry = false;

			aura = false;
		}


		//internal override void UpdateWeight()
		//{
		//	updaterTable.RaiseEvent(backendID);
		//}








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
			controller.UnitInit(backendID, ownership, name, category, cost, description, this);
		}
		internal override void UpdateInfo()
		{
			controller.UpdateInfo(cost, dynAttackReader, maxHealthReader, dynAttackCounter, operateCounter, 
				state, moveRange, aura, dynAttackReader - oriAttack, maxHealthReader - oriHealth);
		}
		internal override void UpdateState()
		{
			controller.UpdateState(state);
		}
		internal void UpdateHealth()
		{
			controller.UpdateHealth(this.dynHealth);
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
		internal LightArmorElement(UnitCard __card, BattleSystem system, IUnitElementController controller) 
			: base(__card, system, controller) { }
	}
	internal sealed class MotorizedElement : UnitElement
	{
		internal MotorizedElement(UnitCard __card, BattleSystem system, IUnitElementController controller) 
			: base(__card, system, controller) { }
		internal override void Move(BattleLine resLine, BattleLine dstLine, int resIdx, int dstPos)
		{
			eventTable.RaiseEvent("BeforeMove", this, battleSystem);

            dstLine.Receive(resLine.Send(resIdx), dstPos);
            int lower = resLine.index > dstLine.index ? dstLine.index : resLine.index;
            int upper = resLine.index > dstLine.index ? resLine.index : dstLine.index;
            for (int i = lower; i < upper; i++)
            {
                battleSystem.battleLines[i].ownerShip = ownership;
                battleSystem.battleLines[i].UpdateElements();
                battleSystem.battleLines[i].UpdateInfo();
            }

			this.dynAttackCounter -= 1;
			this.operateCounter--;


			eventTable.RaiseEvent("AfterMove", this, battleSystem);
			if (dstLine.index == battleSystem.frontLines[ownership])
			{
				battleSystem.eventTable[ownership].RaiseEvent("EnterFrontLine", this, battleSystem);
				eventTable.RaiseEvent("EnterFrontLine", this, battleSystem);
			}
			battleSystem.eventTable[ownership].RaiseEvent("UnitMoved", this, battleSystem);
			UpdateInfo();
		}
	}
	internal sealed class ArtilleryElement : UnitElement
	{
		internal UnitElement tmpTarget;
		internal ArtilleryElement(UnitCard __card, BattleSystem system, IUnitElementController controller) 
			: base(__card, system, controller) { }
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
				tmpTarget = battleSystem.RandomEnemy(this.ownership);
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
		internal GuardianElement(UnitCard __card, BattleSystem system, IUnitElementController controller) 
			: base(__card, system, controller) { }
	}
	internal sealed class ConstructionElement : UnitElement
	{
		internal ConstructionElement(UnitCard __card, BattleSystem system, IUnitElementController controller) 
			: base(__card, system, controller) { }
		internal override void Move(BattleLine resLine, BattleLine dstLine, int resIdx, int dstPos)
		{
			return;
		}
	}
	//legacy
	[Obsolete]
	internal sealed class BehemothsElement : UnitElement
	{
		internal BehemothsElement(UnitCard __card, BattleSystem system, IUnitElementController controller) 
			: base(__card, system, controller)
		{
		}
	}








	internal sealed class CommandElement : BattleElement
	{
		private ICommandElementController Controller;
		internal ICommandElementController controller
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

		internal string type;
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

		[Obsolete]
		internal int tempBufferForCommMush07;

		internal CommandElement(CommandCard __card, BattleSystem system) : base(__card, system)
		{
			this.type = __card.type;
			this.oriDurability = __card.maxDurability;
			this.DynDurability = __card.maxDurability;

			tempBufferForCommMush07 = 0;

			eventTable.RaiseEvent("Initialize", this, null);
		}


		internal void Cast(UnitElement target)
		{
			EffectsReParse();
			eventTable.RaiseEvent("Cast", target, battleSystem);
			UnloadEffects();


			dynDurability -= 1;
			if(dynDurability <= 0)
			{
				Terminate();
			}


			if(type == "NonTarget" || ownership == 1)
			{
				controller.NonTargetCastAnimationEvent("append");
			}
			UpdateInfo();
		}
		internal void Recover(int heal)
		{
			dynDurability += heal;
		}
		internal void Terminate()
		{
			//由自己修改的状态
			this.state = ElementState.destroyed;

			//Unload
			tempBufferForCommMush07 = 0;
		}

		/// <summary>
		/// 由手牌区初始化
		/// </summary>
		internal void Init()
		{
			UpdateInfo();
			controller.CommandInit(backendID, ownership, name, type, cost, description);
		}
		internal override void UpdateInfo()
		{
			controller.UpdateInfo(cost, dynDurability, state);
		}
		public void UpdateManual()
		{
			UpdateInfo();
		}
		internal override void UpdateState()
		{
			controller.UpdateState(state);
		}
		internal void UnloadEffects()
		{
			eventTable = new EventTable();
			effectsTable = new EffectsTable(this);
		}
	}





	public enum ElementState
	{
		inDeck,
		inStack,
		inHandicap,
		inBattleLine,
		destroyed,
		lost
	}
}