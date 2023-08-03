//Author@Archer
using System.Collections;
using System.Collections.Generic;
using DataCore.BattleElements;
using DataCore.Cards;
using InputHandler;

namespace DisplayInterface
{
	public interface IGameManagement
	{

	}
	public interface IPoolDisplay
	{

	}
	public interface IBuildingDisplay
	{

	}
	public interface ICardDisplay
	{
		//public void TextDisplay(string __name, CardCategories __categories, int __attackPoint, int __cost, int __maxHealthPoint);
	}
	public interface ICommodityDisplay
	{

	}










	public interface IBattleSceneController
	{
		/// <summary>
		/// 战斗系统渲染层初始化
		/// </summary>
		/// <param name="handler"></param>
		public void FieldInitialize(IBattleSystemInput handler);

		public IBattleLineController InstantiateBattleLine(int idx, int capacity);
		public IHandicapController InstantiateHandicap(int turn);
		public ICardStackController InstantiateCardStack(int turn);

		public void UpdateTurnWithSettlement();
		public void Settlement();
		/// <summary>
		/// 更新回合
		/// </summary>
		/// <param name="TURN"></param>
		public void UpdateTurn(int TURN);
		public void UpdateEnergy(int energy);
		public void UpdateEnergy(int turn, int energy);
		public void UpdateEnergySupply(int supply);
		public void UpdateEnergySupply(int turn, int supply);


		//public void PushElementIntoHandicap(IUnitElementController element);
		//public void PushElementIntoHandicap(int turn, IUnitElementController element);

	}
	public interface IBattleLineController
	{
		public void Init(int capacity, int ownership);

		public void Receive(IUnitElementController element, int dstPos);
		public void UpdateInfo(int curlength, int ownership);
	}

	public interface ICardStackController
	{
		public IUnitElementController InstantiateUnitElement();
		public IUnitElementController InstantiateUnitElementInBattle(int turn);
		public ICommandElementController InstantiateCommandElementInBattle(int turn);
		public ICommandElementController InstantiateCommandElement();
	}



	public interface IHandicapController
	{
		public void Init();
		public void Fill(List<IBattleElementController> list);
		public void Push(IBattleElementController element);
		public IBattleElementController Pop(int handicapIdx);
	}








	public interface IBattleElementController
	{

	}
	public interface IUnitElementController : IBattleElementController
	{
		public void Init(string ID, int ownership, string name, string categories, string description, IUnitInput input);

		public void UpdateInfo(int cost, int attackPoint, int healthPoint, int maxHealthPoint, int attackCounter, int operateCounter,
			ElementState state, int moveRange, bool aura);

		public void UpdateTarget(IUnitElementController t1, IUnitElementController t2, IUnitElementController t3, IUnitElementController target, int targetIdx);

		public void AttackAnimationEvent(int resIdx, int count);

		public void RandomAttackAnimationEvent(IUnitElementController target);

		public void CleaveAttackAnimationEvent(int resIdx, int count);

		public void DamageAnimationEvent(int health, string method);

		public void TerminateAnimationEvent(string method);

	}
	public interface ICommandElementController : IBattleElementController
	{
		public void Init(string ID, int ownership, string name, string description);
	}
}