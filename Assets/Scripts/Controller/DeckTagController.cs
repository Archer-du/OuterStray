using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class DeckTagController : MonoBehaviour, IComparable<DeckTagController>,
		IDragHandler,
		IBeginDragHandler, IEndDragHandler,
		IPointerEnterHandler, IPointerExitHandler
{
	public DeckController controller;
	public PanelController panel
	{
		get => controller.sceneManager.currentNode.panel;
	}

	[Header("Data")]
	public int deckID;

	[Header("DeckTag")]
	public TMP_Text deckNameText;
	public Image deckCategoryIcon;


	[Header("Display")]
	public Transform buffer;

	public Image ground;
	public float duration;

	[Header("Inspector")]
	public CardInspector inspector; 

	public void Init(string ID, int dynInfo)
	{
		inspector.RenderInspector(ID, dynInfo);

		deckNameText.text = inspector.nameContent;
		deckCategoryIcon.sprite = inspector.categoryIcon.sprite;
	}
	public void Init(string ID)
	{
		inspector.RenderInspector(ID);

		deckNameText.text = inspector.nameContent;
		deckCategoryIcon.sprite = inspector.categoryIcon.sprite;
	}


	public Transform Component;
	public float initDuration;

	public Color color;


	public void OnBeginDrag(PointerEventData eventData)
	{
		if (GameManager.GetInstance().gameState != SceneState.GameState.Tactical) return;
		if (!controller.sceneManager.panelEnabled) return;
		if (controller.sceneManager.currentNode is not MedicalNodeController) return;
		if (inspector.category == "Command") return;
		GetComponent<InspectPanelController>().fadeDisable = true;
		GetComponent<InspectPanelController>().inspectPanel.alpha = 1.0f;

	}

	public void OnDrag(PointerEventData eventData)
	{
		if (GameManager.GetInstance().gameState != SceneState.GameState.Tactical) return;
		if (!controller.sceneManager.panelEnabled) return;
		if (controller.sceneManager.currentNode is not MedicalNodeController) return;
		if (inspector.category == "Command") return;

		Vector2 localPosition = new Vector2((eventData.position.x / Screen.width) * 3840, (eventData.position.y / Screen.height) * 2160);

		//TODO
		GetComponent<InspectPanelController>().inspectPanel.alpha = 1.0f;
		inspector.transform.position = localPosition - BattleElementController.inputOffset;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		GetComponent<InspectPanelController>().fadeDisable = false;
		if (GameManager.GetInstance().gameState != SceneState.GameState.Tactical) return;
		if (!controller.sceneManager.panelEnabled) return;
		if (controller.sceneManager.currentNode is not MedicalNodeController) return;
		if (inspector.category == "Command") return;

		Vector2 localPosition = new Vector2((eventData.position.x / Screen.width) * 3840, (eventData.position.y / Screen.height) * 2160);
		inspector.transform.localPosition = originPosition;

		Debug.Log(localPosition);
		panel.AddNewTag(localPosition, inspector.ID, inspector.category == "Command" ? inspector.counter : inspector.health, deckID);

		GetComponent<InspectPanelController>().inspectPanel.alpha = 0f;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (controller.deckTagLock) return;
		ground.DOFade(0.5f, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (controller.deckTagLock) return;
		ground.DOFade(0, duration);
	}




	private Vector3 originPosition;
	public void Start()
	{
		originPosition = inspector.transform.localPosition;
	}
	//TODO
	public int CompareTo(DeckTagController other)
	{
		if(inspector.category == other.inspector.category)
		{
			return inspector.cost.CompareTo(other.inspector.cost);
		}
		else return inspector.category.CompareTo(other.inspector.category);
	}
}
