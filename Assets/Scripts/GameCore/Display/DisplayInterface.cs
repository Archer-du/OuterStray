//Author@Archer
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DataCore.BattleElements;
using DataCore.Cards;
using InputHandler;
// using Unity.Plastic.Antlr3.Runtime.Tree;

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
	public interface IResourceLoader
	{
		public StreamReader OpenText(string path);

		public string ReadAllText(string path);
	}





	public interface ICultivateSceneController
	{
		public IDeckController InstantiateDeck();
		public void UpdateBasicInfo(int gasMine, int cardNum);
		public void UpdateBaseInfo(List<string> IDs, List<string> names, List<string> categories, List<int> healths, List<string> description);
	}





	public interface ITacticalSceneController
	{
		public void TerrrainsInitialize(ITacticalSystemInput handler, int terrainsLength);
		public ITerrainController InstantiateTerrain(int idx);
		public IDeckController InstantiateDeck();
		public void EnterNextTerrain();
		public void UpdateCurrentNode(INodeController controller);

		public void UpdateGasMineToken(int gasMineToken);
		public void UpdateCardNum(int cardNum);
		public void UpdateBaseHealth(int baseHealth, int baseMaxHealth);

		public void CampaignCompleted();
		public void CampaignFailed();
		public void LateUpdateTacticalLayer(INodeController currentNode, int gasMine, int baseHealth);

		public void Exit();
	}
	public interface IDeckController
	{
		public void Init(IDeckInput input);

		public void UnloadDeckTags();

		public void InstantiateDeckTag(string ID, string name, string category, int index, string description);

		public void UpdateUnitTagInfo(string category, int index, int cost, int attack, int health, int maxHealth, int counter);

		public void UpdateCommandTagInfo(int index, int cost, int durability);

		public void UpdateHealth(int index, int health);

		public void UpdateHierachy();
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
		public void DisplayElement(List<string> IDs, List<string> names, List<string> category, List<int> costs, List<int> attacks, List<int> healths, List<int> counters, List<int> gasMineCosts, List<string> descriptions);
		public void UpdateBasicInfo(int legacy, int medicalPrice) { }
		public void UpdateHealth(int health) { }
	}





	public interface IBattleSceneController
	{
		/// <summary>
		/// 战斗系统渲染层初始化
		/// </summary>
		/// <param name="handler"></param>
		public void FieldInitialize(IBattleSystemInput handler, int fieldCapacity, int BTindex);
		public void InitBases(IUnitElementController humanBase, IUnitElementController plantBase);

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

		public void UpdateFrontLine(int humanFrontLine);


		public IUnitElementController InstantiateUnitInBattleField(int ownership, int lineIdx, int pos);

		public IUnitElementController InstantiateUnitInStack(int turn);


		public void BattleFailed();
		public void BattleWinned();

		public void InstantiateReward(string[] IDs, string[] names, string[] categories, int[] cost, int[] attacks, int[] healths, int[] counters, string[] description);
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
		public void Init(int ownership);
		public IUnitElementController InstantiateUnitElementInBattle();
		public ICommandElementController InstantiateCommandElementInBattle();
	}



	public interface IHandicapController
	{
		public void Init(int ownership);
		public void Fill(List<IBattleElementController> list, int initialTurn);
		public void Push(IBattleElementController element, string method, int position);
		public IBattleElementController Pop(int handicapIdx);
	}








	public interface IBattleElementController
	{
		public void Init(string ID, int ownership, string name, string categories, int cost, string description);
	}
	public interface IUnitElementController : IBattleElementController
	{
		public void UnitInit(string ID, int ownership, string name, string categories, int cost, string description, IUnitInput input);

		public void UpdateInfo(int cost, int attackPoint, int maxHealthPoint, int attackCounter, int operateCounter,
			ElementState state, int moveRange, bool aura, int attackBuff, int maxHealthBuff);

		public void UpdateState(ElementState state);
		public void UpdateHealth(int dynHealth);

		public void UpdateTarget(int t1, int t2, int t3, IUnitElementController target, int targetIdx, bool mocking, bool cleave);

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
		public void UpdateState(ElementState state);

		public void NonTargetCastAnimationEvent(string method);
	}
}