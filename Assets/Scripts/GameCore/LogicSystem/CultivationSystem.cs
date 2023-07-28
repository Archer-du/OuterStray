//Author@Archer
using System.Collections;
using System.Collections.Generic;
using System.IO;
using InputHandler;
using System;
using DisplayInterface;
using SystemEventHandler;
using DataCore.CultivateItems;

namespace LogicCore
{
	//天赋树在这里extend
	public class CultivationSystem : ICultivationSystemInput
	{
		IGameManagement gameManagement;

		internal static event DeckImportHandler PackExportEvent;
		internal static event TacticalInitHandler StartExpeditionEvent;

		private Pool pool;
		private List<Department> departments;
		private int unlockProgress;

		public CultivationSystem()
		{
			//register
			TacticalSystem.ProgressEvent += new UnlockHandler(UnlockBuilding);

			//display
			//gameManagement = gmdspl;

			//init
			//pool = new Pool();
			//pool.LoadCardPool();//TODO

			departments = new List<Department>(SystemConfig.buildingNum);
			for(int i = 0; i < SystemConfig.buildingNum; i++)
			{
				departments.Add(new Department());
			}
			foreach (var department in departments)
			{
				department.Fill(pool);
			}
			unlockProgress = 1;
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
			if (StartExpeditionEvent != null)
			{
				StartExpeditionEvent();
			}
		}

		public void FromPackImportDeck(int buildingID, int packID)
		{
			if(buildingID < 0 || buildingID > unlockProgress - 1)
			{
				throw new InvalidOperationException();
			}
			else
			{
				//PackExportEvent?.Invoke(buildings[buildingID].GetPack(packID));
			}
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