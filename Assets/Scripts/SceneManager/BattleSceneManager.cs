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
using BehaviorTree;
using UnityEngine.SceneManagement;

public enum Result
{
	win,
	fail,
	normal
}
public class BattleSceneManager : MonoBehaviour,
	IBattleSceneController
{
	public static event Action InputLocked;
	public static event Action InputUnlocked;
	/// <summary>
	/// GameManager单例
	/// </summary>
	public GameManager gameManager;

	public bool tutorial
	{
		get => gameManager.config.tutorial;
	}

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

	public GameObject rewardPrototype;

	[Header("SubControllers")]
	public BattleLineController[] battleLines;

	public CardStackController[] cardStackController;

	public HandicapController[] handicapController;

	public TurnMappedDialogger dialogController;
	public TMP_Text[] energyText;
	public TMP_Text[] energySupplyText;

	[Header("Data")]
	/// <summary>
	/// 渲染层TURN备份
	/// </summary>
	public static int Turn;
	/// <summary>
	/// dialog trigger & counter
	/// </summary>
	public event Action<int> TurnChanged;
	private int TurnNum;
	public int turnNum
	{
		get => TurnNum;
		set
		{
			TurnNum = value;
			TurnChanged?.Invoke(value);
		}
	}

	public int[] energy;
	public int[] energySupply;

	public int fieldCapacity;
	public UnitElementController[] bases;
	//TODO
	[Header("Display")]
	public GameObject FrontLine;
	public GameObject humanEnergy;
	public GameObject[] humanSlots;
	public GameObject[] plantSlots;

	public GameObject[] turnText;
	public Vector3 turnTextPosition;

	[Header("Components")]
	public BaseInfoDisplay baseDisplay;
	public EnergyDisplay energyDisplay;

	[Header("Interaction")]
	public Button skipButton;
	public Image SkipbuttonImage;

	public CommandElementController castingCommand;

	public Button exitButton;
	public Button rewardConfirmButton;
	public bool completed;

	public int rewardSelectionIndex;
	public RewardSelection[] rewards;

	[Header("Sequence")]
	public Sequence rotateSequence;
	public float sequenceTime;

	//结算锁
	public static bool settlement = false;

	// 行为树
	public BTBattleNode btBattleNode;

	public Result result;

    public void Start()
	{
		Settler.gameObject.transform.position = new Vector3(0, 2160, 0);
		SettleButton.onClick.AddListener(BattleOverChecked);
    }
	public GameObject dialog;
	public GameObject guide;
	public void FieldInitialize(IBattleSystemInput handler, int fieldCapacity, int BTindex)
	{
		gameManager = GameManager.GetInstance();
		battleSystem = handler;

		result = Result.normal;

		if (!gameManager.config.tutorial)
		{
			dialog.SetActive(false);
			guide.SetActive(false);
		}
		switch (BTindex)
		{
			case 0: btBattleNode = new BTBattleNode0(); break;
            case 1: btBattleNode = new BTBattleNode1(); break;
			case 2: btBattleNode = new BTBattleNode2(); break;
			case 3:	btBattleNode = new BTBattleNode3();	break;
			case 100: 
				btBattleNode = new BTBattleNode100();
                dialogController = transform.GetComponent<TurnMappedDialogger>();
                dialogController.StartTutorial();
                break;
		}


        //TODO config
        //data
        this.fieldCapacity = fieldCapacity;
		battleLines = new BattleLineController[fieldCapacity];

		energy = new int[2];
		energySupply = new int[2];

		cardStackController = new CardStackController[2];
		handicapController = new HandicapController[2];

		bases = new UnitElementController[2];

		skipButton.onClick.AddListener(Skip);
		SkipbuttonImage = skipButton.gameObject.GetComponent<Image>();

		exitButton.onClick.AddListener(Exit);
		rewardConfirmButton.onClick.AddListener(ContinueExpedition);

		rewardConfirmButton.interactable = false;
		
		rewards = new RewardSelection[3];
		//TODO trigger
		turnNum = 0;

		for (int i = 0; i < 5; i++)
		{
			humanSlots[i].SetActive(false);
			plantSlots[i].SetActive(false);
		}

		InputLocked?.Invoke();
		AcquireSequence();

		FrontLine.transform.position = 
			new Vector3(0, -900 + BattleLineController.lineWidth + BattleLineController.lineInterval / 2, 0);
	}
	public void InitBases(IUnitElementController humanBase, IUnitElementController plantBase)
	{
		bases[0] = humanBase as UnitElementController;
		bases[1] = plantBase as UnitElementController;

		baseDisplay.BaseInfoInitialize(bases[0].color, bases[0].CardImage, bases[0].categoryIcon, bases[0].description);
	}
	public void UpdateBaseHealth(int health)
	{
		baseDisplay.UpdateBaseHealth(health, bases[0].maxHealthPoint);
	}

	/// <summary>
	/// 
	/// </summary>
	public void UpdateTurnWithSettlement()
	{
		//结算攻击动画
		rotateSequence.InsertCallback(sequenceTime, () =>
		{
			InputUnlocked?.Invoke();
			inputLock = false;
			UpdateTurn();
		});
	}
	public void Settlement()
	{
		rotateSequence.InsertCallback(sequenceTime, () =>
		{
			InputUnlocked?.Invoke();
			inputLock = false;
		});
	}
	public void UpdateTurn(int TURN)
	{

		Turn = TURN;
		turnNum++;

        TurnUpdateAnimation(TURN);
		
		if (Turn == 0)
		{
		}
		//如果是敌方回合，启动行为树
		else
		{
            StartCoroutine(btBattleNode.BehaviorTree());
        }
	}
	private void UpdateTurn()
	{
		Turn = (Turn + 1) % 2;
		turnNum++;

		TurnUpdateAnimation(Turn);

		if (Turn == 0)
		{
		}
		//如果是敌方回合，启动行为树
		else
		{
            StartCoroutine(btBattleNode.BehaviorTree());
        }
    }


	public bool updatingTurn;
	public void TurnUpdateAnimation(int TURN)
	{
		float duration = 0.5f;
		float waitTime = 1f;

		inputLock = true;
		updatingTurn = true;
		skipButton.interactable = false;

		Sequence seq = DOTween.Sequence();

		int temp = TURN;
		seq.Append(turnText[temp].transform.DOBlendableMoveBy(new Vector3(3000, 0, 0), duration));
		seq.AppendInterval(waitTime);
		seq.Append(turnText[temp].transform.DOBlendableMoveBy(new Vector3(3000, 0, 0), duration / 2)
			.OnComplete(() =>
			{
				inputLock = false;
				updatingTurn = false;
				turnText[temp].transform.position = turnTextPosition;
				skipButton.interactable = temp == 0;
			}));
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
		for (int i = 0; i < this.energySupply[1]; i++)
		{
			plantSlots[i].SetActive(true);
		}
	}
	public float shiftTime;
	public void UpdateFrontLine(int index)
	{
		shiftTime = 0.4f;
		//TODO
		Vector3 dstPos = new Vector3(0, -900 + BattleLineController.lineWidth + BattleLineController.lineInterval / 2
			+ index * (BattleLineController.lineWidth + BattleLineController.lineInterval), 0);
		FrontLine.transform.DOMove(dstPos, shiftTime);
	}
	public void UpdateAllBattleLine()
	{
		foreach (BattleLineController line in battleLines)
		{
			line.UpdateElementPosition();
		}
	}










	public IBattleLineController InstantiateBattleLine(int idx, int capacity)
	{
		GameObject battleLine = Instantiate(battleLinePrototype, GameObject.Find("BattleField").transform);
		//TODO
		battleLine.transform.position = new Vector3(0, idx * 466 - 700, 0);
		battleLines[idx] = battleLine.GetComponent<BattleLineController>();
		battleLines[idx].index = idx;
		return battleLines[idx];
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

	/// <summary>
	/// 
	/// </summary>
	/// <param name="ownership"></param>
	/// <param name="lineIdx"></param>
	/// <param name="pos"></param>
	/// <returns></returns>
	public IUnitElementController InstantiateUnitInBattleField(int ownership, int lineIdx, int pos)
	{
		Quaternion initRotation = Quaternion.Euler(new Vector3(0, 0, ownership * 180));

		GameObject unit = Instantiate(unitPrototype, battleLines[lineIdx].GetLogicPosition(pos), initRotation);

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
		if (y > BattleLineController.fieldLowerBound && y < BattleLineController.fieldUpperBound)
		{
			int idx = (int)((y - 180) / (BattleLineController.lineWidth + BattleLineController.lineInterval));
			if (idx < 0 || idx > fieldCapacity - 1)
			{
				return -1;
			}
			return idx;
		}
		return -1;
	}







	public void Surrender()
	{


		StopAllCoroutines();
		SettleText.text = "Failure";
		SettleText.gameObject.SetActive(true);
		float duration = 0.4f;
		Settler.transform.DOMove(new Vector3(0, 0, 0), duration)
			.OnComplete(() =>
			{
				SettleText.DOFade(1f, duration);
			});
		completed = true;
		result = Result.fail;
	}
	public void BattleFailed()
	{
		if (gameManager.config.tutorial)
		{
			gameManager.config.tutorial = false;
			string modifiedConfig = JsonUtility.ToJson(gameManager.config, true);
			//TODO
		}

		StopAllCoroutines();
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
		completed = true;
		result = Result.fail;
	}
	public void BattleWinned()
	{
		StopAllCoroutines();

		SettleText.text = "Victory";

		SettleText.gameObject.SetActive(true);
		float duration = 0.4f;
		rotateSequence.Append(
		Settler.transform.DOMove(new Vector3(0, 0, 0), duration)
			.OnComplete(() =>
			{
				SettleText.DOFade(1f, duration);
				//rewardText TODO
			})
		);
		completed = false;
		result = Result.win;
	}
	public void BattleOverChecked()
	{
		if (completed)
		{
			BattleElementController[] otherInstances = UnityEngine.Object.FindObjectsOfType<BattleElementController>();

			// 遍历并销毁除自身以外的同类型游戏对象
			foreach (BattleElementController instance in otherInstances)
			{
				Destroy(instance.gameObject);
			}
			DOTween.Clear();
			Settler.transform.position = new Vector3(0, 2160, 0);
			if (battleSystem.BattleOverChecked())
			{
				gameManager.BattleBGM.Stop();
				gameManager.TacticalBGM.Play();

				if(result == Result.win)
				{
					AsyncOperation async = gameManager.UpdateGameState(SceneState.GameState.Tactical);
				}
				else
				{
					gameManager.UpdateGameState(SceneState.GameState.Start);
				}
			}
		}
		else
		{
			SettleButton.enabled = false;
			battleSystem.GetReward();
		}
	}
	public void ContinueExpedition()
	{
		battleSystem.AddReward(rewardSelectionIndex);

		completed = true;
		BattleOverChecked();
	}
	public float duration;
	//TEST
	public void InstantiateReward(string[] IDs, string[] names, string[] categories, int[] cost, int[] attacks, int[] healths, int[] counters, string[] description)
	{
		duration = 0.5f;

		for (int i = 0; i < 3; i++)
		{
			GameObject reward = Instantiate(rewardPrototype, new Vector3(2500, 0, 0), Quaternion.Euler(new Vector3(0, 90, 0)));
			rewards[i] = reward.GetComponent<RewardSelection>();
			rewards[i].transform.SetParent(transform);
			rewards[i].index = i;
			rewards[i].transform.DOBlendableMoveBy(new Vector3(-3500 + i * 1000, 0, 0), duration);
			rewards[i].transform.DOBlendableRotateBy(new Vector3(0, -90, 0), duration);
			rewards[i].SetInfo(IDs[i], names[i], categories[i], cost[i], attacks[i], healths[i], counters[i], description[i]);
		}
		rewardConfirmButton.transform.DOMove(new Vector3(0, -800, 0), duration);

	}
	//interface
	public void ClearOtherSelectionFrame()
	{
		foreach (RewardSelection reward in rewards)
		{
			if (reward.index != rewardSelectionIndex)
			{
				reward.Frame.SetActive(false);
				reward.disableExit = false;
				reward.transform.DOScale(reward.originScale, reward.duration);
			}
		}
	}
	





	public void AcquireSequence()
	{
		if (rotateSequence != null && rotateSequence.active) { Debug.LogWarning("正在杀死结算中的序列"); }
		sequenceTime = 0;
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();
		//不允许新申请队列
		inputLock = true;
		skipButton.enabled = false;
		rotateSequence.OnComplete(() =>
		{
			//允许新申请队列
			inputLock = false;
			skipButton.enabled = Turn == 0;
		});
	}








	public void Exit()
	{
		if(skipButton.interactable)
		{
			battleSystem.Exit();
		}
	}
	/// <summary>
	/// 敌我双方公用的输入 结束回合
	/// </summary>
	public void Skip()
	{
		AcquireSequence();

		battleSystem.Skip();
	}






	public int PlayerDeploy(int handicapIdx, int lineIdx, int pos)
	{
		Deploy?.Invoke(handicapController[0][handicapIdx].ID);
		AcquireSequence();

		UnitElementController controller = handicapController[0].Pop(handicapIdx) as UnitElementController;

		battleSystem.Deploy(handicapIdx, lineIdx, pos);

		return 1;
	}
	public int PlayerMove(int resLineIdx, int resIdx, int dstLineIdx, int dstPos)
	{
		Move?.Invoke(battleLines[resLineIdx][resIdx].ID);
		AcquireSequence();

        battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);
        return 1;
	}

	public int PlayerRetreat(int resLineIdx, int resIdx)
	{
		Retreat?.Invoke(battleLines[resLineIdx][resIdx].ID);
		AcquireSequence();
		battleSystem.Retreat(resLineIdx, resIdx);

		return 1;
	}
	public void PlayerTargetCast(int handicapIdx, int dstLineIdx, int dstIdx)
	{
		Cast?.Invoke(castingCommand.ID);
		AcquireSequence();

		battleSystem.Cast(handicapIdx, dstLineIdx, dstIdx);
	}
	public void PlayerNonTargetCast(int handicapIdx)
	{
		Cast?.Invoke(handicapController[0][handicapIdx].ID);
		AcquireSequence();

        CommandElementController controller = handicapController[0].Pop(handicapIdx) as CommandElementController;

		battleSystem.Cast(handicapIdx);
	}


	public void DisableAllSelectionFrame()
	{
		foreach(BattleLineController line in battleLines)
		{
			line.lineDisplay.DisableSelectionFrame();
		}
	}




	//行为树
	HandicapController AIHandicap;
	BattleLineController AISupportLine;
	BattleLineController AIAdjacentLine;
	// string[] deployEffectUnits = new string[5] { "mush_04", "mush_09", "mush_10", "mush_11", "mush_13"};

	IEnumerator AIBehavior()
	{
        float waitTime = 1f;
        yield return new WaitForSeconds(waitTime * 3);
        AIHandicap = handicapController[1];

		int AISupportLineIdx = fieldCapacity - 1;
        AISupportLine = battleLines[AISupportLineIdx];

        int frontLineIdx = GetFrontLineIdx();
		AIAdjacentLine = battleLines[frontLineIdx + 1];

		int whileCounter = 30;

        while (whileCounter != 0)
		{
            Debug.Log(whileCounter--);

            // 优先使用“蔓延”，补充手牌
            if (TryCast("comm_mush_07"))
			{				
                yield return new WaitForSeconds(sequenceTime + waitTime);
                continue;
            }

			// 撤退一些带有部署效果的卡片
			if (TryRetreatUnits(AISupportLineIdx))
			{				
                yield return new WaitForSeconds(sequenceTime + waitTime);
                continue;
            }

            // 调整战线
            if (TryAdjustFoward(frontLineIdx))
            {				
                yield return new WaitForSeconds(sequenceTime + waitTime);
                continue;
            }

			// 部署低费卡
			if (TryDeployLowCostUnit(AISupportLineIdx))
			{				
				yield return new WaitForSeconds(sequenceTime + waitTime);
				continue;
			}

			// 菇军奋战，铺场
            if (TryCastComm01(AISupportLine))
            {				
                yield return new WaitForSeconds(sequenceTime + waitTime);
                continue;
            }

            // 散播孢子，扩大场面
            if (TryCastComm13(AIAdjacentLine))
            {
				
                yield return new WaitForSeconds(sequenceTime + waitTime);
                continue;
            }

            // 增殖，赚卡
            if (TryCast("comm_mush_08"))
            {				
                yield return new WaitForSeconds(sequenceTime + waitTime);
                continue;
            }

            // 腐蚀，攻击血量高的单位
            if (TryCastComm15(frontLineIdx))
            {				
                yield return new WaitForSeconds(sequenceTime + waitTime);
                continue;
            }

			break;
		}
        yield return new WaitForSeconds(waitTime * 1.5f);
        AISkip();        
    }

	private bool TryCast(string cardID)
	{
		for (int i = 0; i < AIHandicap.count; i++)
		{
			if (AIHandicap[i].ID == cardID && energy[Turn] >= AIHandicap[i].cost && AIHandicap[i] is CommandElementController)
			{
				AINoneTargetCast(i);
				Debug.Log($"Cast '{cardID}'");
				return true;
			}
		}
		return false;
	}

    private bool TryCastComm15(int frontLineIdx)
	{
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].ID == "comm_mush_15" && energy[Turn] >= AIHandicap[i].cost && AIHandicap[i] is CommandElementController)
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
                AITargetCast(i, dstLineIdx, dstPos);
                Debug.Log("Cast 'comm_mush_15'");
                return true;
            }
        }
		return false;
    }

    private bool TryCastComm01(BattleLineController AISupportLine)
	{
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].ID == "comm_mush_01" && energy[Turn] >= AIHandicap[i].cost && AISupportLine.count < AISupportLine.capacity - 1)
            {
                AINoneTargetCast(i);
                Debug.Log("Cast 'comm_mush_01'");
                return true;
            }
        }
        return false;
    }


    private bool TryCastComm13(BattleLineController AIAdjacentLine)
	{
        for (int i = 0; i < AIHandicap.count; i++)
        {
            if (AIHandicap[i].ID == "comm_mush_13" && energy[Turn] >= AIHandicap[i].cost && AIAdjacentLine.count < AIAdjacentLine.capacity)
            {
                AINoneTargetCast(i);
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
            BattleLineController battleLine = battleLines[i];

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

	private bool TryRetreatUnits(int AISupportLineIdx)
	{
        BattleLineController battleLine = battleLines[AISupportLineIdx];
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
                    return true;
                }
            }
        }
		return false;
    }

	/// <summary>
	/// 获取某条战线的建筑数量
	/// </summary>
	/// <param name="idx">战线索引</param>
	/// <returns></returns>
	private int GetConstructionNum(int idx)
	{
		int num = 0;
		BattleLineController battleLine = battleLines[idx];
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
		return battleLines[idx].count < battleLines[idx].capacity;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <returns>返回可操作战线的索引-1</returns>
    private int GetFrontLineIdx()
	{
		int idx = fieldCapacity - 1;
		while (battleLines[idx].ownership == 1 || battleLines[idx].count == 0)
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
        BattleLineController battleLine = battleLines[battleLineIdx];
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
        BattleLineController battleLine = battleLines[battleLineIdx];
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
        BattleLineController battleLine = battleLines[battleLineIdx];
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
	private int GetMinCostUnitPointer(int lowerBound = -1)
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

	//public void AICast(int handicapIdx, int dstLineIdx, int dstPos)
	//{
	//	UnitElementController controller = AIHandicap.Pop(handicapIdx) as UnitElementController;


	//	sequenceTime = 0;
	//	rotateSequence.Kill();
	//	rotateSequence = DOTween.Sequence();

	//	battleSystem.Cast(handicapIdx, dstLineIdx, dstPos);
	//}

	public void AISkip()
	{
		Skip();
	}

	public void AIMove(int resLineIdx, int resIdx, int dstLineIdx, int dstPos)
	{
        AcquireSequence();

        battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);
	}

	public void AIRetreat(int resLineIdx, int resPos)
	{
		BattleLineController battleLine = battleLines[resLineIdx];
		if(resLineIdx == fieldCapacity - 1 && battleLine[resPos].operateCounter == 1)
		{
            AcquireSequence();
            Debug.Log($"Retreat第{resPos}位{battleLine[resPos].ID}");
			battleSystem.Retreat(resLineIdx, resPos);
		}
	}
    public void AITargetCast(int handicapIdx, int dstLineIdx, int dstIdx)
    {
        AcquireSequence();

        CommandElementController controller = handicapController[1].Pop(handicapIdx) as CommandElementController;

        battleSystem.Cast(handicapIdx, dstLineIdx, dstIdx);
    }

    public void AINoneTargetCast(int handicapIdx)
    {
        AcquireSequence();

        CommandElementController controller = handicapController[1].Pop(handicapIdx) as CommandElementController;

        battleSystem.Cast(handicapIdx);
    }
    public void AIDeploy(int handicapIdx, int dstIdx = 3, int dstPos = 0)
    {
        AcquireSequence();

        UnitElementController controller = handicapController[1].Pop(handicapIdx) as UnitElementController;

        //data input 显示层检查完了再动数据层！！！
        battleSystem.Deploy(handicapIdx, dstIdx, dstPos);
    }






	public event Action<string> Retreat;
	public event Action<string> Deploy;
	public event Action<string> Cast;
	public event Action<string> Move;
}