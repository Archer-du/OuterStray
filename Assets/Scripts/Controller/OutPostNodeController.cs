using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutPostNodeController : NodeController
{
	public override void Init()
	{
		base.Init();
		LoadResource();

		exitButton = transform.Find("UI/Panels/OutPostPanel/Mask/PanelComponent/ExitButton").GetComponent<Button>();
		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);

		panelDisplay.Init("OutPost");
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
}
