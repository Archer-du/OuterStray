using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutPostNodeController : NodeController
{
	public override void Init()
	{
		base.Init();
		LoadResource();
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
