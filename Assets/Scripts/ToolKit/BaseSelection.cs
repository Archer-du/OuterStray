using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseSelection : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler,
	IPointerClickHandler
{
	public CultivateSceneManager manager;
	public GameObject Frame;
	public int index;

	public float inspectFactor;

	public float duration;

	public Vector3 originScale;
	public Vector3 inspectScale;

	public bool disableExit;
	public void OnPointerClick(PointerEventData eventData)
	{
		disableExit = true;
		manager.selectionIndex = index;
		manager.startExpedition.interactable = true;
		manager.ClearOtherSelectionFrame();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		transform.DOScale(inspectScale, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if(!disableExit)
		{
			transform.DOScale(originScale, duration);
		}
	}

	public void Start()
	{
		Frame.SetActive(false);

		originScale = transform.localScale;
		inspectScale = transform.localScale * inspectFactor;
	}
}
