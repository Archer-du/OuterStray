using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class DeckTagController : MonoBehaviour, IComparable<DeckTagController>,
		IDragHandler,
		IBeginDragHandler, IEndDragHandler,
		IPointerEnterHandler, IPointerExitHandler
{
	public DeckController controller;
	public TacticalPanelDisplay panel
	{
		get => controller.sceneManager.currentNode.GetComponent<TacticalPanelDisplay>();
	}

	[Header("Data")]
	public int deckID;
	public string nameContent;
	public string category;
	public int cost;
	public int attack;
	public int health;
	public int maxHealth;
	public int counter;
	public string description;

	[Header("DeckTag")]
	public TMP_Text deckNameText;
	public Image deckCategoryIcon;


	[Header("Display")]
	public Transform buffer;

	public Image ground;
	public float duration;

	[Header("Inspector")]
	public GameObject inspector;

	public Image costTag;
	public Image nameTag;
	public Image backGround;
	public Image frame;
	public Image cardImage;
	public Image categoryIcon;

	public TMP_Text descriptionText;

	public TMP_Text nameText;
	public TMP_Text costText;
	public TMP_Text counterText;

	public Image attackIcon;
	public Image healthIcon;
	public TMP_Text attackText;
	public TMP_Text healthText;

	public void Init(string ID)
	{
		LoadCardResources(ID);

		deckNameText.text = nameContent;
		nameText.text = nameContent;
		descriptionText.text = description;

	}
	public Transform Component;
	public void Start()
	{
		
	}
	public void UpdateInfo()
	{
		if (category == "Command")
		{
			attackIcon.enabled = false;
			attackText.enabled = false;
			healthIcon.enabled = false;
			healthText.enabled = false;
		}
		else
		{
			attackText.text = attack.ToString();
			healthText.text = health.ToString();
		}
		costText.text = cost.ToString();
		counterText.text = category == "Construction" ? "" : counter.ToString();
	}
	private void LoadCardResources(string ID)
	{
		cardImage.sprite = Resources.Load<Sprite>("CardImage/" + ID);

		Color color;
		switch (category)
		{
			case "LightArmor":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#429656", out color))
				{
					backGround.color = color;
					frame.color = color;
					nameTag.color = color;
					costTag.color = color;
					deckCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[11];
					categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[11];
				}
				break;
			case "Artillery":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#CE8849", out color))
				{
					backGround.color = color;
					frame.color = color;
					nameTag.color = color;
					costTag.color = color;
					deckCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[8];
					categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[8];
				}
				break;
			case "Motorized":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#426A84", out color))
				{
					backGround.color = color;
					frame.color = color;
					nameTag.color = color;
					costTag.color = color;
					deckCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[9];
					categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[9];
				}
				break;
			case "Guardian":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#97A5A4", out color))
				{
					backGround.color = color;
					frame.color = color;
					nameTag.color = color;
					costTag.color = color;
					deckCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[10];
					categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[10];
				}
				break;
			case "Construction":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color))
				{
					backGround.color = color;
					frame.color = color;
					nameTag.color = color;
					costTag.color = color;
					deckCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[12];
					categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[12];
				}
				break;
			case "Command":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color))
				{
					color = Color.gray;
					backGround.color = color;
					frame.color = color;
					nameTag.color = color;
					costTag.color = color;
				}
				break;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!controller.sceneManager.panelEnabled) return;
		if (controller.sceneManager.currentNode is not MedicalNodeController) return;
		if (category == "Command") return;
		GetComponent<InspectPanelController>().disable = true;
		GetComponent<InspectPanelController>().inspectPanel.alpha = 1.0f;

	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!controller.sceneManager.panelEnabled) return;
		if (controller.sceneManager.currentNode is not MedicalNodeController) return;
		if (category == "Command") return;

		//TODO
		GetComponent<InspectPanelController>().inspectPanel.alpha = 1.0f;
		inspector.transform.position = eventData.position - BattleElementController.inputOffset;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		GetComponent<InspectPanelController>().disable = false;
		if (!controller.sceneManager.panelEnabled) return;
		if (controller.sceneManager.currentNode is not MedicalNodeController) return;
		if (category == "Command") return;

		panel.InteractCheck(eventData.position, this);

		GetComponent<InspectPanelController>().inspectPanel.alpha = 0f;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ground.DOFade(0.5f, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ground.DOFade(0, duration);
	}




	public int CompareTo(DeckTagController other)
	{
		if(category == other.category)
		{
			return cost.CompareTo(other.cost);
		}
		else return category.CompareTo(other.category);
	}
}
