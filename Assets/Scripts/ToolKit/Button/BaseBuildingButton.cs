using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseBuildingButton : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler
{
	public CultivateSceneManager manager
	{
		get => GameManager.GetInstance().cultivateSceneManager;
	}

	public Image aura;
	public float inspectFactor;
	public float duration;

	private Vector3 originScale;
	private Vector3 inspectScale;
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (manager.buildingsDisabled) return;
		aura.DOFade(1f, duration);
		transform.DOScale(inspectScale, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (manager.buildingsDisabled) return;
		aura.DOFade(0f, duration);
		transform.DOScale(originScale, duration);
	}
	public void Start()
	{
		aura.DOFade(0f, 0.01f);
		originScale = transform.localScale;
		inspectScale = transform.localScale * inspectFactor;
	}
}
