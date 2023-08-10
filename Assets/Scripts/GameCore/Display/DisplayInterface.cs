//Author@Archer
using System.Collections;
using System.Collections.Generic;
using DataCore.BattleElements;
using DataCore.Cards;
using InputHandler;
using Unity.Plastic.Antlr3.Runtime.Tree;

namespace DisplayInterface
{
	public interface IGameManagement
	{

	}
	//public interface IPoolDisplay
	//{

	//}
	//public interface IBuildingDisplay
	//{

	//}
	//public interface ICardDisplay
	//{
	//	//public void TextDisplay(string __name, CardCategories __categories, int __attackPoint, int __cost, int __maxHealthPoint);
	//}
	//public interface ICommodityDisplay
	//{

	//}






	public interface ICultivateSceneController
	{

	}





	public interface ITacticalSceneController
	{
		public void TerrrainsInitialize(ITacticalSystemInput handler, int terrainsLength);
		public ITerrainController InstantiateTerrain(int idx);
		public void EnterNextTerrain();
		public void UpdateCurrentNode(INodeController controller);
	}
	public interface ITerrainController
	{
		public INodeController InstantiateNode(int length, int width, int hrztIdx, int vtcIdx, string category);
		public void Init();

		public void GenerateLineNetFromSource();
	}
	public interface INodeController
	{
		public void Init();
		public void SetAdjacentNode(List<INodeController> adjList);
		public void CastEvent();
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


		public IUnitElementController InstantiateUnitInBattleField(int ownership, int lineIdx, int pos);

		public IUnitElementController InstantiateUnitInStack(int turn);


		//public void PushElementIntoHandicap(IUnitElementController element);
		//public void PushElementIntoHandicap(int turn, IUnitElementController element);

	}
	public interface IBattleLineController
	{
		public void Init(int capacity, int ownership);

		public void Receive(IUnitElementController element, int dstPos);

		public IUnitElementController Send(int idx);

		public void UpdateInfo(int curlength, int ownership);

		public void UpdateElementLogicPosition(List<IUnitElementController> list);
	}

	public interface ICardStackController
	{
		public IUnitElementController InstantiateUnitElementInBattle();
		public ICommandElementController InstantiateCommandElementInBattle();
		public IUnitElementController InstantiateUnitElement();
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
		public void UnitInit(string ID, int ownership, string name, string categories, int cost, string description, IUnitInput input);

		public void UpdateInfo(int cost, int attackPoint, int maxHealthPoint, int attackCounter, int operateCounter,
			ElementState state, int moveRange, bool aura, int attackBuff, int maxHealthBuff);

		public void UpdateHealth(int dynHealth);

		public void UpdateTarget(IUnitElementController target, int targetIdx, bool mocking, bool cleave);

		public void DeployAnimationEvent();

		public void MoveAnimationEvent();

		public void AttackAnimationEvent(int resIdx, int count);

		public void RandomAttackAnimationEvent(IUnitElementController target);

		public void CleaveAttackAnimationEvent(int resIdx, int count);

		public void DamageAnimationEvent(int health, string method);

		public void RecoverAnimationEvent(int recover, string method);

		public void TerminateAnimationEvent(string method);

		public void RetreatAnimationEvent(string method);
	}
	public interface ICommandElementController : IBattleElementController
	{
		public void CommandInit(string ID, int ownership, string name, string type, int cost, string description);

		public void UpdateInfo(int cost, int durability, ElementState state);

		public void CastAnimationEvent(string method);
	}
}