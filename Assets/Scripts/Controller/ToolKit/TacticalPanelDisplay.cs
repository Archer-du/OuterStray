using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticalPanelDisplay : MonoBehaviour
{
	public GameObject Panel;

	public Button castButton;
	public Button exitButton;

	public string category;
	public void Init(string category)
	{
		this.category = category;
		Panel = GameObject.Find("UI/Panels" + category + "Panel");
	}
	public void EnablePanel()
	{
	}
}
