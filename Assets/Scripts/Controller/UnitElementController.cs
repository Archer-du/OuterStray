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



public enum ControllerState
{
	attacking,
	moving
}
public class UnitElementController : BattleElementController,
	IUnitElementController
{


	public IUnitInput input;

	public BattleLineController line;
	public int resIdx;


	public Vector3 logicPosition;
	public Vector3 logiPosition;


	public int preprocessed;//TODO 敌方特有，之后会改


	public int attackPoint;
	public int healthPoint;
	public int maxHealthPoint;
	public int attackCounter;
	public int operateCounter;
	//legacy
	public int moveRange;

	public TMP_Text nameText;
	public TMP_Text attackText;
	public TMP_Text healthText;
	public TMP_Text attackCounterText;
	public TMP_Text costText;


	public Image backGround;
	//public Image shell;
	public Image frame;
	public Image NameTag;
	public Image slot;
	public Image costTag;

	public Image CardImage;
	public Image mask;


	public float duration = 0.2f;

	public GameObject leftArrow;
	public GameObject rightArrow;
	public GameObject midArrow;

	public ControllerState controllerState;




	/// <summary>
	/// 从牌堆加入手牌或战场时初始化
	/// </summary>
	/// <param name="ownership"></param>
	public void Init(string ID, int ownership, string name, string categories, string description, IUnitInput input)
	{
		this.input = input;
		this.ownership = ownership;
		this.nameContent = name;
		this.category = categories;
		this.description = description;

		line = null;


		LoadCardResources(ID);

		preprocessed = ownership;

		originScale = transform.localScale;
		enlargeScale = 1.35f * originScale;

		arrowScale = leftArrow.transform.localScale;
		enlargeArrowScale = arrowScale * 1.5f;

		originTextScale = healthText.transform.localScale;
		targetTextScale = healthText.transform.localScale * 1.5f;

		

		leftArrow.SetActive(false);
		rightArrow.SetActive(false);
		midArrow.SetActive(false);

		descriptionPanel.color = Color.clear;
		descriptionText.gameObject.SetActive(false);
	}
	/// <summary>
	/// 读取卡面图像音频等资源
	/// </summary>
	private void LoadCardResources(string ID)
	{
		CardImage.sprite = Resources.Load<Sprite>("CardImage/" + ID);
		if (ownership == 1)
		{
			CardImage.rectTransform.sizeDelta = new Vector2(10, 13);
		}
		switch (category)
		{
			case "Guardian":
				Color color;
				if (UnityEngine.ColorUtility.TryParseHtmlString("#97A5A4", out color))
				{
					NameTag.color = color;
					frame.color = color;
					slot.color = color;
					costTag.color = color;

					backGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[21];
				}
				break;
			case "Artillery":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#CE8849", out color))
				{
					NameTag.color = color;
					frame.color = color;
					slot.color = color;
					costTag.color = color;

					backGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[22];
				}
				break;
			case "Motorized":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#426A84", out color)) // 尝试解析色号字符串，如果成功，返回 true 并赋值给 color 变量
				{
					NameTag.color = color; // 赋值给 Image 组件的 color 属性
					frame.color = color;
					slot.color = color;
					costTag.color = color;

					backGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[23];
				}
				break;
			case "LightArmor":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#429656", out color)) // 尝试解析色号字符串，如果成功，返回 true 并赋值给 color 变量
				{
					NameTag.color = color; // 赋值给 Image 组件的 color 属性
					frame.color = color;
					slot.color = color;
					costTag.color = color;

					backGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[24];
				}
				break;
			case "Construction":
				if (UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color)) // 尝试解析色号字符串，如果成功，返回 true 并赋值给 color 变量
				{
					NameTag.color = color; // 赋值给 Image 组件的 color 属性
					frame.color = color;
					slot.color = color;
					costTag.color = color;

					backGround.sprite = Resources.LoadAll<Sprite>("CardFrame/frame2.0")[25];
				}
				break;
		}
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

		nameText.text = name;
		attackText.text = attackPoint.ToString();
		healthText.text = healthPoint.ToString();
		attackCounterText.text = this.category == "Construction" ? "" : attackCounter.ToString();
		costText.text = cost.ToString();
		descriptionText.text = description;

		if (operateCounter == 0)
		{
			mask.DOColor(new Color(0, 0, 0, 0.5f), duration);
		}
		else
		{
			mask.DOColor(new Color(0, 0, 0, 0), duration);
		}
		if(state == ElementState.inBattleLine)
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




	public Vector3 arrowScale;
	public Vector3 enlargeArrowScale;

	public Vector3 originTextScale;
	public Vector3 targetTextScale;

	//TODO remove
	public UnitElementController t1;
	public UnitElementController t2;
	public UnitElementController t3;
	public UnitElementController target;
	public void UpdateTarget(IUnitElementController t1, IUnitElementController t2, IUnitElementController t3, IUnitElementController target, int targetIdx)
	{
		this.t1 = t1 as UnitElementController;
		this.t2 = t2 as UnitElementController;
		this.t3 = t3 as UnitElementController;
		this.target = target as UnitElementController;

		//Debug.Log("line: " + line.lineIdx + "res: " + resIdx);
		//Debug.Log(this.t1?.resIdx);
		//Debug.Log(this.t2?.resIdx);
		//Debug.Log(this.t3?.resIdx);
		//Debug.Log(targetIdx);
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





	public int attackOrder = 0;
	public int oriOrder = -100;


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

		Debug.Log("line: " + line.lineIdx + "res: " + resIdx + " attacked " + "line: " + target.line.lineIdx + "res: " + target.resIdx);

		battleSceneManager.rotateSequence.AppendInterval(BattleLineController.updateTime + 0.2f);
		battleSceneManager.sequenceTime += BattleLineController.updateTime + 0.2f;
		//TODO time config
		battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
			() =>
			{
				canvas.sortingOrder = attackOrder;
			});
		battleSceneManager.rotateSequence.Append(
			transform.DOMove(dstPosition, 0.2f).OnComplete(() =>
			{
				transform.DOMove(oriPosition, 0.2f).OnComplete(() => line.UpdateElementPosition());
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

		Debug.Log("line: " + line.lineIdx + "res: " + resIdx + " random attacked " + "line: " + controller.line.lineIdx + "res: " + controller.resIdx);

		battleSceneManager.rotateSequence.AppendInterval(BattleLineController.updateTime + 0.2f);
		battleSceneManager.sequenceTime += BattleLineController.updateTime + 0.2f;

		battleSceneManager.rotateSequence.Append(
			transform.DOMove(transform.position + 100f * Vector3.up * (2 * ownership - 1), forwardTime).OnComplete(() =>
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
	/// <summary>
	/// 
	/// </summary>
	public void TerminateAnimationEvent(string method)
	{
		Debug.Log("line: " + line.lineIdx + "res: " + resIdx + " destroyed!");


		battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
				() =>
				{
					line.ElementRemove(resIdx);
					gameObject.SetActive(false);
					input.UpdateManual();
				}
			);
		//battleSceneManager.rotateSequence.AppendInterval(0.4f);
		//battleSceneManager.sequenceTime += 0.4f;
	}

	public void CleaveAttackAnimationEvent(int resIdx, int count)
	{
		if (target == null) return;

		float forwardTime = 0.2f;
		float backlashTime = 0.2f;
		Vector3 oriPosition = logicPosition;
		Vector3 dstPosition = target.logicPosition;

		Debug.Log("line: " + line.lineIdx + "res: " + resIdx + " attacked " + "line: " + target.line.lineIdx + "res: " + target.resIdx);

		battleSceneManager.rotateSequence.AppendInterval(BattleLineController.updateTime + 0.2f);
		battleSceneManager.sequenceTime += BattleLineController.updateTime + 0.2f;
		//TODO time config
		battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
			() =>
			{
				canvas.sortingOrder = attackOrder;
			});
		battleSceneManager.rotateSequence.Append(
			transform.DOMove(dstPosition, 0.2f).OnComplete(() =>
			{
				transform.DOMove(oriPosition, 0.2f).OnComplete(() => line.UpdateElementPosition());
				input.UpdateManual();
			})
		);
		battleSceneManager.sequenceTime += forwardTime;
	}















	public override void OnDrag(PointerEventData eventData)
	{
		base.OnDrag(eventData);
		//拖动显示设置
		if (HandicapController.isDragging) // 如果正在拖动
		{
			transform.SetParent(buffer);
			transform.DOScale(originScale, scaleTime);
			transform.position = eventData.position - offset; // 设置当前对象的位置为鼠标位置
		}
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

			HandicapController.isDragging = true;
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		base.OnEndDrag(eventData);
		//移动条件判定
		if (dataState == ElementState.inBattleLine)
		{
			//好逆天的回调。。
			if (operateCounter <= 0)
			{
				return;
			}
			if (category == "Construction")
			{
				return;
			}
			HandicapController.isDragging = false;
			if (battleSceneManager.PlayerMove(eventData.position, this.line, this) >= 0)
			{
				return;
			}
			line.Insert(this);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);

		if (dataState == ElementState.inBattleLine)
		{
			////TODO 检视
			//canvas.sortingOrder = upperOrder;

			//descriptionPanel.DOColor(Color.gray, 0.2f).OnComplete(() => descriptionText.gameObject.SetActive(true));
			//return;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);

		if (dataState == ElementState.inBattleLine)
		{
			////TODO 检视
			//descriptionText.gameObject.SetActive(false);
			//descriptionPanel.DOColor(Color.clear, 0.1f);
			//line.UpdateElementPosition();
			//return;
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



	//public int GetDeployPos(float position)
	//{
	//	if (count >= capacity)
	//	{
	//		return -1;
	//	}
	//	float vtcPos = position - 1980f;
	//	int pos;
	//	//CRITICAL ALGORITHM
	//	if (count % 2 == 0)
	//	{
	//		int start = count / 2;
	//		//一半卡牌 + 一半间隔
	//		int offset = vtcPos > 0 ? (int)((vtcPos + (cardWidth + interval) / 2) / (cardWidth + interval))
	//			: (int)((vtcPos - (cardWidth + interval) / 2) / (cardWidth + interval));
	//		pos = start + offset;
	//		if (start + offset < 0)
	//		{
	//			pos = 0;
	//		}
	//		else if (start + offset > count)
	//		{
	//			pos = count;
	//		}
	//	}
	//	else
	//	{
	//		int offset = (int)(vtcPos / (cardWidth + interval));
	//		int start = vtcPos > 0 ? (count / 2 + 1) : (count / 2);
	//		pos = start + offset;
	//		if (start + offset < 0)
	//		{
	//			pos = 0;
	//		}
	//		else if (start + offset > count)
	//		{
	//			pos = count;
	//		}
	//	}

	//	return pos;
	//}
}
