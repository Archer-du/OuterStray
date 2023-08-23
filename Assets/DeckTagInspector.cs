using DataCore.Cards;
using DataCore.CultivateItems;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete]
public class DeckTagInspector : MonoBehaviour
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

	public void RenderInspector(string ID, int dynHealth)
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
			categoryIcon.enabled = false;
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

			attackText.text = attack.ToString();
			//TODO
			//healthText.text = health.ToString();
		}

		counterIcon.enabled = category != "Construction";
		counterText.text = category == "Construction" ? "" : counter.ToString();
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
				break;
		}

		backGround.color = color;
		frame.color = color;
		nameTag.color = color;
		costTag.color = color;
	}
}
