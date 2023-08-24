using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleEffects : MonoBehaviour
{
	public Image circle;

	public void OnEnable()
	{
		// 创建一个 Tweener 对象
		Tweener tweener = DOTween.To(
			// 获取初始值
			() => new Vector3(20, 20, 1),
			// 设置当前值
			y => circle.transform.localScale = y,
			// 指定最终值
			new Vector3(1, 1, 1),
			// 指定持续时间
			0.4f
		).SetEase(Ease.Linear);
	}
}
