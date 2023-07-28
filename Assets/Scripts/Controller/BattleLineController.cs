using DisplayInterface;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Xml.Linq;

public class BattleLineController : MonoBehaviour,
    IBattleLineController
{
	//如果控件可以轻易获取到所需信息，就没必要使用数据层进行驱动

	public int capacity;
	//之后合并 现在可能出现不一致
	public int count { get => elementList.Count; }

	public List<UnitElementController> elementList;

	public GameObject background;
	public Image image;

	public float width;
	public float interval = 20f;




	public int LineIdx;

	public int ownerShip;





	internal UnitElementController this[int index]
	{
		get => elementList[index];
		set => elementList[index] = value;
	}

	public void Start()
	{
	}

	/// <summary>
	/// 数据层准备好时立刻调用Init
	/// </summary>
	/// <param name="capacity"></param>
	/// <param name="ownership"></param>
	public void Init(int capacity, int ownership)
	{
		elementList = new List<UnitElementController>();

		this.capacity = capacity;

		width = (capacity / 6f) * 2700; //TODO config

		RectTransform size = background.GetComponent<RectTransform>();
		size.sizeDelta = new Vector2(width, size.sizeDelta.y);

		this.ownerShip = ownership;

		image = background.GetComponent<Image>();
		image.color = ownership == 0 ? Color.blue : Color.red;
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
		element.transform.SetSiblingIndex(dstPos + 4);//TODO config child num
		elementList.Insert(dstPos, element);
		//update
		for (int i = 0; i < elementList.Count; i++)
		{
			elementList[i].resIdx = i;
			elementList[i].state = UnitState.inBattleLine;
			elementList[i].line = this;
			elementList[i].transform.SetSiblingIndex(i + 4);
		}

		UpdateDeployPosition();
	}



	/// <summary>
	/// 根据归属权更新战线显示方式
	/// </summary>
	/// <param name="curlength"></param>
	/// <param name="ownerShip"></param>
	public void InfoUpdate(int curlength, int ownerShip)
	{
		if(curlength != count) { throw new System.Exception("inaccurate"); }
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
		if(count >= capacity)
		{
			return -1;
		}
		float vtcPos = position - 1980f;
		int pos;
		//CRITICAL ALGORITHM
		if(count % 2 == 0)
		{
			int start = count / 2;
			//一半卡牌 + 一半间隔
			int offset = vtcPos > 0 ? (int)((vtcPos + 160) / 320) : (int)((vtcPos - 160) / 320);
			pos = start + offset;
			if (start + offset < 0)
			{
				pos = 0;
			}
			else if(start + offset > count)
			{
				pos = count;
			}
		}
		else
		{
			int offset = (int)(vtcPos / 320);
			int start = vtcPos > 0 ? (count / 2 + 1) : (count / 2);
			pos = start + offset;
			if(start + offset < 0)
			{
				pos = 0;
			}
			else if(start + offset > count)
			{
				pos = count;
			}
		}

		return pos;
	}
	//TODO
	public int GetVerticalMovePos(float position)
	{
		if (count >= capacity)
		{
			return -1;
		}
		float vtcPos = position - 1980f;
		int pos;
		//CRITICAL ALGORITHM
		if (count % 2 == 0)
		{
			int start = count / 2;
			//一半卡牌 + 一半间隔
			int offset = vtcPos > 0 ? (int)((vtcPos + 160) / 320) : (int)((vtcPos - 160) / 320);
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
			int offset = (int)(vtcPos / 320);
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



	/// <summary>
	/// 成功移除卡牌时调用
	/// </summary>
	/// <param name="idx"></param>
	/// <returns></returns>
	public IUnitElementController Send(int idx)
	{
		IUnitElementController controller = elementList[idx];
		elementList.RemoveAt(idx);
		//update
		for (int i = 0; i < elementList.Count; i++)
		{
			elementList[i].resIdx = i;
			elementList[i].state = UnitState.inBattleLine;
			elementList[i].line = this;
			elementList[i].transform.SetSiblingIndex(i + 4);
		}

		UpdateDeployPosition();

		return controller;
	}
	public float updateTime = 0.2f;



	public int cardWidth = 360;
	public void UpdateDeployPosition()
	{
		for (int i = 0; i < elementList.Count; i++)
		{
			elementList[i].resIdx = i;
			elementList[i].state = UnitState.inBattleLine;
			elementList[i].line = this;
			elementList[i].transform.SetSiblingIndex(i + 4);
		}
		if (elementList.Count % 2 == 0)
		{
			for (int i = 0; i < elementList.Count; i++)
			{
				Vector3 oriPos = elementList[i].transform.position;

				Vector3 dstPos = transform.position + new Vector3((i - count / 2) * cardWidth + cardWidth / 2, 0, 0);
				//TODO config

				if (elementList[i].preprocessed == 1)
				{
					elementList[i].preprocessed = 0;
					Vector3 moveBy = dstPos - oriPos;
					Vector3 rotateBy = new Vector3(0, 0, 180);

					Sequence seq = DOTween.Sequence();
					seq.Append(elementList[i].transform.DOBlendableMoveBy(moveBy, updateTime));
					seq.Join(elementList[i].transform.DOBlendableRotateBy(rotateBy, updateTime));
					seq.Play();
				}
				else
				{
					elementList[i].transform.DOMove(dstPos, updateTime);
				}

			}
		}
		else
		{
			for (int i = 0; i < elementList.Count; i++)
			{
				Vector3 oriPos = elementList[i].transform.position;

				Vector3 dstPos = transform.position + new Vector3((i - count / 2) * cardWidth, 0, 0);

				if (elementList[i].preprocessed == 1)
				{
					elementList[i].preprocessed = 0;
					Vector3 moveBy = dstPos - oriPos;
					Vector3 rotateBy = new Vector3(0, 0, 180);

					Sequence seq = DOTween.Sequence();
					seq.Append(elementList[i].transform.DOBlendableMoveBy(moveBy, updateTime));
					seq.Join(elementList[i].transform.DOBlendableRotateBy(rotateBy, updateTime));
					seq.Play();
				}
				else
				{
					elementList[i].transform.DOMove(dstPos, updateTime);
				}
			}
		}
	}


	public void ElementDestroyed(int idx)
	{
		elementList.RemoveAt(idx);
		for (int i = 0; i < elementList.Count; i++)
		{
			elementList[i].resIdx = i;
			elementList[i].state = UnitState.inBattleLine;
			elementList[i].line = this;
			elementList[i].transform.SetSiblingIndex(i + 4);
		}
		UpdateDeployPosition();
	}



	public float returnTime = 0.2f;
	public UnitElementController draggingElement;
	public void Insert(UnitElementController element)
	{
		draggingElement = element;
		element.transform.DOMove(GetInsertionPosition(element.resIdx), returnTime).OnComplete(ResetHierachy);
	}
	private void ResetHierachy()
	{
		draggingElement.transform.SetParent(transform);
		draggingElement.transform.SetSiblingIndex(draggingElement.resIdx + 4);//TODO 4
	}
	//可以和UpdateDeploy组出复用块
	private Vector3 GetInsertionPosition(int index)
	{
		return transform.position + new Vector3((index - count / 2) * cardWidth + cardWidth / 2 * ((count + 1) % 2), 0, 0);
	}
}
