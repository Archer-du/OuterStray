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
	public TerrainController terrain;

	public int hrztIdx;
	public int vtcIdx;

	public Button castButton;

	public CanvasGroup InspectPanel;

	public List<NodeController> adjNodes;



	public virtual void CastEvent() { }

	public void Init()
	{
		adjNodes = new List<NodeController>();

		castButton = GetComponent<Button>();

		castButton.onClick.AddListener(CastEvent);
	}
	public void SetAdjacentNode(List<INodeController> adjList)
	{
		Init();
		foreach (INodeController node in adjList)
		{
			adjNodes.Add(node as NodeController);
		}
	}

	//// 在 Start 方法中给按钮添加事件监听
	//void Start()
	//{
	//	loadButton.onClick.AddListener(LoadGameScene);
	//}

	//// 加载 GameScene 的方法
	//void LoadGameScene()
	//{
	//	// 异步加载 GameScene，并获取 AsyncOperation 对象
	//	AsyncOperation async = SceneManager.LoadSceneAsync(1);
	//	// 开始协程，传入 AsyncOperation 对象
	//	StartCoroutine(ShowProgress(async));
	//}

	//// 显示加载进度的协程
	//IEnumerator ShowProgress(AsyncOperation async)
	//{
	//	// 当场景没有加载完时
	//	while (!async.isDone)
	//	{
	//		// 获取加载进度并更新 UI 文本或滑动条的值
	//		float progress = async.progress;
	//		progressText.text = "Loading... " + (progress * 100) + "%";
	//		// 或者 progressBar.value = progress;
	//		// 等待一帧
	//		yield return null;
	//	}
	//	// 场景加载完后执行一些操作，比如隐藏加载界面或播放音效等
	//	progressText.text = "Load Complete!";
	//	// 或者 progressBar.gameObject.SetActive(false);
	//	// 或者 AudioSource.PlayClipAtPoint(loadSound, transform.position);
	//}




	private float timer = 0;
	void Update()
	{
		// 如果鼠标悬停在元素上
		if (timer > 0)
		{
			// 计时器递减
			timer -= Time.deltaTime;
			// 如果计时器小于等于 0
			if (timer <= 0)
			{
				EnableInspector();
			}
		}
	}
	public void EnableInspector()
	{

	}
	public void DisableInspector()
	{

	}


	public void OnPointerEnter(PointerEventData eventData)
	{
		timer = 0.8f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		timer = -1;
		DisableInspector();
	}
}