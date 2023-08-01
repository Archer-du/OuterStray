using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using DisplayInterface;
using System;
using DataCore.BattleElements;

public class HandicapController : MonoBehaviour,
	IHandicapController
{
	public Transform parent;


	private int ownership;

	private float length;
	private float horizontalEdge;
	private float gridWidth;

	public int capacity;//TODO 与数据层链接
	/// <summary>
	/// 手牌数，有上限
	/// </summary>
	/// //TODO
	public int count { get => handiCards.Count; }

	public static bool isDragging;

	public bool awaked = false;

	List<UnitElementController> handiCards;
	internal UnitElementController this[int index]
	{
		get => handiCards[index];
		set => handiCards[index] = value;
	}




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


		awaked = true;
	}








	private float popTime = 0.3f;
	/// <summary>
	/// 
	/// </summary>
	/// <param name="list"></param>
	public void Fill(List<IUnitElementController> list)
	{
		Sequence seq = DOTween.Sequence();
		pushing = true;
		for (int i = 0; i < list.Count; i++)
		{
			UnitElementController element = list[i] as UnitElementController;

			element.gameObject.SetActive(true);
			element.transform.SetParent(transform);

			Vector3 moveBy = GetInsertionPosition(i) - element.transform.position;
			Vector3 rotateBy = new Vector3(0, 0, (1 - (ownership * 2)) * 90);

			seq.Append(element.transform.DOBlendableMoveBy(moveBy, popTime));
			seq.Join(element.transform.DOBlendableRotateBy(rotateBy, popTime)
				.OnComplete(() =>
				{
					handiCards.Add(element);
					UpdateElements();
					UpdateHandicapPosition();
				}));

			
		}
		UpdateElements();

		seq.OnComplete(() =>
		{
			pushing = false;
			UpdateHandicapPosition();
		});
		seq.Play();
	}




	public static bool pushing = false;
	/// <summary>
	/// 播放动画，将element控件加入到手牌列表中
	/// </summary>
	/// <param name="element"></param>
	public void Push(IUnitElementController controller)
	{
		if(count >= capacity)
		{
			return;
		}

		UnitElementController element = controller as UnitElementController;
		pushing = true;
		element.gameObject.SetActive(true);
		element.transform.SetParent(transform);

		PushAnimation(element);

	}

	public void PushAnimation(UnitElementController element)
	{
		Vector3 moveBy = GetInsertionPosition(count) - element.transform.position;
		Vector3 rotateBy = new Vector3(0, 0, (1 - (ownership * 2)) * 90);

		Sequence seq = DOTween.Sequence();
		seq.Append(element.transform.DOBlendableMoveBy(moveBy, popTime));
		seq.Join(element.transform.DOBlendableRotateBy(rotateBy, popTime)
			.OnComplete(() =>
			{
				handiCards.Add(element);
				UpdateElements();
				UpdateHandicapPosition();
				pushing = false;
			}));
		seq.Play();
	}
	private void UpdateElements()
	{
		for (int i = 0; i < handiCards.Count; i++)
		{
			handiCards[i].handicapIdx = i;
			handiCards[i].dataState = UnitState.inHandicap;
		}
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
		UpdateElements();

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
			Vector3 dstPos = GetInsertionPosition(i);
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
	/// <summary>
	/// 根据索引获取对应手牌区位置坐标
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	//public Vector3 GetInsertionPosition(int index)
	//{
	//	return originalPosition + (1 - (ownership * 2)) * (index * new Vector3(gridWidth - length / 2, 0, 0));
	//}
	public Vector3 GetInsertionPosition(int index)
	{
		Vector3 offset = new Vector3((index - count / 2) * gridWidth + gridWidth / 2 * ((count + 1) % 2), 0, 0);
		return transform.position + offset;
	}
	public Vector3 GetInsertionPosition(int index, int count)
	{
		Vector3 offset = new Vector3((index - count / 2) * gridWidth + gridWidth / 2 * ((count + 1) % 2), 0, 0);
		return transform.position + offset;
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

