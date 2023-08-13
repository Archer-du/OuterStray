using DataCore.BattleElements;
using DataCore.Cards;
using DataCore.CultivateItems;
using DisplayInterface;
using LogicCore;
using Serilog.Formatting.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;

namespace DataCore.TacticalItems
{
	internal class Deck
	{
		BattleSystem battleSystem;
		TacticalSystem tacticalSystem;
		private List<BattleElement> deck;
		internal int count { get => deck.Count; }

		//TODO remove
		internal Deck(BattleSystem system, TacticalSystem tacticalSystem)
		{
			deck = new List<BattleElement>();
			battleSystem = system;
			this.tacticalSystem = tacticalSystem;
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
		//TODO
		internal void LoadDeckByPath(string path)
		{
			StreamReader reader = File.OpenText("Assets\\Config\\HumanDeckTest.csv");

			string ID = reader.ReadLine();

			while (ID != null)
			{
				Card card = tacticalSystem.pool.GetCardByID(ID);

				if (card is UnitCard)
				{
					string category = (card as UnitCard).category;
					switch (category)
					{
						case "LightArmor":
							deck.Add(new LightArmorElement(card as UnitCard, battleSystem, null));
							break;
						case "Motorized":
							deck.Add(new MotorizedElement(card as UnitCard, battleSystem, null));
							break;
						case "Artillery":
							deck.Add(new ArtilleryElement(card as UnitCard, battleSystem, null));
							break;
						case "Guardian":
							deck.Add(new GuardianElement(card as UnitCard, battleSystem, null));
							break;
						case "Construction":
							deck.Add(new ConstructionElement(card as UnitCard, battleSystem, null));
							break;
					}
				}
				else
				{
					deck.Add(new CommandElement(card as CommandCard, battleSystem));
				}
				ID = reader.ReadLine();
			}
			reader.Close();

			//CRITICAL
			UpdateBattleID();
		}
		private void UpdateBattleID()
		{
			for (int i = 0; i < count; i++)
			{
				deck[i].battleID = deck[i].ownership == 0 ? i : -1 - i;
			}
		}

		internal void Clear()
		{
			deck.Clear();
		}
	}


	internal class Terrain
	{
		private ITerrainController Controller;
		internal ITerrainController controller
		{
			get => Controller;
			set
			{
				if (value != null)
				{
					Controller = value;
					Controller.Init();
				}
				else { Controller = null; }
			}
		}

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

		internal int interactNodeNum
		{
			//TODO config
			get => 2;
		}
		internal int supplyNodeNum
		{
			get => totalNodeNum - interactNodeNum - 2 + 1;
		}

		internal List<Node> this[int index]
		{
			get => nodeList[index];
			set => nodeList[index] = value;
		}

		internal Terrain(int index, TacticalSystem tacticalSystem, BattleSystem battleSystem, ITerrainController controller)
		{
			this.tacticalSystem = tacticalSystem;
			this.battleSystem = battleSystem;

			prevTerrain = null;

			this.index = index;

			InitializeBasicConfig();

			//display
			this.controller = controller;
		}
		internal void InitializeBasicConfig()
		{
			nodeList = new List<List<Node>>();

			//TODO config
			lengthLowerBound = 4;
			lengthUpperBound = 4;
			widthLowerBound = 2;
			widthUpperBound = 3;

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
			for (int i = 0; i < length; i++)
			{
				totalNodeNum += width[i];
			}
		}
		/// <summary>
		/// generate node and path data in a terrain
		/// </summary>
		internal void GenerateNodes()
		{
			Random random = new Random();

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
							SourceNode s = new(i, j, this, 
								controller.InstantiateNode(length, width[i], i, j, "source"));
							s.controller = controller.InstantiateNode(length, width[i], i, j, "source");
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
						BattleNode bn = new(i, j, this,
							controller.InstantiateNode(length, width[i], i, j, "battle"),
							battleSystem);
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
								PromoteNode pn = new PromoteNode(i, j, this,
									controller.InstantiateNode(length, width[i], i, j, "promote"));
								nodeList[i].Add(pn);
							}
							if(randSeed >= 3 && randSeed < 6)
							{
								LegacyNode ln = new LegacyNode(i, j, this,
									controller.InstantiateNode(length, width[i], i, j, "legacy"));
								nodeList[i].Add(ln);
							}
							if(randSeed >= 6 && randSeed < 8)
							{
								OutPostNode on = new OutPostNode(i, j, this,
									controller.InstantiateNode(length, width[i], i, j, "outpost"));
								nodeList[i].Add(on);
							}
							if(randSeed >= 8 && randSeed < 10)
							{
								MedicalNode mn = new MedicalNode(i, j, this,
									controller.InstantiateNode(length, width[i], i, j, "medical"));
								nodeList[i].Add(mn);
							}
							itNodeCounter--;
						}
						//确定生成补给类节点
						else
						{
							int randSeed = random.Next(0, 6);
							SupplyNode sn = new SupplyNode(i, j, this,
								controller.InstantiateNode(length, width[i], i, j, "supply"));
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
	}






	internal class Node
	{
		internal TacticalSystem tacticalSystem;

		private INodeController Controller;
		internal INodeController controller
		{
			get => Controller;
			set
			{
				if (value != null)
				{
					Controller = value;
					Controller.Init();
				}
				else { Controller = null; }
			}
		}

		internal Terrain terrain;

