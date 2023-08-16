using DataCore.BattleElements;
using DataCore.BattleItems;
using DataCore.Cards;
using DataCore.CultivateItems;
using DisplayInterface;
using LogicCore;
using NetMQ.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Unity.Plastic.Newtonsoft.Json;

namespace DataCore.TacticalItems
{
	internal class Deck
	{
		private IDeckController Controller;
		internal IDeckController controller
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

		BattleSystem battleSystem;
		TacticalSystem tacticalSystem;
		private List<BattleElement> deck;

		internal Dictionary<int, LightArmorElement> lightArmorSet;
		internal Dictionary<int, MotorizedElement> motorizedSet;
		internal Dictionary<int, ArtilleryElement> artillerySet;
		internal Dictionary<int, GuardianElement> guardianSet;
		internal Dictionary<int, ConstructionElement> constructionSet;

		internal Dictionary<int, CommandElement> commandSet;
		internal int count { get => deck.Count; }

		internal UnitElement bases;
		//TODO remove
		internal Deck(BattleSystem battleSystem, TacticalSystem tacticalSystem, IDeckController controller)
		{
			deck = new List<BattleElement>();
			lightArmorSet = new Dictionary<int, LightArmorElement>();
			motorizedSet = new Dictionary<int, MotorizedElement>();
			artillerySet = new Dictionary<int, ArtilleryElement>();
			guardianSet = new Dictionary<int, GuardianElement>();
			constructionSet = new Dictionary<int, ConstructionElement>();
			commandSet = new Dictionary<int, CommandElement>();

			this.battleSystem = battleSystem;
			this.tacticalSystem = tacticalSystem;
			this.controller = controller;
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
		internal void AddTag(BattleElement element)
		{
			deck.Add(element);
			deck.Sort();
			UpdateDeckID();
			//显示层自动更新层级
		}
		//TODO controller rebuild
		internal void LoadDeckByPathHuman(string path)
		{
			StreamReader reader = File.OpenText(path);

			string ID = reader.ReadLine();
			bases = new ConstructionElement(tacticalSystem.pool.GetCardByID(ID) as UnitCard, battleSystem, null);
			ID = reader.ReadLine();

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
			deck.Sort();
			UpdateDeckID();

			for(int i = 0; i < deck.Count; i++)
			{
				switch (deck[i].category)
				{
					case "LightArmor":
						LightArmorElement lightArmor = deck[i] as LightArmorElement;
						lightArmorSet.Add(i, lightArmor);
						break;
					case "Motorized":
						MotorizedElement motorized = deck[i] as MotorizedElement;
						motorizedSet.Add(i, motorized);
						break;
					case "Artillery":
						ArtilleryElement artillery = deck[i] as ArtilleryElement;
						artillerySet.Add(i, artillery);
						break;
					case "Guardian":
						GuardianElement guardian = deck[i] as GuardianElement;
						guardianSet.Add(i, guardian);
						break;
					case "Construction":
						ConstructionElement construction = deck[i] as ConstructionElement;
						constructionSet.Add(i, construction);
						break;
					case "Command":
						CommandElement command = deck[i] as CommandElement;
						commandSet.Add(i, command);
						break;
				}
			}
			//CRITICAL
			UpdateBattleID();

			InstantiateDeckTags();
		}
		internal void LoadDeckByPathPlant(string path)
		{
			StreamReader reader = File.OpenText(path);

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

			UpdateBattleID();
		}
		private void UpdateDeckID()
		{
			for(int i = 0; i < deck.Count; i++)
			{
				deck[i].deckID = i;
			}
		}
		private void UpdateBattleID()
		{
			for (int i = 0; i < count; i++)
			{
				deck[i].battleID = deck[i].ownership == 0 ? i : -1 - i;
			}
		}
		internal void WriteBack()
		{
			RandomCardStack playerStack = battleSystem.stacks[0];
			//lightArmorSet.Clear();
			//motorizedSet.Clear();
			//artillerySet.Clear();
			//guardianSet.Clear();
			//constructionSet.Clear();
			//commandSet.Clear();

			deck.Clear();
			for(int i = 0; i < playerStack.count; i++)
			{
				deck.Add(playerStack.stack[i]);
			}
			for(int i = 0; i < battleSystem.linesCapacity; i++)
			{
				for(int j = 0; j < battleSystem.battleLines[i].count; j++)
				{
					if (battleSystem.battleLines[i][j].ownership == 0)
					{
						deck.Add(battleSystem.battleLines[i][j]);
					}
				}
			}

			foreach(BattleElement element in deck)
			{
				if(element is UnitElement)
				{
					UnitElement unit = element as UnitElement;
					unit.UnloadEffects();
				}
			}

			deck.Sort();
			UpdateDeckID();
			UpdateBattleID();
		}
		internal void InstantiateDeckTags()
		{
			controller.UnloadDeckTags();
			foreach(BattleElement element in deck)
			{
				controller.InstantiateDeckTag(element.backendID, element.name, element.category, element.deckID, element.description);
				if(element.category == "Command")
				{
					CommandElement comm = element as CommandElement;
					controller.UpdateCommandTagInfo(comm.deckID, comm.cost, comm.dynDurability);
				}
				else
				{
					UnitElement unit = element as UnitElement;
					//TODO check
					controller.UpdateUnitTagInfo(unit.category, unit.deckID, unit.cost, unit.dynAttackWriter, unit.dynHealth, unit.oriHealth, unit.oriAttackCounter);
				}
			}
			controller.UpdateHierachy();
		}
		public void UnitHealed(int deckID)
		{
			UnitElement unit = deck[deckID] as UnitElement;
			controller.UpdateHealth(deckID, unit.dynHealth);
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
							battleSystem, "Assets\\Config\\NodeConfigs\\BattleNode_" + 4 + ".json");
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
								//TODO
								MedicalNode pn = new MedicalNode(i, j, this,
									controller.InstantiateNode(length, width[i], i, j, "medical"));
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
			BattleSystem system, string configPath)
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{
			nextTerrain = null;
			battleSystem = system;
			//TODO config
			battleConfigPath = configPath;

			string jsonString = File.ReadAllText(battleConfigPath);
			battleConfig = JsonConvert.DeserializeObject<BattleConfigJson>(jsonString);

			plantDeck = new Deck(battleSystem, tacticalSystem, null);
			plantDeck.LoadDeckByPathPlant(battleConfig.plantDeckPath);
		}
		internal override void CastNodeEvent()
		{
			base.CastNodeEvent();
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
		internal List<BattleElement> commercials;
		internal OutPostNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller) 
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{
			//TODO config
			commercials = new List<BattleElement>();
			BuildCommercials();
		}
		internal void BuildCommercials()
		{
			List<string> IDs = new List<string>();
			List<string> names = new List<string>();
			List<string> categories = new List<string>();
			List<int> costs = new List<int>();
			List<int> attacks = new List<int>();
			List<int> healths = new List<int>();
			List<int> counters = new List<int>();
			List<int> gasMineCosts = new List<int>();
			List<string> descriptions = new List<string>();

			Random random = new Random();
			//TODO config
			for(int i = 0; i < 8; i++)
			{
				int index = random.Next(0, tacticalSystem.pool.humanCardPool.Count);
				while (tacticalSystem.pool.humanCardPool[index].backendID.Contains("base"))
				{
					index = random.Next(0, tacticalSystem.pool.humanCardPool.Count);
				}
				Card card = tacticalSystem.pool.humanCardPool[index];

				IDs.Add(card.backendID);
				names.Add(card.name);
				categories.Add(card.category);
				costs.Add(card.cost);
				gasMineCosts.Add(card.gasMineCost);
				descriptions.Add(card.description);

				string category = tacticalSystem.pool.humanCardPool[index].category;
				switch (category)
				{
					case "LightArmor":
						LightArmorElement lightArmor = new(tacticalSystem.pool.humanCardPool[index] as UnitCard,
							tacticalSystem.battleSystem, null);
						commercials.Add(lightArmor);
						attacks.Add(lightArmor.oriAttack);
						healths.Add(lightArmor.oriHealth);
						counters.Add(lightArmor.oriAttackCounter);
						break;
					case "Motorized":
						MotorizedElement motorized = new(tacticalSystem.pool.humanCardPool[index] as UnitCard,
							tacticalSystem.battleSystem, null);
						commercials.Add(motorized);
						attacks.Add(motorized.oriAttack);
						healths.Add(motorized.oriHealth);
						counters.Add(motorized.oriAttackCounter);
						break;
					case "Artillery":
						ArtilleryElement artillery = new(tacticalSystem.pool.humanCardPool[index] as UnitCard,
							tacticalSystem.battleSystem, null);
						commercials.Add(artillery);
						attacks.Add(artillery.oriAttack);
						healths.Add(artillery.oriHealth);
						counters.Add(artillery.oriAttackCounter);
						break;
					case "Guardian":
						GuardianElement guardian = new(tacticalSystem.pool.humanCardPool[index] as UnitCard,
							tacticalSystem.battleSystem, null);
						commercials.Add(guardian);
						attacks.Add(guardian.oriAttack);
						healths.Add(guardian.oriHealth);
						counters.Add(guardian.oriAttackCounter);
						break;
					case "Construction":
						ConstructionElement construction = new(tacticalSystem.pool.humanCardPool[index] as UnitCard,
							tacticalSystem.battleSystem, null);
						commercials.Add(construction);
						attacks.Add(construction.oriAttack);
						healths.Add(construction.oriHealth);
						counters.Add(construction.oriAttackCounter);
						break;
					case "Command":
						CommandElement command = new(tacticalSystem.pool.humanCardPool[index] as CommandCard,
							tacticalSystem.battleSystem);
						commercials.Add(command);
						//防止错位
						attacks.Add(0);
						healths.Add(0);
						counters.Add(command.oriDurability);
						break;
				}
			}

			controller.DisplayElement(IDs, names, categories, costs, attacks, healths, counters, gasMineCosts, descriptions);
		}
		internal BattleElement Purchase(int index)
		{
			tacticalSystem.gasMineToken -= commercials[index].gasMineCost;
			return commercials[index];
		}
	}
	internal sealed class LegacyNode : Node
	{
		internal int legacy;
		internal LegacyNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller) 
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{
			Random random = new Random();
			//TODO
			legacy = random.Next(5, 10);
			controller.UpdateBasicInfo(legacy, 0);
		}
		internal override void CastNodeEvent()
		{
			base.CastNodeEvent();
			tacticalSystem.gasMineToken += legacy;
		}
	}
	internal sealed class MedicalNode : Node
	{
		//TODO
		internal int pricePerHealth;
		internal MedicalNode(int horizontalIdx, int verticalIdx, Terrain terrain, INodeController controller) 
			: base(horizontalIdx, verticalIdx, terrain, controller)
		{
			//TODO config
			pricePerHealth = 4;
			controller.UpdateBasicInfo(0, pricePerHealth);
		}
		internal void HealElement(bool fullfill, UnitElement unit)
		{
			int gasMineCost = (unit.maxHealthWriter - unit.dynHealth) * pricePerHealth;
			if (fullfill)
			{
				tacticalSystem.gasMineToken -= gasMineCost;
				unit.dynHealth = unit.maxHealthWriter;
				tacticalSystem.playerDeck.UnitHealed(unit.deckID);
				controller.UpdateHealth(unit.dynHealth);
			}
			else
			{
				tacticalSystem.gasMineToken -= pricePerHealth;
				unit.dynHealth += 1;
				tacticalSystem.playerDeck.UnitHealed(unit.deckID);
				controller.UpdateHealth(unit.dynHealth);
			}
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
			BuildSupply();
		}
		internal void BuildSupply()
		{
			List<string> IDs = new List<string>();
			List<string> names = new List<string>();
			List<string> categories = new List<string>();
			List<int> costs = new List<int>();
			List<int> attacks = new List<int>();
			List<int> healths = new List<int>();
			List<int> counters = new List<int>();
			List<int> gasMineCosts = new List<int>();
			List<string> descriptions = new List<string>();

			Random random = new Random();
			int categoryEnum = random.Next(0, 6);
			int index = 0;
			//TODO config
			for (int i = 0; i < 4; i++)
			{
				switch (categoryEnum)
				{
					case 0:
						category = "LightArmor";
						index = random.Next(0, tacticalSystem.pool.humanLightArmorSet.Count);
						supply.Add(new LightArmorElement(tacticalSystem.pool.humanLightArmorSet[index] as UnitCard,
							tacticalSystem.battleSystem, null));
						break;
					case 1:
						category = "Motorized";
						index = random.Next(0, tacticalSystem.pool.humanMotorizedSet.Count);
						supply.Add(new MotorizedElement(tacticalSystem.pool.humanMotorizedSet[index] as UnitCard,
							tacticalSystem.battleSystem, null));
						break;
					case 2:
						category = "Artillery";
						index = random.Next(0, tacticalSystem.pool.humanArtillerySet.Count);
						supply.Add(new ArtilleryElement(tacticalSystem.pool.humanArtillerySet[index] as UnitCard,
							tacticalSystem.battleSystem, null));
						break;
					case 3:
						category = "Guardian";
						index = random.Next(0, tacticalSystem.pool.humanGuardianSet.Count);
						supply.Add(new GuardianElement(tacticalSystem.pool.humanGuardianSet[index] as UnitCard,
							tacticalSystem.battleSystem, null));
						break;
					case 4:
						category = "Construction";
						index = random.Next(0, tacticalSystem.pool.humanConstructionSet.Count);
						while (tacticalSystem.pool.humanConstructionSet[index].backendID.Contains("base"))
						{
							index = random.Next(0, tacticalSystem.pool.humanConstructionSet.Count);
						}
						supply.Add(new ConstructionElement(tacticalSystem.pool.humanConstructionSet[index] as UnitCard,
							tacticalSystem.battleSystem, null));
						break;
					case 5:
						category = "Command";
						index = random.Next(0, tacticalSystem.pool.humanCommandSet.Count);
						supply.Add(new CommandElement(tacticalSystem.pool.humanCommandSet[index] as CommandCard,
							tacticalSystem.battleSystem));
						break;
				}
			}
			for(int i = 0; i < 4; i++)
			{
				BattleElement element = supply[i];

				IDs.Add(element.backendID);
				names.Add(element.name);
				categories.Add(element.category);
				costs.Add(element.cost);
				gasMineCosts.Add(element.gasMineCost);
				descriptions.Add(element.description);
				if(element is UnitElement)
				{
					UnitElement unit = element as UnitElement;
					attacks.Add(unit.oriAttack);
					healths.Add(unit.oriHealth);
					counters.Add(unit.oriAttackCounter);
				}
				else
				{
					CommandElement comm = element as CommandElement;
					attacks.Add(0);
					healths.Add(0);
					counters.Add(comm.oriDurability);
				}
			}

			controller.DisplayElement(IDs, names, categories, costs, attacks, healths, counters, gasMineCosts, descriptions);
		}
		internal BattleElement Choose(int index)
		{
			return supply[index];
		}
	}
}