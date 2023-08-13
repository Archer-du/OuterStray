using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputMask : MonoBehaviour
{
	public Image inputMask;

	public Button castButton;
	public Button exitButton;
	public void Start()
	{
		castButton.onClick.AddListener(EnableInputMask);
		exitButton.onClick.AddListener(DisableInputMask);
	}
	public void EnableInputMask()
	{
		inputMask.raycastTarget = true;
	}
	public void DisableInputMask()
	{
		inputMask.raycastTarget = false;
	}
}
