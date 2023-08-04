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


public class UnitElementController : BattleElementController,
	IUnitElementController
{
	public IUnitInput input;


	public CanvasGroup InspectPanel;
	public GameObject Inspector;
	public Image InspectorImage;

	public Image InspectorGround;
	public Image InspectorShell;
	public Image InspectorFrame;
	public Image InspectorNameTag;
	public Image InspectorCostTag;

	public TMP_Text InspectorName;
	public TMP_Text InspectorCost;
	public TMP_Text InspectorAttack;
	public TMP_Text InspectorMaxHealth;
	public TMP_Text InspectorAttackCounter;
	public TMP_Text InspectorDescription;




	public TMP_Text attackText;
	public TMP_Text healthText;
	public TMP_Text attackCounterText;
	public TMP_Text descriptionText;

	public Image operateMask;


	public GameObject leftArrow;
	public GameObject rightArrow;
	public GameObject midArrow;

	public Vector3 arrowScale;
	public Vector3 enlargeArrowScale;
	/// <summary>
	/// 
	/// </summary>
	public BattleLineController battleLine;
	public int resIdx;
	public UnitElementController target;


	public int attackPoint;
	public int healthPoint;
	public int maxHealthPoint;
	public int attackCounter;
	public int operateCounter;
	//legacy
	public int moveRange;

	public Vector3 logicPosition;

	public float duration = 0.2f;

	public int attackOrder = 0;
	public int oriOrder = -100;

	/// <summary>
	/// 从牌堆加入手牌或战场时初始化
	/// </summary>
	/// <param name="ownership"></param>
	public void UnitInit(string ID, int ownership, string name, string categories, string description, IUnitInput input)
	{
		Init(ID, ownership, name, categories, description);



		arrowScale = leftArrow.transform.localScale;
		enlargeArrowScale = arrowScale * 1.5f;

		leftArrow.SetActive(false);
		rightArrow.SetActive(false);
		midArrow.SetActive(false);

		InspectPanel.alpha = 0f;
	}



	public void UpdateInfo(int cost, int attackPoint, int healthPoint, int maxHealthPoint, int attackCounter, int operateCounter,
		ElementState state, int moveRange, bool aura)
	{
		this.cost = cost;
		this.attackPoint = attackPoint;
		this.healthPoint = healthPoint;
		this.maxHealthPoint = maxHealthPoint;
		this.attackCounter = attackCounter;
		this.operateCounter = operateCounter;
		this.dataState = state;
		this.moveRange = moveRange;

		nameText.text = nameContent;
		attackText.text = attackPoint.ToString();
		healthText.text = healthPoint.ToString();
		attackCounterText.text = this.category == "Construction" ? "" : attackCounter.ToString();
		costText.text = cost.ToString();
		descriptionText.text = description;

		if (operateCounter == 0)
		{
			operateMask.DOColor(new Color(0, 0, 0, 0.5f), duration);
		}
		else
		{
			operateMask.DOColor(new Color(0, 0, 0, 0), duration);
		}
		if (state == ElementState.inBattleLine)
		{
			costTag.gameObject.SetActive(false);
			costText.gameObject.SetActive(false);
		}
		//DOTween.To(
		//	() => "", // getter返回空字符串
		//	value => nameText.text = value, // setter设置costText的内容
		//	nameTag, // endValue是原始内容
		//	0.2f
		//).SetEase(Ease.Linear); // 设置动画为线性变化
	}
	public void UpdateTarget(IUnitElementController t1, IUnitElementController t2, IUnitElementController t3, IUnitElementController target, int targetIdx)
	{
		this.target = target as UnitElementController;

		switch (targetIdx)
		{
			case 0:
				leftArrow.transform.DOScale(enlargeArrowScale, 0.2f);
				midArrow.transform.DOScale(arrowScale, 0.2f);
				rightArrow.transform.DOScale(arrowScale, 0.2f);
				break;
			case 1:
				leftArrow.transform.DOScale(arrowScale, 0.2f);
				midArrow.transform.DOScale(enlargeArrowScale, 0.2f);
				rightArrow.transform.DOScale(arrowScale, 0.2f);
				break;
			case 2:
				leftArrow.transform.DOScale(arrowScale, 0.2f);
				midArrow.transform.DOScale(arrowScale, 0.2f);
				rightArrow.transform.DOScale(enlargeArrowScale, 0.2f);
				break;
			default:
				leftArrow.transform.DOScale(arrowScale, 0.2f);
				midArrow.transform.DOScale(arrowScale, 0.2f);
				rightArrow.transform.DOScale(arrowScale, 0.2f);
				break;
		}
		leftArrow.SetActive(targetIdx == 0);
		midArrow.SetActive(targetIdx == 1);
		rightArrow.SetActive(targetIdx == 2);
	}










	/// <summary>
	/// 攻击动画加入结算队列
	/// </summary>
	/// <param name="target"></param>
	public void AttackAnimationEvent(int resIdx, int count)
	{
		if (target == null) return;

		float forwardTime = 0.2f;
		float backlashTime = 0.2f;
		Vector3 oriPosition = logicPosition;
		Vector3 dstPosition = target.logicPosition;

		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + " attacked " + "line: " + target.battleLine.lineIdx + "res: " + target.resIdx);

		//安全间隔
		battleSceneManager.rotateSequence.AppendInterval(BattleLineController.updateTime + 0.2f);
		battleSceneManager.sequenceTime += BattleLineController.updateTime + 0.2f;
		//TODO time config
		//层级设置
		battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
			() =>
			{
				canvas.sortingOrder = attackOrder;
			});
		//动画设置
		battleSceneManager.rotateSequence.Append(
			transform.DOMove(dstPosition, forwardTime).OnComplete(() =>
			{
				transform.DOMove(oriPosition, backlashTime).OnComplete(() => battleLine.UpdateElementPosition());
				input.UpdateManual();
			})
		);
		battleSceneManager.sequenceTime += forwardTime;
	}
	/// <summary>
	/// 随机攻击动画加入结算队列
	/// </summary>
	/// <param name="target"></param>
	public void RandomAttackAnimationEvent(IUnitElementController target)
	{
		if (target == null) return;

		float forwardTime = 0.2f;
		float backlashTime = 0.2f;
		UnitElementController controller = target as UnitElementController;
		Vector3 oriPosition = logicPosition;

		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + " random attacked " + "line: " + controller.battleLine.lineIdx + "res: " + controller.resIdx);

		battleSceneManager.rotateSequence.AppendInterval(BattleLineController.updateTime + 0.2f);
		battleSceneManager.sequenceTime += BattleLineController.updateTime + 0.2f;

		battleSceneManager.rotateSequence.Append(
			transform.DOMove(logicPosition + 100f * Vector3.up * (2 * ownership - 1), forwardTime).OnComplete(() =>
			{
				transform.DOMove(oriPosition, backlashTime);
				input.UpdateManual();
			})
		);
		battleSceneManager.sequenceTime += forwardTime;
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="health"></param>
	public void DamageAnimationEvent(int health, string method)
	{
		float forwardTime = 0.2f;

		if(method == "append")
		{
			battleSceneManager.rotateSequence.Append(
				transform.DOShakeRotation(forwardTime, 20f)
				);
			battleSceneManager.sequenceTime += forwardTime;
		}
		else
		{
			battleSceneManager.rotateSequence.Join(
				transform.DOShakeRotation(forwardTime, 20f)
				);
		}
		//放大
		battleSceneManager.rotateSequence.Join(
			healthText.transform.DOScale(targetTextScale, forwardTime / 2f)
				.OnComplete(() =>
				{
					healthText.text = health.ToString();
					healthText.transform.DOScale(originTextScale, forwardTime / 2f);
				})
			);
		//变色
		battleSceneManager.rotateSequence.Join(
			healthText.DOColor(Color.red, forwardTime / 2f)
				.OnComplete(() =>
				{
					healthText.DOColor(Color.white, forwardTime / 2f);
				})
			);
	}
	public void RecoverAnimationEvent(int health, string method)
	{
		float forwardTime = 0.2f;

		if (method == "append")
		{
			battleSceneManager.rotateSequence.Append(
				transform.DOShakeRotation(forwardTime, 20f)
				);
			battleSceneManager.sequenceTime += forwardTime;
		}
		else
		{
			battleSceneManager.rotateSequence.Join(
				transform.DOShakeRotation(forwardTime, 20f)
				);
		}
		//放大
		battleSceneManager.rotateSequence.Join(
			healthText.transform.DOScale(targetTextScale, forwardTime / 2f)
				.OnComplete(() =>
				{
					healthText.text = health.ToString();
					healthText.transform.DOScale(originTextScale, forwardTime / 2f);
				})
			);
		//变色
		battleSceneManager.rotateSequence.Join(
			healthText.DOColor(Color.green, forwardTime / 2f)
				.OnComplete(() =>
				{
					healthText.DOColor(Color.white, forwardTime / 2f);
				})
			);
	}
	/// <summary>
	/// 
	/// </summary>
	public void TerminateAnimationEvent(string method)
	{
		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + " destroyed!");


		battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
				() =>
				{
					battleLine.ElementRemove(resIdx);
					gameObject.SetActive(false);
					input.UpdateManual();
				}
			);
	}

	public void CleaveAttackAnimationEvent(int resIdx, int count)
	{
		if (target == null) return;

		float forwardTime = 0.2f;
		float backlashTime = 0.2f;
		Vector3 oriPosition = logicPosition;
		Vector3 dstPosition = target.logicPosition;

		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + " attacked " + "line: " + target.battleLine.lineIdx + "res: " + target.resIdx);

		//安全间隔
		battleSceneManager.rotateSequence.AppendInterval(BattleLineController.updateTime + 0.2f);
		battleSceneManager.sequenceTime += BattleLineController.updateTime + 0.2f;
		//TODO time config
		//层级设置
		battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
			() =>
			{
				canvas.sortingOrder = attackOrder;
			});
		//动画设置
		battleSceneManager.rotateSequence.Append(
			transform.DOMove(dstPosition, forwardTime).OnComplete(() =>
			{
				transform.DOMove(oriPosition, backlashTime).OnComplete(() => battleLine.UpdateElementPosition());
				input.UpdateManual();
			})
		);
		battleSceneManager.sequenceTime += forwardTime;
	}
	public void RetreatAnimationEvent(string method)
	{
		float retreatTime = 0.4f;

		if(method == "append")
		{
			Vector3 rotateBy = new Vector3(0, 0, ((ownership * 2) - 1) * 90);
			battleSceneManager.rotateSequence.Append(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, retreatTime)
				);
			battleSceneManager.rotateSequence.Join(
				transform.DOBlendableRotateBy(rotateBy, retreatTime)
				);
			battleSceneManager.sequenceTime += retreatTime;
		}
		else
		{
			Vector3 rotateBy = new Vector3(0, 0, ((ownership * 2) - 1) * 90);
			battleSceneManager.rotateSequence.Append(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, retreatTime)
				);
			battleSceneManager.rotateSequence.Join(
				transform.DOBlendableRotateBy(rotateBy, retreatTime)
				);
			battleSceneManager.sequenceTime += retreatTime;
		}
	}






















	private float timer = 0;
	void Update()
	{
		// 如果鼠标悬停在元素上
		if (timer > 0)
		{
			// 计时器递减
			timer -= Time.deltaTime;
			// 如果计时器小于等于 0
			if (timer <= 0)
			{
				// 播放动画
				//TODO 检视
				canvas.sortingOrder = attackOrder;
			}
		}
	}
	public override void OnDrag(PointerEventData eventData)
	{
		base.OnDrag(eventData);
		timer = -1;
		InspectPanel.alpha = 0;
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		base.OnBeginDrag(eventData);
		//在战线：移动
		if (dataState == ElementState.inBattleLine)
		{
			if (operateCounter <= 0)
			{
				return;
			}
			if (category == "Construction")
			{
				return;
			}
			timer = -1;
			HandicapController.isDragging = true;
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		base.OnEndDrag(eventData);

		HandicapController.isDragging = false;
		//部署条件判定
		if (dataState == ElementState.inHandicap)
		{
			if (battleSceneManager.PlayerDeploy(eventData.position, this.handicapIdx) >= 0)
			{
				return;
			}
			handicap.Insert(this);
		}
		//移动撤退条件判定
		if (dataState == ElementState.inBattleLine)
		{
			if (operateCounter <= 0)
			{
				return;
			}
			if (category == "Construction")
			{
				return;
			}
			if (battleSceneManager.PlayerMove(eventData.position, this.battleLine, this) >= 0)
			{
				return;
			}
			if (this.battleLine.lineIdx == 0 && battleSceneManager.PlayerRetreat(eventData.position, this.battleLine, this) >= 0)
			{
				return;
			}
			battleLine.Insert(this);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);

		if (dataState == ElementState.inBattleLine)
		{
			timer = 0.8f;

			return;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);

		if (dataState == ElementState.inBattleLine)
		{
			timer = -1;
			//TODO 检视

			canvas.sortingOrder = oriOrder;
			return;
		}
	}











	//TODO
	public int GetBattleLineIdx(float y)
	{
		if (y > 220 && y < 1970)
		{
			return (int)((y - 180) / 466);
		}
		return -1;
	}
}
