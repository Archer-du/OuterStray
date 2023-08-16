using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DisplayInterface;
using TMPro;
using UnityEngine.EventSystems;
using DataCore.Cards;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Rendering;
using InputHandler;
using DataCore.BattleElements;
using System;

public class BattleElementController : MonoBehaviour
{
	public event System.Action<ElementState> OnElementStateChanged;

	public static event System.Action GlobalAnimeLocked;
	public static event System.Action GlobalAnimeUnlocked;

	public event System.Action AnimeLocked;
	public event System.Action AnimeUnlocked;

	private static bool GlobalAnimeLock;
	public static bool globalAnimeLock
	{
		get => GlobalAnimeLock;
		set
		{
			GlobalAnimeLock = value;
			if(value == true)
			{
				GlobalAnimeLocked?.Invoke();
			}
			else
			{
				GlobalAnimeUnlocked?.Invoke();
			}
		}
	}
	private bool AnimeLock;
	public bool animeLock
	{
		get => AnimeLock;
		set
		{
			AnimeLock = value;
			if(value == true)
			{
				AnimeLocked?.Invoke();
			}
			else
			{
				AnimeUnlocked?.Invoke();
			}
		}
	}
	public bool inputLock;

	public static Vector2 inputOffset = new Vector2(1980, 1080);

	public static int cardWidth = 360;

	[Header("BasicInfo")]
	public Vector3 battleFieldScale;
	public Vector3 handicapScale;

	public Vector3 targetTextScale;
	public Vector3 originTextScale;

	public Vector3 counterScaleOrigin;
	public Vector3 counterScaleEnlarge;

	public Vector3 handicapLogicPosition;

	public float counterfontSizeOrigin;
	public float counterfontSizeEnlarge;

	public float normalFontSizeOrigin;
	public float normalFontSizeEnlarge;

	[Header("Connections")]
	public BattleSceneManager battleSceneManager;
	public Transform stack
	{
		get => battleSceneManager.cardStackController[ownership].transform;
	}
	public Canvas canvas;

	public HandicapController handicap
	{
		get => battleSceneManager.handicapController[ownership];
	}

	[Header("Data")]
	private ElementState DataState;
	public ElementState dataState
	{
		get => DataState;
		set
		{
			DataState = value;
			OnElementStateChanged?.Invoke(DataState);
		}
	}
	public int handicapIdx;

	public string ID;
	public int ownership;
	public string nameContent;
	public string category;
	public string description;
	public int cost;

	[Header("Image")]
	public Image CardImage;
	public Image InspectorImage;

	public Image elementGround;
	public Image elementShell;
	public Image elementFrame;
	public Image NameTag;
	public Image costTag;

	[Header("Text")]
	public TMP_Text nameText;
	public TMP_Text costText;

	[Header("Inspector")]
	public CanvasGroup InspectPanel;

	public Image InspectorGround;
	public Image InspectorFrame;
	public Image InspectorCategoryIcon;
	public Image InspectorNameTag;
	public Image InspectorCostTag;

	public TMP_Text InspectorName;
	public TMP_Text InspectorCost;
	public TMP_Text InspectorAttack;
	public TMP_Text InspectorMaxHealth;
	public TMP_Text InspectorAttackCounter;
	public TMP_Text InspectorDescription;

	public GameObject counterIcon;

	public GameObject InspectComponent;
	public Image componentFrame;
	public Image componentCategoryIcon;
	public TMP_Text componentDescriptionText;
	public Vector3 componentPosition;

	[Header("Components")]
	public HandicapInspector handicapInspect;

	public void BasicInfoInit()
	{
		animeLock = false;
		inputLock = false;
		//输入偏移量
		inputOffset = new Vector2(1980, 1080);

		componentPosition = InspectComponent.transform.position;

		targetTextScale = nameText.transform.localScale * 1.35f;
		originTextScale = nameText.transform.localScale;

		counterScaleOrigin = new Vector3(1, 1, 1);
		counterScaleEnlarge = new Vector3(1.5f, 1.5f, 1);

		counterfontSizeOrigin = 36;
		counterfontSizeEnlarge = 60;

		normalFontSizeOrigin = 40;
		normalFontSizeEnlarge = 50;

		canvas = GetComponent<Canvas>();

		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();

		handicapInspect = GetComponent<HandicapInspector>();
		handicapInspect.Init(handicapScale, this);
	}


	public void Init(string ID, int ownership, string name, string categories, int cost, string description)
	{
		this.ownership = ownership;
		this.nameContent = name;
		this.category = categories;
		this.description = description;

		battleFieldScale = transform.localScale;
		handicapScale = 1.35f * battleFieldScale;
		BasicInfoInit();

		nameText.text = name;
		costText.text = cost.ToString();
		componentDescriptionText.text = description;

		LoadCardResources(ID);
	}


	private void LoadCardResources(string ID)
	{
		CardImage.sprite = Resources.Load<Sprite>("CardImage/" + ID);
		if (ownership == 1)
		{
			CardImage.rectTransform.sizeDelta = new Vector2(10, 13);
		}

		Color color;
		switch (category)
		{
			case "LightArmor":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#429656", out color))
				{
					elementGround.color = color;
					elementFrame.color = color;
					componentFrame.color = color;
					NameTag.color = color;
					costTag.color = color;
					componentCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[11];
				}
				break;
			case "Artillery":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#CE8849", out color))
				{
					elementGround.color = color;
					elementFrame.color = color;
					componentFrame.color = color;
					NameTag.color = color;
					costTag.color = color;
					componentCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[8];
				}
				break;
			case "Motorized":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#426A84", out color))
				{
					elementGround.color = color;
					elementFrame.color = color;
					componentFrame.color = color;
					NameTag.color = color;
					costTag.color = color;
					componentCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[9];
				}
				break;
			case "Guardian":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#97A5A4", out color))
				{
					elementGround.color = color;
					elementFrame.color = color;
					componentFrame.color = color;
					NameTag.color = color;
					costTag.color = color;
					componentCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[10];
				}
				break;
			case "Construction":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color))
				{
					elementGround.color = color;
					elementFrame.color = color;
					componentFrame.color = color;
					NameTag.color = color;
					costTag.color = color;
					componentCategoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[12];
				}
				break;
			case "Command":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color))
				{
					color = Color.gray;
					elementGround.color = color;
					elementFrame.color = color;
					componentFrame.color = color;
					NameTag.color = color;
					costTag.color = color;
					CardImage.rectTransform.sizeDelta = new Vector2(10, 13);
				}
				break;
		}
	}
}
