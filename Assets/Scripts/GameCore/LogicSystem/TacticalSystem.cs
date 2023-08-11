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
			get
			{
				if(currentNode is BattleNode)
				{
					BattleNode node = currentNode as BattleNode;
					return node.nextTerrain;
				}
				else
				{
					return currentNode.terrain;
				}
			}
		}

		internal static bool isInNode;


		internal Deck playerDeck;
		internal Deck enemyDeck;

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
			playerPool.LoadCardPool();

			playerDeck = new Deck(system);
			playerDeck.LoadDeckFromPool(playerPool, "ally");
			enemyDeck = new Deck(system);
			enemyDeck.LoadDeckFromPool(playerPool, "enemy");


			//TODO
			controller.TerrrainsInitialize(this, battleNodeNum);
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
			for(int i = 0; i < battleNodeNum - 1; i++)
			{
				terrains[i].dstNode.nextTerrain = terrains[i + 1];
			}

			currentNode = terrains[0].resNode;
			currentNode.CastNodeEvent();

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

			isInNode = true;
			targetNode.CastNodeEvent();
			currentNode = targetNode;


			//TODO test
			if(currentNode is not BattleNode)
			{
				CampaignCompleted();
			}
		}

		public void CampaignCompleted()
		{
			isInNode = false;
			controller.UpdateCurrentNode(currentNode.controller);
			if (currentNode.IsDstNodeCurTerrain())
			{
				controller.EnterNextTerrain();
				currentTerrain.controller.GenerateLineNetFromSource();
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
