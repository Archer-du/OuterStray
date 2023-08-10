using DG.Tweening;
using DisplayInterface;
using InputHandler;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TacticalSceneManager : MonoBehaviour,
    ITacticalSceneController
{
    ITacticalSystemInput tacticalSystem;

    public GameManager gameManager;

	public GameObject arrowPrototype;
    public NodeController currentNode;
    public TerrainController currentTerrain
    {
        get => currentNode.terrain;
    }

    public Transform terrainsGroup;

	public GameObject terrainPrototype;
    public List<TerrainController> terrains;

    public float switchDuration = 0.8f;

	public void TerrrainsInitialize(ITacticalSystemInput handler, int terrainsLength)
	{
        tacticalSystem = handler;

        terrains = new List<TerrainController>();

        DontDestroyOnLoad(this);
	}

    public ITerrainController InstantiateTerrain(int idx)
    {
        GameObject terrain = Instantiate(terrainPrototype, terrainsGroup);
        TerrainController controller = terrain.GetComponent<TerrainController>();
        terrains.Add(controller);

        return controller;
    }


	public void EnterNextTerrain()
	{
        //TODO
        terrainsGroup.DOBlendableMoveBy(2400 * Vector3.left, switchDuration);
	}

	public void UpdateCurrentNode(INodeController controller)
	{
        StopCoroutine("ArrowCaster");
		currentNode = controller as NodeController;
        foreach(NodeController adjNode in currentNode.adjNodes)
        {
            StartCoroutine(ArrowCaster(adjNode));
        }
	}
    IEnumerator ArrowCaster(NodeController adjNode)
    {
        NodeController curNode = currentNode;
        int arrowNum = 4;
        GameObject[] arrows = new GameObject[arrowNum];
        for(int i = 0; i < arrowNum; i++)
        {
            arrows[i] = Instantiate(arrowPrototype, currentTerrain.transform);
            arrows[i].transform.position = curNode.transform.position;
            arrows[i].transform.rotation = Quaternion.Euler(TerrainController.GetDegreeEuler(curNode.transform.position, adjNode.transform.position));
            arrows[i].gameObject.SetActive(false);
        }

        float duration = 1f;
        float waitTime = 0.3f;
        float interval = 1f;
        while (true)
        {
            for(int i = 0; i < arrowNum; i++)
            {
                yield return new WaitForSeconds(waitTime);
                arrows[i].gameObject.SetActive(true);
                int temp = i;
                arrows[i].transform.DOMove(adjNode.transform.position, duration)
                    .OnComplete(() =>
                        {
                            arrows[temp].transform.position = curNode.transform.position;
                            arrows[temp].gameObject.SetActive(false);
                        }
                    );
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
