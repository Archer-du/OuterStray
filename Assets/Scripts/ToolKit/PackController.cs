using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackController : MonoBehaviour
{
	public Button AddButton;
	public TMP_Text text;

	public CardInspect inspector;

	private int Num;
	public int num
	{
		get => Num;
		set
		{
			Num = value;
			if(Num >= 3)
			{
				AddButton.interactable = false;
			}
		}
	}
	public void Start()
	{
		num = 0;
		AddButton.onClick.AddListener(() => num++);
	}
}
