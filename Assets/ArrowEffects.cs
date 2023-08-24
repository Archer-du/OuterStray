using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowEffects : MonoBehaviour
{
	public List<RawImage> arrows;
	public Image circle;

	public float duration;

	public void OnEnable()
	{
		foreach (var arrow in arrows)
		{
			arrow.enabled = false;
		}

		Sequence seq = DOTween.Sequence();
		for(int i = 0; i < arrows.Count; i++)
		{
			int temp = i;
			seq.AppendCallback(() => arrows[temp].enabled = true);
			seq.Append(arrows[temp].DOFade(1, 0.1f));
		}
		seq.AppendCallback(() =>
		{
			circle.enabled = true;
			circle.transform.localScale = new Vector3(10, 10, 1);
		});
		seq.Append(circle.transform.DOScale(new Vector3(1, 1, 1), 0.3f));
		seq.AppendInterval(1f);
		//seq.AppendCallback(() =>
		//{
		//	DisableAllArrow();
		//	circle.enabled = false;
		//});
	}
	public void DisableAllArrow()
	{
		foreach(var arrow in arrows)
		{
			arrow.enabled = false;
			arrow.DOFade(0, 0);
		}
	}
}
