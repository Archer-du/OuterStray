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
	public BattleSceneManager battleSceneManager;
	public Transform stack;
	protected Transform buffer;
	public Canvas canvas;


	public static int cardWidth = 360;


	public HandicapController handicap;
	public int handicapIdx;


	public string ID;
	public int ownership;
	public string nameContent;
	public string category;
	public string description;
	public int cost;
	public ElementState dataState;

	public int preprocessed;//TODO 敌方特有，之后会改

	public Image CardImage;

	public Image elementGround;
	public Image elementShell;
	public Image elementFrame;
	public Image NameTag;
	public Image costTag;
	public Image categoryIcon;

	public GameObject InspectComponent;
	public Image componentFrame;
	public TMP_Text componentAttackText;
	public TMP_Text componentHealthText;
	public TMP_Text componentAttackCounterText;
	public TMP_Text componentDescriptionText;
	public Image componentCategoryIcon;


	public Image slot;


	public TMP_Text nameText;
	public TMP_Text costText;

	public Vector3 originTextScale;
	public Vector3 targetTextScale;

	public Vector3 handicapScale;
	public Vector3 inspectScale;
	public Vector3 battleFieldScale;

	public float scaleTime = 0.3f;

	public float moveTime = 0.2f;

	public float grayScale = 0.2f;

	public int upperOrder = 100;
	public int lowerOrder = 0;

	public Vector2 inputOffset;



	public void OnEnable()
	{
		//输入偏移量
		inputOffset = new Vector2(1980, 1080);

		canvas = GetComponent<Canvas>();
		buffer = GameObject.Find("Buffer").transform;

		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();

		handicap = battleSceneManager.handicapController[ownership];
		stack = battleSceneManager.cardStackController[ownership].transform;
	}




	public void Init(string ID, int ownership, string name, string categories, string description)
	{
		OnEnable();
		this.ownership = ownership;
		this.nameContent = name;
		this.category = categories;
		this.description = description;

		LoadCardResources(ID);

		preprocessed = ownership;

		battleFieldScale = transform.localScale;
		handicapScale = 1.35f * battleFieldScale;
		inspectScale = 1.2f * handicapScale;

		originTextScale = nameText.transform.localScale;
		targetTextScale = nameText.transform.localScale * 1.5f;

		transform.localScale = handicapScale;
	}


	private void LoadCardResources(string ID)
	{
		CardImage.sprite = Resources.Load<Sprite>("CardImage/" + ID);
		//if (ownership == 1)
		//{
		//	CardImage.rectTransform.sizeDelta = new Vector2(10, 13);
		//}
		//else
		//{
		//	CardImage.rectTransform.sizeDelta = new Vector2(10, 11);
		//}

		Color color;
		switch (category)
		{
			case "Guardian":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#97A5A4", out color))
				{
					elementGround.color = color;
					elementFrame.color = color;
					//slot.color = color;
					NameTag.color = color;
					costTag.color = color;
				}
				break;
			case "Artillery":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#CE8849", out color))
				{
					NameTag.color = color;
					elementFrame.color = color;
					slot.color = color;
					costTag.color = color;

					elementGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[22];
				}
				break;
			case "Motorized":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#426A84", out color))
				{
					NameTag.color = color; // 赋值给 Image 组件的 color 属性
					elementFrame.color = color;
					slot.color = color;
					costTag.color = color;

					elementGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[23];
				}
				break;
			case "LightArmor":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#429656", out color))
				{
					NameTag.color = color; // 赋值给 Image 组件的 color 属性
					elementFrame.color = color;
					slot.color = color;
					costTag.color = color;

					elementGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[24];
				}
				break;
			case "Construction":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color))
				{
					NameTag.color = color; // 赋值给 Image 组件的 color 属性
					elementFrame.color = color;
					slot.color = color;
					costTag.color = color;

					elementGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[25];
				}
				break;
			case "Command":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color))
				{
					NameTag.color = color; // 赋值给 Image 组件的 color 属性
					elementFrame.color = color;
					slot.color = color;
					costTag.color = color;

					elementGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[25];
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
		if (HandicapController.isDragging)
		{
			transform.SetParent(buffer);
			transform.DOScale(handicapScale, scaleTime);
			transform.position = eventData.position - inputOffset;
		}
	}
	public virtual void OnBeginDrag(PointerEventData eventData)
	{
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
			HandicapController.isDragging = true;
		}
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
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

	public float moveUp = 400f;
	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (HandicapController.isDragging)
		{
			return;
		}
		if (HandicapController.pushing)
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
		if (HandicapController.isDragging)
		{
			return;
		}
		if (HandicapController.pushing)
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
