using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using DisplayInterface;
using System;
using DataCore.BattleElements;
using static UnityEditor.Rendering.FilterWindow;

public class HandicapController : MonoBehaviour,
	IHandicapController
{
	public BattleSceneManager battleSceneManager;

	public Transform parent;

	private int ownership;

	public static float gridWidth = 240f;

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

	public void Init(int ownership)
	{
		battleSceneManager = GameManager.GetInstance().battleSceneManager;

		this.ownership = ownership;

		//TODO config
		capacity = 8;

		handiCards = new List<BattleElementController>();
	}




	/// <summary>
	/// 自带动画的fill
	/// </summary>
	/// <param name="list"></param>
	public void Fill(List<IBattleElementController> list, int initialTurn)
	{
		Sequence seq = DOTween.Sequence();

		for (int i = 0; i < list.Count; i++)
		{
			if (i == list.Count - 1)
			{
				if (initialTurn == ownership)
				{
					int temp = i;
					seq.AppendCallback(() => battleSceneManager.UpdateTurn(initialTurn));
					break;
				}
			}
			float popTime = 0.3f;
			BattleElementController element = list[i] as BattleElementController;
			element.inspectLock = true;

			ResetElementDisplay(element);


			Vector3 moveBy = GetLogicPosition(i) - element.transform.position;
			Vector3 rotateBy = new Vector3(0, 0, (1 - (ownership * 2)) * 90);

			seq.Append(element.transform.DOBlendableMoveBy(moveBy, popTime));
			seq.Join(element.transform.DOBlendableRotateBy(rotateBy, popTime)
				.OnComplete(() =>
				{
					element.inspectLock = false;
					handiCards.Add(element);
					UpdateHandicapPosition();
				}));
		}

		seq.OnComplete(() =>
		{
			UpdateHandicapPosition();
			if(initialTurn == ownership)
			{
				Push(list[list.Count - 1], "append");
			}
		});
		seq.Play();
	}

	/// <summary>
	/// 播放动画，将element控件加入到手牌列表中
	/// </summary>
	/// <param name="element"></param>
	public void Push(IBattleElementController controller, string method)
	{
		if(count >= capacity)
		{
			return;
		}
		BattleElementController element = controller as BattleElementController;

		handiCards.Add(element);

		PushAnimation(element, method);
	}
	public void ResetElementDisplay(BattleElementController element)
	{
		element.gameObject.SetActive(true);
		element.NameTag.gameObject.SetActive(true);
		element.nameText.gameObject.SetActive(true);
		element.costTag.gameObject.SetActive(true);
		element.costText.gameObject.SetActive(true);
		element.transform.SetParent(transform);
		element.transform.localScale = element.handicapScale;
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
	/// <summary>
	/// 解锁
	/// </summary>
	/// <param name="element"></param>
	public void PushAnimation(BattleElementController element, string method)
	{
		float popTime = 0.3f;
		float waitTime = 1f;

		element.inspectLock = true;
		Sequence seq = DOTween.Sequence();

		seq.AppendInterval(method == "append" ? 1.8f : 0);

		seq.AppendCallback(() => ResetElementDisplay(element));
		//移动到屏幕中心
		if (element.ownership == 0)
		{
			seq.Append(element.transform.DOMove(Vector3.zero, popTime));
			seq.Join(element.transform.DOScale(element.showScale, popTime));
			seq.Join(element.transform.DORotate(new Vector3(0, 0, ownership * 180), popTime));
	
			//展示等待
			seq.AppendInterval(waitTime);
			//加入手牌
			Vector3 dstPos = GetLogicPosition(count);
			seq.Append(element.transform.DOMove(dstPos, popTime)
				.OnComplete(() =>
				{
					element.inspectLock = false;
					UpdateHandicapPosition();
				}));
		}
		else
		{
			//加入手牌
			Vector3 dstPos = GetLogicPosition(count);
			seq.Append(element.transform.DOMove(dstPos, popTime));
			seq.Join(element.transform.DOScale(element.handicapScale, popTime));
			seq.Join(element.transform.DORotate(new Vector3(0, 0, ownership * 180), popTime)
				.OnComplete(() =>
				{
					element.inspectLock = false;
					UpdateHandicapPosition();
				}));
		}
		seq.Play();
	}


	public void UpdateHandicapPosition()
	{
		UpdateElements();

		for (int i = 0; i < handiCards.Count; i++)
		{
			float updateTime = 0.2f;
			//handiCards[i].inspectLock = true;
			Vector3 oriPos = handiCards[i].transform.position;
			Vector3 dstPos = handiCards[i].handicapLogicPosition;
			//TODO config
			int temp = i;
			handiCards[i].transform.DOMove(dstPos, updateTime)
				.OnComplete(() => handiCards[temp].inspectLock = false);
			
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
		draggingElement.inspectLock = true;
		draggingElement.transform.DOScale(draggingElement.handicapScale, returnTime).OnComplete(() => draggingElement.inspectLock = false);
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
}

