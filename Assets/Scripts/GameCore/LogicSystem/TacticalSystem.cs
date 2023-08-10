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
		private Terrain currentTerrain
		{
			get => currentNode.terrain;
		}

		internal static bool isInNode;


		private Deck playerDeck;
		private Deck enemyDeck;

		internal int baseHealth;
		internal int gasMineToken;

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
			isInNode = false;

			//TODO test
			battleNodeNum = 5;

			terrains = new List<Terrain>(battleNodeNum);

			//TODO remove
			playerPool = new Pool();

			playerDeck = new Deck(system);
			enemyDeck = new Deck(system);

			controller.TerrrainsInitialize(this, battleNodeNum);

			//TODO
			BuildTerrains();
		}


		public void BuildTerrains()
		{
			for(int i = 0; i < battleNodeNum; i++)
			{
				Terrain t = new Terrain(i, this, battleSystem);
				t.controller = controller.InstantiateTerrain(i);
				t.Init();
				terrains.Add(t);
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

			controller.UpdateCurrentNode(currentNode.controller);

			currentTerrain.controller.GenerateLineNetFromSource();
		}


		public void EnterNode(int terrainIdx, int hrztIdx, int vtcIdx)
		{
			if(isInNode)
			{
				return;
			}
			Node targetNode = terrains[terrainIdx][hrztIdx][vtcIdx];
			if (!currentNode.IsReachable(targetNode))
			{
				throw new InvalidOperationException();
			}

			targetNode.CastNodeEvent();
			isInNode = true;
			currentNode = targetNode;
		}

		public void CampaignCompleted()
		{
			isInNode = false;
			if (currentNode.IsDstNodeCurTerrain())
			{
				controller.EnterNextTerrain();
			}
		}

		public void CampaignFailed()
		{
			isInNode = false;
			//向上通知
		}

		public void Exit()
		{
			throw new NotImplementedException();
		}
	}
}
