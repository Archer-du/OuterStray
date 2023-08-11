using DG.Tweening;
using DisplayInterface;
using InputHandler;
using SceneState;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR;

public class TacticalSceneManager : MonoBehaviour,
    ITacticalSceneController
{
    ITacticalSystemInput tacticalSystem;

    public GameManager gameManager;

    public GameObject Map;
    public GameObject UI;

	public GameObject arrowPrototype;
    public NodeController currentNode;

    public List<TerrainController> terrains;
    public TerrainController currentTerrain
    {
        get
        {
			if (currentNode is BattleNodeController)
			{
				BattleNodeController node = currentNode as BattleNodeController;
                //TODO
                return terrains[node.terrain.index + 1];
			}
			else
			{
				return currentNode.terrain;
			}
		}
    }

    public Transform terrainsGroup;

	public GameObject terrainPrototype;

    public float switchDuration = 1f;



    public Color originOrange;



    public void OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Battle:
                DontDestroyOnLoad(Map);
                DontDestroyOnLoad(UI);
                break;
        }
    }
	public void TerrrainsInitialize(ITacticalSystemInput handler, int terrainsLength)
	{
        GameManager.OnGameStateChanged += OnGameStateChanged;
        DontDestroyOnLoad(gameObject);

		ColorUtility.TryParseHtmlString("#F6921E", out originOrange);
		tacticalSystem = handler;

        terrains = new List<TerrainController>();
	}

    public ITerrainController InstantiateTerrain(int idx)
    {
        GameObject terrain = Instantiate(terrainPrototype, terrainsGroup);
        TerrainController controller = terrain.GetComponent<TerrainController>();

        terrains.Add(controller);
        controller.tacticalManager = this;
        controller.index = idx;

        return controller;
    }


	public void EnterNextTerrain()
	{
		GameObject[] mapArrow = GameObject.FindGameObjectsWithTag("MapArrow");
		foreach (GameObject obj in mapArrow)
		{
			obj.SetActive(false);
		}
		terrains[currentTerrain.index - 1].ClearNodes();

        currentTerrain.nodes.Insert(0, terrains[currentTerrain.index - 1].dstNode);
		currentTerrain.srcNode = terrains[currentTerrain.index - 1].dstNode;
        //TODO
        terrainsGroup.DOBlendableMoveBy(2700 * Vector3.left, switchDuration);
	}

	public void UpdateCurrentNode(INodeController controller)
	{
        StopAllCoroutines();
		GameObject[] mapArrow = GameObject.FindGameObjectsWithTag("MapArrow");
		foreach (GameObject obj in mapArrow)
		{
			obj.SetActive(false);
		}

		currentNode = controller as NodeController;

        UpdateNodesDisplay();

        foreach(NodeController adjNode in currentNode.adjNodes)
        {

            StartCoroutine(ArrowCaster(adjNode));
        }
	}


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





	public void UpdateNodesDisplay()
	{
        //displaycurrentNode TODO
        currentNode.castButton.enabled = false;
        currentNode.Icon.DOColor(originOrange, fadeDuration);

        foreach(NodeController node in currentTerrain.nodes)
        {
            if(node != currentNode && node.casted)
            {
                node.DisableLines();

                foreach(NodeController adj in node.adjNodes)
                {
                    DisableNode(adj);
                }
            }
        }
	}

    public float fadeDuration = 0.3f;
    public void DisableNode(NodeController node)
    {
        if(node != currentNode && node.castButton.enabled)
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
