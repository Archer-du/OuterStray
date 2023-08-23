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

		panel.BuildPanel(PanelType.Supply);
		panel.PackChosen += SupplyChoose;
		castButton.onClick.AddListener(panel.OpenPanel);

		exitButton = panel.ExitButton;
		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);
	}
	public override void LoadResource()
	{
		switch (category)
		{
			case "LightArmor":
				Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[10];
				break;
			case "Motorized":
				Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[12];
				break;
			case "Artillery":
				Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[11];
				break;
			case "Guardian":
				Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[13];
				break;
			case "Construction":
				Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[14];
				break;
			case "Command":
				Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[6];
				break;
		}
		descriptionText.text = "空投点\n<size=12>从特定种类卡牌中选择一张获得</size>\n\n<size=10><i>\"来自Setinel7的馈赠\"</i></size>";
	}
	public override void CastEvent()
	{
		base.CastEvent();
	}

	public void SupplyChoose(int index)
	{
		Debug.Log(index);
		for (int i = 0; i < panel.packs.Count; i++)
		{
			if (i == index)
			{
				panel.packs[i].SelectButton.enabled = false;
				panel.packs[i].SelectButton.image.color = Color.gray;
			}
			else
			{
				panel.packs[i].SelectButton.interactable = false;
			}
		}

		tacticalManager.SupplyNodeChoose(index);

		CardInspector card = panel.packs[index].inspector;
		tacticalManager.playerDeck.AddNewTag(card.ID);
	}


	public override void DisplayPacks(List<string> IDs)
	{
		panel.InitializePanel(IDs);

		category = panel.packs[0].inspector.category;
		LoadResource();
	}
}
