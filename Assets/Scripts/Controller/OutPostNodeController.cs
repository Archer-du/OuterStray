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

		panel.BuildPanel(PanelType.OutPost);
		panel.PackChosen += OutPostPurchase;
		castButton.onClick.AddListener(panel.OpenPanel);

		LoadResource();

		exitButton = panel.ExitButton;
		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);
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
		if (tacticalManager.gasMineToken < panel.packs[index].gasMineCost)
		{
			//TODO shake
			return;
		}
		panel.DisablePackButton(index);

		tacticalManager.OutPostNodePurchase(index);

		CardInspector card = panel.packs[index].inspector;
		tacticalManager.playerDeck.AddNewTag(card.ID);
	}

	public override void DisplayPacks(List<string> IDs)
	{
		panel.InitializePanel(IDs);
	}

}
