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

public class CommandElementController : BattleElementController,
	ICommandElementController
{
	public TMP_Text durabilityText;

	public CanvasGroup selfCanvas;

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




	public void CastAnimationEvent(string method)
	{
		float castTime = 0.4f;
		float waitTime = 0.8f;
		

		Sequence seq = DOTween.Sequence();

		Debug.Log(nameContent + " casted ");

		if (method == "append")
		{
			//飞向战场侧中
			seq.Append(transform.DOMove(inputOffset / 2 + ownership * 500 * Vector2.down, castTime));
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
		else
		{
		}
	}

	public void UpdateState(ElementState state)
	{
		dataState = state;
	}
}
