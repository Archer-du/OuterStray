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

		public CultivationSystem(ICultivateSceneController controller, TacticalSystem tacticalSystem, BattleSystem battleSystem)
		{
			//display
			//gameManagement = gmdspl;
			this.tacticalSystem = tacticalSystem;
			this.battleSystem = battleSystem;

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
		/// <summary>
		/// console version
		/// </summary>
		public void EnterTacticalSystem()
		{
			// lock
		}

		public void EnterExpedition()
		{
		}

		public void FromPackImportDeck(int buildingID, int packID)
		{
			playerDeck.LoadDeckByPathHuman("Assets\\Config\\HumanDeckTest.csv");
			controller.UpdateBasicInfo(20, playerDeck.count, playerDeck.bases.oriHealth);

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
	}
}