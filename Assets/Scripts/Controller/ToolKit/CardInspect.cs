using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInspect : MonoBehaviour
{
	public int deckID;

	public string ID;
	public string category;

	public string description;

	public Image costTag;
	public Image nameTag;
	public Image backGround;
	public Image frame;
	public Image cardImage;
	public Image categoryIcon;

	public TMP_Text nameText;
	public TMP_Text descriptionText;

	public TMP_Text costText;
	public TMP_Text counterText;

	public Image attackIcon;
	public Image healthIcon;
	public TMP_Text attackText;
	public TMP_Text healthText;

	public void CopyInfo(DeckTagController other)
	{
		deckID = other.deckID;

		costTag.color = other.costTag.color;
		nameTag.color = other.nameTag.color;
		backGround.color = other.backGround.color;
		frame.color = other.frame.color;
		cardImage.sprite = other.cardImage.sprite;
		categoryIcon.sprite = other.categoryIcon.sprite;

		nameText.text = other.nameText.text;
		descriptionText.text = other.descriptionText.text;

		costText.text = other.costText.text;
		counterText.text = other.counterText.text;

		attackIcon.enabled = other.attackIcon.enabled;
		healthIcon.enabled = other.healthIcon.enabled;

		if(other.category != "Command")
		{
			attackText.text = other.attackText.text;
			healthText.text = other.healthText.text;
		}
	}
}
