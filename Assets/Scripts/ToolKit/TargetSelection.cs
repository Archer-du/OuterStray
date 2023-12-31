using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetSelection : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler,
	IPointerClickHandler
{
	public float duration;

	public UnitElementController controller;

	public CommandElementController castingCommand
	{
		get => controller.battleSceneManager.castingCommand;
	}
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

	public void OnPointerClick(PointerEventData eventData)
	{
		if (BattleElementController.targetSelectionLock && controller.dataState == DataCore.BattleElements.ElementState.inBattleLine)
		{
			controller.battleSceneManager.PlayerTargetCast(castingCommand.handicapIdx, controller.battleLine.index, controller.resIdx);

			castingCommand.TargetCastAnimationOver();
		}
	}
}
