//Author@Archer
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DataCore.Cards;

using DisplayInterface;
using Unity.Plastic.Newtonsoft.Json;

namespace DataCore.CultivateItems
{
	/// <summary>
	/// cards unlocked
	/// </summary>
	[Serializable]
	public class Pool
	{
		//之后换成hash
		[JsonProperty("pool")]
		internal List<Card> cardPool { get; set; }

		[JsonIgnore]
		internal Hashtable IDhashPool;
		[JsonIgnore]
		internal List<Card> humanCardPool;

		internal List<Card> humanLightArmorSet;
		internal List<Card> humanMotorizedSet;
		internal List<Card> humanArtillerySet;
		internal List<Card> humanGuardianSet;
		internal List<Card> humanConstructionSet;
		internal List<Card> humanCommandSet;
		[JsonIgnore]
		internal Hashtable plantCardPool;

		[JsonIgnore]
		internal List<Card> allyCards;
		[JsonIgnore]
		internal List<Card> enemyCards;

		public Pool()
		{
			cardPool = new List<Card>();
			IDhashPool = new Hashtable();

			humanCardPool = new List<Card>();
			plantCardPool = new Hashtable();

			humanLightArmorSet = new List<Card>();
			humanMotorizedSet = new List<Card>();
			humanArtillerySet = new List<Card>();
			humanGuardianSet = new List<Card>();
			humanConstructionSet = new List<Card>();
			humanCommandSet = new List<Card>();
		}
		internal Card this[int index]
		{
			get => cardPool[index];
			set => cardPool[index] = value;
		}

		/// <summary>
		/// non-console version: read card info from specific media
		/// </summary>
		/// <param name="__icdspl"></param>
		/// <returns></returns>
		public int LoadCardPool()
		{
			StreamReader reader = File.OpenText("Assets\\Config\\Pool.csv");

			string[] data;
			int num = 0;
			string line = reader.ReadLine();
			int index = 0;


			while (line != null)
			{
				data = line.Split(',');
				if (data[1] == "#")
				{
					line = reader.ReadLine();
					continue;
				}

				DeserializeMethods.CardDeserialize(out Card card, data);

				//TODO
				cardPool.Add(card);
				if (card.ownership == 0)
				{
					switch (card.category)
					{
						case "LightArmor":
							humanLightArmorSet.Add(card);
							break;
						case "Motorized":
							humanMotorizedSet.Add(card);
							break;
						case "Artillery":
							humanArtillerySet.Add(card);
							break;
						case "Guardian":
							humanGuardianSet.Add(card);
							break;
						case "Construction":
							//TODO
							if (!card.category.Contains("base"))
							{
								humanConstructionSet.Add(card);
							}
							break;
						case "Command":
							humanCommandSet.Add(card);
							break;
					}
					humanCardPool.Add(card);
				}
				else
				{
					if(!plantCardPool.ContainsKey(card.backendID))
					{
						plantCardPool.Add(card.backendID, card);
					}
				}

				if (!IDhashPool.ContainsKey(card.backendID))
				{
					IDhashPool.Add(card.backendID, card);
				}


				line = reader.ReadLine();
				num++;
				index++;
			}
			reader.Close();

			cardPool.Sort();
			humanCardPool.Sort();

			return num;
		}
		internal int LoadCardPool(string path)
		{
			StreamReader reader = File.OpenText(path);

			string[] data;
			int num = 0;
			string line = reader.ReadLine();
			int index = 0;


			while (line != null)
			{
				data = line.Split(',');
				if (data[1] == "#")
				{
					line = reader.ReadLine();
					continue;
				}

				DeserializeMethods.CardDeserialize(out Card card, data);

				//TODO
				cardPool.Add(card);

				if (!IDhashPool.ContainsKey(card.backendID))
				{
					IDhashPool.Add(card.backendID, card);
				}

				line = reader.ReadLine();
				num++;
				index++;
			}
			reader.Close();
			return num;
		}
		internal Card GetCardByID(string ID)
		{
			return IDhashPool[ID] as Card;
		}
	}






	internal class Department
	{
		private List<Pack> packs;

		internal Department()
		{
			packs = new List<Pack>(SystemConfig.buildingCapacity);
		}
		/// <summary>
		/// 解锁当前建筑，并将内部卡包全部解锁并设置为可用
		/// </summary>
		internal void Unlock()
		{
			foreach (var pack in packs)
			{
				pack.available = true;
			}
		}
		internal void ImportPacks(Pool pool)
		{
			//for(int i = 0; i < pool.count; i++) { }
		}

		/// <summary>
		/// test method: fill pack with card reference in pool
		/// </summary>
		/// <param name="pool"></param>
		internal void Fill(Pool pool)
		{
			Random random = new Random();
			foreach (var pack in packs)
			{
				for (int i = 0; i < SystemConfig.packCapacity; i++)
				{
					int index = random.Next(0, SystemConfig.poolCapacity);
					pack[i] = pool[index];
				}
			}
		}

		/// <summary>
		/// throw exception if pack not available or used; else set the pack used and return the pack <br/>
		/// 如果index索引的包未解锁或已经被使用则抛出异常，否则将包设置为已经被使用并返回该包
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal Pack GetPack(int index)
		{
			if (!packs[index].available || packs[index].used)
			{
				throw new InvalidOperationException();
			}
			else
			{
				packs[index].used = true;
				return packs[index];
			}
		}
	}







	internal class Pack
	{
		private List<Card> pack;
		internal bool available;
		internal bool used;
		internal Pack()
		{
			pack = new List<Card>(SystemConfig.packCapacity);
			available = false;
			used = false;
		}
		internal Card this[int index]
		{
			get => pack[index];
			set => pack[index] = value;
		}
	}
}