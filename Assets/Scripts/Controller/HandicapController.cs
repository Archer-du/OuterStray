using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using DisplayInterface;
using System;

public class HandicapController : MonoBehaviour,
	IHandicapController
{
	private int ownership;

	private float length;
	private float horizontalEdge;
	private float gridWidth;

	public int capacity;//TODO 与数据层链接
	/// <summary>
	/// 手牌数，有上限
	/// </summary>
	/// //TODO
	private int count { get => handiCards.Count; }

	public static bool isDragging;


	List<UnitElementController> handiCards;
	internal UnitElementController this[int index]
	{
		get => handiCards[index];
		set => handiCards[index] = value;
	}

	// 悬停时向上移动的距离
	private float hoverDistance;
	// 移动的速度
	private float moveTime = 0.4f;
	// 原始位置
	private Vector3 originalPosition;
	// 目标位置
	private Vector3 targetPosition;


	public bool awaked = false;
	//note: start太晚，awake太早，晕了 TODO
	public void Init()
	{
		ownership = transform.GetSiblingIndex();

		horizontalEdge = 100f;
		length = GetComponent<RectTransform>().rect.width - 2 * horizontalEdge;
		gridWidth = 240f;

		capacity = 8;

		isDragging = false;

		handiCards = new List<UnitElementController>();

		hoverDistance = 150f;
		// 保存原始位置
		originalPosition = transform.position;
		// 初始化目标位置为原始位置
		targetPosition = originalPosition + hoverDistance * Vector3.up;
		awaked = true;
	}
	/// <summary>
	/// 播放动画，将element控件加入到手牌列表中
	/// </summary>
	/// <param name="element"></param>
	public float popTime = 0.6f;
	public static bool pushing = false;
	public void Push(IUnitElementController controller)
	{
		if(!awaked) Init();
		if(count >= capacity)
		{
			return;
		}

		UnitElementController element = controller as UnitElementController;

		element.gameObject.SetActive(true);

		element.transform.SetParent(transform);

		Vector3 moveBy = GetTargetPosition() - element.transform.position;

		Vector3 rotateBy = new Vector3(0, 0, (1 - (ownership * 2)) * 90);

		Sequence seq = DOTween.Sequence();
		seq.Append(element.transform.DOBlendableMoveBy(moveBy, popTime));
		seq.Join(element.transform.DOBlendableRotateBy(rotateBy, popTime));
		pushing = true;
		seq.Play();
		seq.OnComplete(() => pushing = false);

		handiCards.Add(element);
		for(int i = 0; i < handiCards.Count; i++)
		{
			handiCards[i].handicapIdx = i;
		}
	}
	/// <summary>
	/// 获取手牌区下一个空位的位置
	/// </summary>
	/// <returns></returns>
	private Vector3 GetTargetPosition()
	{
		return originalPosition + (1 - (ownership * 2)) * (- new Vector3(length / 2, 0, 0) + count * new Vector3(gridWidth, 0, 0));
	}





	/// <summary>
	/// 成功移除卡牌时调用
	/// </summary>
	/// <param name="handicapIdx"></param>
	/// <returns></returns>
	public IUnitElementController Pop(int handicapIdx)
	{
		IUnitElementController controller = handiCards[handicapIdx];
		handiCards.RemoveAt(handicapIdx);
		for (int i = 0; i < handiCards.Count; i++)
		{
			handiCards[i].handicapIdx = i;
		}

		HandicapController.isDragging = true;

		UpdateHandicapPosition();

		return controller;
	}
	public float updateTime = 0.2f;
	public void UpdateHandicapPosition()
	{
		for (int i = 0; i < handiCards.Count; i++)
		{
			Vector3 oriPos = handiCards[i].transform.position;

			Vector3 dstPos = originalPosition + (1 - ownership * 2) * new Vector3(i * gridWidth - length / 2, 0, 0);
			//TODO config

			handiCards[i].transform.DOMove(dstPos, updateTime).OnComplete(() => HandicapController.isDragging = false);

		}
	}





	public float returnTime = 0.2f;
	public UnitElementController draggingElement;
	public void Insert(UnitElementController element)
	{
		//TODO 这里得锁住
		isDragging = true;

		draggingElement = element;
		element.transform.DOMove(GetInsertionPosition(element.handicapIdx), returnTime).OnComplete(ResetHierachy);
	}
	private void ResetHierachy()
	{
		isDragging = false;

		draggingElement.transform.SetParent(transform);
		draggingElement.transform.SetSiblingIndex(draggingElement.handicapIdx + 2);

	}
	public Vector3 GetInsertionPosition(int index)
	{
		return originalPosition + (1 - (ownership * 2)) * (-new Vector3(length / 2, 0, 0) + index * new Vector3(gridWidth, 0, 0));
	}





	//public void OnPointerEnter(PointerEventData eventData)
	//{
	//	if(ownership != 0)
	//	{
	//		return;
	//	}
	//	if(isDragging)
	//	{
	//		return;
	//	}
	//	transform.DOMove(targetPosition, moveTime);
	//}

	//public void OnPointerExit(PointerEventData eventData)
	//{
	//	if (ownership != 0)
	//	{
	//		return;
	//	}
	//	transform.DOMove(originalPosition, moveTime);
	//}
}

