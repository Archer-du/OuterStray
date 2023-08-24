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
		descriptionText.text = "前哨站\n<size=12>消耗气矿购买商品卡牌</size>\n\n<size=10><i>\"商人是从哪里搞来的货？\"</i></size>";
	}
	public override void CastEvent()
	{
		base.CastEvent();
	}

	public void OutPostPurchase(int index)
	{
		if (tacticalManager.gasMineToken < panel.cardPacks[index].gasMineCost)
		{
			//TODO shake
			return;
		}
		panel.DisablePackButton(index);

		tacticalManager.OutPostNodePurchase(index);

		CardInspector card = panel.cardPacks[index].inspector;
		tacticalManager.playerDeck.AddNewTag(card.ID);
	}

	public override void DisplayPacks(List<string> IDs)
	{
		panel.FillCardPack(IDs);
	}

}
