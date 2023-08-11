using DataCore.BattleElements;
using DG.Tweening;
using DisplayInterface;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Tilemaps.TilemapRenderer;

public class NodeController : MonoBehaviour,
	INodeController
{
	[Header("Connection")]
	public TacticalSceneManager tacticalManager;
	public TerrainController terrain;

	[Header("Data")]
	public int hrztIdx;
	public int vtcIdx;

	public bool casted;
	public int inDegree;
	public int disabledInd;

	public string description;


	[Header("Display")]
	public Button castButton;
	public Image Icon;

	public CanvasGroup InspectPanel;
	public TMP_Text descriptionText;

	public List<NodeController> adjNodes;
	public List<Image> lines;
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
		Icon = transform.Find("Image/Icon").GetComponent<Image>();
		descriptionText = transform.Find("InspectPanel/Inspector/DescriptionText").GetComponent <TMP_Text>();

		castButton.onClick.AddListener(CastEvent);

		originScale = transform.localScale;
		largeScale = transform.localScale * 1.5f;
	}
	public virtual void LoadResource() { }
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