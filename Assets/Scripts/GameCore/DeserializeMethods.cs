using DataCore.BattleElements;
using DataCore.Cards;
using System.Collections;
using System.Collections.Generic;

internal class DeserializeMethods
{
	/// <summary>
	/// 解析csv中的卡牌对象
	/// </summary>
	/// <param name="card"></param>
	/// <param name="cardObject"></param>
	internal static void CardDeserialize(out Card card, string[] cardObject)
	{
		string id = cardObject[0];

		int ownership = cardObject[1] == "human" ? 0 : 1;

		string name = cardObject[2];

		if (id.Contains("comm"))
		{
			string type = cardObject[3];
			int cost = int.Parse(cardObject[4]);

			int durability = int.Parse(cardObject[5]);

			string effects = cardObject[10];

			string description = cardObject[9];

			card = new CommandCard(id, ownership, name, type, description, cost, durability, -1, -1, effects);
		}
		else
		{
			string category = cardObject[3];

			int cost = int.Parse(cardObject[4]);
			int atk = int.Parse(cardObject[5]);
			int hp = int.Parse(cardObject[6]);
			//理解鹰角程序员，成为鹰角程序员//TODO
			int atkc = (cardObject[7] == "NA" || cardObject[7] == "") ? 100000 : int.Parse(cardObject[7]);

			string effects = cardObject[10];

			string description = cardObject[9];

			card = new UnitCard(id, ownership, name, category, cost, atk, hp, atkc, description, -1, -1, effects);
		}
		if(!id.Contains("base") && card.ownership == 0)
		{
			card.gasMineCost = int.Parse(cardObject[8]);
		}
	}
}
