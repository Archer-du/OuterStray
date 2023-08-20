using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleLineDisplay : MonoBehaviour
{
	public BattleLineController controller;

	public Image frame;
	public Image selectionFrame;
	public int capacity
	{
		get => controller.capacity;
	}

	private RectTransform size
	{
		get => frame.rectTransform;
	}
	private RectTransform selectionSize
	{
		get => selectionFrame.rectTransform;
	}
	private float width;
	private int ownership
	{
		get => controller.ownership;
	}

	void Start()
	{
		width = (capacity / 6f) * 2500; //TODO config

		size.sizeDelta = new Vector2(width, size.sizeDelta.y);
		selectionSize.sizeDelta = new Vector2 (width + 100, size.sizeDelta.y);

		selectionFrame.gameObject.SetActive(false);

		frame.color = ownership == 0 ? Color.blue : Color.red;
	}

	public float duration;

	public void UpdateInfo()
	{
		if (ownership == 0)
		{
			frame.DOColor(Color.blue, duration);
		}
		else
		{
			frame.DOColor(Color.red, duration);
		}
	}
	public void DisplaySelectionFrame()
	{
		selectionFrame.gameObject.SetActive(true);
	}
	public void DisableSelectionFrame()
	{
		selectionFrame.gameObject.SetActive(false);
	}
}
