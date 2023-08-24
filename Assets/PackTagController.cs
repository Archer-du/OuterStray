using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackTagController : MonoBehaviour
{

	public Button button;

	public int index;

	public string ID;

	public Image categoryIcon;
	public TMP_Text nameText;

	public void Init(int index, string ID)
	{
		this.index = index;
		this.ID = ID;
	}
}
