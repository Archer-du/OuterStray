//Author@Archer
using System;
using System.Collections;
using System.Collections.Generic;

using DataCore.StructClass;

using DisplayInterface;


namespace DataCore.Cards
{
	/// <summary> 
	/// static information about cards
	/// </summary>
	[Serializable]
	internal abstract class Card
	{
		internal string backendID { get; set; }
		internal string name { get; set; }
		internal string description { get; set; }
		internal int cost { get; set; }

		internal int ownership;

		internal int department;

		internal int pack;

		internal string effects;

		protected Card(string __id, string __name, string __description, int __cost, int ownership, int department, int pack, string effects)
		{
			this.backendID = __id;
			this.name = __name;
			this.description = __description;
			this.cost = __cost;
			this.ownership = ownership;
			this.department = department;
			this.pack = pack;
			this.effects = effects;
		}
		//TODO
		protected Card(Card __card)
		{
			this.backendID = __card.backendID;
			this.name = __card.name;
			this.description = __card.description;
			this.cost = __card.cost;
			this.department = __card.department;
			this.pack = __card.pack;
			this.effects = __card.effects;
		}
	}
	/// <summary>
	/// Card class: Unit
	/// </summary>
	[Serializable]
	internal sealed class UnitCard : Card
	{

		internal string category { get; set; }
		internal int attackPoint { get; set; }
		internal int healthPoint { get; set; }
		internal int attackCounter { get; set; }




		//specific argument for different category
		internal int counterDecrease;

		internal bool randomAttack;

		internal bool moveable;

		internal bool mocking;

		internal string[] componentsHash;


		internal UnitCard(string id, int ownership, string name, string category, int cost, int attack, int maxhealth, int attackCounter, string description, int department, int pack, string effects)
			: base(id, name, description, cost, ownership, department, pack, effects)
		{
			this.category = category;
			this.attackPoint = attack;
			this.healthPoint = maxhealth;
			this.attackCounter = attackCounter;


			this.counterDecrease = 0;
			this.randomAttack = false;
			this.moveable = true;
			this.mocking = false;

			this.componentsHash = new string[5];//TODO

			switch(category)
			{
				case "LightArmor":
					break;
				case "Motorized":
					counterDecrease = 2;
					break;
				case "Artillery":
					randomAttack = true;
					break;
				case "Guardian":
					mocking = true;
					break;
				case "Construction":
					moveable = false;
					break;
				case "Behemoths":
					//TODO
					//components
					break;
				default:
					throw new Exception("invalid card data");
			}
		}
	}






	/// <summary>
	/// Card class: Command
	/// </summary>
	[Serializable]
	internal sealed class CommandCard : Card
	{
		internal string type;
		internal int maxDurability { get; set; }

		//TODO
		internal CommandCard(string __id, int ownership, string __name, string type, string __description, int __cost, int __maxDurability, int department, int pack, string effects)
			: base(__id, __name, __description, __cost, ownership, department, pack, effects)
		{
			this.type = type;
			this.maxDurability = __maxDurability;
		}
	}



	//LEGACY

	///// <summary>
	///// Infantry card
	///// </summary>
	//internal class InfantryCard : UnitCard
	//{
	//	internal InfantryCard(int __id, string __name, string __description, int __cost, int __attack, int __maxhealth, int __attackCounter)
	//		: base(__id, __name, __description, __cost, __attack, __maxhealth, __attackCounter)
	//	{
	//		this.category = CardCategories.infantry;
	//	}
	//	public InfantryCard(int __id, string __name, string __description, int __cost, int __attack, int __maxhealth, int __attackCounter, ICardDisplay __display)
	//		: base(__id, __name, __description, __cost, __attack, __maxhealth, __attackCounter)
	//	{
	//		this.category = CardCategories.infantry;
	//		this.cardDisplay = __display;

	//		if (this.cardDisplay != null)
	//			cardDisplay.TextDisplay(this.name, this.category, this.attackPoint, this.cost, this.healthPoint);
	//	}
	//	public InfantryCard(InfantryCard __card)
	//		: base(__card) { }
	//}


	//public class CavalryCard : UnitCard
	//{
	//	public CavalryCard(int __id, string __name, string __description, int __cost, int __attack, int __maxhealth, int __attackCounter)
	//		: base(__id, __name, __description, __cost, __attack, __maxhealth, __attackCounter)
	//	{
	//		this.category = CardCategories.cavalry;
	//	}
	//	public CavalryCard(int __id, string __name, string __description, int __cost, int __attack, int __maxhealth, int __attackCounter, ICardDisplay __display)
	//		: base(__id, __name, __description, __cost, __attack, __maxhealth, __attackCounter)
	//	{
	//		this.category = CardCategories.cavalry;
	//		this.cardDisplay = __display;
	//		if (this.cardDisplay != null)
	//			cardDisplay.TextDisplay(this.name, this.category, this.attackPoint, this.cost, this.healthPoint);
	//	}
	//	public CavalryCard(CavalryCard __card)
	//		: base(__card) { }
	//}


	//public class ArtilleryCard : UnitCard
	//{
	//	public ArtilleryCard(int __id, string __name, string __description, int __cost, int __attack, int __maxhealth, int __attackCounter)
	//		: base(__id, __name, __description, __cost, __attack, __maxhealth, __attackCounter)
	//	{
	//		this.category = CardCategories.artillery;
	//	}
	//	public ArtilleryCard(int __id, string __name, string __description, int __cost, int __attack, int __maxhealth, int __attackCounter, ICardDisplay __display)
	//		: base(__id, __name, __description, __cost, __attack, __maxhealth, __attackCounter)
	//	{
	//		this.category = CardCategories.cavalry;
	//		this.cardDisplay = __display;
	//		if (this.cardDisplay != null)
	//			cardDisplay.TextDisplay(this.name, this.category, this.attackPoint, this.cost, this.healthPoint);
	//	}
	//	public ArtilleryCard(ArtilleryCard __card)
	//		: base(__card) { }
	//}


	//public class BehemothCard : UnitCard
	//{
	//	public BehemothCard(int __id, string __name, string __description, int __cost, int __attack, int __maxhealth, int __attackCounter)
	//		: base(__id, __name, __description, __cost, __attack, __maxhealth, __attackCounter)
	//	{
	//		this.category = CardCategories.behemoths;
	//	}
	//	public BehemothCard(BehemothCard __card)
	//		: base(__card) { }
	//}


	//public class LurkersCard : UnitCard
	//{
	//	public LurkersCard(int __id, string __name, string __description, int __cost, int __attack, int __maxhealth, int __attackCounter)
	//		: base(__id, __name, __description, __cost, __attack, __maxhealth, __attackCounter)
	//	{
	//		this.category = CardCategories.artillery;
	//	}
	//	public LurkersCard(LurkersCard __card)
	//		: base(__card) { }
	//}


	//public class ConstructionCard : UnitCard
	//{
	//	public ConstructionCard(int __id, string __name, string __description, int __cost, int __attack, int __maxhealth, int __attackCounter)
	//		: base(__id, __name, __description, __cost, __attack, __maxhealth, __attackCounter)
	//	{
	//		this.category = CardCategories.construction;
	//	}
	//	public ConstructionCard(ConstructionCard __card)
	//		: base(__card) { }
	//}

}