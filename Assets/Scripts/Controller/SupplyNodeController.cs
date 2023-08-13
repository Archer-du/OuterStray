using System.Collections;
using System.Collections.Generic;
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
}
