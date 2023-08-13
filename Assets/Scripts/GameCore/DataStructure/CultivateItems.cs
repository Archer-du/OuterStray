//Author@Archer
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Codice.CM.Common;
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
		internal Hashtable hashPool;
		[JsonIgnore]
		internal Hashtable humanCardPool;
		[JsonIgnore]
		internal Hashtable plantCardPool;

		[JsonIgnore]
		internal List<Card> allyCards;
		[JsonIgnore]
		internal List<Card> enemyCards;


		public Pool()
		{
			cardPool = new List<Card>();
			hashPool = new Hashtable();

			humanCardPool = new Hashtable();
			plantCardPool = new Hashtable();
		}
		internal Card this[int index]
		{
			get => cardPool[index];
			set => cardPool[index] = value;
		}

		internal void LoadCardPoolJson()
		{

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
				Card card = null;

				DeserializeMethods.CardDeserialize(out card, data);

				//TODO
				cardPool.Add(card);
				if (!hashPool.ContainsKey(card.backendID))
				{
					hashPool.Add(card.backendID, card);
				}

				if(card.ownership == 0)
				{
					if(!humanCardPool.ContainsKey(card.backendID))
					{
						humanCardPool.Add(card.backendID, card);
					}
				}
				else
				{
					if(!plantCardPool.ContainsKey(card.backendID))
					{
						plantCardPool.Add(card.backendID, card);
					}
				}

				line = reader.ReadLine();
				num++;
				index++;
			}
			reader.Close();
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
				Card card = null;

				DeserializeMethods.CardDeserialize(out card, data);

				//TODO
				cardPool.Add(card);

				if (!hashPool.ContainsKey(card.backendID))
				{
					hashPool.Add(card.backendID, card);
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
			return hashPool[ID] as Card;
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