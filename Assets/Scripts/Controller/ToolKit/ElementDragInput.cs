using DataCore.BattleElements;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ElementDragInput : MonoBehaviour,
		IDragHandler,
	    IBeginDragHandler, IEndDragHandler
{
	public BattleElementController controller;
	public InspectPanelController inspector;
	public Transform buffer;

	public Canvas canvas;
	public int upperOrder;
	public int lowerOrder;

	public float duration;

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (BattleElementController.globalAnimeLock) return;
		if (BattleSceneManager.Turn != 0) return;
		if (controller.animeLock) return;
		if (controller.inputLock) return;
		if (controller.ownership != 0) return;

		if (controller.dataState == ElementState.inHandicap)
		{
			if(inspector != null) { inspector.active = false; inspector.DisablePanel(); }
			BattleElementController.globalAnimeLock = true;
		}
		if (controller.dataState == ElementState.inBattleLine)
		{
			UnitElementController unit = controller as UnitElementController;
			if (unit.operateCounter <= 0) return;
			if (unit.category == "Construction") return;

			if(inspector != null) { inspector.active = false; }
			BattleElementController.globalAnimeLock = true;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (BattleSceneManager.Turn != 0) return;
		if (controller.ownership != 0) return;
		if (BattleElementController.globalAnimeLock)
		{
			if(inspector != null) { inspector.active = false; inspector.DisablePanel(); }
			transform.SetParent(buffer);
			transform.DOScale(controller.handicapScale, duration);
			transform.position = eventData.position - BattleElementController.inputOffset;
			canvas.sortingOrder = upperOrder;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (BattleSceneManager.Turn != 0) return;
		if (controller.animeLock) return;
		if (controller.inputLock) return;
		if (controller.ownership != 0) return;

		if (inspector != null) { inspector.active = controller.dataState == ElementState.inBattleLine; }
		BattleElementController.globalAnimeLock = false;

		if (controller.dataState == ElementState.inHandicap)
		{
			if(controller is UnitElementController)
			{
				if (controller.battleSceneManager.PlayerDeploy(eventData.position, controller.handicapIdx) >= 0)
				{
					return;
				}
			}
			else if(controller is CommandElementController)
			{
				if (controller.battleSceneManager.PlayerCast(eventData.position, controller.handicapIdx) >= 0)
				{
					return;
				}
			}
			canvas.sortingOrder = controller.handicapOrder;
			controller.handicap.Insert(controller);
		}

		if (controller.dataState == ElementState.inBattleLine)
		{
			UnitElementController unit = controller as UnitElementController;
			if (unit.operateCounter <= 0) return;
			if (unit.category == "Construction") return;

			if (controller.battleSceneManager.PlayerRetreat(eventData.position, unit.battleLine, unit) >= 0)
			{
				return;
			}
			if (controller.battleSceneManager.PlayerMove(eventData.position, unit.battleLine, unit) >= 0)
			{
				return;
			}
			canvas.sortingOrder = unit.battleOrder;
			unit.battleLine.Insert(unit);
		}
	}

	public void Init(BattleElementController controller)
	{
		this.controller = controller;
	}
	public void Start()
	{
		buffer = GameObject.Find("Buffer").transform;
	}
}
