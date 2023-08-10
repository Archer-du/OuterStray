using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleNodeController : NodeController
{
	public override void CastEvent()
	{
		base.CastEvent();
	}


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
}
