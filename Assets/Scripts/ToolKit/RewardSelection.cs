using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RewardSelection : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler,
	IPointerClickHandler
{
	//后续改为泛型 TODO
	public BattleSceneManager manager;
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
		manager.rewardSelectionIndex = index;
		manager.rewardConfirmButton.interactable = true;
		manager.ClearOtherSelectionFrame();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		transform.DOScale(inspectScale, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!disableExit)
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
