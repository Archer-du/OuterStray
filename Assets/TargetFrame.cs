using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetFrame : MonoBehaviour
{
	public Image targetFrame;

	public void Start()
	{
		targetFrame.enabled = false;
	}
	public void EnableSelectFrame()
	{
		targetFrame.enabled = true;
	}
	public void DisableSelectFrame()
	{
		targetFrame.enabled = false;
	}
}
