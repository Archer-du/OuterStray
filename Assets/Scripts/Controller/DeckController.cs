using DisplayInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckController : MonoBehaviour,
	IDeckController
{
	[Header("Data")]
	public DeckTagController[] deckTags;

	[Header("Prototype")]
	public DeckTagController deckTagPrototype;
	public void Init()
	{
	}
	public void InstantiateDeckTag()
	{

	}
}
