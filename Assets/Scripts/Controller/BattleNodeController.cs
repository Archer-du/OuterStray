using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleNodeController : NodeController
{
	GameManager manager;
	public override void Init()
	{
		manager = GameManager.GetInstance();
		base.Init();
		LoadResource();
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[1];
		descriptionText.text = "battle";
	}
	public override void CastEvent()
	{
		AsyncOperation async = manager.UpdateGameState(SceneState.GameState.Battle);
		StartCoroutine(LateCast(async));
	}
	IEnumerator LateCast(AsyncOperation async)
	{
		while (!async.isDone)
		{
			yield return null;
		}

		casted = true;
		tacticalManager.EnterNode(terrain.index, hrztIdx, vtcIdx);
	}
}
