using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegacyNodeController : NodeController
{
	public int legacy;

	//public ParticleSystem particleSystem;
	public override void Init()
	{
		base.Init();
		LoadResource();

		//particleSystem.Stop();
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[3];
		descriptionText.text = "气矿节点";
	}
	public override void CastEvent()
	{
		base.CastEvent();

		//particleSystem.Play();
	}
	public override void SetBasicInfo(int legacy, int medicalPrice)
	{
		this.legacy = legacy;
	}
}
