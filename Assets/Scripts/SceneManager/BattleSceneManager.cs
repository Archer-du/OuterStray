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
		if (Turn == 0)
		{
			buttonImage.color = Color.white;
			skipButton.enabled = true;
		}
		//如果是敌方回合，启动行为树
		else
		{
			buttonImage.color = Color.gray;

			// StartCoroutine(AIBehavior());
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

			// StartCoroutine(AIBehavior());
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
		for (int i = 0; i < this.energySupply[1]; i++)
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
		if (idx != 0)
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
		sequenceTime = 0;
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
		if (pos < 0 && type == "Target") return -1;

		sequenceTime = 0;
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
		if (dstPos < 0) return -1;

		if (dstLineIdx == resLineIdx) return -1;
		if (Math.Abs(dstLineIdx - resLineIdx) > element.moveRange) return -1;


		if (battleLineControllers[dstLineIdx].ownerShip != element.ownership && battleLineControllers[dstLineIdx].count > 0)
		{
			return -1;
		}
		//解析成功


		InputLocked?.Invoke();
		sequenceTime = 0;
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);

		return 1;
	}
	public int PlayerRetreat(Vector2 position, BattleLineController resLine, UnitElementController element)
	{
		if (resLine.lineIdx != 0)
		{
			return -1;
		}
		//TODO
		if (position.x > 500)
		{
			return -1;
		}

		sequenceTime = 0;
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		battleSystem.Retreat(resLine.lineIdx, element.resIdx);

		return 1;
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
		UnitElementController controller = handicapController[1].Pop(handicapIdx) as UnitElementController;


		sequenceTime = 0;
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();


		//data input 显示层检查完了再动数据层！！！
		battleSystem.Deploy(handicapIdx, lineidx, 0);
	}
	public void AICast(int handicapIdx, int dstLineIdx, int dstPos)
	{
		UnitElementController controller = handicapController[1].Pop(handicapIdx) as UnitElementController;


		sequenceTime = 0;
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		battleSystem.Cast(handicapIdx, dstLineIdx, dstPos);
	}

	public void AISkip()
	{
		Skip();
	}

	public void AIMove(int resLineIdx, int resIdx, int dstLineIdx, int dstPos)
	{
		sequenceTime = 0;
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);
	}

	public void AIRetreat(int resLineIdx, int resPos)
	{
		BattleLineController battleLine = battleLineControllers[resLineIdx];
		if (resLineIdx == fieldCapacity - 1 && battleLine[resPos].operateCounter == 1)
		{
			sequenceTime = 0;
			rotateSequence.Kill();
			rotateSequence = DOTween.Sequence();
			Debug.Log($"Retreat第{resPos}位{battleLine[resPos].ID}");
			battleSystem.Retreat(resLineIdx, resPos);
		}
	}
}