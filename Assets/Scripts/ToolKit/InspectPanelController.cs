using DataCore.BattleElements;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InspectPanelController : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
	public bool active;
	public bool fadeDisable;
	public bool boundaryCorrection;

	public float upperBound;
	public float lowerBound;
	public float leftBound;
	public float rightBound;

	public float timerValve = 0.6f;
	public float duration = 0.2f;

	[Header("Offset")]
	public Vector3 mainDisplayOffset;
	public Vector3 subDisplayOffset;

	[Header("Panels")]
	public CanvasGroup inspectPanel;

	public CanvasGroup SubPanel;
	public GridLayoutGroup textGroup;

	public bool mainPanelEnabled;
	public bool subPanelEnabled;

	public Vector3 mainPanelScale;
	public Vector3 subPanelScale;

	private float timer;

	public void DisablePanel()
	{
		if (inspectPanel != null) inspectPanel.DOFade(0f, duration);
		if (SubPanel != null) SubPanel.DOFade(0f, duration);
	}
	public void OnPointerEnter(PointerEventData eventData)
	{
		if(active)
		{
			timer = timerValve;
		}
		else
		{
			timer = -1;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		timer = -1;
		if (!fadeDisable)
		{
			if (inspectPanel != null) inspectPanel.DOFade(0f, duration);
			if (SubPanel != null) SubPanel.DOFade(0f, duration);
		}
	}

	void Start()
	{
		if(inspectPanel != null) inspectPanel.alpha = 0f;
		fadeDisable = false;
	}
	public void Init()
	{
		if (inspectPanel != null) mainPanelScale = inspectPanel.transform.localScale;
		if (SubPanel != null) subPanelScale = SubPanel.transform.localScale;
	}


    void Update()
    {
        if(timer > 0)
		{
			timer -= Time.deltaTime;
			if(timer <= 0)
			{
				if (active)
				{
					if (inspectPanel != null && mainPanelEnabled)
					{
						inspectPanel.transform.position = gameObject.transform.position + mainDisplayOffset;
						if (boundaryCorrection)
						{
							if(inspectPanel.transform.position.y > upperBound)
							{
								inspectPanel.transform.position = new Vector3(inspectPanel.transform.position.x, upperBound, inspectPanel.transform.position.z);
							}
							if(inspectPanel.transform.position.y < lowerBound)
							{
								inspectPanel.transform.position = new Vector3(inspectPanel.transform.position.x, lowerBound, inspectPanel.transform.position.z);
							}
							if(inspectPanel.transform.position.x < leftBound)
							{
								inspectPanel.transform.position = new Vector3(leftBound, inspectPanel.transform.position.y, inspectPanel.transform.position.z);
							}
							if (inspectPanel.transform.position.x > rightBound)
							{
								inspectPanel.transform.position = new Vector3(rightBound, inspectPanel.transform.position.y, inspectPanel.transform.position.z);
							}
						}
						inspectPanel.DOFade(1f, duration);
					}
					if (SubPanel != null && subPanelEnabled)
					{
						SubPanel.transform.position = gameObject.transform.position + subDisplayOffset;
						SubPanel.DOFade(1f, duration);
					}
				}
			}
		}
    }

	public GameObject explanationPrototype;
	public void AddExplanation(List<string> explanations)
	{
		foreach(string explanation in explanations)
		{
			ExplanationController explain = Instantiate(explanationPrototype, textGroup.transform).GetComponent<ExplanationController>();
			explain.text.text = explanation;
		}
	}








	//TODO remove
	public void OnElementStateChanged(ElementState state)
	{
		if(state == ElementState.inBattleLine)
		{
			active = true;
			if(inspectPanel != null) mainPanelEnabled = true;

			subDisplayOffset = new Vector3(1100, 0, 0);

			SubPanel.transform.localScale = subPanelScale * 1.5f;
		}
		else
		{
			active = true;
			mainPanelEnabled = false;

			subDisplayOffset = new Vector3(0, 800, 0);
		}
	}
}
