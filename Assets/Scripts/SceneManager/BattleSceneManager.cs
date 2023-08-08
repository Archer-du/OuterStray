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

public class AnimationQueue
{
	public Sequence sequence;
	public int eventNum;
	public float sequenceTime;

}
public class BattleSceneManager : MonoBehaviour,
	IBattleSceneController
{
	/// <summary>
	/// 系统输入接口
	/// </summary>
	IBattleSystemInput battleSystem;
	/// <summary>
	/// GameManager单例
	/// </summary>
	public GameManager gameManager;


	/// <summary>
	/// 渲染层TURN备份
	/// </summary>
	public static int Turn;
	public int turnNum;



	public GameObject battleLinePrototype;
	public BattleLineController[] battleLineControllers;

	public int[] energy;
	public int[] energySupply;
	public TMP_Text[] energyText;
	public TMP_Text[] energySupplyText;

	public GameObject cardStackPrototype;
	public CardStackController[] cardStackController;


	public GameObject handicapPrototype;
	public HandicapController[] handicapController;


	public GameObject unitPrototype;
	public GameObject commandPrototype;

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
	public bool check = false;

	//对话框
	public GameObject dialogFrame;
    public TextMeshProUGUI nameText;
	public string dialogs;

	StreamReader reader;


    public void FieldInitialize(IBattleSystemInput handler)
	{
		battleSystem = handler;
		turnNum = 0;

		fieldCapacity = 4;
		battleLineControllers = new BattleLineController[fieldCapacity];

		energy = new int[2];
		energySupply = new int[2];

		cardStackController = new CardStackController[2];
		handicapController = new HandicapController[2];


		skipButton.onClick.AddListener(Skip);
		buttonImage = skipButton.gameObject.GetComponent<Image>();

		fieldWidth = GameObject.Find("BattleField").GetComponent<RectTransform>().rect.width;
		fieldHeight = GameObject.Find("BattleField").GetComponent<RectTransform>().rect.height;
		
		
		dialogFrame = GameObject.Find("Dialog");
        nameText = dialogFrame.transform.Find("Text(TMP)").GetComponent<TextMeshProUGUI>();
        dialogFrame.SetActive(false);

        reader = File.OpenText("\\UnityProject\\AIGC\\OuterStray\\Assets\\Tutorial\\TutorialDialog.txt");

        

        for (int i = 0; i < 5; i++)
		{
			humanSlots[i].SetActive(false);
			plantSlots[i].SetActive(false);
		}

		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		check = true;
	}


    private void DisplayDialog()
    {
        if (turnNum % 2 == 1)
        {
            dialogs = reader.ReadLine();
			if (dialogs == null) return;
            dialogFrame.SetActive(true);
			DOTween.To(
				() => "",
				value => nameText.text = value, // setter设置costText的内容
				dialogs,
				0.8f
			).SetEase(Ease.Linear);
        }
    }

    private void UpdateDialog()
    {
        if (turnNum % 2 == 1)
        {
            DisplayDialog();
        }
        if (turnNum % 2 == 0)
        {
            dialogFrame.SetActive(false);
        }
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
			UpdateTurn();
		});
	}
	public void Settlement()
	{
		rotateSequence.InsertCallback(sequenceTime, () =>
		{
			sequenceTime = 0;
		});
	}
	public void UpdateTurn(int TURN)
	{
		Turn = TURN;
		turnNum++;
		UpdateDialog();
		if(Turn == 0)
		{
			buttonImage.color = Color.white;
		}
		//如果是地方回合，启动行为树
		else
		{
			buttonImage.color = Color.gray;

			StartCoroutine(AIBehavior());
		}
    }
	private void UpdateTurn()
	{
		Turn = (Turn + 1) % 2;
		turnNum++;
		Debug.Log("next turn: " + Turn);
		UpdateDialog();
		if (Turn == 0)
		{
			buttonImage.color = Color.white;
        }
        //如果是敌方回合，启动行为树
        else
		{
			buttonImage.color = Color.gray;

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
			return (int)((y - 180) / (BattleLineController.lineWidth + BattleLineController.lineInterval));
		}
		return -1;
	}









	/// <summary>
	/// 敌我双方公用的输入 结束回合
	/// </summary>
	public void Skip()
	{
		//分配动画队列
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
		//不是自己的回合
		if (Turn != 0)
		{
			return -1;
		}
		if (BattleLineController.updating)
		{
			return -1;
		}
		int idx = GetBattleLineIdx(position.y);
		//没有部署在支援战线 TODO 扩展
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


		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		UnitElementController controller = handicapController[0].Pop(handicapIdx) as UnitElementController;

		//data input 显示层检查完了再动数据层！！！
		battleSystem.Deploy(handicapIdx, idx, pos);

		return 1;
	}
	public int PlayerCast(Vector2 position, int handicapIdx)
	{
		//不是自己的回合
		if (Turn != 0)
		{
			return -1;
		}
		if (BattleLineController.updating)
		{
			return -1;
		}
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
		Debug.Log(pos);
		string type = (handicapController[0][handicapIdx] as CommandElementController).type;
		if(pos < 0 && type == "Target") return -1;

		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		UnitElementController controller = handicapController[0].Pop(handicapIdx) as UnitElementController;

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
		if (Turn != 0)
		{
			return -1;
		}
		if (BattleLineController.updating)
		{
			return -1;
		}
		int dstLineIdx = GetBattleLineIdx(position.y);
		int resLineIdx = resLine.lineIdx;
		int resIdx = element.resIdx;
		int dstPos = battleLineControllers[dstLineIdx].GetDeployPos(position.x);
		if(dstPos < 0) return -1;

		if (element.operateCounter == 0)
		{
			return -1;
		}
		if (dstLineIdx == resLineIdx) return -1;
		if(Math.Abs(dstLineIdx - resLineIdx) > element.moveRange) return -1;


		if (battleLineControllers[dstLineIdx].ownerShip != element.ownership && battleLineControllers[dstLineIdx].count > 0)
		{
			return -1;
		}
		//解析成功


		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);

		return 1;
	}
	public int PlayerRetreat(Vector2 position, BattleLineController resLine, UnitElementController element)
	{
		if (Turn != 0)
		{
			return -1;
		}
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
	IEnumerator AIBehavior()
	{
		AIHandicap = handicapController[1];
		AISupportLine = battleLineControllers[3];
		//TODO
		AIAdjacentLine = battleLineControllers[2];
		float waitTime = 0.9f;

		yield return new WaitForSeconds(waitTime);



		int deploytimes = 1;
		int movetimes = 1;
		int cost = GetMinCost();

		//把支援战线铺满
		while(AISupportLine.count < 5)
		{
			int idx = GetMinCostUnitPointer();
			if (idx >= 0)
			{
				AIDeploy(idx);
				yield return new WaitForSeconds(sequenceTime + waitTime);
				deploytimes++;
			}
			else break;
		}
		while (AIAdjacentLine.count < AIAdjacentLine.capacity)
		{
			if (AIAdjacentLine.count == 0 || AIAdjacentLine.ownerShip == 1)
			{
				int idx = GetOperatorPointerInSupportLine();
				if (idx >= 0)
				{
					AIMove(idx);
					yield return new WaitForSeconds(sequenceTime + waitTime);
					movetimes++;
				}
				else break;
			}
			else break;
		}


		Skip();
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
	/// <returns></returns>
	private int GetMinCostUnitPointer()
	{
		int minCost = 10000;
		int minPointer = -1;
		for (int i = 0; i < AIHandicap.count; i++)
		{
			if (AIHandicap[i].cost < minCost && AIHandicap[i] is UnitElementController)
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
	public void AICast(int handicapIdx)
	{
		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();
	}

	public void AIMove(int resIdx)
	{
		int resLineIdx = 3;
		int dstLineIdx = 2;
		int dstPos = 0;

		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();


		battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);
	}
	public void AISkip()
	{
		Skip();
	}

}
