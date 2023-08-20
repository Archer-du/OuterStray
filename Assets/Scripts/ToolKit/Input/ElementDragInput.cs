using DataCore.BattleElements;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ElementDragInput : MonoBehaviour,
		IDragHandler,
	    IBeginDragHandler, IEndDragHandler
{
	public BattleElementController controller;
	public BattleSceneManager sceneManager
	{
		get => controller.battleSceneManager;
	}
	public InspectPanelController inspector;
	public Transform buffer;

	public Canvas canvas;
	public int upperOrder;
	public int lowerOrder;

	//public int upperPosition;
	//public int lowerPosition;

	public float duration;

	private bool innerLock;

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (BattleElementController.draggingLock) return;
		if (BattleElementController.targetSelectionLock) return;
		if (BattleSceneManager.Turn != 0) return;
		if (BattleSceneManager.inputLock) return;
		if (controller.inspectLock) return;
		if (controller.inputLock) return;
		if (controller.ownership != 0) return;

		//允许拖动
		//transform.position += upperPosition * new Vector3(0, 0, -1);

		if(inspector != null) { inspector.active = false; inspector.DisablePanel(); }
		if (controller.dataState == ElementState.inHandicap)
		{
			BattleElementController.draggingLock = true;
		}
		if (controller.dataState == ElementState.inBattleLine)
		{
			UnitElementController unit = controller as UnitElementController;
			if (unit.operateCounter <= 0) return;
			if (unit.category == "Construction") return;

			if(inspector != null) { inspector.active = false; }
			BattleElementController.draggingLock = true;
		}
	}


	public void OnDrag(PointerEventData eventData)
	{
		if (BattleElementController.targetSelectionLock) return;
		if (BattleSceneManager.Turn != 0) return;
		if (BattleSceneManager.inputLock) return;
		if (controller.inspectLock) return;
		if (controller.inputLock) return;
		if (controller.ownership != 0) return;

		//拖动中每帧更新
		if (BattleElementController.draggingLock)
		{
			if(inspector != null) { inspector.active = false; inspector.DisablePanel(); }

			transform.SetParent(buffer);
			transform.DOScale(controller.handicapScale, duration);
			transform.position = eventData.position - BattleElementController.inputOffset;
			canvas.sortingOrder = upperOrder;

			//输入预检测
			sceneManager.DisableAllSelectionFrame();
			int lineIdx = controller.GetBattleLineIdx(eventData.position.y);
			BattleLineController battleLine = lineIdx >= 0 && lineIdx <= sceneManager.fieldCapacity - 1 ? sceneManager.battleLines[lineIdx] : null;


			if (controller.category != "Command")
			{
				UnitElementController unit = controller as UnitElementController;
				foreach(BattleLineController line in sceneManager.battleLines)
				{
					if (line != battleLine && line != unit.battleLine)
					{
						line.UpdateElementPosition();
					}
				}

				if (battleLine == null) return;

				battleLine.lineDisplay.DisplaySelectionFrame();
				//费用预检测
				if (controller.dataState == ElementState.inHandicap)
				{
					if (battleLine.index != 0) return;
					if (sceneManager.energy[0] < controller.cost)
					{
						return;
					}
					int pos = battleLine.GetOperatePos(eventData.position.x);
					if (pos < 0) return;
					battleLine.PreUpdateElementPosition(pos);
				}
				if (controller.dataState == ElementState.inBattleLine)
				{
					if (battleLine.ownership != 0) return;
					if (battleLine == unit.battleLine) return;
					int pos = battleLine.GetOperatePos(eventData.position.x);
					if (pos < 0) return;
					battleLine.PreUpdateElementPosition(pos);
				}
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (BattleElementController.targetSelectionLock) return;
		if (BattleSceneManager.Turn != 0) return;
		if (BattleSceneManager.inputLock) return;
		if (controller.inspectLock) return;
		if (controller.inputLock) return;
		if (controller.ownership != 0) return;


		//手牌区输入判定
		if (controller.dataState == ElementState.inHandicap)
		{
			//允许落地
			if (inspector != null) { inspector.active = controller.dataState == ElementState.inBattleLine; }
			BattleElementController.draggingLock = false;
			sceneManager.DisableAllSelectionFrame();

			//单位卡判定
			if (controller is UnitElementController)
			{
				UnitElementController unit = controller as UnitElementController;
				//解析eventData/通用输入检测
				int lineIdx = controller.GetBattleLineIdx(eventData.position.y);
				BattleLineController battleLine = lineIdx >= 0 && lineIdx <= sceneManager.fieldCapacity - 1 ? sceneManager.battleLines[lineIdx] : null;
				if (battleLine == null)
				{
					canvas.sortingOrder = controller.handicapOrder;
					controller.handicap.Insert(controller);
					return;
				}
				int dstPos = battleLine.GetOperatePos(eventData.position.x);

				unit.PlayerDeploy(lineIdx, dstPos);
			}
			//指令卡判定
			else if(controller is CommandElementController)
			{
				CommandElementController comm = controller as CommandElementController;

				comm.PlayerCast();
			}
		}

		//战场区输入判定
		if (controller.dataState == ElementState.inBattleLine)
		{
			//二次检测锁
			UnitElementController unit = controller as UnitElementController;
			if (unit.operateCounter <= 0) return;
			if (unit.category == "Construction") return;

			//允许落地
			if (inspector != null) { inspector.active = controller.dataState == ElementState.inBattleLine; }
			BattleElementController.draggingLock = false;
			sceneManager.DisableAllSelectionFrame();

			//TODO
			//撤退判定
			if (unit.battleLine.index == 0 && eventData.position.x >= 3340 && eventData.position.y <= 1080)
			{
				unit.PlayerRetreat();
				return;
			}

			//解析eventData
			int lineIdx = controller.GetBattleLineIdx(eventData.position.y);
			BattleLineController battleLine = lineIdx >= 0 && lineIdx <= sceneManager.fieldCapacity - 1 ? sceneManager.battleLines[lineIdx] : null;
			if (battleLine == null)
			{
				canvas.sortingOrder = controller.handicapOrder;
				controller.handicap.Insert(controller);
				return;
			}
			int dstPos = battleLine.GetOperatePos(eventData.position.x);

			unit.PlayerMove(lineIdx, dstPos);
		}
	}


	public void Start()
	{
		buffer = GameObject.Find("Buffer").transform;
		innerLock = false;
	}
}
