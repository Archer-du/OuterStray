using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetInspector : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler
{
	public float duration;

	public BattleElementController controller;
	public void OnPointerEnter(PointerEventData eventData)
	{
		if(BattleElementController.targetSelectionLock)
		{
			if(controller.dataState == DataCore.BattleElements.ElementState.inBattleLine)
			{
				transform.DOScale(controller.targetScale, duration);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (BattleElementController.targetSelectionLock)
		{
			if (controller.dataState == DataCore.BattleElements.ElementState.inBattleLine)
			{
				transform.DOScale(controller.battleFieldScale, duration);
			}
		}
	}
}
