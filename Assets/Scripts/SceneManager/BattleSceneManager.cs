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


	public GameObject battleLinePrototype;
	public BattleLineController[] battleLineControllers;

	//TODO
	public GameObject humanEnergy;

	public int[] energy;
	public int[] energySupply;
	public TMP_Text[] energyText;
	public TMP_Text[] energySupplyText;


	public GameObject cardStackPrototype;
	public CardStackController[] cardStackController;


	public GameObject HandicapPrototype;
	public HandicapController[] handicapController;


	public GameObject unitPrototype;

	/// <summary>
	/// 战场战线容量
	/// </summary>
	public int fieldCapacity;


	public float fieldWidth;
	public float fieldHeight;


	public Button skipButton;
	public Image buttonImage;


	public Sequence rotateSequence;
	public int sequenceNum = 0;
	public Sequence moveSequence;
	public Sequence immediateSequence;

	//结算锁
	public static bool settlement = false;
	public bool check = false;


	public void FieldInitialize(IBattleSystemInput handler)
	{
		battleSystem = handler;
		

		fieldCapacity = 4;
		battleLineControllers = new BattleLineController[fieldCapacity];

		energy = new int[2];
		energySupply = new int[2];

		cardStackController = new CardStackController[2];
		handicapController = new HandicapController[2];


		skipButton = GameObject.Find("UI/SkipButton").GetComponent<Button>();
		skipButton.onClick.AddListener(Skip);
		buttonImage = skipButton.gameObject.GetComponent<Image>();


		fieldWidth = GameObject.Find("BattleField").GetComponent<RectTransform>().rect.width;
		fieldHeight = GameObject.Find("BattleField").GetComponent<RectTransform>().rect.height;

		check = true;
	}
	/// <summary>
	/// 
	/// </summary>
	public void UpdateTurnWithSettlement()
	{
		//结算攻击动画
		rotateSequence.InsertCallback(sequenceNum * 0.8f, () =>
		{
			sequenceNum = 0;
			UpdateTurn();
		});
		rotateSequence.Play();
	}
	public void UpdateTurn(int TURN)
	{
		Turn = TURN;
		
		if(Turn == 0)
		{
			buttonImage.color = Color.white;
		}
		else
		{
			buttonImage.color = Color.gray;

			//行为树在这里写 //TODO
			AIDeploy(0, 0);
		}
	}
	private void UpdateTurn()
	{
		Turn = (Turn + 1) % 2;
		Debug.Log("next turn: " + Turn);
		if (Turn == 0)
		{
			buttonImage.color = Color.white;
		}
		else
		{
			buttonImage.color = Color.gray;

			//行为树在这里写 TODO
			Sequence seq = DOTween.Sequence();
			seq.AppendInterval(2f);
			seq.InsertCallback(1f, () =>
			{
				AIDeploy(0, 0);
			});
			rotateSequence.Play();
		}
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
	}

	public void UpdateEnergySupply(int turn, int supplyPoint)
	{
		this.energySupply[turn] = supplyPoint;
		energySupplyText[turn].text = "+" + supplyPoint.ToString();
	}






	//TODO instantiate 不安全，可能未经初始化
	//TODO 后续功能组件隔离
	public IBattleLineController InstantiateBattleLine(int idx, int capacity)
	{
		GameObject battleLine = Instantiate(battleLinePrototype, GameObject.Find("BattleField").transform);
		battleLineControllers[idx] = battleLine.GetComponent<BattleLineController>();
		battleLineControllers[idx].lineIdx = idx;
		return battleLineControllers[idx];
	}

	public IHandicapController InstantiateHandicap(int ownership)
	{
		//set position
		GameObject handicap = Instantiate(HandicapPrototype, GetHandicapPosition(ownership), new Quaternion());
		handicap.transform.SetParent(GameObject.Find("RedemptionZone").transform);
		handicapController[ownership] = handicap.GetComponent<HandicapController>();
		return handicapController[ownership];
	}
	private Vector2 GetHandicapPosition(int ownership)
	{
		return new Vector2(0, (ownership * 2 - 1) * 1200f);
	}
	public ICardStackController InstantiateCardStack(int ownership)
	{
		GameObject stack = Instantiate(cardStackPrototype, GetCardStackPosition(ownership), Quaternion.Euler(0, 0, 90));
		stack.transform.SetParent(GameObject.Find("UI/Stacks").transform);//TODO 层级关系可能会改
		cardStackController[ownership] = stack.GetComponent<CardStackController>();
		return cardStackController[ownership];
	}
	private Vector2 GetCardStackPosition(int ownership)
	{
		return new Vector2(-1700, (ownership * 2 - 1) * 400f);
	}






	/// <summary>
	/// 将
	/// </summary>
	/// <param name="element"></param>
	public void PushElementIntoHandicap(IUnitElementController element)
	{
		handicapController[Turn].Push(element as UnitElementController);
	}
	/// <summary>
	/// 重载的方法，用于初始化
	/// </summary>
	/// <param name="turn"></param>
	/// <param name="element"></param>
	public void PushElementIntoHandicap(int turn, IUnitElementController element)
	{
		handicapController[turn].Push(element as UnitElementController);
	}





	//TODO
	public int GetBattleLineIdx(float y)
	{
		if(y > 220 && y < 1970)
		{
			return (int)((y - 180) / 466);
		}
		return -1;
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
		int idx = GetBattleLineIdx(position.y);
		if(idx < 0)
		{
			return -1;
		}
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

		battleLineControllers[idx].Receive(controller, pos);

		//data input 显示层检查完了再动数据层！！！
		battleSystem.Deploy(handicapIdx, idx, pos);

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
		int dstLineIdx = GetBattleLineIdx(position.y);
		int resLineIdx = resLine.lineIdx;
		int resIdx = element.resIdx;
		int dstPos = battleLineControllers[dstLineIdx].GetDeployPos(position.x);
		if(dstPos < 0) return -1;

		if (dstLineIdx == resLineIdx)
		{
			//int verPos = battleLineControllers[dstLineIdx].GetVerticalMovePos(position.x);
			//if (verPos < 0) return -1;
			//battleSystem.VerticalMove(resLineIdx, resIdx, verPos - 1);
			return -1;
		}
		if (element.operateCounter == 0)
		{
			return -1;
		}

		if(Math.Abs(dstLineIdx - resLineIdx) > 1) return -1;//TODO


		if (battleLineControllers[dstLineIdx].ownerShip != element.ownership && battleLineControllers[dstLineIdx].count > 0)
		{
			return -1;
		}
		//解析成功


		rotateSequence.Kill();
		rotateSequence = DOTween.Sequence();

		battleLineControllers[dstLineIdx].Receive(battleLineControllers[resLineIdx].Send(resIdx), dstPos);

		battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);

		return 1;
	}














	public int AIDeploy(int pos, int handicapIdx)
	{
		int idx = 3;//TODO
		if(handicapIdx >= handicapController[1].count)
		{
			AIMove();
			Sequence seq1 = DOTween.Sequence();
			seq1.AppendInterval(2f);
			seq1.InsertCallback(1f, () =>
			{
				Skip();
			});
			rotateSequence.Play();
			return -1;
		}
		if (energy[1] < handicapController[1][handicapIdx].cost)
		{
			AIMove();
			Sequence seq1 = DOTween.Sequence();
			seq1.AppendInterval(2f);
			seq1.InsertCallback(1f, () =>
			{
				Skip();
			});
			rotateSequence.Play();
			return -1;
		}
		if (battleLineControllers[idx].count >= battleLineControllers[idx].capacity)
		{
			AIMove();
			Sequence seq2 = DOTween.Sequence();
			seq2.AppendInterval(2f);
			seq2.InsertCallback(1f, () =>
			{
				Skip();
			});
			rotateSequence.Play();
			return -1;
		}

		UnitElementController controller = handicapController[1].Pop(handicapIdx) as UnitElementController;

		battleLineControllers[idx].Receive(controller, pos);


		//data input 显示层检查完了再动数据层！！！
		battleSystem.Deploy(handicapIdx, idx, pos);


		Sequence seq = DOTween.Sequence();
		seq.AppendInterval(2f);
		seq.InsertCallback(1f, () =>
		{
			Skip();
		});
		rotateSequence.Play();


		return 1;
	}
	public int AIMove()
	{
		int resLineIdx = 3;
		int dstLineIdx = 2;
		int resIdx = battleLineControllers[resLineIdx].count - 1;
		int dstPos = 0;
		if (battleLineControllers[resLineIdx].count <= 0) return -1;

		UnitElementController controller = battleLineControllers[resLineIdx][resIdx] as UnitElementController;
		if (controller.operateCounter == 0)
		{
			return -1;
		}
		if (battleLineControllers[dstLineIdx].ownerShip != controller.ownership && battleLineControllers[dstLineIdx].count > 0)
		{
			return -1;
		}

		battleLineControllers[dstLineIdx].Receive(battleLineControllers[resLineIdx].Send(resIdx), dstPos);

		battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);

		return 1;
	}
}
