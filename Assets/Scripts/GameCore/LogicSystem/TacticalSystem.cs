//Author@Archer
using DataCore.CultivateItems;
using DataCore.TacticalItems;
using System.Collections;
using System.Collections.Generic;
using DataCore.StructClass;
using InputHandler;
using DisplayInterface;
using SystemEventHandler;
using System;
using System.Diagnostics;

namespace LogicCore
{
	public class TacticalSystem : ITacticalSystemInput
	{
		IGameManagement gameManagement;

		public ITacticalSceneController controller;

		private BattleSystem battleSystem;


		private List<Terrain> terrains;

		private Node currentNode;

		private Deck playerDeck;
		private Deck enemyDeck;


		public int battleNodeNum;

		//data access (test)
		internal Pool playerPool;
		//Enemy config TODO
		private Pool enemyPool;

		public TacticalSystem(ITacticalSceneController controller, BattleSystem system)
		{
			//display
			this.controller = controller;
			
			//connection
			this.battleSystem = system;

			//init
			//TODO test
			battleNodeNum = 5;

			terrains = new List<Terrain>(battleNodeNum - 1);

			playerDeck = new Deck(system);
			enemyDeck = new Deck(system);
		}


		public void BuildTerrains()
		{
			for(int i = 0; i < battleNodeNum; i++)
			{
				terrains.Add(new Terrain(i));
			}
			//连接各terrain
			for(int i = 0; i < battleNodeNum - 1; i++)
			{
				terrains[i + 1].prevTerrain = terrains[i];
			}

			foreach(Terrain terrain in terrains)
			{
				terrain.GenerateNodes();
			}
			currentNode = terrains[0].resNode;
		}



		public void EnterNode(int hrztIdx, int vtcIdx)
		{

		}


		public void EnterNextTerrain()
		{

		}
	}
}
