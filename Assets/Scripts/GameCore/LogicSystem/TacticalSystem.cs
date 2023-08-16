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
using DataCore.BattleElements;
using Codice.Client.Common.Threading;

namespace LogicCore
{
	public class TacticalSystem : ITacticalSystemInput
	{
		IGameManagement gameManagement;

		public ITacticalSceneController controller;

		internal BattleSystem battleSystem;
		internal CultivationSystem cultivationSystem;

		internal List<Terrain> terrains;

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


		internal Deck playerDeck
		{
			get => cultivationSystem.playerDeck;
		}
		internal UnitElement playerBase;

		internal int baseHealth
		{
			get => playerBase.dynHealth;
		}

		internal int cardNum
		{
			get => playerDeck.count;
		}

		private int GasMineToken;
		internal int gasMineToken
		{
			get => GasMineToken;
			set
			{
				GasMineToken = value;
				controller.UpdateGasMineToken(value);
			}
		}

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
			battleNodeNum = 2;
			terrains = new List<Terrain>(battleNodeNum);

		}
		public void SetSceneController(ITacticalSceneController tsdspl)
		{
			controller = tsdspl;

			//playerDeck = new Deck(battleSystem, this, controller.InstantiateDeck());
			////TODO remove
			//playerDeck.LoadDeckByPathHuman("Assets\\Config\\HumanDeckTest.csv");

			controller.TerrrainsInitialize(this, battleNodeNum);
			playerBase = playerDeck.bases;

			controller.UpdateCardNum(cardNum);
			controller.UpdateBaseHealth(baseHealth, playerBase.maxHealthWriter);

			//TODO
			gasMineToken = 20;
			//TODO

			BuildTerrains();
		}
		public void SetCultivateSystem(ICultivationSystemInput cultivationSystem)
		{
			this.cultivationSystem = cultivationSystem as CultivationSystem;
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
			if (currentNode is not BattleNode)
			{
				CampaignCompleted();
			}
		}

		public void CampaignCompleted()
		{
			isInNode = false;
			controller.UpdateCurrentNode(currentNode.controller);
		}
		public void BattleCampaignCompleted()
		{
			isInNode = false;
			controller.LateUpdateTacticalLayer(currentNode.controller);
		}

		public void CampaignFailed()
		{
			isInNode = false;
			//向上通知
			//controller.CampaignFailed();
			throw new Exception("expedition failed");
		}

		public void Exit()
		{
			throw new NotImplementedException();
		}



		public void MedicalNodeHeal(bool fullfill, int deckID)
		{
			if(currentNode is not MedicalNode)
			{
				throw new InvalidCastException();
			}
			if (playerDeck[deckID] is not UnitElement)
			{
				throw new InvalidCastException();
			}
			MedicalNode medical = currentNode as MedicalNode;
			UnitElement unit = playerDeck[deckID] as UnitElement;
			if(gasMineToken < medical.pricePerHealth)
			{
				throw new InvalidOperationException();
			}
			if(fullfill && (unit.maxHealthWriter - unit.dynHealth) * medical.pricePerHealth > gasMineToken)
			{
				throw new InvalidOperationException();
			}

			medical.HealElement(fullfill, unit);
		}

		public void OutPostNodePurchase(int index)
		{
			if(currentNode is not OutPostNode)
			{
				throw new InvalidCastException();
			}
			OutPostNode outPost = currentNode as OutPostNode;
			if (gasMineToken < outPost.commercials[index].gasMineCost)
			{
				throw new InvalidOperationException();
			}

			playerDeck.AddTag(outPost.Purchase(index));
			controller.UpdateCardNum(cardNum);
		}

		public void SupplyNodeChoose(int index)
		{
			if(currentNode is not SupplyNode)
			{
				throw new InvalidCastException();
			}
			SupplyNode supply = currentNode as SupplyNode;

			playerDeck.AddTag(supply.Choose(index));
			controller.UpdateCardNum(cardNum);
		}
	}
}
