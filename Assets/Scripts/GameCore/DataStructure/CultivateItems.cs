//Author@Archer
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using DataCore.Cards;

using DisplayInterface;

namespace DataCore.CultivateItems
{
	/// <summary>
	/// cards unlocked
	/// </summary>
	internal class Pool
	{
		//之后换成hash
		internal List<Card> cardPool { get; private set; }

		internal Hashtable hashPool;
		internal Hashtable humanCardPool;
		internal Hashtable plantCardPool;

		internal List<Card> allyCards;
		internal List<Card> enemyCards;


		internal Pool()
		{
			cardPool = new List<Card>(SystemConfig.poolCapacity);
			hashPool = new Hashtable();

			humanCardPool = new Hashtable();
			plantCardPool = new Hashtable();
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
		internal int LoadCardPool()
		{
			StreamReader reader = File.OpenText("\\UnityProject\\AIGC\\OuterStray\\Assets\\Config\\UnitCardData.csv");

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

				string id		= data[0];

				int ownership = data[1] == "human" ? 0 : 1;

				string name = data[2];

				if (id.Contains("comm"))
				{
					int cost = int.Parse(data[3]);

					int durability = int.Parse(data[4]);

					int department = int.Parse(data[6]);
					int pack = int.Parse(data[7]);

					string effects = data[8];
					effects = "none";

					string description = data[5];

					card = new CommandCard(id, ownership, name, description, cost, durability, department, pack, effects);
				}
				else
				{
					string category = data[3];

					int cost		= int.Parse(data[4]);
					int atk			= int.Parse(data[5]);
					int hp			= int.Parse(data[6]);
					//理解鹰角程序员，成为鹰角程序员//TODO
					int atkc		= data[7] == "NA" ? 100000 : int.Parse(data[7]);

					int department	= int.Parse(data[9]);
					int pack		= int.Parse(data[10]);
				
					string effects	= data[11];
					effects = "none";//TODO

					string description = data[8];


					card = new UnitCard(id, ownership, name, category, cost, atk, hp, atkc, description, department, pack, effects);
				}

				//TODO
				cardPool.Add(card);
				hashPool.Add(id, card);

				if(ownership == 0)
				{
					humanCardPool.Add(id, card);
				}
				else
				{
					plantCardPool.Add(id, card);
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