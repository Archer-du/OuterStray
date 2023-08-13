using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalNodeController : NodeController
{
	public override void Init()
	{
		base.Init();
		LoadResource();
		panelDisplay.Init("Promote");

		exitButton = panelDisplay.exitButton;

		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[5];
		descriptionText.text = "医疗节点";
	}
	public override void CastEvent()
	{
		base.CastEvent();
	}
}
