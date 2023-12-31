using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardPackController : MonoBehaviour
{
	public static event Action<int> CloseOtherExplanation;

	public PanelController panel;

	public CardInspector inspector;

	public GameObject explanationPrototype;

	public CanvasGroup explainCanvas;
	public Button explainButton;
	public GridLayoutGroup textGroup;

	public Button SelectButton;
	public TMP_Text buttonText;

	public int index;

	[Header("External")]
	public Button detailInfoButton;



	public int gasMineCost
	{
		get => inspector.gasMineCost;
	}
	//private int Num;
	//public int num
	//{
	//	get => Num;
	//	set
	//	{
	//		Num = value;
	//		if(Num >= 1)
	//		{
	//			SelectButton.interactable = false;
	//		}
	//	}
	//}

	public float duration;
	public void Init(PanelController controller)
	{
		panel = controller;

		explainButton = inspector.nameTag.gameObject.AddComponent<Button>();
		explainButton.onClick.AddListener(ExplainButtonClick);

		detailInfoButton = inspector.cardImage.gameObject.AddComponent<Button>();
	}

	public void ExplainButtonClick()
	{
		explainCanvas.DOFade(explainCanvas.alpha == 0 ? 1 : 0, duration);
		foreach(CardPackController pack in panel.cardPacks)
		{
			if(pack != this)
			{
				pack.explainCanvas.DOFade(0, duration);
			}
		}
	}
	public void RenderInspector(string ID)
	{
		inspector.RenderInspector(ID);
		AddExplanations(inspector.explanations);
	}
	public void AddExplanations(List<string> explanations)
	{
		foreach (string explanation in explanations)
		{
			ExplanationController explain = Instantiate(explanationPrototype, textGroup.transform).GetComponent<ExplanationController>();
			explain.text.text = explanation;
		}
	}



	public void DisplayGasMineCost()
	{
		buttonText.text = gasMineCost.ToString();
	}
}
