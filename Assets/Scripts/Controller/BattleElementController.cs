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

public class BattleElementController : MonoBehaviour,
		IPointerEnterHandler, IPointerExitHandler,
	    IDragHandler,
	    IBeginDragHandler, IEndDragHandler
{
	public event System.Action<ElementState> OnElementStateChanged;

	[Header("BasicInfo")]
	public static int cardWidth = 360;

	[Header("Connections")]
	public BattleSceneManager battleSceneManager;
	public Transform stack;
	protected Transform buffer;
	public Canvas canvas;

	public HandicapController handicap;

	[Header("Data")]
	private ElementState DataState;
	public ElementState dataState
	{
		get => DataState;
		set
		{
			DataState = value;
			OnElementStateChanged?.Invoke(DataState);
			//switch (DataState)
			//{
			//	case ElementState.inStack:
			//		handicapInspect.active = false;
			//		battleLineInspect.active = false;
			//		break;
			//	case ElementState.inHandicap:
			//		handicapInspect.active = true;
			//		battleLineInspect.active = false;
			//		break;
			//	case ElementState.inBattleLine:
			//		handicapInspect.active = false;
			//		battleLineInspect.active = true;
			//		break;
			//	case ElementState.destroyed:
			//		handicapInspect.active = false;
			//		battleLineInspect.active = false;
			//		break;
			//}
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
	public GameObject Inspector;

	public Image InspectorGround;
	public Image InspectorShell;
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

	public Vector3 handicapScale;
	public Vector3 inspectScale;
	public Vector3 battleFieldScale;

	public Vector3 battleFieldPosition;

	public Vector3 targetTextScale;
	public Vector3 originTextScale;

	public Vector3 counterScaleOrigin;
	public Vector3 counterScaleEnlarge;

	public float counterfontSizeOrigin;
	public float counterfontSizeEnlarge;

	public float normalFontSizeOrigin;
	public float normalFontSizeEnlarge;

	public float scaleTime = 0.3f;

	public float moveTime = 0.2f;

	public int upperOrder = 100;
	public int lowerOrder = 0;

	public Vector2 inputOffset;

	public bool animeLock;

	[Header("Components")]
	public TransformInspector handicapInspect;
	public InspectPanelController battleLineInspect;

	public void BasicInfoInit()
	{
		animeLock = false;
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
		buffer = GameObject.Find("Buffer").transform;

		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();

		handicap = battleSceneManager.handicapController[ownership];
		stack = battleSceneManager.cardStackController[ownership].transform;
	}


	public void Init(string ID, int ownership, string name, string categories, int cost, string description)
	{
		this.ownership = ownership;
		this.nameContent = name;
		this.category = categories;
		this.description = description;

		BasicInfoInit();

		nameText.text = name;
		costText.text = cost.ToString();
		componentDescriptionText.text = description;

		LoadCardResources(ID);

		battleFieldScale = transform.localScale;
		handicapScale = 1.35f * battleFieldScale;
		inspectScale = 1.2f * handicapScale;

		handicapInspect.Init(battleFieldScale);
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


	public virtual void OnDrag(PointerEventData eventData)
	{
		if (ownership != 0)
		{
			return;
		}
		if (BattleSceneManager.Turn != 0)
		{
			return;
		}
		//拖动显示设置
		if (handicap.isDragging)
		{
			transform.SetParent(buffer);
			transform.DOScale(handicapScale, scaleTime);
			transform.position = eventData.position - inputOffset;
		}
	}
	public virtual void OnBeginDrag(PointerEventData eventData)
	{
		if (animeLock)
		{
			return;
		}
		if (ownership != 0)
		{
			return;
		}
		//锁住玩家操作
		if (BattleSceneManager.Turn != 0)
		{
			return;
		}
		//在手牌区：部署或cast
		if (dataState == ElementState.inHandicap)
		{
			handicap.isDragging = true;
		}
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
		if (animeLock)
		{
			return;
		}
		if (ownership != 0)
		{
			return;
		}
		//锁住玩家操作
		if (BattleSceneManager.Turn != 0)
		{
			return;
		}
	}

	public static float moveUp = 450f;
	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if(animeLock)
		{
			return;
		}
		if(BattleLineController.updating)
		{
			return;
		}
		if (handicap.isDragging)
		{
			return;
		}
		if (handicap.pushing)
		{
			return;
		}
		if (ownership != 0)
		{
			return;
		}
		if (dataState == ElementState.inHandicap)
		{
			canvas.sortingOrder = upperOrder;
			transform.DOScale(inspectScale, moveTime);
			transform.DOMove(handicap.GetInsertionPosition(handicapIdx) + moveUp * Vector3.up, moveTime);
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (animeLock)
		{
			return;
		}
		if (BattleLineController.updating)
		{
			return;
		}
		if (handicap.isDragging)
		{
			return;
		}
		if (handicap.pushing)
		{
			return;
		}
		if (ownership != 0)
		{
			return;
		}
		if (dataState == ElementState.inHandicap)
		{
			handicap.UpdateHandicapPosition();
			transform.DOScale(handicapScale, 0.2f);
			transform.DOMove(handicap.GetInsertionPosition(handicapIdx), moveTime);
		}
	}
}
