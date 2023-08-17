using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainButton : MonoBehaviour, 
	IPointerEnterHandler, IPointerExitHandler
{
	public CanvasGroup frame;
	public float duration;
	public void OnPointerEnter(PointerEventData eventData)
	{
		frame.DOFade(1f, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		frame.DOFade(0f, duration);
	}
	public void Start()
	{
		frame.DOFade(0f, 0.01f);
	}
}
