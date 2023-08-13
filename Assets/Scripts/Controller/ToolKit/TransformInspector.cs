using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransformInspector : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler
{
	public bool active;
	public Transform inspectee;
	public float inspectFactor;
	public Vector3 offset;
	public float duration;
	public Canvas canvas;
	public int upperOrder;
	public int lowerOrder;

	private Vector3 originScale;
	private Vector3 inspectScale;
	public void OnPointerEnter(PointerEventData eventData)
	{
		if(!active) return;
		canvas.sortingOrder = upperOrder;
		//transform.DOMove(handicap.GetInsertionPosition(handicapIdx) + offset, duration);
		inspectee.DOScale(inspectScale, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if(!active) return;
		canvas.sortingOrder = lowerOrder;
		//transform.DOMove(handicap.GetInsertionPosition(handicapIdx), duration);
		inspectee.DOScale(originScale, duration);
	}
	public void Init(Vector3 originScale)
	{
		active = false;
		this.originScale = originScale;
		inspectScale = originScale * inspectFactor;
	}
}
