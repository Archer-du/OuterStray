using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete]
public class TacticalPanelDisplay : MonoBehaviour
{
	public TacticalSceneManager sceneManager;


	public GameObject OutPostPanel;
	public CardInspector[] OutPostInspectors;
	public List<int> gasMineCosts;
	public List<TMP_Text> gasMineCostTexts;
	public Button[] OutPostButtons;
	public GameObject PromotePanel;
	public CardInspector PromoteInspector;
	public Button onceButton;
	public Button fullfillButton;
	public GameObject SupplyPanel;
	public CardInspector[] SupplyInspectors;
	public Button[] SupplyButtons;

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
		sceneManager = GameManager.GetInstance().tacticalSceneManager;
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
		if(category == "Promote") PromoteInspector.gameObject.SetActive(false);

		sceneManager.panelEnabled = true;
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
		sceneManager.panelEnabled = false;
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
	//TODO
	public bool AddNewTag(Vector3 position, int ID, int dynHealth, int durability)
	{
		if ((category != "Promote" && category != "OutPost") || !sceneManager.panelEnabled)
		{
			return false;
		}
		if ((position.y > 350 && position.y < 1250) && (position.x > 1900 && position.x < 2600))
		{
			PromoteInspector.gameObject.SetActive(true);

		}
		return false;
	}
}
