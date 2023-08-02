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
public class UnitElementController : MonoBehaviour,
	IUnitElementController, 
	IPointerEnterHandler, IPointerExitHandler,
	IDragHandler,
	IBeginDragHandler, IEndDragHandler
{

	public BattleSceneManager battleSceneManager;

	public IUnitInput input;

	public HandicapController handicap;
	public BattleLineController line;

	public int handicapIdx;
	public int resIdx;

	public Vector3 logicPosition;
	public Vector3 logiPosition;

	public int ownership;
	public int preprocessed;//TODO 敌方特有，之后会改


	public string ID;
	public string nameContent;
	public string category;
	public int cost;
	public int attackPoint;
	public int healthPoint;
	public int maxHealthPoint;
	public int attackCounter;
	public int operateCounter;
	public int moveRange;

	public int cardWidth = 360;

	public Canvas canvas;

	public UnitState dataState;
	public ControllerState controllerState;

	public Vector2 offset;
	public void OnEnable()
	{
		//sortingGroup = transform.GetComponent<SortingGroup>();
		canvas = GetComponent<Canvas>();
		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();//TODO

		//????
		handicap = battleSceneManager.handicapController[ownership];
		line = null;
		//TODO

		buffer = GameObject.Find("Buffer").transform;

	}


	/// <summary>
	/// 从牌堆加入手牌或战场时初始化
	/// </summary>
	/// <param name="ownership"></param>
	public void Init(string ID, int ownership, IUnitInput input)
	{
		this.input = input;
		this.ownership = ownership;

		LoadCardResources();

		preprocessed = ownership;

		originScale = transform.localScale;
		enlargeScale = 1.35f * originScale;

		arrowScale = leftArrow.transform.localScale;
		enlargeArrowScale = arrowScale * 1.5f;

		originTextScale = healthText.transform.localScale;
		targetTextScale = healthText.transform.localScale * 1.5f;

		offset = new Vector2(1980, 1080);

		leftArrow.SetActive(false);
		rightArrow.SetActive(false);
		midArrow.SetActive(false);
	}
	/// <summary>
	/// 读取卡面图像音频等资源
	/// </summary>
	private void LoadCardResources()
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


	//public Color originalColor;
	//public float darkenFactor = 0.5f; // 0到1之间的数，表示加深的程度
	public float duration = 0.2f;

	public GameObject leftArrow;
	public GameObject rightArrow;
	public GameObject midArrow;


	public void UpdateInfo(string name, string categories, 
		int cost, int attackPoint, int healthPoint, int maxHealthPoint, int attackCounter, int operateCounter, 
		UnitState state, int moveRange)
	{
		this.nameContent = name;
		this.category = categories;
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
		attackCounterText.text = attackCounter.ToString();
		costText.text = cost.ToString();

		if (operateCounter == 0)
		{
			mask.DOColor(new Color(0, 0, 0, 0.5f), duration);
		}
		else
		{
			mask.DOColor(new Color(0, 0, 0, 0), duration);
		}
		if(state == UnitState.inBattleLine)
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
		leftArrow.SetActive(t1 != null);
		midArrow.SetActive(t2 != null);
		rightArrow.SetActive(t3 != null);
	}








	/// <summary>
	/// 攻击动画加入结算队列
	/// </summary>
	/// <param name="target"></param>
	public void AttackAnimationEvent(int resIdx, int count)
	{
		if (target == null) return;
		Vector3 oriPosition = logicPosition;
		Vector3 dstPosition = target.logicPosition;

		Debug.Log("line: " + line.lineIdx + "res: " + resIdx + " attacked " + "line: " + target.line.lineIdx + "res: " + target.resIdx);


		//TODO time config
		battleSceneManager.rotateSequence.Append(
			transform.DOMove(dstPosition, 0.2f).OnComplete(() =>
			{
				transform.DOMove(oriPosition, 0.2f).OnComplete(() => line.UpdateElementPosition());
				input.UpdateManual();
			})
		);
		battleSceneManager.sequenceTime += 0.4f;
	}
	/// <summary>
	/// 随机攻击动画加入结算队列
	/// </summary>
	/// <param name="target"></param>
	public void RandomAttackAnimationEvent(IUnitElementController target)
	{
		if (target == null) return;
		UnitElementController controller = target as UnitElementController;
		Vector3 oriPosition = logicPosition;

		Debug.Log("line: " + line.lineIdx + "res: " + resIdx + " random attacked " + "line: " + controller.line.lineIdx + "res: " + controller.resIdx);


		battleSceneManager.rotateSequence.Append(
			transform.DOMove(transform.position + 100f * Vector3.up * (2 * ownership - 1), 0.2f).OnComplete(() =>
			{
				transform.DOMove(oriPosition, 0.2f);
				input.UpdateManual();
			})
		);
		battleSceneManager.sequenceTime += 0.4f;
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="health"></param>
	public void DamageAnimationEvent(int health)
	{
		battleSceneManager.rotateSequence.Append(
			transform.DOShakeRotation(0.2f, 20f)
			);
		//放大
		battleSceneManager.rotateSequence.Join(
			healthText.transform.DOScale(targetTextScale, 0.1f)
				.OnComplete(() =>
				{
					healthText.text = health.ToString();
					healthText.transform.DOScale(originTextScale, 0.1f);
				})
			);
		//变色
		battleSceneManager.rotateSequence.Join(
			healthText.DOColor(Color.red, 0.1f)
				.OnComplete(() =>
				{
					healthText.DOColor(Color.white, 0.1f);
				})
			);

		battleSceneManager.rotateSequence.AppendInterval(0.2f);
		battleSceneManager.sequenceTime += 0.4f;
	}
	/// <summary>
	/// 
	/// </summary>
	public void TerminateAnimationEvent()
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
		battleSceneManager.rotateSequence.AppendInterval(0.4f);
		battleSceneManager.sequenceTime += 0.4f;
	}








	private Transform buffer;

	public float scaleTime = 0.3f;
	public void OnDrag(PointerEventData eventData)
	{
		if (ownership != 0)
		{
			return;
		}
		if (BattleSceneManager.Turn != 0)
		{
			return;
		}
		//拖动显示设置
		if (HandicapController.isDragging) // 如果正在拖动
		{
			transform.SetParent(buffer);
			transform.DOScale(originScale, scaleTime);
			transform.position = eventData.position - offset; // 设置当前对象的位置为鼠标位置
		}
	}
	public int handicapChildNum = 2;
	public int inlineChildNum = 4;
	public void OnBeginDrag(PointerEventData eventData)
	{
		if(ownership != 0)
		{
			return;
		}
		//锁住玩家操作
		if(BattleSceneManager.Turn != 0)
		{
			return;
		}
		//在战线：移动
		if(dataState == UnitState.inBattleLine)
		{
			//if(operateCounter == 0)
			//{
			//	return;
			//}

			HandicapController.isDragging = true;
		}
		//在手牌区：部署或cast
		if(dataState == UnitState.inHandicap)
		{
			HandicapController.isDragging = true; // 开始拖动
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (ownership != 0)
		{
			return;
		}
		//锁住玩家操作
		if (BattleSceneManager.Turn != 0)
		{
			return;
		}
		//移动条件判定
		if (dataState == UnitState.inBattleLine)
		{
			//好逆天的回调。。TODO
			if (operateCounter <= 0)
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
		//部署条件判定
		if(dataState == UnitState.inHandicap)
		{
			HandicapController.isDragging = false; // 结束拖动
			if (battleSceneManager.PlayerDeploy(eventData.position, this.handicapIdx) >= 0)
			{
				return;
			}
			handicap.Insert(this);
		}
	}

	// 原始位置
	private Vector3 originalPosition;
	// 目标位置
	private Vector3 targetPosition;

	public Vector3 enlargeScale;
	public Vector3 originScale;
	public float moveTime = 0.2f;

	public float grayScale = 0.2f;
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (HandicapController.isDragging)
		{
			return;
		}
		if (HandicapController.pushing)
		{
			return;
		}
		if (ownership != 0)
		{
			return;
		}
		if (dataState == UnitState.inHandicap)
		{
			//sortingGroup.sortingOrder = 100;
			transform.DOScale(enlargeScale, moveTime);
			transform.DOMove(handicap.GetInsertionPosition(handicapIdx) + 400f * Vector3.up, moveTime);
			//transform.DOBlendableLocalMoveBy(targetPosition, moveTime);
			//transform.DOBlendableScaleBy(enlargeScale, moveTime);
		}
		if (dataState == UnitState.inBattleLine)
		{
			//TODO 检视
			return;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (HandicapController.isDragging)
		{
			return;
		}
		if (HandicapController.pushing)
		{
			return;
		}
		if (ownership != 0)
		{
			return;
		}
		if(dataState == UnitState.inHandicap)
		{
			//sortingGroup.sortingOrder = 0;
			transform.DOScale(originScale, 0.2f);
			transform.DOMove(handicap.GetInsertionPosition(handicapIdx), moveTime);
			//transform.DOMove(originalPosition, moveTime);
			//transform.DOBlendableScaleBy(originScale, moveTime);
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
