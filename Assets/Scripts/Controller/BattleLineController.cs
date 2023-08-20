using DisplayInterface;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using DataCore.BattleElements;

public class BattleLineController : MonoBehaviour,
    IBattleLineController
{
	public BattleSceneManager battleSceneManager;

	public static float updateTime = 0.2f;

	public static float fieldLowerBound = 220f;
	public static float fieldUpperBound = 1970f;
	public static float lineInterval = 66f;
	public static float lineWidth = 400f;


	public List<UnitElementController> elementList;
	public int count { get => elementList.Count; }

	[Header("Data")]
	/// <summary>
	/// 战线容量
	/// </summary>
	public int capacity;
	public int index;
	public int ownership;

	public int childNum;
	internal UnitElementController this[int index]
	{
		get => elementList[index];
		set => elementList[index] = value;
	}

	[Header("Components")]
	public BattleLineDisplay lineDisplay;


	public void Init(int capacity, int ownership)
	{
		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();

		elementList = new List<UnitElementController>();

		this.capacity = capacity;
		this.ownership = ownership;

		childNum = transform.childCount;

		lineDisplay.UpdateInfo();
	}
	/// <summary>
	/// 根据归属权更新战线显示方式
	/// </summary>
	/// <param name="curlength"></param>
	/// <param name="ownerShip"></param>
	public void UpdateInfo(int curlength, int ownerShip)
	{
		if (curlength != count)
		{ throw new Exception("Data inconsistencies"); }

		UpdateElements();

		this.ownership = ownerShip;

		lineDisplay.UpdateInfo();
	}



	/// <summary>
	/// 根据指针横向坐标判断部署位置，限制输入
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	public int GetOperatePos(float position)
	{
		float inputOffsetX = 1980f;
		float interval = 20f;


		if (count >= capacity)
		{
			return -1;
		}
		float vtcPos = position - inputOffsetX;
		int pos;
		//CRITICAL ALGORITHM
		if (count % 2 == 0)
		{
			int start = count / 2;
			//一半卡牌 + 一半间隔
			int offset = vtcPos > 0 ? (int)((vtcPos + (BattleElementController.cardWidth + interval) / 2) / (BattleElementController.cardWidth + interval))
				: (int)((vtcPos - (BattleElementController.cardWidth + interval) / 2) / (BattleElementController.cardWidth + interval));
			pos = start + offset;
			if (start + offset < 0)
			{
				pos = 0;
			}
			else if (start + offset > count)
			{
				pos = count;
			}
		}
		else
		{
			int offset = (int)(vtcPos / (BattleElementController.cardWidth + interval));
			int start = vtcPos > 0 ? (count / 2 + 1) : (count / 2);
			pos = start + offset;
			if (start + offset < 0)
			{
				pos = 0;
			}
			else if (start + offset > count)
			{
				pos = count;
			}
		}

		return pos;
	}
	public int GetCastPos(float position)
	{
		float inputOffsetX = 1980f;
		float interval = 20f;

		float vtcPos = position - inputOffsetX;
		int pos;

		if(count % 2 == 0)
		{
			int start = count / 2;
			int offset = vtcPos > 0 
				? (int)(vtcPos / (BattleElementController.cardWidth + interval)) 
				: (int)((vtcPos - BattleElementController.cardWidth - interval) / (BattleElementController.cardWidth + interval));
			pos = start + offset;
			if(pos < 0 || pos > count - 1)
			{
				return -1;
			}
		}
		else
		{
			int start = count / 2;
			int offset = vtcPos + (BattleElementController.cardWidth + interval) / 2 > 0 
				? (int)((vtcPos + (BattleElementController.cardWidth + interval) / 2) / (BattleElementController.cardWidth + interval))
				: (int)((vtcPos - (BattleElementController.cardWidth + interval) / 2) / (BattleElementController.cardWidth + interval));
			pos = start + offset;
			if (pos < 0 || pos > count - 1)
			{
				return -1;
			}
		}
		return pos;
	}





	/// <summary>
	/// 接受Unit到战线
	/// </summary>
	/// <param name="controller"></param>
	/// <param name="dstPos"></param>
	public void Receive(IUnitElementController controller, int dstPos)
	{
		UnitElementController element = controller as UnitElementController;

		element.transform.SetParent(transform);
		elementList.Insert(dstPos, element);

		UpdateElementPosition();
	}
	/// <summary>
	/// 成功移除卡牌时调用
	/// </summary>
	/// <param name="idx"></param>
	/// <returns></returns>
	public IUnitElementController Send(int idx)
	{
		IUnitElementController controller = elementList[idx];

		elementList.RemoveAt(idx);

		UpdateElementPosition();

		return controller;
	}
	/// <summary>
	/// 死亡时调用
	/// </summary>
	/// <param name="idx"></param>
	public void ElementRemove(int idx)
	{
		elementList.RemoveAt(idx);

		UpdateElements();
	}


	
	/// <summary>
	/// 动画效果
	/// </summary>
	public void UpdateElementPosition()
	{
		UpdateElements();

		for(int i = 0; i < elementList.Count; i++)
		{
			elementList[i].inspectLock = true;

			Vector3 oriPos = elementList[i].transform.position;
			Vector3 dstPos = elementList[i].battleLineLogicPosition;

			int temp = i;
			elementList[i].transform.DOMove(dstPos, updateTime)
				.OnComplete(() => elementList[temp].inspectLock = false);
		}
	}

	private void UpdateElements()
	{
		for (int i = 0; i < elementList.Count; i++)
		{
			elementList[i].resIdx = i;
			elementList[i].battleLine = this;
			elementList[i].transform.SetSiblingIndex(i + childNum);
			elementList[i].battleLineLogicPosition = GetLogicPosition(i);
			elementList[i].battleOrder = i - 100;
		}
	}



	public void UpdateElementLogicPosition(List<IUnitElementController> list)
	{
		for(int i = 0; i < list.Count; i++)
		{
			UnitElementController unit = list[i] as UnitElementController;
			unit.battleLineLogicPosition = GetLogicPosition(i, list.Count);
		}
	}



	/// <summary>
	/// 显示层位置显示
	/// </summary>
	/// <param name="pos"></param>
	public void PreUpdateElementPosition(int pos)
	{
		for (int i = 0; i < elementList.Count; i++)
		{
			elementList[i].inspectLock = true;
			Vector3 dstPos;
			if (i < pos)
			{
				dstPos = GetLogicPosition(i, count + 1);
			}
			else
			{
				dstPos = GetLogicPosition(i + 1, count + 1);
			}
			int temp = i;
			elementList[i].transform.DOMove(dstPos, updateTime)
				.OnComplete(() => elementList[temp].inspectLock = false);
		}
	}




	public float returnTime = 0.2f;
	public UnitElementController draggingElement;
	public void Insert(UnitElementController element)
	{
		//updating = true;
		draggingElement = element;
		draggingElement.inspectLock = true;
		draggingElement.transform.DOScale(draggingElement.battleFieldScale, returnTime).OnComplete(() => draggingElement.inspectLock = false);
		element.transform.DOMove(element.battleLineLogicPosition, returnTime).OnComplete(ResetHierachy);
	}
	private void ResetHierachy()
	{
		draggingElement.transform.SetParent(transform);
		draggingElement.transform.SetSiblingIndex(draggingElement.resIdx + childNum);
	}

	//TODO config
	public Vector3 GetLogicPosition(int pos)
	{
		return new Vector3(0, -700 + index * (lineInterval + lineWidth), 0) + new Vector3((pos - count / 2) * BattleElementController.cardWidth + BattleElementController.cardWidth / 2 * ((count + 1) % 2), 0, 0);
	}
	public Vector3 GetLogicPosition(int pos, int count)
	{
		return new Vector3(0, -700 + index * (lineInterval + lineWidth), 0) + new Vector3((pos - count / 2) * BattleElementController.cardWidth + BattleElementController.cardWidth / 2 * ((count + 1) % 2), 0, 0);
	}
}
