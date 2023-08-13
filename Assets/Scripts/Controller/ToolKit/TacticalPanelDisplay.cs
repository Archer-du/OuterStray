using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticalPanelDisplay : MonoBehaviour
{
	public GameObject OutPostPanel;
	public GameObject PromotePanel;
	public GameObject SupplyPanel;

	public GameObject Panel;
	public RectTransform mask;

	public Button castButton;
	public Button exitButton;

	public float duration;
	public float finalHeight;

	public string category;
	public void Init(string category)
	{
		//TODO
		mask.anchoredPosition = - gameObject.GetComponent<RectTransform>().anchoredPosition - new Vector2(300, 0);

		this.category = category;
		switch (category)
		{
			case "OutPost":
				Panel = OutPostPanel;
				break;
			case "Promote":
				Panel = PromotePanel;
				break;
			case "Supply":
				Panel = SupplyPanel;
				break;
		}
		exitButton = Panel.transform.Find("ExitButton").GetComponent<Button>();

		castButton.onClick.AddListener(EnablePanel);
		exitButton.onClick.AddListener(DisablePanel);
	}
	public void EnablePanel()
	{
		Panel.SetActive(true);
		// 创建一个 Tweener 对象
		Tweener tweener = DOTween.To(
			// 获取初始值
			() => 0,
			// 设置当前值
			y => mask.sizeDelta = new Vector2(mask.sizeDelta.x, y),
			// 指定最终值
			finalHeight,
			// 指定持续时间
			duration
		);
	}
	public void DisablePanel()
	{
		// 创建一个 Tweener 对象
		Tweener tweener = DOTween.To(
			// 获取初始值
			() => finalHeight,
			// 设置当前值
			y => mask.sizeDelta = new Vector2(mask.sizeDelta.x, y),
			// 指定最终值
			0,
			// 指定持续时间
			duration
		).OnComplete(() =>
		{
			Panel.SetActive(false);
		});
	}
}
