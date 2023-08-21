using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DisplayInterface;
using TMPro;
using UnityEngine.EventSystems;
using DataCore.Cards;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Rendering;
using InputHandler;
using DataCore.BattleElements;
using LogicCore;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class CommandElementController : BattleElementController,
	ICommandElementController
{
	public TMP_Text durabilityText;

	public int durability;
	public string type;

	public void CommandInit(string ID, int ownership, string name, string type, int cost, string description)
	{
		Init(ID, ownership, name, "Command", cost, description);

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
	public void PlayerCast()
	{
		//能量判定
		if (battleSceneManager.energy[0] < cost)
		{
			//TODO
			battleSceneManager.humanEnergy.transform.DOShakePosition(0.3f, 30f);
			canvas.sortingOrder = handicapOrder;
			handicap.Insert(this);
			return;
		}

		if(type == "NonTarget")
		{
			battleSceneManager.PlayerNonTargetCast(handicapIdx);
		}
		else
		{
			handicap.Pop(handicapIdx);
			TargetCastAnimationEvent();
		}
	}






	public void NonTargetCastAnimationEvent(string method)
	{
		float castTime = 0.4f;
		float waitTime = 1f;

		Sequence seq = DOTween.Sequence();

		Debug.Log(nameContent + " casted ");

		if (method == "append")
		{
			//飞向战场侧中
			seq.Append(transform.DOMove(inputOffset / 2 + 500 * Vector2.down, castTime));
			seq.Join(transform.DOScale(castScale, castTime));
			seq.Join(transform.DORotate(new Vector3(0, 0, 0), castTime));
			//等待
			seq.AppendInterval(waitTime);

			if(durability == 0)
			{
				seq.Append(selfCanvas.DOFade(0, castTime));
				return;
			}

			Vector3 rotateBy = new Vector3(0, 0, - 90);

			//回归
			seq.Append(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, castTime)
				.OnComplete(() =>
				{
					this.gameObject.SetActive(false);
					transform.rotation = Quaternion.Euler(Vector3.zero);
					transform.localScale = handicapScale;
				})
			);
			seq.Join(
				transform.DOBlendableRotateBy(rotateBy, castTime)
				);
			seq.Join(transform.DOScale(handicapScale, castTime));
		}
	}
	public void TargetCastAnimationEvent()
	{
		float castTime = 0.4f;

		Sequence seq = DOTween.Sequence();

		Debug.Log(nameContent + " casted ");

		targetSelectionLock = true;
		battleSceneManager.castingCommand = this;

		seq.Append(transform.DOMove(inputOffset / 2 + 500 * Vector2.down, castTime));
		seq.Join(transform.DOScale(castScale, castTime));
		seq.Join(transform.DORotate(new Vector3(0, 0, 0), castTime));
	}
	public void TargetCastAnimationOver()
	{
		float castTime = 0.4f;

		Sequence seq = DOTween.Sequence();

		Vector3 rotateBy = new Vector3(0, 0, - 90);

		targetSelectionLock = false;
		battleSceneManager.castingCommand = null;

		//消耗
		if (durability == 0)
		{
			//TODO
			seq.Append(selfCanvas.DOFade(0, castTime));
			return;
		}
		//回归
		seq.Append(
			transform.DOMove(stack.transform.position + 500 * Vector3.left, castTime)
			.OnComplete(() =>
			{
				this.gameObject.SetActive(false);
				transform.rotation = Quaternion.Euler(Vector3.zero);
				transform.localScale = handicapScale;
			})
		);
		seq.Join(
			transform.DOBlendableRotateBy(rotateBy, castTime)
			);
		seq.Join(transform.DOScale(handicapScale, castTime));
	}






	public void UpdateState(ElementState state)
	{
		dataState = state;
	}

}
