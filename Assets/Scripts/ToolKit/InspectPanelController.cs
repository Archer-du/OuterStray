using DataCore.BattleElements;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class InspectPanelController : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
	public bool active;
	public bool disable;
	public bool boundaryCorrection;

	public float upperBound;
	public float lowerBound;
	public float leftBound;
	public float rightBound;

	public float timerValve = 0.6f;
	public float duration = 0.2f;
	public Vector3 displayOffset;
	public CanvasGroup inspectPanel;

	public CanvasGroup SubPanel;

	public bool mainPanelEnabled;
	public bool subPanelEnabled;

	private float timer;

	public void DisablePanel()
	{
		if (inspectPanel != null) inspectPanel.DOFade(0f, duration);
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
		if (!disable)
		{
			if (inspectPanel != null) inspectPanel.DOFade(0f, duration);
		}
	}

	void Start()
	{
		if(inspectPanel != null) inspectPanel.alpha = 0f;
		disable = false;
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
						inspectPanel.transform.position = gameObject.transform.position + displayOffset;
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
					if (SubPanel != null && subPanelEnabled) SubPanel.DOFade(1f, duration);
				}
			}
		}
    }


	//TODO remove
	public void OnElementStateChanged(ElementState state)
	{
		if(state == ElementState.inBattleLine)
		{
			active = true;
		}
		else
		{
			active = true;
			mainPanelEnabled = false;
		}
	}
}
