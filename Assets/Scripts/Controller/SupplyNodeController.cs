using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SupplyNodeController : NodeController
{
    public string category;
	public override void Init()
	{
		base.Init();
		LoadResource();
		panelDisplay.Init("Supply");

		exitButton = panelDisplay.exitButton;

		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);


		for (int i = 0; i < panelDisplay.SupplyButtons.Length; i++)
		{
			int temp = i;
			panelDisplay.SupplyButtons[i].onClick.AddListener(() => SupplyChoose(temp));
		}
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[6];
		descriptionText.text = "空投";
	}
	public override void CastEvent()
	{
		base.CastEvent();
	}

	public void SupplyChoose(int index)
	{
		for (int i = 0; i < panelDisplay.SupplyButtons.Length; i++)
		{
			if (i == index)
			{
				panelDisplay.SupplyButtons[index].enabled = false;
				panelDisplay.SupplyButtons[index].image.color = Color.gray;
			}
			else
			{
				panelDisplay.SupplyButtons[i].interactable = false;
			}
		}
		tacticalManager.SupplyNodeChoose(index);
		CardInspect card = panelDisplay.SupplyInspectors[index];
		tacticalManager.playerDeck.InstantiateDeckTag(card.ID, card.nameText.text, card.category, 0, card.descriptionText.text);
		tacticalManager.playerDeck.UpdateHierachy();
	}

	public override void DisplayElement(List<string> IDs, List<string> names, List<string> category, List<int> costs, List<int> attacks, List<int> healths, List<int> counters, List<int> gasMineCosts, List<string> descriptions)
	{
		for (int i = 0; i < IDs.Count; i++)
		{
			panelDisplay.SupplyInspectors[i].ID = IDs[i];
			panelDisplay.SupplyInspectors[i].nameText.text = names[i];
			panelDisplay.SupplyInspectors[i].category = category[i];
			panelDisplay.SupplyInspectors[i].costText.text = costs[i].ToString();
			panelDisplay.SupplyInspectors[i].attackText.text = attacks[i].ToString();
			panelDisplay.SupplyInspectors[i].healthText.text = healths[i].ToString();
			panelDisplay.SupplyInspectors[i].counterText.text = category[i] == "Construction" ? "" : counters[i].ToString();

			panelDisplay.SupplyInspectors[i].descriptionText.text = descriptions[i];

			panelDisplay.gasMineCosts = gasMineCosts;


			panelDisplay.SupplyInspectors[i].cardImage.sprite = Resources.Load<Sprite>("CardImage/" + IDs[i]);

			Color color;
			switch (category[i])
			{
				case "LightArmor":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#429656", out color))
					{
						panelDisplay.SupplyInspectors[i].backGround.color = color;
						panelDisplay.SupplyInspectors[i].frame.color = color;
						panelDisplay.SupplyInspectors[i].nameTag.color = color;
						panelDisplay.SupplyInspectors[i].costTag.color = color;
						panelDisplay.SupplyInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[11];
					}
					break;
				case "Artillery":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#CE8849", out color))
					{
						panelDisplay.SupplyInspectors[i].backGround.color = color;
						panelDisplay.SupplyInspectors[i].frame.color = color;
						panelDisplay.SupplyInspectors[i].nameTag.color = color;
						panelDisplay.SupplyInspectors[i].costTag.color = color;
						panelDisplay.SupplyInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[8];
					}
					break;
				case "Motorized":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#426A84", out color))
					{
						panelDisplay.SupplyInspectors[i].backGround.color = color;
						panelDisplay.SupplyInspectors[i].frame.color = color;
						panelDisplay.SupplyInspectors[i].nameTag.color = color;
						panelDisplay.SupplyInspectors[i].costTag.color = color;
						panelDisplay.SupplyInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[9];
					}
					break;
				case "Guardian":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#97A5A4", out color))
					{
						panelDisplay.SupplyInspectors[i].backGround.color = color;
						panelDisplay.SupplyInspectors[i].frame.color = color;
						panelDisplay.SupplyInspectors[i].nameTag.color = color;
						panelDisplay.SupplyInspectors[i].costTag.color = color;
						panelDisplay.SupplyInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[10];
					}
					break;
				case "Construction":
					if (UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color))
					{
						panelDisplay.SupplyInspectors[i].backGround.color = color;
						panelDisplay.SupplyInspectors[i].frame.color = color;
						panelDisplay.SupplyInspectors[i].nameTag.color = color;
						panelDisplay.SupplyInspectors[i].costTag.color = color;
						panelDisplay.SupplyInspectors[i].categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[12];
					}
					break;
				case "Command":
					color = Color.gray;
					panelDisplay.SupplyInspectors[i].backGround.color = color;
					panelDisplay.SupplyInspectors[i].frame.color = color;
					panelDisplay.SupplyInspectors[i].nameTag.color = color;
					panelDisplay.SupplyInspectors[i].costTag.color = color;
					break;
			}
			if (category[i] == "Command")
			{
				panelDisplay.SupplyInspectors[i].attackIcon.enabled = false;
				panelDisplay.SupplyInspectors[i].attackText.enabled = false;
				panelDisplay.SupplyInspectors[i].healthIcon.enabled = false;
				panelDisplay.SupplyInspectors[i].healthText.enabled = false;
			}
		}
	}

}
