using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseSelection : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler,
	IPointerClickHandler
{

	public CultivateSceneManager manager;
	public GameObject Frame;
	public int index;

	public float inspectFactor;

	public float duration;

	public Vector3 originScale;
	public Vector3 inspectScale;

	public bool disableExit;

	[Header("Display")]
	public string ID;
	public string names;
	public string category;
	public int health;
	public string description;

	public Color color;

	public Image nameTag;
	public Image backGround;
	public Image frame;
	public Image cardImage;
	public Image categoryIcon;

	public TMP_Text nameText;
	public TMP_Text descriptionText;

	public Image healthIcon;
	public TMP_Text healthText;

	public void OnPointerClick(PointerEventData eventData)
	{
		disableExit = true;
		Frame.SetActive(true);

		manager.baseSelectionIndex = index;
		manager.startExpedition.enabled = true;
		manager.startExpedition.image.color = Color.white;
		manager.ClearOtherSelectionFrame();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		transform.DOScale(inspectScale, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if(!disableExit)
		{
			transform.DOScale(originScale, duration);
		}
	}

	public void Start()
	{
		manager = GameManager.GetInstance().cultivateSceneManager;

		Frame.SetActive(false);

		originScale = transform.localScale;
		inspectScale = transform.localScale * inspectFactor;
	}



	public void SetInfo(string ID, string name, string category, int health, string description)
	{
		this.ID = ID;
		this.names = name;
		this.category = category;
		this.health = health;
		this.description = description;

		LoadCardResources(ID);

		nameText.text = name;
		healthText.text = health.ToString();
		descriptionText.text = description;
	}


	//TODO
	private void LoadCardResources(string ID)
	{
		cardImage.sprite = Resources.Load<Sprite>("CardImage/" + ID);

		switch (category)
		{
			case "LightArmor":
				UnityEngine.ColorUtility.TryParseHtmlString("#429656", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[11];
				break;
			case "Artillery":
				UnityEngine.ColorUtility.TryParseHtmlString("#CE8849", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[8];
				break;
			case "Motorized":
				UnityEngine.ColorUtility.TryParseHtmlString("#426A84", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[9];
				break;
			case "Guardian":
				UnityEngine.ColorUtility.TryParseHtmlString("#97A5A4", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[10];
				break;
			case "Construction":
				UnityEngine.ColorUtility.TryParseHtmlString("#7855A5", out color);
				categoryIcon.sprite = Resources.LoadAll<Sprite>("CardFrame/Atlas-Icon")[12];
				break;
			case "Command":
				color = Color.gray;
				cardImage.rectTransform.sizeDelta = new Vector2(10, 13);
				break;
		}

		backGround.color = color;
		frame.color = color;
		nameTag.color = color;
	}
}
