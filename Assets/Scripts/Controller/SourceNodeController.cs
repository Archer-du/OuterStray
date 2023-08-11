using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceNodeController : NodeController
{
	public override void Init()
	{
		base.Init();
		LoadResource();

		casted = true;
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[0];
		descriptionText.text = "Home";
	}
	public override void CastEvent()
	{
		base.CastEvent();
	}
}
