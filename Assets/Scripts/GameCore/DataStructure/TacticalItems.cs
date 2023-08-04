using DataCore.BattleElements;
using DataCore.Cards;
using DataCore.CultivateItems;
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

		//test TODO remove
		//internal int LoadDeck(int ownerShip, List<IUnitElementDisplay> csdspl)
		//{
		//	StreamReader reader = File.OpenText("Assets\\Config\\Test.csv");
		//	string[] data;
		//	int num = 0;
		//	string line = reader.ReadLine();
		//	int index = 0;
		//	while (line != null)
		//	{
		//		data = line.Split(",");
		//		if (data[0] == "#")
		//		{
		//			line = reader.ReadLine();
		//			continue;
		//		}
		//		UnitElement element = null;
		//		UnitCard card = null;
		//		//TODO
		//		switch (data[2])
		//		{
		//			case "Infantry":
		//				card = new InfantryCard(int.Parse(data[0]), data[1], data[7], int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]), int.Parse(data[6]));
		//				element = new UnitElement(card, ownerShip, csdspl[index]);
		//				deck.Add(element);
		//				break;
		//			case "Cavalry":
		//				card = new CavalryCard(int.Parse(data[0]), data[1], data[7], int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]), int.Parse(data[6]));
		//				element = new UnitElement(card, ownerShip, csdspl[index]);
		//				deck.Add(element);
		//				break;
		//			case "Artillery":
		//				card = new ArtilleryCard(int.Parse(data[0]), data[1], data[7], int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]), int.Parse(data[6]));
		//				element = new UnitElement(card, ownerShip, csdspl[index]);
		//				break;
		//		}
		//		//if (data[8] == "U")
		//		//	unlocked.Add(card);
		//		//else
		//		//	locked.Add(card);

		//		line = reader.ReadLine();
		//		num++;
		//		index++;
		//	}
		//	reader.Close();
		//	return num;
		//}
		internal void Clear()
		{
			deck.Clear();
		}
	}



	internal class Terrain
	{
		//internal TerrainCategories category { get; set; }

		internal string resPoint;
		internal string dstPoint;

		private int length;
		private List<int> width;

		internal Hashtable nodesTable;
		internal Terrain()//TODO config: TerrainCategories category
		{
			//this.category = category;
			nodesTable = new Hashtable();

		}
		/// <summary>
		/// generate node and path data in a terrain
		/// </summary>
		internal void GenerateNodeNormal()
		{
			Random random = new Random();
			length = random.Next(SystemConfig.minTerrainLength, SystemConfig.maxTerrainLength + 1);

			width = new List<int>(length);
			width[0] = 1;
			width[length - 1] = 1;

			for (int i = 1; i < length - 1; i++)
			{
				width[i] = random.Next(SystemConfig.minTerrainLength, SystemConfig.maxTerrainLength + 1);
			}

			nodesTable.Clear();
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < width[i]; j++)
				{
					if (i == 0)
					{
						//TODO
						int connects = random.Next(1, width[i + 1]);
						resPoint = i.ToString() + j.ToString();
						nodesTable.Add(resPoint, new ResNode(i, j));
					}
					else if (i == length - 1)
					{
						dstPoint = i.ToString() + j.ToString();
						nodesTable.Add(dstPoint, new DstNode(i, j));
					}
					else
					{
						int randomEnum = random.Next(0, 5);
						switch (randomEnum)
						{
							case 0:
								nodesTable.Add(i.ToString() + j.ToString(), new BattleNode(i, j));
								break;
							case 1:
								nodesTable.Add(i.ToString() + j.ToString(), new OutPostNode(i, j));
								break;
							case 2:
								nodesTable.Add(i.ToString() + j.ToString(), new AirdropNode(i, j));
								break;
							case 3:
								nodesTable.Add(i.ToString() + j.ToString(), new LegacyNode(i, j));
								break;
							case 4:
								nodesTable.Add(i.ToString() + j.ToString(), new MedicalNode(i, j));
								break;
						}
					}
				}
			}
		}

		internal void GenerateNodeRes()
		{

		}
		internal void GenerateNodeDst()
		{

		}
		private void ReBuildPath()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 返回index或point标识的节点
		/// </summary>
		/// <param name="hrztIdx"></param>
		/// <param name="vtcIdx"></param>
		/// <returns></returns>
		internal Node GetNode(int hrztIdx, int vtcIdx)
		{
			string key = hrztIdx.ToString() + vtcIdx.ToString();
			if (nodesTable.ContainsKey(key))
			{
				return nodesTable[key] as Node;
			}
			else return null;
		}
		/// <summary>
		/// 返回index或point标识的节点
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		internal Node GetNode(string point)
		{
			if (nodesTable.ContainsKey(point))
			{
				return nodesTable[point] as Node;
			}
			else return null;
		}
		internal Type GetNodeType(int hrztIdx, int vtcIdx)
		{
			string key = hrztIdx.ToString() + vtcIdx.ToString();
			if (nodesTable.ContainsKey(key))
			{
				return nodesTable[key].GetType();
			}
			else return null;
		}
		internal Type GetNodeType(string point)
		{
			if (nodesTable.ContainsKey(point))
			{
				return nodesTable[point].GetType();
			}
			else return null;
		}
		internal bool IsResNode(string point)
		{
			return point == resPoint;
		}
		internal bool IsDstNode(string point)
		{
			return point == dstPoint;
		}
		internal bool Reachable(string res, string dst)
		{
			Node resNode = GetNode(res);
			Node dstNode = GetNode(dst);
			if (IsResNode(dst))
			{
				return true;
				//TODO
			}
			else if (dstNode.horizontalIdx == resNode.horizontalIdx + 1)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}


	internal class Node
	{
		//internal NodeCategories category;
		internal int horizontalIdx;
		internal int verticalIdx;

		internal List<int> reachableIdx;
		internal Node(int horizontalIdx, int verticalIdx)
		{
			this.horizontalIdx = horizontalIdx;
			this.verticalIdx = verticalIdx;

			reachableIdx = new List<int>();
		}
	}
	internal sealed class ResNode : Node
	{
		internal ResNode(int horizontalIdx, int verticalIdx) : base(horizontalIdx, verticalIdx)
		{

		}
	}
	internal sealed class DstNode : Node
	{
		internal DstNode(int horizontalIdx, int verticalIdx) : base(horizontalIdx, verticalIdx)
		{

		}
	}
	internal sealed class BattleNode : Node
	{
		internal BattleNode(int horizontalIdx, int verticalIdx) : base(horizontalIdx, verticalIdx)
		{
		}
	}
	internal sealed class OutPostNode : Node
	{
		internal OutPostNode(int horizontalIdx, int verticalIdx) : base(horizontalIdx, verticalIdx)
		{

		}
	}
	internal sealed class AirdropNode : Node
	{
		internal AirdropNode(int horizontalIdx, int verticalIdx) : base(horizontalIdx, verticalIdx)
		{

		}
	}
	internal sealed class LegacyNode : Node
	{
		internal LegacyNode(int horizontalIdx, int verticalIdx) : base(horizontalIdx, verticalIdx)
		{

		}
	}
	internal sealed class MedicalNode : Node
	{
		internal MedicalNode(int horizontalIdx, int verticalIdx) : base(horizontalIdx, verticalIdx)
		{

		}
	}
}