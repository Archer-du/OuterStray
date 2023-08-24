using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleEffects : MonoBehaviour
{
	public float duration;

	public void OnEnable()
	{
		// 创建一个 Tweener 对象
		Tweener tweener = DOTween.To(
			// 获取初始值
			() => new Vector3(100, 100, 100),
			// 设置当前值
			y => transform.localScale = y,
			// 指定最终值
			new Vector3(1, 1, 1),
			// 指定持续时间
			duration
		);
	}
}
