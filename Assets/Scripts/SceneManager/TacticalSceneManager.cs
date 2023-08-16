using DG.Tweening;
using DisplayInterface;
using InputHandler;
using SceneState;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class TacticalSceneManager : MonoBehaviour,
    ITacticalSceneController
{
    [Header("Input")]
    public ITacticalSystemInput tacticalSystem;

    [Header("Connection")]
    public GameManager gameManager;

    public GameObject Map;
    public GameObject UI;

    public Image InputMask;

    public DeckController playerDeck;

    [Header("Prototype")]
	public GameObject arrowPrototype;
	public GameObject terrainPrototype;

    [Header("Data")]
    public bool panelEnabled;
    public NodeController currentNode;

    public int gasMineToken;
    public int cardNum;
    public int baseHealth;
    public int baseMaxHealth;

    public List<TerrainController> terrains;
    public TerrainController currentTerrain
    {
        get
        {
			if (currentNode is BattleNodeController)
			{
                return terrains[currentNode.terrain.index + 1];
			}
			else
			{
				return currentNode.terrain;
			}
		}
    }
    public TerrainController prevTerrain
    {
        get => terrains[currentTerrain.index - 1];
    }

    [Header("Display")]
    public Transform terrainsGroup;
    public float terrainLength = 2700f;

    public TMP_Text gasMineText;
    public TMP_Text cardNumText;
    public TMP_Text baseHealthText;
    public TMP_Text baseMaxHealthText;

    public float switchDuration = 1f;

    public Color originOrange;


	public void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Battle:
				DontDestroyOnLoad(gameObject);

				DontDestroyOnLoad(Map);
                DontDestroyOnLoad(UI);
                break;
            //TODO
        }
    }
    public void Init()
    {
		GameManager.OnGameStateChanged += OnGameStateChanged;

		ColorUtility.TryParseHtmlString("#F6921E", out originOrange);
		terrains = new List<TerrainController>();
	}
	public void TerrrainsInitialize(ITacticalSystemInput handler, int terrainsLength)
	{
        Init();
		tacticalSystem = handler;
	}


    public void EnableInputMask()
    {
        //InputMask.raycastTarget = true;
    }
    public void DisableInputMask()
    {
        //InputMask.raycastTarget = false;
    }
    //TODO
	public void CampaignCompleted()
	{
        DisableInputMask();
    }
	public void CampaignFailed()
	{
        DisableInputMask();
    }



    public void MedicalNodeHeal(bool fullfill, int deckID)
    {
        tacticalSystem.MedicalNodeHeal(fullfill, deckID);
    }
    public void OutPostNodePurchase(int index)
    {
        tacticalSystem.OutPostNodePurchase(index);
    }
    public void SupplyNodeChoose(int index)
    {
        tacticalSystem.SupplyNodeChoose(index);
    }
	/// <summary>
	/// 生成Terrain控件，返回句柄
	/// </summary>
	/// <param name="idx"></param>
	/// <returns></returns>
	public ITerrainController InstantiateTerrain(int idx)
    {
        GameObject terrain = Instantiate(terrainPrototype, terrainsGroup);
        TerrainController controller = terrain.GetComponent<TerrainController>();

        terrains.Add(controller);
        controller.tacticalManager = this;
        controller.index = idx;

        return controller;
    }
	public IDeckController InstantiateDeck()
	{
        return playerDeck;
	}

	/// <summary>
	/// 进入下一层（此时当前节点已更新）
	/// </summary>
	public void EnterNextTerrain()
	{
        DisableArrowCaster();

		terrains[currentTerrain.index - 1].ClearNodes();

        currentTerrain.nodes.Insert(0, prevTerrain.dstNode);
		currentTerrain.srcNode = prevTerrain.dstNode;
        //TODO
        terrainsGroup.DOBlendableMoveBy(terrainLength * Vector3.left, switchDuration);
	}
    /// <summary>
    /// 更新当前节点
    /// </summary>
    /// <param name="controller"></param>
	public void UpdateCurrentNode(INodeController controller)
	{
        DisableArrowCaster();

        NodeController prevNode = currentNode;
		currentNode = controller as NodeController;

        UpdateNodesDisplay(prevNode);

        foreach(NodeController adjNode in currentNode.adjNodes)
        {
            StartCoroutine(ArrowCaster(adjNode));
        }
	}
	public void UpdateGasMineToken(int gasMineToken)
	{
        this.gasMineToken = gasMineToken;
        gasMineText.text = gasMineToken.ToString();
	}

	public void UpdateCardNum(int cardNum)
	{
        this.cardNum = cardNum;
        cardNumText.text = cardNum.ToString();
	}

	public void UpdateBaseHealth(int baseHealth, int baseMaxHealth)
	{
        this.baseMaxHealth = baseMaxHealth;
        this.baseHealth = baseHealth;
        baseHealthText.text = baseHealth.ToString();
        baseMaxHealthText.text = baseMaxHealth.ToString();
	}

	private void DisableArrowCaster()
    {
		StopAllCoroutines();
		GameObject[] mapArrow = GameObject.FindGameObjectsWithTag("MapArrow");
		foreach (GameObject obj in mapArrow)
		{
			obj.SetActive(false);
		}
	}

    /// <summary>
    /// 从当前节点发射箭头的动画协程
    /// </summary>
    /// <param name="adjNode"></param>
    /// <returns></returns>
    IEnumerator ArrowCaster(NodeController adjNode)
    {
        yield return new WaitForSeconds(switchDuration);

        NodeController curNode = currentNode;

		int arrowNum = 10;
		GameObject[] arrows = new GameObject[arrowNum];

		for (int i = 0; i < arrowNum; i++)
		{
			arrows[i] = Instantiate(arrowPrototype, currentTerrain.transform);
			arrows[i].transform.position = curNode.transform.position;
			arrows[i].transform.rotation = Quaternion.Euler(TerrainController.GetDegreeEuler(curNode.transform.position, adjNode.transform.position));
			arrows[i].gameObject.SetActive(false);
		}

		float duration = 4f;
        float waitTime = 1f;
        //float interval = 2f;
        while (true)
        {
            for(int i = 0; i < arrowNum; i++)
            {
                arrows[i].gameObject.SetActive(true);
                int temp = i;
                arrows[i].transform.DOMove(adjNode.transform.position, duration).SetEase(Ease.Linear)
                    .OnComplete(() =>
                        {
                            arrows[temp].transform.position = curNode.transform.position;
                            arrows[temp].gameObject.SetActive(false);
                        }
                    );
                yield return new WaitForSeconds(waitTime);
            }
            //yield return new WaitForSeconds(interval);
        }
    }



    public void EnterNode(int terrainIdx, int hrztIdx, int vtcIdx)
    {
        tacticalSystem.EnterNode(terrainIdx, hrztIdx, vtcIdx);
    }




    /// <summary>
    /// 当前节点更新后更新显示层
    /// </summary>
	public void UpdateNodesDisplay(NodeController prevNode)
	{
        //displaycurrentNode TODO
        currentNode.castButton.enabled = false;
        currentNode.Icon.DOColor(originOrange, fadeDuration);
		foreach (NodeController node in currentTerrain.nodes)
        {
            node.castButton.enabled = false;
        }
        //只有当前节点的邻接节点可用
        foreach(NodeController adj in currentNode.adjNodes)
        {
            adj.castButton.enabled = true;
        }

        if(prevNode == null) { return; }
        prevNode.DisableLines();
        foreach(NodeController adj in prevNode.adjNodes)
        {
            DisableNode(adj);
        }
		//foreach (NodeController node in currentTerrain.nodes)
  //      {
  //          if(node != currentNode && node.casted)
  //          {
  //              node.DisableLines();

  //              foreach(NodeController adj in node.adjNodes)
  //              {
  //                  DisableNode(adj);
  //              }
  //          }
  //      }
	}
    public float fadeDuration = 0.3f;
    public void DisableNode(NodeController node)
    {
        if(node != currentNode && !node.casted)
        {
            node.disabledInd++;
		    if (node.inDegree - node.disabledInd <= 0)
		    {
			    node.castButton.enabled = false;
                node.Icon.DOColor(Color.gray, fadeDuration);
                node.DisableLines();

                foreach(NodeController adj in node.adjNodes)
                {
                    DisableNode(adj);
                }
		    }
        }
	}



    public bool IsReachable(NodeController node)
    {
        foreach(NodeController adj in currentNode.adjNodes)
        {
            if(adj == node) return true;
        }
        return false;
    }


}
