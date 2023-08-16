using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegacyNodeController : NodeController
{
	int legacy;
	public override void Init()
	{
		base.Init();
		LoadResource();
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[3];
		descriptionText.text = "气矿节点";
	}
	public override void CastEvent()
	{
		base.CastEvent();
	}
	public override void UpdateBasicInfo(int legacy, int medicalPrice)
	{
		this.legacy = legacy;
	}
}
