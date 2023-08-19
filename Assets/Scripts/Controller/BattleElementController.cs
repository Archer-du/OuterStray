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
	public event Action<ElementState> OnElementStateChanged;

	public static event Action GlobalAnimeLocked;
	public static event Action GlobalAnimeUnlocked;

	public event Action AnimeLocked;
	public event Action AnimeUnlocked;

	private static bool GlobalAnimeLock;
	public static bool draggingLock
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
	[SerializeField] private bool AnimeLock;
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

	[Header("StaticInfo")]
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

	private int HandicapOrder;
	public int handicapOrder
	{
		get => HandicapOrder;
		set
		{
			HandicapOrder = value;
			canvas.sortingOrder = value;
		}
	}
	private int BattleOrder;
	public int battleOrder
	{
		get => BattleOrder;
		set
		{
			BattleOrder = value;
			canvas.sortingOrder = value;
		}
	}
	public Canvas canvas;

	public HandicapController handicap
	{
		get => battleSceneManager.handicapController[ownership];
	}

	[Header("Data")]
	[SerializeField] private ElementState DataState;
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

	public GameObject InspectComponent;

	public GameObject counterIcon;
	public Image categoryIcon;
	public Image componentFrame;
	public TMP_Text componentDescriptionText;
	public Vector3 componentPosition;

	[Header("Components")]
	public HandicapInspector handicapInspect;

	public void TransformInfoInit()
	{
		componentPosition = InspectComponent.transform.position;

		battleFieldScale = transform.localScale;
		handicapScale = 1.35f * battleFieldScale;

		targetTextScale = nameText.transform.localScale * 1.35f;
		originTextScale = nameText.transform.localScale;

		counterScaleOrigin = new Vector3(1, 1, 1);
		counterScaleEnlarge = new Vector3(1.5f, 1.5f, 1);

		counterfontSizeOrigin = 36;
		counterfontSizeEnlarge = 60;

		normalFontSizeOrigin = 40;
		normalFontSizeEnlarge = 50;
	}


	public void Init(string ID, int ownership, string name, string categories, int cost, string description)
	{
		//data
		this.ID = ID;
		this.ownership = ownership;
		this.nameContent = name;
		this.category = categories;
		this.description = description;

		animeLock = false;
		inputLock = false;
		//输入偏移量
		inputOffset = new Vector2(1980, 1080);

		//transform
		TransformInfoInit();

		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();

		handicapInspect = GetComponent<HandicapInspector>();
		handicapInspect.Init(handicapScale, this);

		nameText.text = name;
		costText.text = cost.ToString();
		componentDescriptionText.text = description;

		LoadCardResources(ID);
	}

	public Color color;
	/// <summary>
	/// 根据ID索引加载卡牌资源
	/// </summary>
	/// <param name="ID"></param>
	private void LoadCardResources(string ID)
	{
		CardImage.sprite = Resources.Load<Sprite>("CardImage/" + ID);

		switch (category)
		{
			case "LightArmor":
				UnityEngine.ColorUtility.TryParseHtmlString("#429656", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[11];
				break;
			case "Artillery":
				UnityEngine.ColorUtility.TryParseHtmlString("#CE8849", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[8];
				break;
			case "Motorized":
				UnityEngine.ColorUtility.TryParseHtmlString("#426A84", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[9];
				break;
			case "Guardian":
				UnityEngine.ColorUtility.TryParseHtmlString("#97A5A4", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[10];
				break;
			case "Construction":
				UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[12];
				break;
			case "Command":
				color = Color.gray;
				CardImage.rectTransform.sizeDelta = new Vector2(10, 13);
				break;
		}

		elementGround.color = color;
		elementFrame.color = color;
		componentFrame.color = color;
		NameTag.color = color;
		costTag.color = color;
	}
}
