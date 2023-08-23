using DisplayInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TerrainController : MonoBehaviour,
	ITerrainController
{
	public TacticalSceneManager tacticalManager;

	public int index;
	public GameObject linePrototype;

    public GameObject nodePrototype;
	public List<NodeController> nodes;
	/// <summary>
	/// 第二层terrain的src需要后续维护
	/// </summary>
	public NodeController srcNode;
	public NodeController dstNode;

	public float leftBound = -1200;
	public float rightBound = 1200;
	public float topBound = 1000;
	public float bottomBound = -1000;

	public float vtcOffsetRange = 100; 
	public float hrztOffsetRange = 200; 

	public void Init()
	{
		nodes = new List<NodeController>();
	}
	public INodeController InstantiateNode(int length, int width, int hrztIdx, int vtcIdx, string category)
	{
		NodeController controller = null;
		GameObject Node = Instantiate(nodePrototype, transform);
		switch(category)
		{
			case "source":
				controller = Node.AddComponent<SourceNodeController>();
				srcNode = controller;
				break;
			case "battle":
				controller = Node.AddComponent<BattleNodeController>();
				dstNode = controller;
				break;
			case "promote":
				controller = Node.AddComponent<PromoteNodeController>();
				break;
			case "legacy":
				controller = Node.AddComponent<LegacyNodeController>();
				break;
			case "outpost":
				controller = Node.AddComponent<OutPostNodeController>();
				break;
			case "medical":
				controller = Node.AddComponent<MedicalNodeController>();
				break;
			case "supply":
				controller = Node.AddComponent<SupplyNodeController>();
				break;
		}
		nodes.Add(controller);
		controller.tacticalManager = tacticalManager;
		SetPosition(controller, length, width, hrztIdx, vtcIdx);
		return controller;
	}
	public void SetPosition(NodeController controller, int length, int width, int hrztIdx, int vtcIdx)
	{
		controller.terrain = this;
		controller.hrztIdx = hrztIdx;
		controller.vtcIdx = vtcIdx;

		float totalLength = rightBound - leftBound;
		float totalWidth = topBound - bottomBound;
		float hrztPos = leftBound + hrztIdx * (totalLength / (length - 1));
		float vtcPos = bottomBound + (vtcIdx + 1) * (totalWidth / (width + 1));

		float hrztOffset = Random.Range(-hrztOffsetRange, hrztOffsetRange);
		float vtcOffset = Random.Range(-vtcOffsetRange, vtcOffsetRange);

		if(controller is BattleNodeController || controller is SourceNodeController)
		{
			RectTransform rectTransform = controller.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(hrztPos, vtcPos);
		}
		else
		{
			RectTransform rectTransform = controller.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(hrztPos + hrztOffset, vtcPos + vtcOffset);
		}
	}
	public void GenerateLineNetFromSource()
	{
		GenerateLineNet(srcNode);
	}
	public void GenerateLineNet(NodeController node)
	{
		if(node == dstNode)
		{
			return;
		}
		foreach(NodeController adj in node.adjNodes)
		{
			InstantiateLine(node, adj);
			GenerateLineNet(adj);
		}
	}
	/// <summary>
	/// 清楚节点及节点内部线网
	/// </summary>
	public void ClearNodes()
	{
		foreach(NodeController node in nodes)
		{
			if(node != dstNode)
			{
				node.ClearLines();
				node.gameObject.SetActive(false);
			}
		}
	}
	private void InstantiateLine(NodeController resNode, NodeController dstNode)
	{
		GameObject line = Instantiate(linePrototype, resNode.transform);
		resNode.lines.Add(line.GetComponent<Image>());

		Vector3 vector = dstNode.transform.position - resNode.transform.position;
		
		Vector3 dstPos = resNode.transform.position + vector / 2;
		line.transform.position = dstPos;

		Vector3 euler = GetDegreeEuler(resNode.transform.position, dstNode.transform.position);
		line.transform.rotation = Quaternion.Euler(euler);

		line.GetComponent<RectTransform>().sizeDelta = new Vector2(vector.magnitude, 10);
	}






	public static Vector3 GetDegreeEuler(Vector3 src, Vector3 dst)
	{
		Vector3 vector = dst - src;
		float tan = vector.y / vector.x;
		float rad = Mathf.Atan(tan);
		float deg = Mathf.Rad2Deg * rad;
		Vector3 euler = new Vector3(0, 0, deg);
		return euler;
	}
}
