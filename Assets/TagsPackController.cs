using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using DataCore.Cards;

public class TagsPackController : MonoBehaviour,
	IPointerClickHandler
{
	public static event Action<int> TagClicked;


	public PanelController panel;


	public CardInspector inspector;

	public GameObject tagPrototype;

	public List<PackTagController> tags;
	public Transform gridGroup;

	public CanvasGroup inspectorCanvas;

	public int index;

	[Header("External")]
	public CanvasGroup frameCanvas;

	public void Init(PanelController controller, int index)
	{
		tags = new List<PackTagController>();

		panel = controller;
		this.index = index;

	}

	public void FillPack(List<string> IDs)
	{
		for(int i = 0; i < IDs.Count; i++)
		{
			GameObject obj = Instantiate(tagPrototype, gridGroup);
			PackTagController tag = obj.GetComponent<PackTagController>();
			tags.Add(tag);

			tag.Init(i, IDs[i]);
			Card card = GameManager.GetInstance().pool.GetCardByID(IDs[i]);
			tag.nameText.text = card.name;
			switch (card.category)
			{
				case "LightArmor":
					tag.categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[11];
					break;
				case "Artillery":
					tag.categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[8];
					break;
				case "Motorized":
					tag.categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[9];
					break;
				case "Guardian":
					tag.categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[10];
					break;
				case "Construction":
					tag.categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[12];
					break;
				case "Command":
					tag.categoryIcon.sprite = Resources.LoadAll<Sprite>("Map-icon")[6];
					break;
			}
			int temp = i;
			tag.button.onClick.AddListener(() => DisplayInspector(tags[temp].ID));
			tag.button.onClick.AddListener(() => TagClicked?.Invoke(index));
		}
	}
	public void OnPointerClick(PointerEventData eventData)
	{
		panel.packSelectionIndex = index;
		panel.FinalConfirmButton.interactable = true;
		inspectorCanvas.DOFade(0, 0.1f);
	}
	public void DisplayInspector(string ID)
	{
		inspector.RenderInspector(ID);
		inspectorCanvas.alpha = 0;
		inspectorCanvas.DOFade(1, 0.1f);
	}

}
