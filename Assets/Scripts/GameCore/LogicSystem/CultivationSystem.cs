//Author@Archer
using System.Collections;
using System.Collections.Generic;
using System.IO;
using InputHandler;
using System;
using DisplayInterface;
using SystemEventHandler;
using DataCore.CultivateItems;
using DataCore.TacticalItems;
using DataCore.BattleElements;
using DataCore.Cards;
using UnityEditor.Animations;

namespace LogicCore
{

	//天赋树在这里extend
	public class CultivationSystem : ICultivationSystemInput
	{
		IGameManagement gameManagement;

		ICultivateSceneController controller;

		internal TacticalSystem tacticalSystem;
		internal BattleSystem battleSystem;

		private Pool pool;
		internal Deck playerDeck;

		private List<Department> departments;
		private int unlockProgress;

		public bool tutorial;

		internal UnitElement[] bases;

		public CultivationSystem(Pool pool, ICultivateSceneController controller, TacticalSystem tacticalSystem, BattleSystem battleSystem)
		{
			this.pool = pool;
			tutorial = false;
			//display
			//gameManagement = gmdspl;
			this.tacticalSystem = tacticalSystem;
			this.battleSystem = battleSystem;

			bases = new UnitElement[3];

			bases[0] = new ConstructionElement(pool.GetCardByID("base_01") as UnitCard, battleSystem, null);
			bases[1] = new ConstructionElement(pool.GetCardByID("base_02") as UnitCard, battleSystem, null);
			bases[2] = new ConstructionElement(pool.GetCardByID("base_03") as UnitCard, battleSystem, null);

			//departments = new List<Department>(SystemConfig.buildingNum);
			//for(int i = 0; i < SystemConfig.buildingNum; i++)
			//{
			//	departments.Add(new Department());
			//}
			//foreach (var department in departments)
			//{
			//	department.Fill(pool);
			//}
			//unlockProgress = 1;

			
		}
		public void SetSceneController(ICultivateSceneController ctdspl)
		{
			this.controller = ctdspl;
			playerDeck = new Deck(battleSystem, tacticalSystem, controller.InstantiateDeck());
		}
		public void SetBase(int index)
		{
			playerDeck.bases = bases[index];
			controller.UpdateBaseInfo(playerDeck.bases.oriHealth);
		}



		public void FromPackImportDeck(int buildingID, int packID)
		{
			playerDeck.LoadDeckByPathDisplay("Config\\HumanDeckTest.csv");
			controller.UpdateBasicInfo(tacticalSystem.gasMineToken, playerDeck.count);

			//if (buildingID < 0 || buildingID > unlockProgress - 1)
			//{
			//	throw new InvalidOperationException();
			//}
			//else
			//{
			//	//PackExportEvent?.Invoke(buildings[buildingID].GetPack(packID));
			//}
		}

		/// <summary>
		/// registered function
		/// </summary>
		internal void UnlockBuilding()
		{
			departments[unlockProgress++].Unlock();
		}







		public void LoadTutorialHumanDeck()
		{
			tutorial = true;
			playerDeck.LoadDeckByPathDisplay("Config\\TutorialData.csv");
		}
	}
}