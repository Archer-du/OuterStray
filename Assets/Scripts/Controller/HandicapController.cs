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

	public int capacity;
	/// <summary>
	/// 手牌数，有上限
	/// </summary>
	/// //TODO
	public int count { get => handiCards.Count; }

	List<BattleElementController> handiCards;
	internal BattleElementController this[int index]
	{
		get => handiCards[index];
		set => handiCards[index] = value;
	}

	public void Init()
	{
		ownership = transform.GetSiblingIndex();

		horizontalEdge = 100f;
		length = GetComponent<RectTransform>().rect.width - 2 * horizontalEdge;
		gridWidth = 240f;

		//TODO config
		capacity = 8;

		//isDragging = false;

		handiCards = new List<BattleElementController>();
	}








	private float popTime = 0.3f;
	/// <summary>
	/// 
	/// </summary>
	/// <param name="list"></param>
	public void Fill(List<IBattleElementController> list)
	{
		Sequence seq = DOTween.Sequence();
		//pushing = true;
		for (int i = 0; i < list.Count; i++)
		{
			BattleElementController element = list[i] as BattleElementController;

			element.animeLock = true;
			element.gameObject.SetActive(true);
			element.transform.SetParent(transform);
			element.transform.localScale = element.handicapScale;

			Vector3 moveBy = GetLogicPosition(i) - element.transform.position;
			Vector3 rotateBy = new Vector3(0, 0, (1 - (ownership * 2)) * 90);

			seq.Append(element.transform.DOBlendableMoveBy(moveBy, popTime));
			seq.Join(element.transform.DOBlendableRotateBy(rotateBy, popTime)
				.OnComplete(() =>
				{
					element.animeLock = false;
					handiCards.Add(element);
					UpdateHandicapPosition();
				}));
		}

		seq.OnComplete(() =>
		{
			UpdateHandicapPosition();
		});
		seq.Play();
	}




	public bool pushing;
	/// <summary>
	/// 播放动画，将element控件加入到手牌列表中
	/// </summary>
	/// <param name="element"></param>
	public void Push(IBattleElementController controller)
	{
		if(count >= capacity)
		{
			return;
		}

		BattleElementController element = controller as BattleElementController;

		handiCards.Add(element);

        element.animeLock = true;
		element.gameObject.SetActive(true);
        element.NameTag.gameObject.SetActive(true);
        element.nameText.gameObject.SetActive(true);
        element.costTag.gameObject.SetActive(true);
        element.costText.gameObject.SetActive(true);
		element.transform.SetParent(transform);
		element.transform.localScale = element.handicapScale;

		PushAnimation(element);

	}
	/// <summary>
	/// 成功移除卡牌时调用
	/// </summary>
	/// <param name="handicapIdx"></param>
	/// <returns></returns>
	public IBattleElementController Pop(int handicapIdx)
	{
		BattleElementController controller = handiCards[handicapIdx];
		handiCards.RemoveAt(handicapIdx);

		UpdateHandicapPosition();

		return controller as IBattleElementController;
	}
	public void PushAnimation(BattleElementController element)
	{
		Vector3 moveBy = GetLogicPosition(count) - element.transform.position;

		Sequence seq = DOTween.Sequence();

		seq.Append(element.transform.DOBlendableMoveBy(moveBy, popTime));
		seq.Join(element.transform.DORotate(new Vector3(0, 0, ownership * 180), popTime)
			.OnComplete(() =>
			{
				element.animeLock = false;
				UpdateHandicapPosition();
			}));
		seq.Play();
	}






	private float updateTime = 0.2f;
	public void UpdateHandicapPosition()
	{
		UpdateElements();

		for (int i = 0; i < handiCards.Count; i++)
		{
			handiCards[i].animeLock = true;
			Vector3 oriPos = handiCards[i].transform.position;
			Vector3 dstPos = handiCards[i].handicapLogicPosition;
			//TODO config
			int temp = i;
			handiCards[i].transform.DOMove(dstPos, updateTime)
				.OnComplete(() => handiCards[temp].animeLock = false);
			
			handiCards[i].handicapOrder = i;
		}
	}
	private void UpdateElements()
	{
		for (int i = 0; i < handiCards.Count; i++)
		{
			handiCards[i].handicapIdx = i;
			handiCards[i].handicapLogicPosition = GetLogicPosition(i);
		}
	}






	public float returnTime = 0.2f;
	public BattleElementController draggingElement;
	public void Insert(BattleElementController element)
	{
		//isDragging = true;
		draggingElement = element;
		draggingElement.animeLock = true;
		draggingElement.transform.DOScale(draggingElement.handicapScale, returnTime).OnComplete(() => draggingElement.animeLock = false);
		element.transform.DOMove(GetLogicPosition(element.handicapIdx), returnTime).OnComplete(ResetHierachy);
	}
	private void ResetHierachy()
	{
		draggingElement.transform.SetParent(transform);
		draggingElement.transform.SetSiblingIndex(draggingElement.handicapIdx + 2);

	}
	public Vector3 GetLogicPosition(int index)
	{
		Vector3 offset = new Vector3((index - count / 2) * gridWidth + gridWidth / 2 * ((count + 1) % 2), 0, 0);
		return transform.position + offset;
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

