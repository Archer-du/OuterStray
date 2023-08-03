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

	public Transform stack;


	public TMP_Text durabilityText;
	public TMP_Text costText;

	public int durability;

	public void Init(string ID, int ownership, string name, string description)
	{
		this.ownership = ownership;
		this.nameContent = name;
		this.category = "Command";
		this.description = description;

		LoadCardResources(ID);

		preprocessed = ownership;

		originScale = transform.localScale;
		enlargeScale = 1.35f * originScale;

		originTextScale = nameText.transform.localScale;
		targetTextScale = nameText.transform.localScale * 1.5f;


		descriptionPanel.color = Color.clear;
		descriptionText.gameObject.SetActive(false);
	}

	public void UpdateInfo(int cost, int durability)
	{
		this.cost = cost;
		this.durability = durability;

		nameText.text = nameContent;
		costText.text = cost.ToString();
		durabilityText.text = durability.ToString();
		descriptionText.text = description;
	}


	/// <summary>
	/// 读取卡面图像音频等资源
	/// </summary>
	private void LoadCardResources(string ID)
	{
		CardImage.sprite = Resources.Load<Sprite>("CardImage/" + ID);
		CardImage.rectTransform.sizeDelta = new Vector2(10, 13);
		if (ownership == 1)
		{
			//TODO
		}
	}


	public void RetreatAnimationEvent(string method)
	{
		float retreatTime = 0.4f;

		if (method == "append")
		{
			battleSceneManager.rotateSequence.Append(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, retreatTime)
				);
			battleSceneManager.sequenceTime += retreatTime;
		}
		else
		{
			battleSceneManager.rotateSequence.Append(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, retreatTime)
				);
			battleSceneManager.sequenceTime += retreatTime;
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

			foreach(GameObject obj in eventData.hovered)
			{
				if(obj.GetComponent<UnitElementController>() != null)
				{
					if (battleSceneManager.PlayerCast(eventData.position, this.handicapIdx) >= 0)
					{
						return;
					}
					else break;
				}
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
