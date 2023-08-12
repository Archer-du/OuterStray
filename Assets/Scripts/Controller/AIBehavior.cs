using DG.Tweening;
using LogicCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehavior : MonoBehaviour
{
	//BattleSceneManager manager;

	////行为树
	//HandicapController AIHandicap;
	//BattleLineController AISupportLine;
	//BattleLineController AIAdjacentLine;
	//IEnumerator Behavior()
	//{
	//	AIHandicap = handicapController[1];
	//	//TODO
	//	AISupportLine = battleLineControllers[3];
	//	//TODO
	//	AIAdjacentLine = battleLineControllers[2];
	//	float waitTime = 0.9f;

	//	yield return new WaitForSeconds(waitTime);



	//	int deploytimes = 1;
	//	int movetimes = 1;
	//	int cost = GetMinCost();

	//	//把支援战线铺满
	//	while (AISupportLine.count < 5)
	//	{
	//		int idx = GetMinCostUnitPointer();
	//		if (idx >= 0)
	//		{
	//			AIDeploy(idx);
	//			yield return new WaitForSeconds(sequenceTime + waitTime);
	//			deploytimes++;
	//		}
	//		else break;
	//	}
	//	while (AIAdjacentLine.count < AIAdjacentLine.capacity)
	//	{
	//		if (AIAdjacentLine.count == 0 || AIAdjacentLine.ownerShip == 1)
	//		{
	//			int idx = GetOperatorPointerInSupportLine();
	//			if (idx >= 0)
	//			{
	//				AIMove(idx);
	//				yield return new WaitForSeconds(sequenceTime + waitTime);
	//				movetimes++;
	//			}
	//			else break;
	//		}
	//		else break;
	//	}


	//	Skip();
	//}
	//private int GetMaxCost()
	//{
	//	int maxCost = 0;
	//	int maxPointer = -1;
	//	for (int i = 0; i < AIHandicap.count; i++)
	//	{
	//		if (AIHandicap[i].cost > maxCost)
	//		{
	//			maxCost = AIHandicap[i].cost;
	//			maxPointer = i;
	//		}
	//	}
	//	return maxCost;
	//}
	//private int GetMinCost()
	//{
	//	int minCost = 100;
	//	int minPointer = -1;
	//	for (int i = 0; i < AIHandicap.count; i++)
	//	{
	//		if (AIHandicap[i].cost < minCost)
	//		{
	//			minCost = AIHandicap[i].cost;
	//			minPointer = i;
	//		}
	//	}
	//	return minCost;
	//}
	//private int GetMaxCostUnitPointer()
	//{
	//	int maxCost = 0;
	//	int maxPointer = -1;
	//	for (int i = 0; i < AIHandicap.count; i++)
	//	{
	//		if (AIHandicap[i].cost >= maxCost && AIHandicap[i] is UnitElementController)
	//		{
	//			maxCost = AIHandicap[i].cost;
	//			maxPointer = i;
	//		}
	//	}
	//	if (AIHandicap[maxPointer].cost > energy[1])
	//	{
	//		return -1;
	//	}
	//	return maxPointer;
	//}
	///// <summary>
	///// 只有在费用不足时才会返回-1
	///// </summary>
	///// <returns></returns>
	//private int GetMinCostUnitPointer()
	//{
	//	int minCost = 10000;
	//	int minPointer = -1;
	//	for (int i = 0; i < AIHandicap.count; i++)
	//	{
	//		if (AIHandicap[i].cost < minCost && AIHandicap[i] is UnitElementController)
	//		{
	//			minCost = AIHandicap[i].cost;
	//			minPointer = i;
	//		}
	//	}
	//	if (minPointer == -1)
	//	{
	//		return -1;
	//	}
	//	if (AIHandicap[minPointer].cost > energy[1])
	//	{
	//		return -1;
	//	}
	//	return minPointer;
	//}
	///// <summary>
	///// 若没有合适的操作目标返回-1
	///// </summary>
	///// <returns></returns>
	//private int GetOperatorPointerInSupportLine()
	//{
	//	for (int i = 0; i < AISupportLine.count; i++)
	//	{
	//		if (AISupportLine[i].operateCounter == 1 && (AISupportLine[i].category != "Construction" && AISupportLine[i].category != "Artillery"))
	//		{
	//			return i;
	//		}
	//	}
	//	return -1;
	//}
	////private int GetArtilleryUnitPointer()
	////{

	////}
	////private int GetConstructionUnitPointer()
	////{

	////}
	//public void AIDeploy(int handicapIdx)
	//{
	//	int lineidx = 3;//supportline
	//	UnitElementController controller = AIHandicap.Pop(handicapIdx) as UnitElementController;


	//	rotateSequence.Kill();
	//	rotateSequence = DOTween.Sequence();


	//	//data input 显示层检查完了再动数据层！！！
	//	battleSystem.Deploy(handicapIdx, lineidx, 0);
	//}
	//public void AICast(int handicapIdx)
	//{
	//	rotateSequence.Kill();
	//	rotateSequence = DOTween.Sequence();
	//}

	//public void AIMove(int resIdx)
	//{
	//	int resLineIdx = 3;
	//	int dstLineIdx = 2;
	//	int dstPos = 0;

	//	rotateSequence.Kill();
	//	rotateSequence = DOTween.Sequence();


	//	battleSystem.Move(resLineIdx, resIdx, dstLineIdx, dstPos);
	//}
	//public void AISkip()
	//{
	//	Skip();
	//}

}
