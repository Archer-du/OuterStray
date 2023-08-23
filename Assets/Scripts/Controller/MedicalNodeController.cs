using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalNodeController : NodeController
{
	public int pricePerHealth;
	public override void Init()
	{
		base.Init();

		panel.BuildPanel(PanelType.Medical);
		panel.MainConfirm += MedicalHeal;
		panel.SubConfirm += MedicalHeal;
		castButton.onClick.AddListener(panel.OpenPanel);

		LoadResource();

		exitButton = panel.ExitButton;
		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[5];
		descriptionText.text = "医疗站\n<size=12>消耗气矿为已有卡牌回复耐久</size>\n\n<size=10><i>\"她不是自己人吗？为什么要收费\"</i></size>";
	}
	public override void CastEvent()
	{
		base.CastEvent();
	}
	public void MedicalHeal(int deckID)
	{
		if (deckID == -1) return;
		if (tacticalManager.playerDeck.tags[deckID].inspector.maxHealth == tacticalManager.playerDeck.tags[deckID].inspector.health) return;
		if(pricePerHealth > tacticalManager.gasMineToken)
		{
			return;
		}
		tacticalManager.MedicalNodeHeal(false, deckID);
	}
	public void MedicalHealFullFill(int deckID)
	{
		if (deckID == -1) return;
		if (tacticalManager.playerDeck.tags[deckID].inspector.maxHealth == tacticalManager.playerDeck.tags[deckID].inspector.health) return;
		if (pricePerHealth * (tacticalManager.playerDeck.tags[deckID].inspector.maxHealth - tacticalManager.playerDeck.tags[deckID].inspector.health) > tacticalManager.gasMineToken)
		{
			return;
		}
		tacticalManager.MedicalNodeHeal(true, deckID);
	}





	public override void UpdateHealth(int health)
	{
		panel.selection.healthText.text = health.ToString();
	}


	public override void SetBasicInfo(int legacy, int medicalPrice)
	{
		this.pricePerHealth = medicalPrice;
	}
}
