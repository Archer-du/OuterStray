using DG.Tweening;
using DisplayInterface;
using InputHandler;
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
	public void InstantiateDeckTag(string ID, string name, string category, int index, string description, string method)
	{
		GameObject deckTag = Instantiate(deckTagPrototype, transform);
		DeckTagController controller = deckTag.GetComponent<DeckTagController>();
		controller.controller = this;

		controller.nameContent = name;
		controller.category = category;
		controller.description = description;

		controller.Init(ID);
		controller.deckID = index;

		if(method == "append")
		{
			controller.Component.DOLocalMove(Vector3.zero, controller.initDuration);
		}
		else
		{
			controller.Component.transform.localPosition = Vector3.zero;
		}

		tags.Add(controller);
	}

	public void UpdateCommandTagInfo(int index, int cost, int durability)
	{
		DeckTagController controller = tags[index];

		controller.cost = cost;
		controller.counter = durability;
		controller.UpdateInfo();
	}


	public void UpdateUnitTagInfo(string category, int index, int cost, int attack, int health, int maxHealth, int attackCounter)
	{
		DeckTagController controller = null;

		controller = tags[index];
		controller.cost = cost;
		controller.attack = attack;
		controller.health = health;
		controller.maxHealth = maxHealth;
		controller.counter = attackCounter;

		controller.UpdateInfo();
	}

	public void UpdateHierachy()
	{
		tags.Sort();
		for(int i = 0; i < tags.Count; i++)
		{
			tags[i].deckID = i;
		}
		foreach(var tag in tags)
		{
			tag.transform.SetSiblingIndex(tag.deckID);
		}
	}

	public void UpdateHealth(int index, int health)
	{
		tags[index].health = health;
		tags[index].healthText.text = health.ToString();
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




	public void AddDeckTagFromPool(int index)
	{
		deck.AddDeckTagFromPool(index);
	}
	public bool IsEmpty()
	{
		return tags.Count == 0;
	}
}
