using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromoteNodeController : NodeController
{
	public override void Init()
	{
		base.Init();
		LoadResource();

		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[2];
		descriptionText.text = "晋升节点";
	}
	public override void CastEvent()
	{
		base.CastEvent();
	}
}
