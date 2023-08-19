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
	public int moveRange;

	public Vector3 battleLineLogicPosition;

	public float duration = 0.2f;

	public int attackOrder = 0;
	public int oriOrder = -100;

	public static float componentMove = 10f;
	public static float componentMoveTime = 0.25f;

	[Header("Audio")]
	private AudioSource attackAudio;
	private AudioSource deployAudio;
	private AudioSource randomAttackAudio;
	private AudioSource healAudio;
	[Header("Clip")]
	public AudioClip attackClip;
	public AudioClip deployClip;
	public AudioClip randomAttackClip;
	public AudioClip healClip;

	[Header("Components")]
	public InspectPanelController battleLineInspect;
	public CardInspect inspector;
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

		inspector.CopyInfo(this);

		battleLineInspect = GetComponent<InspectPanelController>();
		OnElementStateChanged += battleLineInspect.OnElementStateChanged;

		BattleSceneManager.InputLocked += EnableInputLock;
		BattleSceneManager.InputUnlocked += DisableInputLock;



		UnitAudioInitialize();
	}
	private void UnitAudioInitialize()
	{
		attackAudio = gameObject.AddComponent<AudioSource>();
		attackAudio.clip = attackClip;
		attackAudio.loop = false;
		attackAudio.playOnAwake = false;
		deployAudio = gameObject.AddComponent<AudioSource>();
		deployAudio.clip = deployClip;
		deployAudio.loop = false;
		deployAudio.playOnAwake = false;
		randomAttackAudio = gameObject.AddComponent<AudioSource>();
		randomAttackAudio.clip = randomAttackClip;
		randomAttackAudio.loop = false;
		randomAttackAudio.playOnAwake = false;
		healAudio = gameObject.AddComponent<AudioSource>();
		healAudio.clip = healClip;
		healAudio.loop = false;
		healAudio.playOnAwake = false;
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
			operateMask.DOColor(new Color(0, 0, 0, 0.5f), duration);
		}
		else
		{
			operateMask.DOColor(new Color(0, 0, 0, 0), duration);
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
	}

	public void UpdateHealth(int dynHealth)
	{
		battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
			() =>
			{
				healthPoint = dynHealth;
				healthText.text = dynHealth.ToString();
				healthText.DOColor(new Color(1, (float)dynHealth / maxHealthPoint, (float)dynHealth / maxHealthPoint), duration);
            }
        );
	}
	public void UpdateHealthImmediate(int dynHealth)
	{
		healthPoint = dynHealth;
        healthText.text = dynHealth.ToString();
        healthText.DOColor(new Color(1, (float)dynHealth / maxHealthPoint, (float)dynHealth / maxHealthPoint), duration);
    }

	public void UpdateTarget(int t1, int t2, int t3, IUnitElementController target, int targetIdx, bool mocking, bool cleave)
	{
		this.target = target as UnitElementController;
		battleSceneManager.rotateSequence.InsertCallback(battleSceneManager.sequenceTime,
			() =>
			{
				if (cleave)
				{
					UpdateTarget(t1, t2, t3);
				}
				else
				{
					UpdateTarget(targetIdx, mocking);
				}
			}
		);
	}

	private void UpdateTarget(int t1, int t2, int t3)
	{
		arrowsGroup.alpha = 1;
		leftArrow.DOFade(t1, duration);
		midArrow.DOFade(t2, duration);
		rightArrow.DOFade(t3, duration);
	}

	private void UpdateTarget(int targetIdx, bool mocking)
	{
		arrowsGroup.alpha = 0;
		if (category == "Construction") return;

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

	public void UpdateInspectComponent()
	{

	}
	public void UpdateState(ElementState state)
	{
		dataState = state;
	}



	private void EnableInputLock()
	{
		inputLock = true;
	}
	private void DisableInputLock()
	{
		inputLock = false;
	}




	public void DeployAnimationEvent()
	{
		deployAudio.Play();
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
		InspectComponent.transform.DOBlendableLocalMoveBy(new Vector3(0, -componentMove, 0), componentMoveTime);
		RectTransform counterTransform = counterIcon.GetComponent<RectTransform>();
		counterTransform.DOScale(counterScaleEnlarge, componentMoveTime);
		//healthText.fontSize = normalFontSizeEnlarge;
		//attackText.fontSize = normalFontSizeEnlarge;
		attackCounterText.fontSize = counterfontSizeEnlarge;
		transform.DOScale(battleFieldScale, componentMoveTime);
	}
	public void MoveAnimationEvent()
	{
		transform.DOScale(battleFieldScale, componentMoveTime);
	}
	/// <summary>
	/// 攻击动画加入结算队列
	/// </summary>
	/// <param name="target"></param>
	public void AttackAnimationEvent(int resIdx, int count)
	{
		if (target == null) return;

		float forwardTime = 0.15f;
        float backlashTime = 0.2f;
		Vector3 oriPosition = battleLineLogicPosition;
		Vector3 dstPosition = target.battleLineLogicPosition;

		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + nameContent + " attacked " + "line: " + target.battleLine.lineIdx + "res: " + target.resIdx + target.nameContent);

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
				attackAudio.Play();
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

		float forwardTime = 0.15f;
		float backlashTime = 0.2f;
		UnitElementController controller = target as UnitElementController;
		Vector3 oriPosition = battleLineLogicPosition;

		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + nameContent + " random attacked " + "line: " + controller.battleLine.lineIdx + "res: " + controller.resIdx + controller.nameContent);

		battleSceneManager.rotateSequence.AppendInterval(BattleLineController.updateTime + 0.2f);
		battleSceneManager.sequenceTime += BattleLineController.updateTime + 0.2f;

		battleSceneManager.rotateSequence.Append(
			transform.DOMove(battleLineLogicPosition + 100f * Vector3.up * (2 * ownership - 1), forwardTime).OnComplete(() =>
			{
				randomAttackAudio.Play();
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

		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + nameContent + " damaged " + (healthPoint - health).ToString());
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
					if(this == battleSceneManager.bases[0])
					{
						battleSceneManager.UpdateBaseHealth(health);
					}
				})
			);
		//变色
		battleSceneManager.rotateSequence.Join(
			healthText.DOColor(Color.red, forwardTime / 2f)
				.OnComplete(() =>
				{
                    UpdateHealthImmediate(health);
                })
			);
	}
	public void RecoverAnimationEvent(int health, string method)
	{
		float forwardTime = 0.2f;

		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + nameContent + " healed " + (health - healthPoint).ToString());
		if (method == "append")
        {
			battleSceneManager.rotateSequence.Append(
				transform.DOShakeRotation(forwardTime, 20f)
				.OnComplete(() => healAudio.Play())
				);
			battleSceneManager.sequenceTime += forwardTime;
		}
		else
		{
			battleSceneManager.rotateSequence.Join(
				transform.DOShakeRotation(forwardTime, 20f)
				);
			if (battleSceneManager.sequenceTime == 0)
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
					if (this == battleSceneManager.bases[0])
					{
						battleSceneManager.UpdateBaseHealth(health);
					}
				})
			);
		//变色
		battleSceneManager.rotateSequence.Join(
			healthText.DOColor(Color.green, forwardTime / 2f)
				.OnComplete(() =>
				{
					UpdateHealthImmediate(health);
				})
			);
	}
	/// <summary>
	/// 
	/// </summary>
	public void TerminateAnimationEvent(string method)
	{
		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + nameContent + " destroyed!");

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

		float forwardTime = 0.15f;
		float backlashTime = 0.2f;
		Vector3 oriPosition = battleLineLogicPosition;
		Vector3 dstPosition = target.battleLineLogicPosition;

		Debug.Log("line: " + battleLine.lineIdx + "res: " + resIdx + nameContent + " CleaveAttacked ");

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

		Vector3 rotateBy = new Vector3(0, 0, ((ownership * 2) - 1) * 90);
		if (method == "append")
		{
			battleSceneManager.rotateSequence.Append(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, retreatTime)
				.OnComplete(() =>
				{
					arrowsGroup.alpha = 0;
					this.gameObject.SetActive(false);
				})
			);
			battleSceneManager.sequenceTime += retreatTime;
		}
		else
		{
			battleSceneManager.rotateSequence.Join(
				transform.DOMove(stack.transform.position + 500 * Vector3.left, retreatTime)
				.OnComplete(() =>
				{
					this.gameObject.SetActive(false);
				})
			);
			if (battleSceneManager.sequenceTime == 0)
			{
				battleSceneManager.sequenceTime += retreatTime;
			}
		}

		transform.DOScale(handicapScale, retreatTime);
		battleSceneManager.rotateSequence.Join(
			transform.DOBlendableRotateBy(rotateBy, retreatTime)
			);
		battleSceneManager.rotateSequence.Join(transform.DOScale(handicapScale, retreatTime));
	}



	public Transform Component;
	public void Start()
	{
		
	}
	void Update()
	{
		if(draggingLock == false && dataState == ElementState.inBattleLine)
		{
			transform.DOScale(battleFieldScale, duration);
		}
		//if(HandicapController.isDragging == false && dataState == ElementState.inHandicap)
		//{
		//	transform.DOScale(handicapScale, duration);
		//}

		// 如果鼠标悬停在元素上
		//if (timer > 0)
		//{
		//	// 计时器递减
		//	timer -= Time.deltaTime;
		//	// 如果计时器小于等于 0
		//	if (timer <= 0)
		//	{
		//		// 播放动画
		//		//TODO 检视
		//		InspectPanel.transform.position = transform.position + 400 * Vector3.right;
		//		InspectPanel.DOFade(1f, duration);

		//		canvas.sortingOrder = attackOrder;
		//	}
		//}
	}
}
