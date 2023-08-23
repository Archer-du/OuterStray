using DataCore.Cards;
using DataCore.CultivateItems;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardInspector : MonoBehaviour
{
	[Header("Data")]
	public string ID;
	public string category;
	public int cost;
	public string nameContent;
	public string description;
	public int counter;
	public int attack;
	public int health;
	public int maxHealth;

	[Header("Display")]
	public Image cardImage;
	public Image categoryIcon;
	public Image costTag;
	public Image nameTag;

	public Color color;
	public Image backGround;
	public Image frame;

	public TMP_Text costText;
	public TMP_Text nameText;
	public TMP_Text descriptionText;
	public TMP_Text counterText;
	public TMP_Text attackText;
	public TMP_Text healthText;

	public Image counterIcon;
	public Image attackIcon;
	public Image healthIcon;

	[Header("External")]
	public int deckID;
	public int gasMineCost;

	public void Start()
	{
		deckID = -1;
	}
	public void RenderInspector(string ID)
	{

		Pool pool = GameManager.GetInstance().pool;
		Card card = pool.GetCardByID(ID);

		this.ID = ID;
		this.category = card.category;
		this.cost = card.cost;
		this.nameContent = card.name;
		this.description = card.description;

		LoadCardResources(ID);
		nameText.text = card.name;
		costText.text = card.cost.ToString();
		descriptionText.text = description;


		if (category == "Command")
		{
			CommandCard comm = card as CommandCard;
			this.counter = comm.maxDurability;

			attackIcon.enabled = false;
			attackText.enabled = false;
			healthIcon.enabled = false;
			healthText.enabled = false;
		}
		else
		{
			UnitCard unit = card as UnitCard;
			this.counter = unit.attackCounter;
			this.attack = unit.attackPoint;
			this.health = unit.healthPoint;
			this.maxHealth = unit.healthPoint;

			attackText.text = attack.ToString();
			healthText.text = health.ToString();
		}

		counterIcon.enabled = category != "Construction";
		counterText.text = category == "Construction" ? "" : counter.ToString();

		this.gasMineCost = card.gasMineCost;
	}
	public void RenderInspector(string ID, int dynInfo)
	{

		Pool pool = GameManager.GetInstance().pool;
		Card card = pool.GetCardByID(ID);

		this.ID = ID;
		this.category = card.category;
		this.cost = card.cost;
		this.nameContent = card.name;
		this.description = card.description;

		LoadCardResources(ID);
		nameText.text = card.name;
		costText.text = card.cost.ToString();
		descriptionText.text = description;


		if (category == "Command")
		{
			CommandCard comm = card as CommandCard;
			this.counter = dynInfo;

			attackIcon.enabled = false;
			attackText.enabled = false;
			healthIcon.enabled = false;
			healthText.enabled = false;
		}
		else
		{
			UnitCard unit = card as UnitCard;
			this.counter = unit.attackCounter;
			this.attack = unit.attackPoint;
			this.maxHealth = unit.healthPoint;
			this.health = dynInfo;

			attackText.text = attack.ToString();
			healthText.text = health.ToString();
		}

		counterIcon.enabled = category != "Construction";
		counterText.text = category == "Construction" ? "" : counter.ToString();

		this.gasMineCost = card.gasMineCost;
	}
	private void LoadCardResources(string ID)
	{
		cardImage.sprite = Resources.Load<Sprite>("CardImage/" + ID);

		switch (category)
		{
			case "LightArmor":
				UnityEngine.ColorUtility.TryParseHtmlString("#429656", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[11];
				break;
			case "Artillery":
				UnityEngine.ColorUtility.TryParseHtmlString("#CE8849", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[8];
				break;
			case "Motorized":
				UnityEngine.ColorUtility.TryParseHtmlString("#426A84", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[9];
				break;
			case "Guardian":
				UnityEngine.ColorUtility.TryParseHtmlString("#97A5A4", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[10];
				break;
			case "Construction":
				UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[12];
				break;
			case "Command":
				color = Color.gray;
				cardImage.rectTransform.sizeDelta = new Vector2(18, 24);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("Map-icon")[6];
				break;
		}

		backGround.color = color;
		frame.color = color;
		nameTag.color = color;
		costTag.color = color;
	}








	//[Obsolete]
	//public void CopyInfo(DeckTagController other)
	//{
	//	deckID = other.deckID;

	//	costTag.color = other.costTag.color;
	//	nameTag.color = other.nameTag.color;
	//	backGround.color = other.backGround.color;
	//	frame.color = other.frame.color;
	//	cardImage.sprite = other.cardImage.sprite;
	//	categoryIcon.sprite = other.categoryIcon.sprite;

	//	nameText.text = other.nameText.text;
	//	descriptionText.text = other.descriptionText.text;

	//	costText.text = other.costText.text;
	//	counterText.text = other.counterText.text;

	//	attackIcon.enabled = other.attackIcon.enabled;
	//	healthIcon.enabled = other.healthIcon.enabled;

	//	if(other.category != "Command")
	//	{
	//		attackText.text = other.attackText.text;
	//		healthText.text = other.healthText.text;
	//	}
	//}
	//public void CopyInfo(UnitElementController other)
	//{
	//	cardImage.sprite = other.CardImage.sprite;
	//	//TODO
	//	backGround.color = other.color;
	//	frame.color = other.color;
	//	nameTag.color = other.color;
	//	costTag.color = other.color;
	//	categoryIcon.sprite = other.categoryIcon.sprite;

	//	nameText.text = other.nameContent;
	//	costText.text = other.cost.ToString();
	//	descriptionText.text = other.description;
	//	attackText.text = other.attackText.text;
	//	healthText.text = other.maxHealthPoint.ToString();
	//	counterText.text = other.category == "Construction" ? "" : other.attackCounter.ToString();
	//}
	//[Obsolete]
	//public void SetInfo(string ID, string name, string category, int cost, int attack, int health, int counter, string description)
	//{
	//	this.ID = ID;
	//	this.category = category;
	//	this.cost = cost;
	//	this.description = description;

	//	LoadCardResources(ID);

	//	nameText.text = name;
	//	costText.text = cost.ToString();
	//	if (category == "Command")
	//	{
	//		attackIcon.enabled = false;
	//		attackText.enabled = false;
	//		healthIcon.enabled = false;
	//		healthText.enabled = false;
	//	}
	//	else
	//	{
	//		attackText.text = attack.ToString();
	//		healthText.text = health.ToString();
	//	}
	//	counterText.text = category == "Construction" ? "" : counter.ToString();
	//	descriptionText.text = description;
	//}
}
