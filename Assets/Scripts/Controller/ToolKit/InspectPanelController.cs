using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class InspectPanelController : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
	public float timeValve = 0.6f;
	public float duration = 0.2f;
	public float displayOffset = 400f;
	public CanvasGroup inspectPanel;

	private float timer;

	public void OnPointerEnter(PointerEventData eventData)
	{
		timer = timeValve;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		timer = -1;
		inspectPanel.DOFade(0f, duration);
	}

	void Start()
	{
		inspectPanel.alpha = 0f;
	}
    void Update()
    {
        if(timer > 0)
		{
			timer -= Time.deltaTime;
			if(timer <= 0)
			{
				inspectPanel.transform.position = gameObject.transform.position + displayOffset * Vector3.right;
				inspectPanel.DOFade(1f, duration);
			}
		}
    }
}
