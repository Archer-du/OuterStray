using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.UIElements;

public class OutPostNodeController : NodeController
{
	public override void Init()
	{
		base.Init();
		LoadResource();
		panelDisplay.Init("OutPost");

		exitButton = panelDisplay.exitButton;

		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);

		for(int i = 0; i < panelDisplay.OutPostButtons.Length; i++)
		{
			int temp = i;
			panelDisplay.OutPostButtons[i].onClick.AddListener(() => OutPostPurchase(temp));
		}
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[4];
		descriptionText.text = "前哨站";
	}
	public override void CastEvent()
	{
		base.CastEvent();
	}

	public void OutPostPurchase(int index)
	{
		if (tacticalManager.gasMineToken < panelDisplay.gasMineCosts[index])
		{
			//TODO shake
			return;
		}
		tacticalManager.OutPostNodePurchase(index);
		panelDisplay.OutPostButtons[index].interactable = false;
		CardInspect card = panelDisplay.OutPostInspectors[index];
		tacticalManager.playerDeck.InstantiateDeckTag(card.ID, card.nameText.text, card.category, 0, card.descriptionText.text, "append");
		//TODO
		if (card.category == "Command")
		{
			tacticalManager.playerDeck.UpdateCommandTagInfo(0, int.Parse(card.costText.text), int.Parse(card.counterText.text));
		}
		else
		{
			tacticalManager.playerDeck.UpdateUnitTagInfo("", 0, int.Parse(card.costText.text), int.Parse(card.attackText.text), int.Parse(card.healthText.text),
				int.Parse(card.healthText.text), card.counterText.text == "" ? 1000000 : int.Parse(card.counterText.text));
		}
		tacticalManager.playerDeck.UpdateHierachy();
	}

	public override void DisplayElement(List<string> IDs, List<string> names, List<string> category, List<int> costs, List<int> attacks, List<int> healths, List<int> counters, List<int> gasMineCosts, List<string> descriptions)
	{
		for(int i = 0; i < IDs.Count; i++)
		{
			panelDisplay.OutPostInspectors[i].ID = IDs[i];
			panelDisplay.OutPostInspectors[i].nameText.text = names[i];
			panelDisplay.OutPostInspectors[i].category = category[i];
			panelDisplay.OutPostInspectors[i].costText.text = costs[i].ToString();
			panelDisplay.OutPostInspectors[i].attackText.text = attacks[i].ToString();
			panelDisplay.OutPostInspectors[i].healthText.text = healths[i].ToString();
			panelDisplay.OutPostInspectors[i].counterText.text = category[i] == "Construction" ? "" : counters[i].ToString();

			panelDisplay.OutPostInspectors[i].descriptionText.text = descriptions[i];

			panelDisplay.gasMineCosts = gasMineCosts;
			for (int j = 0; j < panelDisplay.gasMineCostTexts.Count; j++)
			{
				panelDisplay.gasMineCostTexts[j].text = gasMineCosts[j].ToString();
			}

			panelDisplay.OutPostInspectors[i].cardImage.sprite = Resources.Load<Sprite>("CardImage/" + IDs[i]);

			Color color;
			switch (category[i])
			{
				case "LightArmor":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#429656", out color))
					{
						panelDisplay.OutPostInspectors[i].backGround.color = color;
						panelDisplay.OutPostInspectors[i].frame.color = color;
						panelDisplay.OutPostInspectors[i].nameTag.color = color;
						panelDisplay.OutPostInspectors[i].costTag.color = color;
						panelDisplay.OutPostInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[11];
					}
					break;
				case "Artillery":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#CE8849", out color))
					{
						panelDisplay.OutPostInspectors[i].backGround.color = color;
						panelDisplay.OutPostInspectors[i].frame.color = color;
						panelDisplay.OutPostInspectors[i].nameTag.color = color;
						panelDisplay.OutPostInspectors[i].costTag.color = color;
						panelDisplay.OutPostInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[8];
					}
					break;
				case "Motorized":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#426A84", out color))
					{
						panelDisplay.OutPostInspectors[i].backGround.color = color;
						panelDisplay.OutPostInspectors[i].frame.color = color;
						panelDisplay.OutPostInspectors[i].nameTag.color = color;
						panelDisplay.OutPostInspectors[i].costTag.color = color;
						panelDisplay.OutPostInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[9];
					}
					break;
				case "Guardian":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#97A5A4", out color))
					{
						panelDisplay.OutPostInspectors[i].backGround.color = color;
						panelDisplay.OutPostInspectors[i].frame.color = color;
						panelDisplay.OutPostInspectors[i].nameTag.color = color;
						panelDisplay.OutPostInspectors[i].costTag.color = color;
						panelDisplay.OutPostInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[10];
					}
					break;
				case "Construction":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color))
					{
						panelDisplay.OutPostInspectors[i].backGround.color = color;
						panelDisplay.OutPostInspectors[i].frame.color = color;
						panelDisplay.OutPostInspectors[i].nameTag.color = color;
						panelDisplay.OutPostInspectors[i].costTag.color = color;
						panelDisplay.OutPostInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[12];
					}
					break;
				case "Command":
					color = Color.gray;
					panelDisplay.OutPostInspectors[i].backGround.color = color;
					panelDisplay.OutPostInspectors[i].frame.color = color;
					panelDisplay.OutPostInspectors[i].nameTag.color = color;
					panelDisplay.OutPostInspectors[i].costTag.color = color;
					break;
			}
			if (category[i] == "Command")
			{
				panelDisplay.OutPostInspectors[i].attackIcon.enabled = false;
				panelDisplay.OutPostInspectors[i].attackText.enabled = false;
				panelDisplay.OutPostInspectors[i].healthIcon.enabled = false;
				panelDisplay.OutPostInspectors[i].healthText.enabled = false;
			}
		}
	}

}
