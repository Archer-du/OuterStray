using DataCore.BattleElements;
using DataCore.Cards;
using DataCore.CultivateItems;
using DisplayInterface;
using LogicCore;
using Serilog.Formatting.Json;
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
		internal ITerrainController controller;

		internal TacticalSystem tacticalSystem;
		internal BattleSystem battleSystem;

		internal int index;
		internal Terrain prevTerrain;

		internal List<List<Node>> nodeList;

		internal Node resNode
		{
			get => nodeList[0][0];
		}
		internal BattleNode dstNode
		{
			get => nodeList[length - 1][0] as BattleNode;
		}


		public int lengthUpperBound;
		public int lengthLowerBound;
		public int widthUpperBound;
		public int widthLowerBound;


		private int length;
		private List<int> width;

		internal int totalNodeNum;

		internal int interactNodeNum;
		internal int supplyNodeNum;

		internal List<Node> this[int index]
		{
			get => nodeList[index];
			set => nodeList[index] = value;
		}

		internal Terrain(int index, TacticalSystem tacticalSystem, BattleSystem battleSystem)
		{
			this.tacticalSystem = tacticalSystem;
			this.battleSystem = battleSystem;

			prevTerrain = null;
			//TODO config
			lengthLowerBound = 4;
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
			width.Add(1);
			for (int i = 1; i < length - 1; i++)
			{
				width.Add(random.Next(widthLowerBound, widthUpperBound + 1));
			}
			width.Add(1);
			totalNodeNum = 0;
			for(int i = 0; i < length; i++)
			{
				totalNodeNum += width[i];
			}

			//TODO
			interactNodeNum = 2;
			supplyNodeNum = totalNodeNum - interactNodeNum - 2 + 1;



			//生成节点
			int itNodeCounter = interactNodeNum;
			int SplNodeCounter = supplyNodeNum;

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
							//SourceNode 全局唯一
							SourceNode s = new SourceNode(i, j, this);
							s.controller = controller.InstantiateNode(length, width[i], i, j, "source");
							s.controller.Init();
							nodeList[i].Add(s);
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
						BattleNode bn = new BattleNode(i, j, this, battleSystem);
						bn.controller = controller.InstantiateNode(length, width[i], i, j, "battle");
						bn.controller.Init();
						nodeList[i].Add(bn);
					}
					//以一定权重随机生成 TODO
					else
					{
						int randomEnum = random.Next(0, itNodeCounter + SplNodeCounter);
						//确定生成互动类节点
						if(randomEnum < itNodeCounter)
						{
							int randSeed = random.Next(0, 10);
							if(randSeed >= 0 && randSeed < 3)
							{
								PromoteNode pn = new PromoteNode(i, j, this);
								pn.controller = controller.InstantiateNode(length, width[i], i, j, "promote");
								pn.controller.Init();
								nodeList[i].Add(pn);
							}
							if(randSeed >= 3 && randSeed < 6)
							{
								LegacyNode ln = new LegacyNode(i, j, this);
								ln.controller = controller.InstantiateNode(length, width[i], i, j, "legacy");
								ln.controller.Init();
								nodeList[i].Add(ln);
							}
							if(randSeed >= 6 && randSeed < 8)
							{
								OutPostNode on = new OutPostNode(i, j, this);
								on.controller = controller.InstantiateNode(length, width[i], i, j, "outpost");
								on.controller.Init();
								nodeList[i].Add(on);
							}
							if(randSeed >= 8 && randSeed < 10)
							{
								MedicalNode mn = new MedicalNode(i, j, this);
								mn.controller = controller.InstantiateNode(length, width[i], i, j, "medical");
								mn.controller.Init();
								nodeList[i].Add(mn);
							}
							itNodeCounter--;
						}
						//确定生成补给类节点
						else
						{
							int randSeed = random.Next(0, 6);
							SupplyNode sn = new SupplyNode(i, j, this);
							sn.controller = controller.InstantiateNode(length, width[i], i, j, "supply");
							sn.controller.Init();
							nodeList[i].Add(sn);
							switch (randSeed)
							{
								case 0:
									sn.category = "LightArmor";
									break;
								case 1:
									sn.category = "Motorized";
									break;
								case 2:
									sn.category = "Artillery";
									break;
								case 3:
									sn.category = "Guardian";
									break;
								case 4:
									sn.category = "Construction";
									break;
								case 5:
									sn.category = "Command";
									break;
							}
							SplNodeCounter--;
						}
					}
				}
			}
			//生成边
			for(int i = 0; i < length - 1; i++)
			{
				int connectionCounter = width[i + 1];
				for(int j = 0; j < width[i]; j++)
				{
					//源节点与下一层节点全部连通
					if(i == 0)
					{
						//TODO
						for(int k = 0; k < width[i + 1]; k++)
						{
							nodeList[i][j].adjNode[k] = nodeList[i + 1][k];
							nodeList[i + 1][k].inDegree++;
						}
						break;
					}
					//其余节点大概率仅与其他一个或两个节点连通
					int nodeIdx1 = random.Next(0, width[i + 1]);
					int nodeIdx2 = random.Next(0, width[i + 1]);
					if(nodeIdx1 == nodeIdx2)
					{
						nodeList[i][j].adjNode[0] = nodeList[i + 1][nodeIdx1];
						nodeList[i][j].adjNode[1] = null;
						nodeList[i + 1][nodeIdx1].inDegree++;
					}
					else
					{
						nodeList[i][j].adjNode[0] = nodeList[i + 1][nodeIdx1];
						nodeList[i][j].adjNode[1] = nodeList[i + 1][nodeIdx2];
						nodeList[i + 1][nodeIdx1].inDegree++;
						nodeList[i + 1][nodeIdx2].inDegree++;
					}
				}
				//维护连通性
				for(int j = 0; j < width[i + 1]; j++)
				{
					if (nodeList[i + 1][j].inDegree == 0)
					{
						int randomEnum = random.Next(0, width[i]);
						nodeList[i][randomEnum].adjNode[2] = nodeList[i + 1][j];
						nodeList[i + 1][j].inDegree++;
					}
				}
				for(int j = 0; j < width[i]; j++)
				{
					nodeList[i][j].SetAdjacentNode();
				}
			}
		}
		internal void Init()
		{
			controller.Init();
		}
	}






	internal class Node
	{
		internal TacticalSystem tacticalSystem;
		internal INodeController controller;

		internal Terrain terrain;

		internal int horizontalIdx;
		internal int verticalIdx;

		internal Node[] adjNode;
		internal int inDegree;

		internal bool casted;

		internal Node(int horizontalIdx, int verticalIdx, Terrain terrain)
		{
			inDegree = 0;
			casted = false;
			this.horizontalIdx = horizontalIdx;
			this.verticalIdx = verticalIdx;

			//TODO length
			adjNode = new Node[3] { null, null, null };
			this.terrain = terrain;
			this.tacticalSystem = terrain.tacticalSystem;
		}


		internal bool IsReachable(Node node)
		{
			foreach(Node n in adjNode)
			{
				if(n == node)
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsDstNodeCurTerrain()
		{
			return this == terrain.dstNode;
		}

		internal virtual void CastNodeEvent()
		{
			casted = true;
		}
		internal void SetAdjacentNode()
		{
			List<INodeController> adjList = new List<INodeController>();
			//TODO
			for(int i = 0; i < 3; i++)
			{
				if (adjNode[i] != null)
				{
					adjList.Add(adjNode[i].controller);
				}
			}
			if (adjList.Count == 0) throw new Exception("adj");
			controller.SetAdjacentNode(adjList);
		}
	}


	internal sealed class SourceNode : Node
	{
		internal SourceNode(int horizontalIdx, int verticalIdx, Terrain terrain) : base(horizontalIdx, verticalIdx, terrain) { }
	}
	internal sealed class BattleNode : Node
	{
		internal BattleSystem system;

		internal Terrain nextTerrain;

		internal string[] battleConfig;

		internal string plots;
		internal string enemyConfig;

		internal int fieldCapacity;
		internal int[] battleLinesLength;
		internal int[] initialEnergy;

		internal BattleNode(int horizontalIdx, int verticalIdx, Terrain terrain, BattleSystem system)
			: base(horizontalIdx, verticalIdx, terrain)
		{
			nextTerrain = null;
			this.system = system;
		}

		internal override void CastNodeEvent()
		{
			system.BuildBattleField(tacticalSystem.playerDeck, tacticalSystem.enemyDeck, 4);

			//system.FieldPreset();
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
	internal sealed class PromoteNode : Node
	{
		internal PromoteNode(int horizontalIdx, int verticalIdx, Terrain terrain) : base(horizontalIdx, verticalIdx, terrain)
		{
		}
	}
	internal sealed class SupplyNode : Node
	{
		internal string category;
		internal List<BattleElement> supply;
		internal SupplyNode(int horizontalIdx, int verticalIdx, Terrain terrain) : base(horizontalIdx, verticalIdx, terrain)
		{
			supply = new List<BattleElement>();
		}
	}
}