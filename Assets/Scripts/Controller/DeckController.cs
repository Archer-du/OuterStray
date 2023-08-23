using DG.Tweening;
using DisplayInterface;
using InputHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class DeckController : MonoBehaviour,
	IDeckController
{
	public IDeckInput deck;

	public TacticalSceneManager sceneManager
	{
		get => GameManager.GetInstance().tacticalSceneManager;
	}

	[Header("Data")]
	public List<DeckTagController> tags;

	[Header("Prototype")]
	public GameObject deckTagPrototype;


	public void Init(IDeckInput deck)
	{
		deckTagLock = false;
		tags = new List<DeckTagController>();
		this.deck = deck;
	}



	public void UnloadDeckTags()
	{
		tags = new List<DeckTagController>();

		int childCount = transform.childCount;

		// 倒序遍历子物体并销毁它们
		for (int i = childCount - 1; i >= 0; i--)
		{
			Transform child = transform.GetChild(i);
			Destroy(child.gameObject);
		}
	}
	public void InstantiateDeckTag(int index, string ID, int dynInfo, string method)
	{
		GameObject deckTag = Instantiate(deckTagPrototype, transform);
		DeckTagController tag = deckTag.GetComponent<DeckTagController>();

		tag.controller = this;

		tag.Init(ID, dynInfo);

		tag.Component.transform.localPosition = Vector3.zero;

		tags.Add(tag);
	}
	public void AddNewTag(string ID)
	{
		GameObject deckTag = Instantiate(deckTagPrototype, transform);
		DeckTagController tag = deckTag.GetComponent<DeckTagController>();

		tag.controller = this;

		tag.Component.DOLocalMove(Vector3.zero, tag.initDuration);

		tag.Init(ID);
		AddDeckTagFromPool(ID);
		tags.Add(tag);

		UpdateHierachy();
	}
	public void UpdateHierachy()
	{
		tags.Sort();
		for (int i = 0; i < tags.Count; i++)
		{
			tags[i].deckID = i;
		}
		foreach (var tag in tags)
		{
			tag.transform.SetSiblingIndex(tag.deckID);
		}
	}
	//public void UpdateCommandTagInfo(int index, int cost, int durability)
	//{
	//	DeckTagController controller = tags[index];

	//	controller.cost = cost;
	//	controller.counter = durability;
	//	controller.UpdateInfo();
	//}
	//public void UpdateUnitTagInfo(string category, int index, int cost, int attack, int health, int maxHealth, int attackCounter)
	//{
	//	DeckTagController controller = null;

	//	controller = tags[index];
	//	controller.cost = cost;
	//	controller.attack = attack;
	//	controller.health = health;
	//	controller.maxHealth = maxHealth;
	//	controller.counter = attackCounter;

	//	controller.UpdateInfo();
	//}




	[Obsolete]
	public void UpdateHealthDisplay(int deckID, int health)
	{
		tags[deckID].inspector.health = health;
		tags[deckID].inspector.healthText.text = health.ToString();
	}


	public bool deckTagLock;
	public void DisableAllDeckTags()
	{
		deckTagLock = true;
		foreach(var tag in tags)
		{
			tag.GetComponent<InspectPanelController>().active = false;
		}
	}
	public void EnableAllDeckTags()
	{
		deckTagLock = false;
		foreach (var tag in tags)
		{
			tag.GetComponent<InspectPanelController>().active = true;
		}
	}



	public void AddDeckTagFromPool(string ID)
	{
		deck.AddDeckTagFromPool(ID);
	}
	public bool IsEmpty()
	{
		return tags.Count == 0;
	}
}
