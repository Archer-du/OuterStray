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

	public TacticalSceneManager sceneManager;

	[Header("Data")]
	public Dictionary<int, DeckTagController> deckTags;
	public List<DeckTagController> tags;

	//public Dictionary<int, DeckTagController> lightArmorDeckTags;
	//public Dictionary<int, DeckTagController> motorizedDeckTags;
	//public Dictionary<int, DeckTagController> artilleryDeckTags;
	//public Dictionary<int, DeckTagController> guardianDeckTags;
	//public Dictionary<int, DeckTagController> constructionDeckTags;
	//public Dictionary<int, DeckTagController> commandDeckTags;

	[Header("Prototype")]
	public GameObject deckTagPrototype;
	public void Init(IDeckInput deck)
	{
		tags = new List<DeckTagController>();
		this.deck = deck;
		//lightArmorDeckTags = new Dictionary<int, DeckTagController>();
		//motorizedDeckTags = new Dictionary<int, DeckTagController>();
		//artilleryDeckTags = new Dictionary<int, DeckTagController>();
		//guardianDeckTags = new Dictionary<int, DeckTagController>();
		//constructionDeckTags = new Dictionary<int, DeckTagController>();
		//commandDeckTags = new Dictionary<int, DeckTagController>();
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
	public void InstantiateDeckTag(string ID, string name, string category, int index, string description)
	{
		GameObject deckTag = Instantiate(deckTagPrototype, transform);
		DeckTagController controller = deckTag.GetComponent<DeckTagController>();
		controller.controller = this;

		controller.nameContent = name;
		controller.category = category;
		controller.description = description;

		controller.Init(ID);
		controller.deckID = index;

		//deckTags.Add(index, controller);
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
}
