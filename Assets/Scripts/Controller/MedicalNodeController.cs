using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalNodeController : NodeController
{
	public int pricePerHealth;
	public override void Init()
	{
		base.Init();
		LoadResource();
		panelDisplay.Init("Promote");

		exitButton = panelDisplay.exitButton;

		exitButton.onClick.AddListener(tacticalManager.CampaignCompleted);

		panelDisplay.onceButton.onClick.AddListener(() => MedicalHeal(false, panelDisplay.PromoteInspector.deckID));
		panelDisplay.fullfillButton.onClick.AddListener(() => MedicalHeal(true, panelDisplay.PromoteInspector.deckID));
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
	public void MedicalHeal(bool fullfill, int deckID)
	{
		if (tacticalManager.playerDeck.tags[deckID].maxHealth == tacticalManager.playerDeck.tags[deckID].health) return;
		if(fullfill)
		{
			if (pricePerHealth * (tacticalManager.playerDeck.tags[deckID].maxHealth - tacticalManager.playerDeck.tags[deckID].health) > tacticalManager.gasMineToken)
			{
				return;
			}
			tacticalManager.MedicalNodeHeal(fullfill, deckID);
		}
		else
		{
			if(pricePerHealth > tacticalManager.gasMineToken)
			{
				return;
			}
			tacticalManager.MedicalNodeHeal(fullfill, deckID);
		}

	}
	public override void UpdateHealth(int health)
	{
		panelDisplay.PromoteInspector.healthText.text = health.ToString();
	}
	public override void UpdateBasicInfo(int legacy, int medicalPrice)
	{
		this.pricePerHealth = medicalPrice;
	}
}