		internal int horizontalIdx;
		internal int verticalIdx;

		internal Node[] adjNode;
		internal int inDegree;

		internal bool casted;

		internal Node(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller)
		{
			inDegree = 0;
			casted = false;
			this.horizontalIdx = horizontalIdx;
			this.verticalIdx = verticalIdx;

			//TODO length
			adjNode = new Node[3] { null, null, null };
			this.terrain = terrain;
			this.tacticalSystem = terrain.tacticalSystem;

			this.controller = controller;
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

		/// <summary>
		/// 设置邻接节点，更新控件逻辑节点
		/// </summary>
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
			controller.SetAdjacentNode(adjList);
		}
	}




	internal sealed class SourceNode : Node
	{
		internal SourceNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller) 
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{
			casted = true;
		}
	}



	internal sealed class BattleNode : Node
	{
		internal BattleSystem battleSystem;

		internal Terrain nextTerrain;

		internal string battleConfigPath;
		internal BattleConfigJson battleConfig;

		internal BattleNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller,
			BattleSystem system)
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{
			nextTerrain = null;
			battleSystem = system;
			//TODO config
			battleConfigPath = "Assets\\Config\\NodeConfigs\\TotorialNode.json";

			string jsonString = File.ReadAllText(battleConfigPath);
			battleConfig = JsonConvert.DeserializeObject<BattleConfigJson>(jsonString);

			plantDeck = new Deck(battleSystem, tacticalSystem);
			plantDeck.LoadDeckByPath(battleConfig.plantDeckPath);
		}
		internal override void CastNodeEvent()
		{
			battleSystem.BuildBattleField(playerDeck, plantDeck, fieldCapacity, battleLinesCapacity, 
				initialTurn, initialHumanEnergy, initialPlantEnergy, initialHumanHandicaps, initialPlantHandicaps,
				fieldPresets);
		}



		internal int initialTurn
		{
			get => battleConfig.initialTurn;
		}
		internal int fieldCapacity
		{
			get => battleConfig.fieldCapacity;
		}
		internal int[] battleLinesCapacity
		{
			get => battleConfig.battleLinesCapacity;
		}
		internal int initialHumanEnergy
		{
			get => battleConfig.initialHumanEnergy;
		}
		internal int initialPlantEnergy
		{
			get => battleConfig.initialPlantEnergy;
		}
		internal int initialHumanHandicaps
		{
			get => battleConfig.initialHumanHandicaps;
		}
		internal int initialPlantHandicaps
		{
			get => battleConfig.initialPlantHandicaps;
		}
		internal List<FieldPreset> fieldPresets
		{
			get => battleConfig.fieldPreset;
		}
		internal Deck playerDeck
		{
			get => tacticalSystem.playerDeck;
		}
		internal Deck plantDeck;

		[Serializable]
		internal class BattleConfigJson
		{
			[JsonProperty("initialTurn")]
			internal int initialTurn;

			[JsonProperty("fieldCapacity")]
			internal int fieldCapacity;

			[JsonProperty("battleLinesCapacity")]
			internal int[] battleLinesCapacity;

			[JsonProperty("plantDeckPath")]
			internal string plantDeckPath;

			[JsonProperty("initialHumanEnergy")]
			internal int initialHumanEnergy;

			[JsonProperty("initialPlantEnergy")]
			internal int initialPlantEnergy;

			[JsonProperty("initialHumanHandicaps")]
			internal int initialHumanHandicaps;

			[JsonProperty("initialPlantHandicaps")]
			internal int initialPlantHandicaps;

			[JsonProperty("fieldPreset")]
			internal List<FieldPreset> fieldPreset;
		}
		[Serializable]
		internal class FieldPreset
		{
			[JsonProperty("cardPreset")]
			public CardPreset cardPreset;

			[JsonProperty("position")]
			public int position;
		}
		[Serializable]
		internal class CardPreset
		{
			[JsonProperty("boss")]
			public bool boss;

			[JsonProperty("backendID")]
			public string ID;

			[JsonProperty("attack")]
			public int attackPreset;

			[JsonProperty("health")]
			public int healthPreset;

			[JsonProperty("attackCounter")]
			public int attackCounterPreset;
		}
	}





	internal sealed class OutPostNode : Node
	{
		internal List<Pack> commercials;
		internal OutPostNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller) 
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{
			//TODO config
			commercials = new List<Pack>();
		}
		internal void BuildCommercials()
		{
			
		}

		internal class Pack
		{
			internal List<BattleElement> pack;
			internal int packCapacity;

			internal Pack()
			{
				packCapacity = 4;
				pack = new List<BattleElement>();
			}

			internal void BuildPack()
			{
				for(int i = 0; i < packCapacity; i++)
				{
					//pack.Add(new BattleElement());
				}
			}
		}
	}
	internal sealed class LegacyNode : Node
	{
		internal int legacy;
		internal LegacyNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller) 
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{

		}
	}
	internal sealed class MedicalNode : Node
	{
		internal int cost;
		internal MedicalNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller) 
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{

		}
	}
	internal sealed class PromoteNode : Node
	{
		internal PromoteNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller) 
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{
		}

	}
	internal sealed class SupplyNode : Node
	{
		internal string category;
		internal List<BattleElement> supply;
		internal SupplyNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller) 
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{
			supply = new List<BattleElement>();
		}
	}
}