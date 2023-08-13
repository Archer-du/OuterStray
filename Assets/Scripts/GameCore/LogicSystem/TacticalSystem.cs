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

		internal int baseHealth;
		internal int gasMineToken;

		public int battleNodeNum;

		//data access (test)
		internal Pool pool;

		public TacticalSystem(Pool pool, ITacticalSceneController controller, BattleSystem system)
		{
			this.pool = pool;

			//display
			this.controller = controller;
			
			//connection
			this.battleSystem = system;

			//init
			isInNode = false;

			//TODO test
			battleNodeNum = 5;
			terrains = new List<Terrain>(battleNodeNum);

			//TODO
			baseHealth = 30;
			gasMineToken = 30;

			playerDeck = new Deck(system, this);
			//TODO remove
			playerDeck.LoadDeckByPath("Assets\\Config\\HumanDeckTest.csv");

			//TODO
			controller.TerrrainsInitialize(this, battleNodeNum);

			BuildTerrains();
		}


		public void BuildTerrains()
		{
			//构建terrain
			for(int i = 0; i < battleNodeNum; i++)
			{
				Terrain t = new(i, this, battleSystem, controller.InstantiateTerrain(i));
				terrains.Add(t);
			}
			//连接各terrain
			for(int i = 0; i < battleNodeNum - 1; i++)
			{
				terrains[i + 1].prevTerrain = terrains[i];
			}

			//各terrain生成节点
			foreach(Terrain terrain in terrains)
			{
				terrain.GenerateNodes();
			}
			for(int i = 0; i < battleNodeNum - 1; i++)
			{
				terrains[i].dstNode.nextTerrain = terrains[i + 1];
			}

			//全局唯一source节点
			currentNode = terrains[0].resNode;

			//显示层：当前节点更新
			controller.UpdateCurrentNode(currentNode.controller);

			//显示层：当前terrain生成线网
			currentTerrain.controller.GenerateLineNetFromSource();
		}

		/// <summary>
		/// 进入节点
		/// </summary>
		/// <param name="terrainIdx"></param>
		/// <param name="hrztIdx"></param>
		/// <param name="vtcIdx"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void EnterNode(int terrainIdx, int hrztIdx, int vtcIdx)
		{
			if(isInNode)
			{
				throw new InvalidOperationException();
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

			//显示层更新
			if (currentNode.IsDstNodeCurTerrain())
			{
				controller.EnterNextTerrain();
				currentTerrain.controller.GenerateLineNetFromSource();
			}
			//controller.CampaignCompleted();
		}

		public void CampaignFailed()
		{
			isInNode = false;
			//向上通知
			//controller.CampaignFailed();
		}

		public void Exit()
		{
			throw new NotImplementedException();
		}
	}
}
