using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DisplayInterface;
using TMPro;
using UnityEngine.EventSystems;
using DataCore.Cards;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Rendering;
using InputHandler;
using DataCore.BattleElements;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class CommandElementController : BattleElementController,
	ICommandElementController
{


	public TMP_Text durabilityText;

	public int durability;
	public string type;

	public void CommandInit(string ID, int ownership, string name, string type, string description)
	{
		Init(ID, ownership, name, "Command", description);

		this.type = type;
	}

	public void UpdateInfo(int cost, int durability, ElementState state)
	{
		this.cost = cost;
		this.durability = durability;
		this.dataState = state;

		nameText.text = nameContent;
		costText.text = cost.ToString();
		durabilityText.text = durability.ToString();
	}








	public void CastAnimationEvent(string method)
	{
		float castTime = 0.4f;
		float waitTime = 0.4f;

		if (method == "append")
		{
			battleSceneManager.rotateSequence.Append(transform.DOMove(new Vector3(1920 / 2, 1080 / 2, 0), castTime));
			battleSceneManager.rotateSequence.AppendInterval(waitTime);
			Vector3 rotateBy = new Vector3(0, 0, ((ownership * 2) - 1) * 90);
			battleSceneManager.rotateSequence.Append(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, castTime)
				);
			battleSceneManager.rotateSequence.Join(
				transform.DOBlendableRotateBy(rotateBy, castTime)
				);
			battleSceneManager.sequenceTime += castTime + waitTime;
		}
		else
		{
			battleSceneManager.rotateSequence.Append(transform.DOMove(new Vector3(1920 / 2, 1080 / 2, 0), castTime));
			battleSceneManager.rotateSequence.AppendInterval(waitTime);
			Vector3 rotateBy = new Vector3(0, 0, ((ownership * 2) - 1) * 90);
			battleSceneManager.rotateSequence.Append(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, castTime)
				);
			battleSceneManager.rotateSequence.Join(
				transform.DOBlendableRotateBy(rotateBy, castTime)
				);
			battleSceneManager.sequenceTime += castTime + waitTime;
		}
	}







	public override void OnBeginDrag(PointerEventData eventData)
	{
		base.OnBeginDrag(eventData);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		base.OnEndDrag(eventData);
		//cast条件判定
		if (dataState == ElementState.inHandicap)
		{
			HandicapController.isDragging = false; // 结束拖动

			if (battleSceneManager.PlayerCast(eventData.position, this.handicapIdx) >= 0)
			{
				return;
			}

			handicap.Insert(this);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
	}


}
