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

		LoadResource();

		exitButton = panel.ExitButton;
		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);
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
	}
}
