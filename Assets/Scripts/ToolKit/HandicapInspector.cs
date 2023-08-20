using DataCore.BattleElements;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Transactions;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandicapInspector : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler
{
	public BattleElementController controller;

	public float inspectFactor;
	public Vector3 offset;
	public float duration;
	public Canvas canvas;
	public int upperOrder;
	public int lowerOrder
	{
		get => controller.handicapOrder;
	}

	private Vector3 originScale;
	private Vector3 inspectScale;
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (BattleElementController.targetSelectionLock) return;
		if (BattleElementController.draggingLock) return;
		if (controller.inspectLock) return;
		if (controller.ownership != 0) return;
		if (controller.dataState != ElementState.inHandicap) return;

		canvas.sortingOrder = upperOrder;
		transform.DOMove(controller.handicapLogicPosition + offset, duration);
		transform.DOScale(inspectScale, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (BattleElementController.targetSelectionLock) return;
		if (BattleElementController.draggingLock) return;
		if (controller.inspectLock) return;
		if (controller.ownership != 0) return;
		if (controller.dataState != ElementState.inHandicap) return;

		canvas.sortingOrder = lowerOrder;
		transform.DOMove(controller.handicapLogicPosition, duration);
		transform.DOScale(originScale, duration);
	}
	public void Init(Vector3 originScale, BattleElementController controller)
	{
		this.controller = controller;

		this.originScale = originScale;
		inspectScale = originScale * inspectFactor;
	}
}
