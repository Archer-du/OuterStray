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

	public static int cardWidth = 360;


	public HandicapController handicap;
	public int handicapIdx;



	public string ID;
	public int ownership;
	public string nameContent;

	public string type;
	public string category;
	public string description;
	public int cost;

	public Image descriptionPanel;
	public TMP_Text descriptionText;

	public ElementState dataState;



	protected Transform buffer;

	public float scaleTime = 0.3f;

	public Canvas canvas;





	// 原始位置
	private Vector3 originalPosition;
	// 目标位置
	private Vector3 targetPosition;

	public Vector3 enlargeScale;
	public Vector3 originScale;
	public float moveTime = 0.2f;

	public float grayScale = 0.2f;

	public int upperOrder = 100;
	public int lowerOrder = 0;

	public Vector2 offset;


	public void OnEnable()
	{
		offset = new Vector2(1980, 1080);

		canvas = GetComponent<Canvas>();
		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();//TODO

		//????
		handicap = battleSceneManager.handicapController[ownership];

		buffer = GameObject.Find("Buffer").transform;
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
		if (HandicapController.isDragging) // 如果正在拖动
		{
			transform.SetParent(buffer);
			transform.DOScale(originScale, scaleTime);
			transform.position = eventData.position - offset; // 设置当前对象的位置为鼠标位置
		}
	}
	public int handicapChildNum = 2;
	public int inlineChildNum = 4;
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
			descriptionText.gameObject.SetActive(false);
			descriptionPanel.DOColor(Color.clear, 0.1f);

			HandicapController.isDragging = true; // 开始拖动
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
		//部署条件判定
		if (dataState == ElementState.inHandicap)
		{
			HandicapController.isDragging = false; // 结束拖动
			if (battleSceneManager.PlayerDeploy(eventData.position, this.handicapIdx) >= 0)
			{
				return;
			}
			handicap.Insert(this);
		}

	}

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
			transform.DOScale(enlargeScale, moveTime);
			transform.DOMove(handicap.GetInsertionPosition(handicapIdx) + 400f * Vector3.up, moveTime);

			//TODO
			descriptionPanel.DOColor(Color.gray, 0.2f).OnComplete(() => descriptionText.gameObject.SetActive(true));
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
			transform.DOScale(originScale, 0.2f);
			transform.DOMove(handicap.GetInsertionPosition(handicapIdx), moveTime);

			descriptionText.gameObject.SetActive(false);
			descriptionPanel.DOColor(Color.clear, 0.1f);
		}
	}
}
