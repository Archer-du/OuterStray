using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckTagController : MonoBehaviour,
		IDragHandler,
		IBeginDragHandler, IEndDragHandler,
		IPointerEnterHandler, IPointerExitHandler
{
	public DeckController controller;
	public Transform buffer;

	public Image ground;

	public float duration;

	[Header("Data")]
	public string category;
	public int index;

	public void OnBeginDrag(PointerEventData eventData)
	{
		throw new System.NotImplementedException();
	}

	public void OnDrag(PointerEventData eventData)
	{
		throw new System.NotImplementedException();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		throw new System.NotImplementedException();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ground.DOFade(0.5f, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ground.DOFade(0, duration);
	}
}
