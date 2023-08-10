using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnlargedInspect : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler
{
	public Transform inspectee;
	public float inspectFactor;
	public float duration;

	private Vector3 originScale;
	private Vector3 inspectScale;
	public void OnPointerEnter(PointerEventData eventData)
	{
		inspectee.DOScale(inspectScale, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		inspectee.DOScale(originScale, duration);
	}
	public void Start()
	{
		originScale = inspectee.localScale;
		inspectScale = originScale * inspectFactor;
	}
}
