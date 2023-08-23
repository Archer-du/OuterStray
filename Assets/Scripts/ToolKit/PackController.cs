using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackController : MonoBehaviour
{
	public PanelController panel;

	public CardInspector inspector;

	public Button detailInfoButton;

	public Button SelectButton;

	public TMP_Text text;

	public int gasMineCost
	{
		get => inspector.gasMineCost;
	}
	private int Num;
	public int num
	{
		get => Num;
		set
		{
			Num = value;
			if(Num >= 3)
			{
				SelectButton.interactable = false;
			}
		}
	}

	public void Init()
	{
		num = 0;
		SelectButton.onClick.AddListener(() => num++);

		detailInfoButton = inspector.nameTag.gameObject.AddComponent<Button>();
	}

	public void RenderInspector(string ID)
	{
		inspector.RenderInspector(ID);
	}
	public void DisplayGasMineCost()
	{
	}
}
