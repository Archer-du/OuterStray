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

		internal static event UnlockHandler ProgressEvent;
		internal static event BattleStartHandler EnterBattleEvent;

		//
		private List<Terrain> terrains;
		private string currentPoint;
		private int currentTerrain;

		private Deck deck;

		//Enemy config TODO
		private Pool enemyPool;

		public TacticalSystem()
		{
			//register
			//CultivationSystem.PackExportEvent += new DeckImportHandler(ImportDeck);
			//CultivationSystem.StartExpeditionEvent += new TacticalInitHandler(RebuildTerrains);

			//display
			//gameManagement = gmdspl;

			//init
			terrains = new List<Terrain>(SystemConfig.terrainNum);
			deck = new Deck();
			//testDeck = new Deck();

			//test TODO

		}


		public void RebuildTerrains()
		{
			terrains.Clear();
			for (int i = 0; i < SystemConfig.terrainNum; i++)
			{
				terrains.Add(new Terrain());
				if(i == 0)
				{
					terrains[i].GenerateNodeRes();
				}
				else if (i == SystemConfig.terrainNum - 1)
				{
					terrains[i].GenerateNodeDst();
				}
				else
				{
					terrains[i].GenerateNodeNormal();
				}
			}
			currentTerrain = 0;
			currentPoint = "00";
		}



		public void EnterNode(int hrztIdx, int vtcIdx)
		{
			string targetPoint = hrztIdx.ToString() + vtcIdx.ToString();
			if (!terrains[currentTerrain].Reachable(currentPoint, targetPoint))
			{
				throw new InvalidOperationException();
			}
			else
			{
				currentPoint = targetPoint;
				Node node = terrains[currentTerrain].GetNode(targetPoint);
				if (node != null)
				{
					if (node is ResNode)
					{

					}
					else if(node is DstNode)
					{

					}
					else if(node is BattleNode)
					{
						//EnterBattleEvent?.Invoke(deck, testDeck);
					}
				}
			}
		}
		public void EnterNode(string targetPoint)
		{
			if (!terrains[currentTerrain].Reachable(currentPoint, targetPoint))
			{
				throw new InvalidOperationException();
			}
			else
			{
				currentPoint = targetPoint;
				Node node = terrains[currentTerrain].GetNode(targetPoint);
				if (node != null)
				{
					if (node is ResNode)
					{

					}
					else if (node is DstNode)
					{

					}
					else if (node is BattleNode)
					{
						//EnterBattleEvent?.Invoke(deck, testDeck);
					}
					//TODO
				}
			}
		}

		public void EnterNextTerrain()
		{
			if (!terrains[currentTerrain].IsDstNode(currentPoint))
			{
				throw new InvalidOperationException();
			}
			else
			{
				if(currentTerrain == SystemConfig.terrainNum - 1)
				{
					//expedition completed
					return;
				}
				currentTerrain++;
				currentPoint = "00";
			}
		}

		/// <summary>
		/// PackExportEvent registered function
		/// </summary>
		/// <param name="pack"></param>
		internal void ImportDeck(Pack pack)
		{
			//this.deck.AddPack(pack, 1);
			//this.testDeck.AddPack(pack, -1);
		}
	}
}
