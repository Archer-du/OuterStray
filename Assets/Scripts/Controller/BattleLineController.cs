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
	/// <summary>
	/// 战线容量
	/// </summary>
	public int capacity;

	public List<UnitElementController> elementList;
	public int count { get => elementList.Count; }


	public GameObject background;
	public Image image;

	public int lineIdx;
	public int ownerShip;

	public float width;


	public static float updateTime = 0.2f;

	public int childNum;

	internal UnitElementController this[int index]
	{
		get => elementList[index];
		set => elementList[index] = value;
	}

	public static bool updating;

	public static float fieldLowerBound = 220f;
	public static float fieldUpperBound = 1970f;
	public static float lineInterval = 66f;
	public static float lineWidth = 400f;
	/// <summary>
	/// 数据层准备好时立刻调用Init
	/// </summary>
	/// <param name="capacity"></param>
	/// <param name="ownership"></param>
	public void Init(int capacity, int ownership)
	{
		updating = false;

		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();
		elementList = new List<UnitElementController>();

		this.capacity = capacity;
		this.ownerShip = ownership;

		width = (capacity / 6f) * 2700; //TODO config

		RectTransform size = background.GetComponent<RectTransform>();
		size.sizeDelta = new Vector2(width, size.sizeDelta.y);


		image = background.GetComponent<Image>();
		image.color = ownership == 0 ? Color.blue : Color.red;

		childNum = transform.childCount;
	}
	/// <summary>
	/// 根据归属权更新战线显示方式
	/// </summary>
	/// <param name="curlength"></param>
	/// <param name="ownerShip"></param>
	public void UpdateInfo(int curlength, int ownerShip)
	{
		if (curlength != count)
		{ throw new System.Exception("Data inconsistencies"); }

		UpdateElements();
		this.ownerShip = ownerShip;

		if (ownerShip == 0)
		{
			image.DOColor(Color.blue, 0.5f);
		}
		else
		{
			image.DOColor(Color.red, 0.5f);
		}
	}



	/// <summary>
	/// 根据指针横向坐标判断部署位置，限制输入
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	public int GetDeployPos(float position)
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
			int offset = vtcPos > 0 ? (int)(vtcPos / (BattleElementController.cardWidth + interval)) 
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
			int offset = vtcPos + (BattleElementController.cardWidth + interval) / 2 > 0 ? (int)((vtcPos - (BattleElementController.cardWidth + interval) / 2) / (BattleElementController.cardWidth + interval))
				: (int)((vtcPos - 3 * (BattleElementController.cardWidth + interval) / 2) / (BattleElementController.cardWidth + interval));
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
		//updating = true;
		UpdateElements();

		for(int i = 0; i < elementList.Count; i++)
		{
			Vector3 oriPos = elementList[i].transform.position;
			Vector3 dstPos = elementList[i].logicPosition;

			elementList[i].transform.DOMove(dstPos, updateTime);
				//.OnComplete(() => elementList[i].transform.localScale = elementList[i].battleFieldScale);
		}
	}
	private void UpdateElements()
	{
		for (int i = 0; i < elementList.Count; i++)
		{
			elementList[i].resIdx = i;
			elementList[i].battleLine = this;
			elementList[i].transform.SetSiblingIndex(i + childNum);
			elementList[i].logicPosition = GetLogicPosition(i);
			elementList[i].canvas.sortingOrder = i - 100;
		}
	}


	public void UpdateElementLogicPosition(List<IUnitElementController> list)
	{
		for(int i = 0; i < list.Count; i++)
		{
			UnitElementController unit = list[i] as UnitElementController;
			unit.logicPosition = GetLogicPosition(i, list.Count);
		}
	}





	public float returnTime = 0.2f;
	public UnitElementController draggingElement;
	public void Insert(UnitElementController element)
	{
		updating = true;
		draggingElement = element;
		draggingElement.transform.DOScale(draggingElement.battleFieldScale, updateTime).OnComplete(() => updating = false);
		element.transform.DOMove(GetLogicPosition(element.resIdx), returnTime).OnComplete(ResetHierachy);
	}
	private void ResetHierachy()
	{
		draggingElement.transform.SetParent(transform);
		draggingElement.transform.SetSiblingIndex(draggingElement.resIdx + childNum);
	}

	//TODO
	public Vector3 GetLogicPosition(int pos)
	{
		return new Vector3(0, -700 + lineIdx * 466, 0) + new Vector3((pos - count / 2) * BattleElementController.cardWidth + BattleElementController.cardWidth / 2 * ((count + 1) % 2), 0, 0);
	}
	public Vector3 GetLogicPosition(int pos, int count)
	{
		return new Vector3(0, -700 + lineIdx * 466, 0) + new Vector3((pos - count / 2) * BattleElementController.cardWidth + BattleElementController.cardWidth / 2 * ((count + 1) % 2), 0, 0);
	}
}
