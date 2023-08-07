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
using System;


public class UnitElementController : BattleElementController,
	IUnitElementController
{
	public IUnitInput input;


	public TMP_Text attackText;
	public TMP_Text healthText;
	public TMP_Text attackCounterText;
	public TMP_Text descriptionText;

	public Image operateMask;

	public GameObject Arrows;
	public CanvasGroup arrowsGroup;
	public Image leftArrow;
	public Image rightArrow;
	public Image midArrow;
	public Image leftArrowMocked;
	public Image rightArrowMocked;
	public Image midArrowMocked;

	/// <summary>
	/// 
	/// </summary>
	public BattleLineController battleLine;
	public int resIdx;
	public UnitElementController target;

	public Image attackBuff;
	public Image maxHealthBuff;

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

	public static float componentMove = 10f;
	public static float componentMoveTime = 0.25f;
	/// <summary>
	/// 从牌堆加入手牌或战场时初始化
	/// </summary>
	/// <param name="ownership"></param>
	public void UnitInit(string ID, int ownership, string name, string categories, int cost, string description, IUnitInput input)
	{
		Init(ID, ownership, name, categories, cost, description);

		healthText.text = maxHealthPoint.ToString();

		this.input = input;
		Arrows.transform.localScale = new Vector3(1, (1 - ownership * 2) * 1, 1);

		arrowsGroup = Arrows.GetComponent<CanvasGroup>();
		arrowsGroup.alpha = 0;
		InspectPanel.alpha = 0f;

		InspectorName.text = name;
		InspectorCost.text = cost.ToString();
		InspectorDescription.text = description;
		InspectorAttack.text = attackText.text;
		InspectorMaxHealth.text = maxHealthPoint.ToString();
		InspectorAttackCounter.text = this.category == "Construction" ? "" : attackCounter.ToString();

		InspectorImage.sprite = CardImage.sprite;
		InspectorGround.color = elementGround.color;
		InspectorFrame.color = elementGround.color;
		InspectorNameTag.color = elementGround.color;
		InspectorCostTag.color = elementGround.color;
		InspectorCategoryIcon.sprite = componentCategoryIcon.sprite;
	}
	public void UpdateInfo(int cost, int attackPoint, int maxHealthPoint, int attackCounter, int operateCounter,
		ElementState state, int moveRange, bool aura, int attackBuff, int maxHealthBuff)
	{
		this.cost = cost;
		this.attackPoint = attackPoint;
		this.maxHealthPoint = maxHealthPoint;
		this.attackCounter = attackCounter;
		this.operateCounter = operateCounter;
		this.dataState = state;
		this.moveRange = moveRange;

		nameText.text = nameContent;
		attackText.text = attackPoint.ToString();
		attackCounterText.text = attackCounter > 100 ? "" : attackCounter.ToString();
		costText.text = cost.ToString();
		descriptionText.text = description;

		if (operateCounter == 0)
		{
			operateMask.DOColor(new UnityEngine.Color(0, 0, 0, 0.5f), duration);
		}
		else
		{
			operateMask.DOColor(new UnityEngine.Color(0, 0, 0, 0), duration);
		}

		this.attackBuff.gameObject.SetActive(false);
		this.maxHealthBuff.gameObject.SetActive(false);
		if (attackBuff != 0)
		{
			this.attackBuff.gameObject.SetActive(true);
			if(attackBuff > 0)
			{
				this.attackBuff.color = Color.green;
				this.attackBuff.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
			}
			if(attackBuff < 0)
			{
				this.attackBuff.color = Color.red;
				this.attackBuff.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
			}
		}
		if(maxHealthBuff != 0)
		{
			this.maxHealthBuff.gameObject.SetActive(true);
			if (attackBuff > 0)
			{
				this.maxHealthBuff.color = Color.green;
				this.maxHealthBuff.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
			}
			if (attackBuff < 0)
			{
				this.maxHealthBuff.color = Color.red;
				this.maxHealthBuff.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
			}
		}
		//DOTween.To(
		//	() => "", // getter返回空字符串
		//	value => nameText.text = value, // setter设置costText的内容
		//	nameTag, // endValue是原始内容
		//	0.2f
		//).SetEase(Ease.Linear); // 设置动画为线性变化
	}
	public void UpdateTarget(IUnitElementController target, int targetIdx, bool mocking, bool cleave)
	{
		this.target = target as UnitElementController;
		battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
			() =>
			{
				UpdateTarget(targetIdx, mocking, cleave);
			}
		);
	}
	private void UpdateTarget(int targetIdx, bool mocking, bool cleave)
	{
		arrowsGroup.alpha = 0;
		
		if (cleave)
		{
			arrowsGroup.alpha = 1;
			leftArrow.DOFade(1, duration);
			midArrow.DOFade(1, duration);
			rightArrow.DOFade(1, duration);
			return;
		}
		if (targetIdx == 0)
		{
			arrowsGroup.alpha = 1;
			leftArrow.DOFade(1, duration);
			leftArrowMocked.DOFade(mocking ? 1 : 0, duration);
			midArrow.DOFade(0, duration);
			midArrowMocked.DOFade(0, duration);
			rightArrow.DOFade(0, duration);
			rightArrowMocked.DOFade(0, duration);
		}
		if (targetIdx == 1)
		{
			arrowsGroup.alpha = 1;
			leftArrow.DOFade(0, duration);
			leftArrowMocked.DOFade(0, duration);
			midArrow.DOFade(1, duration);
			midArrowMocked.DOFade(mocking ? 1 : 0, duration);
			rightArrow.DOFade(0, duration);
			rightArrowMocked.DOFade(0, duration);
		}
		if (targetIdx == 2)
		{
			arrowsGroup.alpha = 1;
			leftArrow.DOFade(0, duration);
			leftArrowMocked.DOFade(0, duration);
			midArrow.DOFade(0, duration);
			midArrowMocked.DOFade(0, duration);
			rightArrow.DOFade(1, duration);
			rightArrowMocked.DOFade(mocking ? 1 : 0, duration);
		}
	}









	public void DeployAnimationEvent()
	{
		animeLock = true;

		NameTag.gameObject.SetActive(false);
		nameText.gameObject.SetActive(false);
		costTag.gameObject.SetActive(false);
		costText.gameObject.SetActive(false);

		if (transform.eulerAngles != Vector3.zero)
		{
			Vector3 rotateBy = -transform.eulerAngles;
			transform.DOBlendableRotateBy(rotateBy, componentMoveTime);
		}
		gameObject.SetActive(true);
		InspectComponent.transform.DOBlendableLocalMoveBy(new Vector3(0, -componentMove, 0), componentMoveTime)
			.OnComplete(() => animeLock = false);
		RectTransform counterTransform = counterIcon.GetComponent<RectTransform>();
		counterTransform.DOScale(counterScaleEnlarge, componentMoveTime);
		//healthText.fontSize = normalFontSizeEnlarge;
		//attackText.fontSize = normalFontSizeEnlarge;
		attackCounterText.fontSize = counterfontSizeEnlarge;
		transform.DOScale(battleFieldScale, componentMoveTime);
	}
	public void MoveAnimationEvent()
	{
		animeLock = true;
		transform.DOScale(battleFieldScale, componentMoveTime)
			.OnComplete(() => animeLock = false);
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
			if(battleSceneManager.sequenceTime == 0)
			{
				battleSceneManager.sequenceTime += forwardTime;
			}
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
			healthText.DOColor(UnityEngine.Color.red, forwardTime / 2f)
				.OnComplete(() =>
				{
					healthText.DOColor(UnityEngine.Color.white, forwardTime / 2f);
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
			healthText.DOColor(UnityEngine.Color.green, forwardTime / 2f)
				.OnComplete(() =>
				{
					healthText.DOColor(UnityEngine.Color.white, forwardTime / 2f);
				})
			);
	}
	/// <summary>
	/// 
	/// </summary>
	public void TerminateAnimationEvent(string method)
	{
		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + " destroyed!");

		if(method == "append")
		{
			battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
				() =>
				{
					battleLine.ElementRemove(resIdx);
					battleLine.UpdateElementPosition();
					gameObject.SetActive(false);
					input.UpdateManual();
				}
			);
		}
		else
		{
			battleLine.ElementRemove(resIdx);
			battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
				() =>
				{
					battleLine.UpdateElementPosition();
					gameObject.SetActive(false);
					input.UpdateManual();
				}
			);
		}
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

		InspectComponent.transform.DOBlendableLocalMoveBy(new Vector3(0, componentMove, 0), componentMoveTime);
		RectTransform counterTransform = counterIcon.GetComponent<RectTransform>();
		counterTransform.DOScale(counterScaleOrigin, componentMoveTime);
		//healthText.fontSize = normalFontSizeOrigin;
		//attackText.fontSize = normalFontSizeOrigin;
		attackCounterText.fontSize = counterfontSizeOrigin;
		UpdateInspectComponent();

		if (method == "append")
		{
			Vector3 rotateBy = new Vector3(0, 0, ((ownership * 2) - 1) * 90);
			battleSceneManager.rotateSequence.Append(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, retreatTime)
				.OnComplete(() =>
				{
					animeLock = false;
					this.gameObject.SetActive(false);
				})
			);
			battleSceneManager.rotateSequence.Join(
				transform.DOBlendableRotateBy(rotateBy, retreatTime)
				);
			battleSceneManager.rotateSequence.Join(transform.DOScale(handicapScale, retreatTime));
			battleSceneManager.sequenceTime += retreatTime;
		}
		else
		{
			Vector3 rotateBy = new Vector3(0, 0, ((ownership * 2) - 1) * 90);
			battleSceneManager.rotateSequence.Join(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, retreatTime)
				.OnComplete(() =>
				{
					animeLock = false;
					this.gameObject.SetActive(false);
				})
			);
			battleSceneManager.rotateSequence.Join(
				transform.DOBlendableRotateBy(rotateBy, retreatTime)
				);
			battleSceneManager.rotateSequence.Join(transform.DOScale(handicapScale, retreatTime));
			transform.DOScale(handicapScale, retreatTime);
			battleSceneManager.sequenceTime += retreatTime;
		}
	}
	public void UpdateInspectComponent()
	{

	}





















	private float timer = 0;
	public Vector3 pastScale;
	void Update()
	{
		if(handicap.isDragging == false && dataState == ElementState.inBattleLine)
		{
			transform.DOScale(battleFieldScale, duration);
		}
		//if(HandicapController.isDragging == false && dataState == ElementState.inHandicap)
		//{
		//	transform.DOScale(handicapScale, duration);
		//}

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
				InspectPanel.transform.position = transform.position + 400 * Vector3.right;
				InspectPanel.DOFade(1f, duration);

				canvas.sortingOrder = attackOrder;
			}
		}
	}
	public override void OnDrag(PointerEventData eventData)
	{
		base.OnDrag(eventData);
		Arrows.GetComponent<CanvasGroup>().alpha = 0f;
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
			handicap.isDragging = true;
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		base.OnEndDrag(eventData);

		//HandicapController.isDragging = false;
		//部署条件判定
		if (dataState == ElementState.inHandicap)
		{
			if (battleSceneManager.PlayerDeploy(eventData.position, this.handicapIdx) >= 0)
			{
				handicap.isDragging = false;
				return;
			}
			handicap.isDragging = false;
			handicap.Insert(this);
		}
		//移动撤退条件判定
		if (dataState == ElementState.inBattleLine)
		{
			if (operateCounter <= 0)
			{
				handicap.isDragging = false;
				return;
			}
			if (category == "Construction")
			{
				handicap.isDragging = false;
				return;
			}
			if (this.battleLine.lineIdx == 0 && battleSceneManager.PlayerRetreat(eventData.position, this.battleLine, this) >= 0)
			{
				handicap.isDragging = false;
				return;
			}
			if (battleSceneManager.PlayerMove(eventData.position, this.battleLine, this) >= 0)
			{
				handicap.isDragging = false;
				return;
			}
			handicap.isDragging = false;
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
			InspectPanel.DOFade(0f, duration);

			canvas.sortingOrder = oriOrder;
			return;
		}
	}

}
