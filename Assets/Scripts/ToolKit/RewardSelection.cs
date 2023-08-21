using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardSelection : MonoBehaviour,
	IPointerEnterHandler, IPointerExitHandler,
	IPointerClickHandler
{
	//后续改为泛型 TODO
	public BattleSceneManager manager;
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
	public int cost;
	public int attack;
	public int health;
	public int counter;
	public string description;

	public Color color;

	public Image nameTag;
	public Image costTag;

	public Image backGround;
	public Image frame;
	public Image cardImage;
	public Image categoryIcon;

	public TMP_Text costText;
	public TMP_Text nameText;
	public TMP_Text descriptionText;

	public TMP_Text attackText;
	public TMP_Text healthText;
	public TMP_Text counterText;

	public Image attackIcon;
	public Image healthIcon;
	public void OnPointerClick(PointerEventData eventData)
	{
		disableExit = true;
		Frame.SetActive(true);

		manager.rewardSelectionIndex = index;
		manager.rewardConfirmButton.interactable = true;
		manager.ClearOtherSelectionFrame();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		transform.DOScale(inspectScale, duration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!disableExit)
		{
			transform.DOScale(originScale, duration);
		}
	}

	public void Start()
	{
		manager = GameManager.GetInstance().battleSceneManager;
		Frame.SetActive(false);

		originScale = transform.localScale;
		inspectScale = transform.localScale * inspectFactor;
	}
	public void SetInfo(string ID, string name, string category, int cost, int attack, int health, int counter, string description)
	{
		this.ID = ID;
		this.names = name;
		this.category = category;
		this.cost = cost;
		this.attack = attack;
		this.health = health;
		this.counter = counter;
		this.description = description;

		LoadCardResources(ID);

		nameText.text = name;
		costText.text = cost.ToString();
		if (category == "Command")
		{
			attackIcon.enabled = false;
			attackText.enabled = false;
			healthIcon.enabled = false;
			healthText.enabled = false;
		}
		else
		{
			attackText.text = attack.ToString();
			healthText.text = health.ToString();
		}
		counterText.text = category == "Construction" ? "" : counter.ToString();
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
				break;
		}

		backGround.color = color;
		frame.color = color;
		nameTag.color = color;
		costTag.color = color;
	}
}
