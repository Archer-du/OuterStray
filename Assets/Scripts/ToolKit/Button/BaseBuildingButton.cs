using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
	public RectTransform nameBar;
	public TMP_Text nameContent;
	[HideInInspector] public CanvasGroup canvasGroup;

	public string nameText;
	public float finalLength;

	public Image aura;
	public float inspectFactor;
	public float duration;

	private Vector3 originScale;
	private Vector3 inspectScale;
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (manager.buildingsDisabled) return;

		Tweener tweener = DOTween.To(
			// 获取初始值
			() => 0,
			// 设置当前值
			x => nameBar.sizeDelta = new Vector2(x, nameBar.sizeDelta.y),
			// 指定最终值
			finalLength,
			// 指定持续时间
			duration
		);

		aura.DOFade(1f, duration);
		canvasGroup.DOFade(1f, duration);
		nameContent.DOFade(1f, duration);
		transform.DOScale(inspectScale, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (manager.buildingsDisabled) return;

		Tweener tweener = DOTween.To(
			// 获取初始值
			() => finalLength,
			// 设置当前值
			x => nameBar.sizeDelta = new Vector2(x, nameBar.sizeDelta.y),
			// 指定最终值
			0,
			// 指定持续时间
			duration
		);

		aura.DOFade(0.5f, duration);
		canvasGroup.DOFade(0.5f, duration);
		nameContent.DOFade(0.5f, duration);
		transform.DOScale(originScale, duration);
	}
	public void Start()
	{
		canvasGroup = nameBar.gameObject.GetComponent<CanvasGroup>();

		nameContent.DOFade(0.5f, 0);
		aura.DOFade(0.5f, 0);
		canvasGroup.alpha = 0.5f;

		originScale = transform.localScale;
		inspectScale = transform.localScale * inspectFactor;
	}
}
