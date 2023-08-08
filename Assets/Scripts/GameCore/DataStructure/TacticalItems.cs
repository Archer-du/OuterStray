using DataCore.BattleElements;
using DataCore.Cards;
using DataCore.CultivateItems;
using DisplayInterface;
using LogicCore;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DataCore.TacticalItems
{
	internal class Deck
	{
		BattleSystem battleSystem;
		private List<BattleElement> deck;
		internal int count { get => deck.Count; }
		//internal Deck()
		//{
		//	deck = new List<BattleElement>();
		//}
		//TODO remove
		internal Deck(BattleSystem system)
		{
			deck = new List<BattleElement>();
			battleSystem = system;
		}
		internal BattleElement this[int index]
		{
			get => deck[index];
			set => deck[index] = value;
		}



		/// <summary>
		/// 根据pack内容创建element对象加入到Deck中
		/// </summary>
		/// <param name="pack"></param>
		internal void AddPack(Pack pack)
		{
			throw new NotImplementedException();
		}

		//TODO test function remove
		internal void LoadDeckFromPool(Pool pool, string ownership)
		{
			if(ownership == "ally")
			{
				foreach(Card card in pool.cardPool)
				{
					if(card.ownership == 0)
					{
						if(card is UnitCard)
						{
							string category = (card as UnitCard).category;
							switch (category)
							{
								case "LightArmor":
									deck.Add(new LightArmorElement(card as UnitCard, battleSystem));
									break;
								case "Motorized":
									deck.Add(new MotorizedElement(card as UnitCard, battleSystem));
									break;
								case "Artillery":
									deck.Add(new ArtilleryElement(card as UnitCard, battleSystem));
									break;
								case "Guardian":
									deck.Add(new GuardianElement(card as UnitCard, battleSystem));
									break;
								case "Construction":
									deck.Add(new ConstructionElement(card as UnitCard, battleSystem));
									break;
							}
						}
						else
						{
							deck.Add(new CommandElement(card as CommandCard, battleSystem));
						}
					}
				}
				//CRITICAL
				for(int i = 0; i < count; i++)
				{
					deck[i].battleID = i;
				}
			}
			else
			{
				foreach (Card card in pool.cardPool)
				{
					if (card.ownership == 1)
					{
						if (card is UnitCard)
						{
							string category = (card as UnitCard).category;
							switch (category)
							{
								case "LightArmor":
									deck.Add(new LightArmorElement(card as UnitCard, battleSystem));
									break;
								case "Motorized":
									deck.Add(new MotorizedElement(card as UnitCard, battleSystem));
									break;
								case "Artillery":
									deck.Add(new ArtilleryElement(card as UnitCard, battleSystem));
									break;
								case "Guardian":
									deck.Add(new GuardianElement(card as UnitCard, battleSystem));
									break;
								case "Construction":
									deck.Add(new ConstructionElement(card as UnitCard, battleSystem));
									break;
							}
						}
						else
						{
							deck.Add(new CommandElement(card as CommandCard, battleSystem));
						}
					}
				}
				//CRITICAL
				for(int i = 0; i < count; i++)
				{
					deck[i].battleID = -1 - i;
				}
			}
		}
		internal void Clear()
		{
			deck.Clear();
		}
	}


	internal class Terrain
	{
		internal int index;
		internal Terrain prevTerrain;

		internal List<List<Node>> nodeList;

		internal Node resNode
		{
			get => nodeList[0][0];
		}
		internal Node dstNode
		{
			get => nodeList[length - 1][0];
		}


		public int lengthUpperBound;
		public int lengthLowerBound;
		public int widthUpperBound;
		public int widthLowerBound;


		private int length;
		private List<int> width;

		internal List<Node> this[int index]
		{
			get => nodeList[index];
			set => nodeList[index] = value;
		}

		internal Terrain(int index)
		{
			prevTerrain = null;
			//TODO config
			lengthLowerBound = 3;
			lengthUpperBound = 4;
			widthLowerBound = 2;
			widthUpperBound = 3;

			nodeList = new List<List<Node>>();

			this.index = index;
		}
		/// <summary>
		/// generate node and path data in a terrain
		/// </summary>
		internal void GenerateNodes()
		{
			Random random = new Random();
			length = random.Next(lengthLowerBound, lengthUpperBound + 1);

			width = new List<int>(length);
			width[0] = 1;
			width[length - 1] = 1;

			for (int i = 1; i < length - 1; i++)
			{
				width[i] = random.Next(widthLowerBound, widthUpperBound + 1);
			}

			//生成节点
			for (int i = 0; i < length; i++)
			{
				nodeList.Add(new List<Node>(width[i]));

				for (int j = 0; j < width[i]; j++)
				{
					if (i == 0)
					{
						//如果是源terrain
						if(prevTerrain == null)
						{
							nodeList[i].Add(new SourceNode(i, j, this));
						}
						//否则源节点是上一terrain的目的节点
						else
						{
							nodeList[i].Add(prevTerrain.dstNode);
						}
					}
					//每一个terrain的目的节点一定是战斗节点
					else if (i == length - 1)
					{
						nodeList[i].Add(new BattleNode(i, j, this));
					}
					else
					{
						int randomEnum = random.Next(0, 3);
						switch (randomEnum)
						{
							case 0:
								nodeList[i].Add(new OutPostNode(i, j, this));
								break;
							case 1:
								nodeList[i].Add(new LegacyNode(i, j, this));
								break;
							case 2:
								nodeList[i].Add(new MedicalNode(i, j, this));
								break;
						}
					}
				}
			}
			//生成边
			for(int i = 0; i < length - 1; i++)
			{
				for(int j = 0; j < width[i]; j++)
				{
					//源节点与下一层节点全部连通
					if(i == 0)
					{
						for(int k = 0; k < width[i + 1]; k++)
						{
							nodeList[i][j].adjNode[k] = nodeList[i + 1][k];
						}
						break;
					}
					//其余节点最多与其他两个节点连通
					int nodeIdx1 = random.Next(0, width[i + 1]);
					int nodeIdx2 = random.Next(0, width[i + 1]);
					if(nodeIdx1 == nodeIdx2)
					{
						nodeList[i][j].adjNode[0] = nodeList[i + 1][nodeIdx1];
						nodeList[i][j].adjNode[1] = null;
					}
					else
					{
						nodeList[i][j].adjNode[0] = nodeList[i + 1][nodeIdx1];
						nodeList[i][j].adjNode[1] = nodeList[i + 1][nodeIdx2];
					}
				}
			}
		}

	}






	internal class Node
	{
		internal INodeController controller;

		internal Terrain terrain;

		internal int horizontalIdx;
		internal int verticalIdx;

		internal Node[] adjNode;

		internal Node(int horizontalIdx, int verticalIdx, Terrain terrain)
		{
			this.horizontalIdx = horizontalIdx;
			this.verticalIdx = verticalIdx;

			//TODO length
			adjNode = new Node[3] { null, null, null };
			this.terrain = terrain;
		}

		//display
		internal void SetPosition()
		{

		}
	}
	internal sealed class SourceNode : Node
	{
		internal SourceNode(int horizontalIdx, int verticalIdx, Terrain terrain) : base(horizontalIdx, verticalIdx, terrain) { }
	}
	internal sealed class BattleNode : Node
	{
		internal BattleSystem system;

		internal string plots;
		internal string enemyConfig;

		internal int fieldCapacity;
		internal int[] battleLinesLength;
		internal int[] initialEnergy;

		internal BattleNode(int horizontalIdx, int verticalIdx, Terrain terrain) : base(horizontalIdx, verticalIdx, terrain)
		{
		}
	}
	internal sealed class OutPostNode : Node
	{
		internal OutPostNode(int horizontalIdx, int verticalIdx, Terrain terrain) : base(horizontalIdx, verticalIdx, terrain)
		{

		}
	}
	internal sealed class LegacyNode : Node
	{
		internal int legacy;
		internal LegacyNode(int horizontalIdx, int verticalIdx, Terrain terrain) : base(horizontalIdx, verticalIdx, terrain)
		{

		}
	}
	internal sealed class MedicalNode : Node
	{
		internal int cost;
		internal MedicalNode(int horizontalIdx, int verticalIdx, Terrain terrain) : base(horizontalIdx, verticalIdx, terrain)
		{

		}
	}
}