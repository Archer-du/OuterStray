using InputHandler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DisplayInterface;
using TMPro;
using UnityEngine.UI;
using System;
using System.Xml.Linq;
using System.Data;
using System.IO;
using UnityEditor;
using JetBrains.Annotations;
using System.Linq;

public class BattleSceneManager : MonoBehaviour,
	IBattleSceneController
{
	public static event Action InputLocked;
	public static event Action InputUnlocked;
	/// <summary>
	/// GameManager单例
	/// </summary>
	public GameManager gameManager;

	public Canvas Settler;
	public Button SettleButton;
	public TMP_Text SettleText;

	public static bool inputLock;

	[Header("Input")]
	/// <summary>
	/// 系统输入接口
	/// </summary>
	IBattleSystemInput battleSystem;

	[Header("Prototype")]
	public GameObject battleLinePrototype;
	public GameObject cardStackPrototype;
	public GameObject handicapPrototype;

	public GameObject unitPrototype;
	public GameObject commandPrototype;

	[Header("SubControllers")]
	public BattleLineController[] battleLineControllers;

	public CardStackController[] cardStackController;

	public HandicapController[] handicapController;

	public TurnMappedDialogger dialogController;
	public TMP_Text[] energyText;
	public TMP_Text[] energySupplyText;

	public AIBehavior behavior;

	[Header("Data")]
	/// <summary>
	/// 渲染层TURN备份
	/// </summary>
	public static int Turn;
	/// <summary>
	/// dialog trigger & counter
	/// </summary>
	private int TurnNum;
	public int turnNum
	{
		get => TurnNum;
		set
		{
			TurnNum = value;
			dialogController.UpdateDialog();
		}
	}
	public int[] energy;
	public int[] energySupply;

	public UnitElementController[] bases;

	public Image baseImage;
	public Image baseFrame;
	public Image baseGround;
	public Image baseIcon;
	public TMP_Text baseHealth;
	public TMP_Text baseDescription;


	public Button exitButton;


	/// <summary>
	/// 战场战线容量
	/// </summary>
	public int fieldCapacity;


	public float fieldWidth;
	public float fieldHeight;


	public Button skipButton;
	public Image buttonImage;


	public Sequence rotateSequence;
	public float sequenceTime = 0;

	//TODO
	public GameObject humanEnergy;
	public GameObject[] humanSlots;
	public GameObject[] plantSlots;

	//结算锁
	public static bool settlement = false;

	public void Start()
	{
		Settler.gameObject.transform.position = new Vector3(0, 2160, 0);
		SettleButton.onClick.AddListener(BattleOverChecked);
	}
	public void FieldInitialize(IBattleSystemInput handler, int fieldCapacity)
	{
		gameManager = GameManager.GetInstance();

		battleSystem = handler;

		//TODO config
		this.fieldCapacity = fieldCapacity;
		battleLineControllers = new BattleLineController[fieldCapacity];

		energy = new int[2];
		energySupply = new int[2];

		cardStackController = new CardStackController[2];
		handicapController = new HandicapController[2];

		bases = new UnitElementController[2];


		skipButton.onClick.AddListener(Skip);
		buttonImage = skipButton.gameObject.GetComponent<Image>();

		fieldWidth = GameObject.Find("BattleField").GetComponent<RectTransform>().rect.width;
		fieldHeight = GameObject.Find("BattleField").GetComponent<RectTransform>().rect.height;
		
		dialogController = GetComponent<TurnMappedDialogger>();
		//TODO trigger
		turnNum = 0;
		behavior = GetComponent<AIBehavior>();

        for (int i = 0; i < 5; i++)
		{
			humanSlots[i].SetActive(false);
			plantSlots[i].SetActive(false);
		}

		exitButton.onClick.AddListener(Exit);

		InputLocked?.Invoke();
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();
	}
	public void InitBases(IUnitElementController humanBase, IUnitElementController plantBase)
	{
		bases[0] = humanBase as UnitElementController;
		bases[1] = plantBase as UnitElementController;

		Color color = bases[0].elementFrame.color;
		baseImage.sprite = bases[0].CardImage.sprite;
		baseFrame.color = color;
		baseGround.color = color;
		baseIcon.sprite = bases[0].InspectorCategoryIcon.sprite;
		baseDescription.text = bases[0].description;
	}
	public float healthDuration = 0.2f;
	public void UpdateBaseHealth(int health)
	{
		baseHealth.text = health.ToString();
		baseHealth.DOColor(new Color(1, (float)health / bases[0].maxHealthPoint, (float)health / bases[0].maxHealthPoint), healthDuration);
	}

    /// <summary>
    /// 
    /// </summary>
    public void UpdateTurnWithSettlement()
	{
		//结算攻击动画
		rotateSequence.InsertCallback(sequenceTime, () =>
		{
			sequenceTime = 0;
			InputUnlocked?.Invoke();
			UpdateTurn();
		});
	}
	public void Settlement()
	{
		rotateSequence.InsertCallback(sequenceTime, () =>
		{
			sequenceTime = 0;
			InputUnlocked?.Invoke();
		});
	}
	public void UpdateTurn(int TURN)
	{
		Turn = TURN;
		turnNum++;
		if(Turn == 0)
		{
			buttonImage.color = Color.white;
			skipButton.enabled = true;
		}
		//如果是敌方回合，启动行为树
		else
		{
			buttonImage.color = Color.gray;

			StartCoroutine(AIBehavior());
			skipButton.enabled = false;
		}
	}
	private void UpdateTurn()
	{
		Turn = (Turn + 1) % 2;
		turnNum++;
		if (Turn == 0)
		{
			buttonImage.color = Color.white;
			skipButton.enabled = true;
		}
		//如果是敌方回合，启动行为树
		else
		{
			buttonImage.color = Color.gray;
			skipButton.enabled = false;

			StartCoroutine(AIBehavior());
		}
	}




    public void UpdateEnergy(int energy)
	{
		this.energy[Turn] = energy;
		this.energyText[Turn].text = energy.ToString();
	}

	public void UpdateEnergy(int turn, int energyPoint)
	{
		this.energy[turn] = energyPoint;
		energyText[turn].text = energyPoint.ToString();
	}

	public void UpdateEnergySupply(int supply)
	{
		this.energySupply[Turn] = supply;
		this.energySupplyText[Turn].text = "+" + supply.ToString();
		UpdateSlotsDisplay();
	}

	public void UpdateEnergySupply(int turn, int supplyPoint)
	{
		this.energySupply[turn] = supplyPoint;
		energySupplyText[turn].text = "+" + supplyPoint.ToString();
		UpdateSlotsDisplay();
	}
	public void UpdateSlotsDisplay()
	{
		for (int i = 0; i < this.energySupply[0]; i++)
		{
			humanSlots[i].SetActive(true);
		}
		for(int i = 0; i < this.energySupply[1]; i++)
		{
			plantSlots[i].SetActive(true);
		}
	}





	public IBattleLineController InstantiateBattleLine(int idx, int capacity)
	{
		GameObject battleLine = Instantiate(battleLinePrototype, GameObject.Find("BattleField").transform);
		//TODO
		battleLine.transform.position = new Vector3(0, idx * 466 - 700, 0);
		battleLineControllers[idx] = battleLine.GetComponent<BattleLineController>();
		battleLineControllers[idx].lineIdx = idx;
		return battleLineControllers[idx];
	}

	public IHandicapController InstantiateHandicap(int ownership)
	{
		//set position
		GameObject handicap = Instantiate(handicapPrototype, GetHandicapPosition(ownership), new Quaternion());
		handicap.transform.SetParent(GameObject.Find("RedemptionZone").transform);
		handicapController[ownership] = handicap.GetComponent<HandicapController>();
		return handicapController[ownership];
	}

	public ICardStackController InstantiateCardStack(int ownership)
	{
		GameObject stack = Instantiate(cardStackPrototype, GetCardStackPosition(ownership), Quaternion.Euler(0, 0, 90));
		stack.transform.SetParent(GameObject.Find("UI/Stacks").transform);//TODO 层级关系可能会改
		cardStackController[ownership] = stack.GetComponent<CardStackController>();
		return cardStackController[ownership];
	}






	public IUnitElementController InstantiateUnitInBattleField(int ownership, int lineIdx, int pos)
	{
		Quaternion initRotation = Quaternion.Euler(new Vector3(0, 0, ownership * 180));

		GameObject unit = Instantiate(unitPrototype, battleLineControllers[lineIdx].GetLogicPosition(pos), initRotation);

		return unit.GetComponent<UnitElementController>();
	}
	/// <summary>
	/// 在牌堆位置生成单位并返回句柄（通常用于Summon的动画）
	/// </summary>
	/// <param name="turn"></param>
	/// <returns></returns>
	public IUnitElementController InstantiateUnitInStack(int turn)
	{
		Quaternion initRotation = Quaternion.Euler(new Vector3(0, 0, turn * 180));

		GameObject unit = Instantiate(unitPrototype, transform.position - new Vector3(100, 0, 0), initRotation);

		return unit.GetComponent<UnitElementController>();
	}




	float fieldLowerBound = 220f;
	float fieldUpperBound = 1970f;
	float lineInterval = 66f;
	float lineWidth = 400f;

	private Vector2 GetHandicapPosition(int ownership)
	{
		float handicapOffsetY = 1200f;
		return new Vector2(0, (ownership * 2 - 1) * handicapOffsetY);
	}
	private Vector2 GetCardStackPosition(int ownership)
	{
		float cardStackOffsetX = -1700f;
		float cardStackOffsetY = 400f;
		return new Vector2(cardStackOffsetX, (ownership * 2 - 1) * cardStackOffsetY);
	}
	public int GetBattleLineIdx(float y)
	{
		if(y > BattleLineController.fieldLowerBound && y < BattleLineController.fieldUpperBound)
		{
			int idx = (int)((y - 180) / (BattleLineController.lineWidth + BattleLineController.lineInterval));
			if(idx < 0 || idx > fieldCapacity - 1)
			{
				return -1;
			}
			return idx;
		}
		return -1;
	}







	public void BattleFailed()
	{
		StopAllCoroutines();
		StopCoroutine(AIBehavior());
		SettleText.text = "Failure";
		SettleText.gameObject.SetActive(true);
		float duration = 0.4f;
		rotateSequence.Append(
		Settler.transform.DOMove(new Vector3(0, 0, 0), duration)
			.OnComplete(() =>
			{
				SettleText.DOFade(1f, duration);
			})
		);
	}

	public void BattleWinned()
	{
		StopAllCoroutines();
		StopCoroutine(AIBehavior());
		SettleText.text = "Victory";
		SettleText.gameObject.SetActive(true);
		float duration = 0.4f;
		rotateSequence.Append(
		Settler.transform.DOMove(new Vector3(0, 0, 0), duration)
			.OnComplete(() =>
			{
				SettleText.DOFade(1f, duration);
			})
		);
	}
	public void BattleOverChecked()
	{
		DOTween.Clear();
		Settler.transform.position = new Vector3(0, 2160, 0);
		if (battleSystem.BattleOverChecked())
		{
			gameManager.BattleBGM.Stop();
			gameManager.TacticalBGM.Play();

			AsyncOperation async = gameManager.UpdateGameState(SceneState.GameState.Tactical);
		}

		//StartCoroutine(LateWriteBack(async));
	}
	//IEnumerator LateWriteBack(AsyncOperation async)
	//{
	//	while (!async.isDone)
	//	{
	//		yield return null;
	//	}
	//	battleSystem.BattleOverChecked();
	//}


	public void Exit()
	{
		battleSystem.Exit();
	}
	/// <summary>
	/// 敌我双方公用的输入 结束回合
	/// </summary>
	public void Skip()
	{
		//分配动画队列
		InputLocked?.Invoke();
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();
		battleSystem.Skip();
	}
	/// <summary>
	/// 仅限玩家的输入层部署
	/// </summary>
	/// <param name="position"></param>
	/// <param name="handicapIdx"></param>
	/// <returns></returns>
	public int PlayerDeploy(Vector2 position, int handicapIdx)
	{
		int idx = GetBattleLineIdx(position.y);
		if(idx != 0)
		{
			return -1;
		}
		if (energy[0] < handicapController[0][handicapIdx].cost)
		{
			humanEnergy.transform.DOShakePosition(0.3f, 30f);
			return -1;
		}
		//如果容量不足
		int pos = battleLineControllers[idx].GetDeployPos(position.x);
		if (pos < 0) return -1;
		//---解析输入---


		InputLocked?.Invoke();
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		UnitElementController controller = handicapController[0].Pop(handicapIdx) as UnitElementController;

		//data input 显示层检查完了再动数据层！！！
		battleSystem.Deploy(handicapIdx, idx, pos);

		return 1;
	}
	public int PlayerCast(Vector2 position, int handicapIdx)
	{
		int idx = GetBattleLineIdx(position.y);
		if (idx < 0)
		{
			return -1;
		}
		if (energy[0] < handicapController[0][handicapIdx].cost)
		{
			humanEnergy.transform.DOShakePosition(0.3f, 30f);
			return -1;
		}
		int pos = battleLineControllers[idx].GetCastPos(position.x);
		string type = (handicapController[0][handicapIdx] as CommandElementController).type;
		if(pos < 0 && type == "Target") return -1;

		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		CommandElementController controller = handicapController[0].Pop(handicapIdx) as CommandElementController;

		if (type == "Target") Debug.Log(controller.nameContent + " Cast on : " + idx + " " + pos + battleLineControllers[idx][pos].nameContent);
		battleSystem.Cast(handicapIdx, idx, pos);

		return 1;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="position"></param>
	/// <param name="resLine"></param>
	/// <param name="element"></param>
	/// <returns></returns>
	public int PlayerMove(Vector2 position, BattleLineController resLine, UnitElementController element)
	{
		int dstLineIdx = GetBattleLineIdx(position.y);
		if (dstLineIdx < 0) return -1;
		int resLineIdx = resLine.lineIdx;
		int resIdx = element.resIdx;
		int dstPos = battleLineControllers[dstLineIdx].GetDeployPos(position.x);
		if(dstPos < 0) return -1;

		if (dstLineIdx == resLineIdx) return -1;
		if(Math.Abs(dstLineIdx - resLineIdx) > element.moveRange) return -1;


		if (battleLineControllers[dstLineIdx].ownerShip != element.ownership && battleLineControllers[dstLineIdx].count > 0)
		{
			return -1;
		}
		//解析成功


		InputLocked?.Invoke();
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);

		return 1;
	}
	public int PlayerRetreat(Vector2 position, BattleLineController resLine, UnitElementController element)
	{
		if(resLine.lineIdx != 0)
		{
			return -1;
		}
		//TODO
		if(position.x > 500)
		{
			return -1;
		}

		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		battleSystem.Retreat(resLine.lineIdx, element.resIdx);

		return 1;
	}










	//行为树
	HandicapController AIHandicap;
	BattleLineController AISupportLine;
	BattleLineController AIAdjacentLine;
	// string[] deployEffectUnits = new string[5] { "mush_04", "mush_09", "mush_10", "mush_11", "mush_13"};

	IEnumerator AIBehavior()
	{
        float waitTime = 1f;
        yield return new WaitForSeconds(waitTime);
        AIHandicap = handicapController[1];

		int AISupportLineIdx = fieldCapacity - 1;
        AISupportLine = battleLineControllers[AISupportLineIdx];

        int frontLineIdx = GetFrontLineIdx();
		AIAdjacentLine = battleLineControllers[frontLineIdx + 1];


		int whileCounter = 30;
	startwhile:
		whileCounter--;
        Debug.Log(whileCounter);
        while (energy[Turn] > 2 && whileCounter != 0)
		{
            // 优先使用“蔓延”，补充手牌
            if (TryCast("comm_mush_07"))
			{
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

			// 撤退一些带有部署效果的卡片
			if (TryRetreatSomeUnits(AISupportLineIdx))
			{
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 调整战线
            if (TryAdjustFoward(frontLineIdx))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

			// 部署低费卡
			if (TryDeployLowCostUnit(AISupportLineIdx))
			{
				yield return new WaitForSeconds(sequenceTime + waitTime);
				goto startwhile;
			}

			// 菇军奋战，铺场
            if (TryCast("comm_mush_01"))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 散播孢子，扩大场面
            if (TryCastComm13(AIAdjacentLine))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 增殖，赚卡
            if (TryCast("comm_mush_08"))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

            // 腐蚀，攻击血量高的单位
            if (TryCastComm18(frontLineIdx))
            {
                yield return new WaitForSeconds(sequenceTime + waitTime);
                goto startwhile;
            }

			break;
		}

		yield return new WaitForSeconds(waitTime);
		Skip();
	}

	private bool TryCast(string cardID)
	{
		for (int i = 0; i < AIHandicap.count; i++)
		{
			if (AIHandicap[i].ID == cardID && energy[Turn] >= AIHandicap[i].cost)
			{
				AICast(i, 0, 0);
				Debug.Log($"Cast '{cardID}'");
				return true;
			}
		}
		return false;
	}

    private bool TryCastComm18(int frontLineIdx)
	{
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].ID == "comm_mush_18" && energy[Turn] >= 3)
            {

                int dstLineIdx = 0;
                int dstPos = 0;
                int maxHealth = 0;
                Tuple<int, int> lineMaxHealth = Tuple.Create(0, 0);
                for (int j = 0; j < frontLineIdx + 1; j++)
                {
                    lineMaxHealth = GetMaxHealth(j);
                    if (maxHealth > lineMaxHealth.Item1)
                    {
                        dstLineIdx = j;
                        dstPos = lineMaxHealth.Item2;
                    }
                }
                AICast(i, dstLineIdx, dstPos);
                Debug.Log("Cast 'comm_mush_18'");
                return true;
            }
        }
		return false;
    }

	private bool TryCastComm13(BattleLineController AIAdjacentLine)
	{
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].ID == "comm_mush_13" && energy[Turn] >= 6 && AIAdjacentLine.count < AIAdjacentLine.count)
            {
                AICast(i, 0, 0);
                Debug.Log("Cast 'comm_mush_13'");
                return true;
            }
        }
		return false;
    }

    private bool TryAdjustFoward(int frontLineIdx)
	{
        for (int i = fieldCapacity - 1; i > frontLineIdx + 1; i--)
        {
            BattleLineController battleLine = battleLineControllers[i];

            // 前一条线有空位则尝试前移
            if (GetIsLineAvailable(i - 1))
            {
				int artilleryIdx = -1;

				// 遍历该战线
                for (int j = 0; j < battleLine.count; j++)
                {
					// 判断规则上能移动的卡（轰击boss || 建筑 || 移动次数为0）
                    if (battleLine[j].ID != "mush_102" && battleLine[j].category != "Construction" && battleLine[j].operateCounter == 1)
					{
						// 若该战线有空，则不会移动轰击
                        if (GetIsLineAvailable(i))
						{
                            if (battleLine[j].category != "Artillery")
							{
                                AIMove(i, j, i - 1, 0);
                                Debug.Log($"AIMove({i}, {j}, {i - 1}, 0)");
                                return true;
                            }
							else
							{
								artilleryIdx = j;
							}
                        }

						//TODO 若战线没有空，则可移动轰击，但移动轰击的优先级最低
						else
						{
                            AIMove(i, j, i - 1, 0);
                            Debug.Log($"AIMove({i}, {j}, {i - 1}, 0)");
                            return true;
                        }
					}
                }
            }
        }
		return false;
    }

	private bool TryRetreatSomeUnits(int AISupportLineIdx)
	{
        BattleLineController battleLine = battleLineControllers[AISupportLineIdx];
        if (battleLine.count == battleLine.capacity)
		{
			for (int i = 0; i < battleLine.count; i++)
			{
				if (battleLine[i].operateCounter == 1)
                {
                    if (battleLine[i].ID == "mush_04" || battleLine[i].ID == "mush_09" || battleLine[i].ID == "mush_10" || battleLine[i].ID == "mush_11" || battleLine[i].ID == "mush_13")
                    {
                        AIRetreat(AISupportLineIdx, i);
                        return true;
                    }
                }
			}
		}
		return false;
	}

	private bool TryDeployLowCostUnit(int AISupportLineIdx)
	{
        if (AISupportLine.count < AISupportLine.capacity)
        {
            // 若支援战线的建筑数小于战线容量减二，则建筑也可部署
            if (GetConstructionNum(AISupportLineIdx) < AISupportLine.capacity - 2)
            {
                int idx = GetMinCostUnitPointer();
                if (idx >= 0)
                {
                    AIDeploy(idx); 
					Debug.Log("Deploy");
                    return true;
                }
            }
            // 若支援战线的建筑数大于等于战线容量减二，则不部署建筑
            else
            {
                int idx = GetMinCostUnitPointerExcConstr();
                if (idx >= 0)
                {
                    AIDeploy(idx);
                    Debug.Log("Deploy");
                    return true;
                }
            }
        }
		return false;
    }

    /*IEnumerator AIBehaviourNode3()
	{
        HandicapController handicap = handicapController[1];

        float waitTime = 0.9f;
        yield return new WaitForSeconds(waitTime);

        int movetimes = 1;
		int frontLineIdx = GetFrontLineIdx();

        // 移动策略：调整各蘑人站位，血量少的往前推，血量多的往后撤，使回合结束增殖时收益最大
		// 从支援战线开始遍历一次，调整站位
        for (int i = battleLineControllers.Length - 1; i > frontLineIdx; i--)
        {
            int halfCapacity = battleLineControllers[i].capacity / 2;

            // 单位数大于容量的一半时，将本战线血量低的单位往前推
            while (battleLineControllers[i].count > halfCapacity && i > frontLineIdx + 1)
            {
                Tuple<int, int> minHealthInfo = GetAvailableMinHealth(i);
                int minHealthPointer = minHealthInfo.Item2;

				// 若存在可操作对象，则执行操作
				if (minHealthPointer > -1)
				{
                    AIMove(i, minHealthPointer, i + 1, 0);
                    movetimes++;
                }
            }

            // 当单位数小于或等于容量的一半减一，且前一条战线单位数大于容量一半时，将前一条战线血量高的往后撤
            while (battleLineControllers[i].count <= halfCapacity - 1 && battleLineControllers[i - 1].count > battleLineControllers[i - 1].capacity / 2 && i > frontLineIdx + 1)
            {
                Tuple<int, int> maxHealthInfo = GetAvailableMaxHealth(i - 1);
                int maxHealthPointer = maxHealthInfo.Item2;

				if(maxHealthPointer > -1)
				{
                    AIMove(i - 1, maxHealthPointer, i, 0);
                    movetimes++;
                }
            }
        }
		// 反向遍历一次，使站位更合理
		for (int i = frontLineIdx + 1; i < battleLineControllers.Length; i++)
		{
			int halfCapacity = battleLineControllers[i].capacity / 2;

			// 当单位数大于容量一半，且后一条战线单位数小于容量一半时，将本战线血量高的往后撤
			while (battleLineControllers[i].count > halfCapacity && battleLineControllers[i + 1].count <= battleLineControllers[i + 1].capacity / 2 && i < fieldCapacity - 1)
            {
				Tuple<int, int> maxHealthInfo = GetAvailableMaxHealth(i);
				int maxHealthPointer = maxHealthInfo.Item2;

				if (maxHealthPointer > -1)
				{
                    AIMove(i, maxHealthPointer, i + 1, 0);
                    movetimes++;
                }
			}

			// 当单位数小于或等于容量一半加一,且后一条战线单位数大于或等于容量一半时，将后一条战线血量低的往前推
			while (battleLineControllers[i].count <= halfCapacity + 1 && battleLineControllers[i + 1].count > battleLineControllers[i + 1].capacity / 2 && i < fieldCapacity - 1)
			{
				Tuple<int, int> minHealthInfo = GetAvailableMinHealth(i + 1);
				int minHealthPointer = minHealthInfo.Item2;

				if(minHealthPointer > -1)
				{
                    AIMove(i + 1, minHealthPointer, i, 0);
                    movetimes++;
                }
			}
		}

		// 指令卡策略：费用够就出
		while (energy[Turn] > 3 && handicap.count > 0)
		{
			AICast(0, 0, 0);
		}

		Skip();
    }*/

	/// <summary>
	/// 获取某条战线的建筑数量
	/// </summary>
	/// <param name="idx">战线索引</param>
	/// <returns></returns>
	private int GetConstructionNum(int idx)
	{
		int num = 0;
		BattleLineController battleLine = battleLineControllers[idx];
		for (int i = 0; i < battleLine.count; i++)
		{
			if (battleLine[i].category == "Construction")
			{
				num++;
			}
		}
		return num;
	}


    private bool GetIsLineAvailable(int idx)
	{
		return battleLineControllers[idx].count < battleLineControllers[idx].capacity;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <returns>返回可操作战线的索引-1</returns>
    private int GetFrontLineIdx()
	{
		int idx = fieldCapacity - 1;
		while (battleLineControllers[idx].ownerShip == 1 || battleLineControllers[idx].count == 0)
		{
			idx--;
		}
		return idx;
	}

    /// <summary>
    /// 获取某条战线上可操作的单位中生命值最小的单位的生命值和索引
    /// </summary>
    /// <returns>返回一个元组，包含生命值最小单位的生命值和索引</returns>
    private Tuple<int, int> GetAvailableMinHealth(int battleLineIdx)
    {
        BattleLineController battleLine = battleLineControllers[battleLineIdx];
        int minHealth = 100;
        int minHealthPointer = -1;
        for (int i = 0; i < battleLine.count; i++)
        {
            if (battleLine[i].healthPoint < minHealth && battleLine[i].operateCounter == 1)
            {
                minHealth = battleLine[i].healthPoint;
                minHealthPointer = i;
            }
        }
        return Tuple.Create(minHealth, minHealthPointer);
    }

    private Tuple<int, int> GetAvailableMaxHealth(int battleLineIdx)
    {
        BattleLineController battleLine = battleLineControllers[battleLineIdx];
        int maxHealth = 0;
        int maxHealthPointer = -1;
        for (int i = 0; i < battleLine.count; i++)
        {
            if (battleLine[i].healthPoint > maxHealth && battleLine[i].operateCounter == 1)
            {
                maxHealth = battleLine[i].healthPoint;
                maxHealthPointer = i;
            }
        }
        return Tuple.Create(maxHealth, maxHealthPointer);
    }

    private Tuple<int, int> GetMaxHealth(int battleLineIdx)
    {
        BattleLineController battleLine = battleLineControllers[battleLineIdx];
        int maxHealth = 0;
        int maxHealthPointer = -1;
        for (int i = 0; i < battleLine.count; i++)
        {
            if (battleLine[i].healthPoint > maxHealth)
            {
                maxHealth = battleLine[i].healthPoint;
                maxHealthPointer = i;
            }
        }
        return Tuple.Create(maxHealth, maxHealthPointer);
    }

    private int GetMaxCost()
	{
		int maxCost = 0;
		int maxPointer = -1;
		for(int i = 0; i < AIHandicap.count; i++)
		{
			if (AIHandicap[i].cost > maxCost)
			{
				maxCost = AIHandicap[i].cost;
				maxPointer = i;
			}
		}
		return maxCost;
	}
	private int GetMinCost()
	{
		int minCost = 100;
		int minPointer = -1;
		for(int i = 0; i < AIHandicap.count; i++)
		{
			if (AIHandicap[i].cost < minCost)
			{
				minCost = AIHandicap[i].cost;
				minPointer = i;
			}
		}
		return minCost;
	}
	private int GetMaxCostUnitPointer()
	{
		int maxCost = 0;
		int maxPointer = -1;
		for (int i = 0; i < AIHandicap.count; i++)
		{
			if (AIHandicap[i].cost >= maxCost && AIHandicap[i] is UnitElementController)
			{
				maxCost = AIHandicap[i].cost;
				maxPointer = i;
			}
		}
		if (AIHandicap[maxPointer].cost > energy[1])
		{
			return -1;
		}
		return maxPointer;
	}
	/// <summary>
	/// 只有在费用不足时才会返回-1
	/// </summary>
	/// <param name="lowerBound">下界，返回的索引对应卡费用不会低于这个值</param>
	/// <returns></returns>
	private int GetMinCostUnitPointer(int lowerBound = 0)
	{
		int minCost = 10000;
		int minPointer = -1;
		for (int i = 0; i < AIHandicap.count; i++)
		{
			if (lowerBound < AIHandicap[i].cost && AIHandicap[i].cost < minCost && AIHandicap[i] is UnitElementController)
			{
				minCost = AIHandicap[i].cost;
				minPointer = i;
			}
		}
		if(minPointer == -1)
		{
			return -1;
		}
		if (AIHandicap[minPointer].cost > energy[1])
		{
			return -1;
		}
		return minPointer;
	}
	private int GetMinCostUnitPointerExcConstr()
	{
        int minCost = 10000;
        int minPointer = -1;
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].category != "Construction" && AIHandicap[i].cost < minCost && AIHandicap[i] is UnitElementController)
            {
                minCost = AIHandicap[i].cost;
                minPointer = i;
            }
        }
        if (minPointer == -1)
        {
            return -1;
        }
        if (AIHandicap[minPointer].cost > energy[1])
        {
            return -1;
        }
        return minPointer;
    }
	/// <summary>
	/// 若没有合适的操作目标返回-1
	/// </summary>
	/// <returns></returns>
	private int GetOperatorPointerInSupportLine()
	{
		for (int i = 0; i < AISupportLine.count; i++)
		{
			if (AISupportLine[i].operateCounter == 1 && (AISupportLine[i].category != "Construction" && AISupportLine[i].category != "Artillery"))
			{
				return i;
			}
		}
		return -1;
	}
	//private int GetArtilleryUnitPointer()
	//{

	//}
	//private int GetConstructionUnitPointer()
	//{

	//}
	public void AIDeploy(int handicapIdx)
	{
		int lineidx = 3;//supportline
		UnitElementController controller = AIHandicap.Pop(handicapIdx) as UnitElementController;


		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();


		//data input 显示层检查完了再动数据层！！！
		battleSystem.Deploy(handicapIdx, lineidx, 0);
	}
	public void AICast(int handicapIdx, int dstLineIdx, int dstPos)
	{
		UnitElementController controller = AIHandicap.Pop(handicapIdx) as UnitElementController;


		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		battleSystem.Cast(handicapIdx, dstLineIdx, dstPos);
	}

	public void AISkip()
	{
		Skip();
	}
    
	private void AIMove(int resLineIdx, int resIdx, int dstLineIdx, int dstPos)
    {
        rotateSequence.Kill();
        rotateSequence = DOTween.Sequence();

        battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);
    }

	private void AIRetreat(int resLineIdx, int resPos)
	{
		BattleLineController battleLine = battleLineControllers[resLineIdx];
		if(resLineIdx == fieldCapacity - 1 && battleLine[resPos].operateCounter == 1)
		{
            rotateSequence.Kill();
            rotateSequence = DOTween.Sequence();
            Debug.Log($"Retreat第{resPos}位{battleLine[resPos].ID}");
            battleSystem.Retreat(resLineIdx, resPos);
		}
	}


	// 行为树节点类
	public abstract class BTNode
	{
		public abstract bool Execute();
	}

	public class Selector : BTNode
	{
		private List<BTNode> children;

		public Selector(List<BTNode> children)
		{
			this.children = children;
		}

		public override bool Execute()
		{
			foreach (BTNode child in children)
			{
				if (child.Execute())
				{
					return true;
				}
			}
			return false;
		}
	}
}
