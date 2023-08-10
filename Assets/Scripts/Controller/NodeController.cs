using DataCore.BattleElements;
using DG.Tweening;
using DisplayInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Tilemaps.TilemapRenderer;

public class NodeController : MonoBehaviour,
	INodeController
{
	public TacticalSceneManager tacticalManager;
	public TerrainController terrain;

	public int hrztIdx;
	public int vtcIdx;

	public Button castButton;
	public Image Icon;

	public CanvasGroup InspectPanel;

	public List<NodeController> adjNodes;
	public List<Image> lines;

	public bool casted;
	public int inDegree;
	public int disabledInd;

	public Vector3 largeScale;
	public Vector3 originScale;
	public virtual void CastEvent()
	{
		casted = true;
		//TODO test virtual segment
		tacticalManager.EnterNode(terrain.index ,hrztIdx, vtcIdx);
	}

	public virtual void Init()
	{
		casted = false;
		inDegree = 0;
		disabledInd = 0;
		adjNodes = new List<NodeController>();
		lines = new List<Image>();

		castButton = GetComponent<Button>();
		Icon = transform.Find("Icon").GetComponent<Image>();
		castButton.onClick.AddListener(CastEvent);

		originScale = transform.localScale;
		largeScale = transform.localScale * 1.5f;
	}
	public void SetAdjacentNode(List<INodeController> adjList)
	{
		foreach (INodeController node in adjList)
		{
			NodeController controller = node as NodeController;
			adjNodes.Add(controller);
			controller.inDegree++;
		}
	}
	public void DisableLines()
	{
		foreach(Image image in lines)
		{
			image.color = Color.gray;
		}
	}
	public void ClearLines()
	{
		foreach (Image image in lines)
		{
			image.gameObject.SetActive(false);
		}
	}

}