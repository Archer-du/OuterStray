using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DisplayInterface;
using TMPro;
using UnityEngine.EventSystems;
using DataCore.Cards;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Rendering;
using InputHandler;
using DataCore.BattleElements;
using System;
using UnityEngine.SceneManagement;

public class BattleElementController : MonoBehaviour
{
	public event Action<ElementState> OnElementStateChanged;

	public static event Action GlobalAnimeLocked;
	public static event Action GlobalAnimeUnlocked;

	public event Action AnimeLocked;
	public event Action AnimeUnlocked;

	public static bool targetSelectionLock = false;

	private static bool DraggingLock = false;
	public static bool draggingLock
	{
		get => DraggingLock;
		set
		{
			DraggingLock = value;
			if(value == true)
			{
				GlobalAnimeLocked?.Invoke();
			}
			else
			{
				GlobalAnimeUnlocked?.Invoke();
			}
		}
	}
	[SerializeField] private bool InspectLock;
	public bool inspectLock
	{
		get => InspectLock;
		set
		{
			InspectLock = value;
			if(value == true)
			{
				AnimeLocked?.Invoke();
			}
			else
			{
				AnimeUnlocked?.Invoke();
			}
		}
	}
	public bool inputLock;

	[Header("StaticInfo")]
	public static Vector2 inputOffset = new Vector2(1980, 1080);
	public static int cardWidth = 360;


	[Header("BasicInfo")]
	public Vector3 battleFieldScale;
	public Vector3 handicapScale;
	public Vector3 castScale;
	public Vector3 targetScale;
	public Vector3 showScale;

	public Vector3 targetTextScale;
	public Vector3 originTextScale;

	public Vector3 counterScaleOrigin;
	public Vector3 counterScaleEnlarge;

	public Vector3 handicapLogicPosition;

	public float counterfontSizeOrigin;
	public float counterfontSizeEnlarge;

	public float normalFontSizeOrigin;
	public float normalFontSizeEnlarge;

	[Header("Connections")]
	public BattleSceneManager battleSceneManager;
	public Transform stack
	{
		get => battleSceneManager.cardStackController[ownership].transform;
	}

	private int HandicapOrder;
	public int handicapOrder
	{
		get => HandicapOrder;
		set
		{
			HandicapOrder = value;
			canvas.sortingOrder = value;
		}
	}
	private int BattleOrder;
	public int battleOrder
	{
		get => BattleOrder;
		set
		{
			BattleOrder = value;
			canvas.sortingOrder = value;
		}
	}
	public Canvas canvas;

	public CanvasGroup selfCanvas;

	public HandicapController handicap
	{
		get => battleSceneManager.handicapController[ownership];
	}

	[Header("Data")]
	[SerializeField] private ElementState DataState;
	public ElementState dataState
	{
		get => DataState;
		set
		{
			DataState = value;
			OnElementStateChanged?.Invoke(DataState);
		}
	}
	public int handicapIdx;

	public string ID;
	public int ownership;
	public string nameContent;
	public string category;
	public string description;
	public int cost;

	public List<string> explanations;

	[Header("Image")]
	public Image CardImage;
	public Image InspectorImage;

	public Image elementGround;
	public Image elementShell;
	public Image elementFrame;
	public Image NameTag;
	public Image costTag;

	[Header("Text")]
	public TMP_Text nameText;
	public TMP_Text costText;

	[Header("Inspector")]
	public CanvasGroup InspectPanel;

	public GameObject InspectComponent;

	public GameObject counterIcon;
	public Image categoryIcon;
	public Image componentFrame;
	public TMP_Text componentDescriptionText;
	public Vector3 componentPosition;

	[Header("Components")]
	public HandicapInspector handicapInspect;
	public InspectPanelController inspectPanel;


	public void TransformInfoInit()
	{
		componentPosition = InspectComponent.transform.position;

		battleFieldScale = transform.localScale;
		handicapScale = 1.35f * battleFieldScale;
		castScale = 2f * battleFieldScale;
		targetScale = 1.25f * battleFieldScale;
		showScale = 2f * battleFieldScale;

		targetTextScale = nameText.transform.localScale * 1.35f;
		originTextScale = nameText.transform.localScale;

		counterScaleOrigin = new Vector3(1, 1, 1);
		counterScaleEnlarge = new Vector3(1.5f, 1.5f, 1);

		counterfontSizeOrigin = 36;
		counterfontSizeEnlarge = 60;

		normalFontSizeOrigin = 40;
		normalFontSizeEnlarge = 50;
	}


	public void Init(string ID, int ownership, string name, string categories, int cost, string description)
	{
		//data
		this.ID = ID;
		this.ownership = ownership;
		this.nameContent = name;
		this.category = categories;
		this.description = description;

		explanations = new List<string>();
		ExplanationParse(category, description, ref explanations);

		inspectLock = false;
		inputLock = false;
		//输入偏移量
		inputOffset = new Vector2(1980, 1080);

		//transform
		TransformInfoInit();

		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();

		handicapInspect = GetComponent<HandicapInspector>();
		handicapInspect.Init(handicapScale, this);

		inspectPanel = GetComponent<InspectPanelController>();
		inspectPanel.Init();

		inspectPanel.AddExplanation(explanations);

		nameText.text = name;
		costText.text = cost.ToString();
		componentDescriptionText.text = description;

		LoadCardResources(ID);
	}

	public Color color;
	/// <summary>
	/// 根据ID索引加载卡牌资源
	/// </summary>
	/// <param name="ID"></param>
	private void LoadCardResources(string ID)
	{
		CardImage.sprite = Resources.Load<Sprite>("CardImage/" + ID);

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
				CardImage.rectTransform.sizeDelta = new Vector2(10, 13);
				break;
		}

		elementGround.color = color;
		elementFrame.color = color;
		componentFrame.color = color;
		NameTag.color = color;
		costTag.color = color;
	}





	public int GetBattleLineIdx(float y)
	{
		if (y > BattleLineController.fieldLowerBound && y < BattleLineController.fieldUpperBound)
		{
			int idx = (int)((y - 180) / (BattleLineController.lineWidth + BattleLineController.lineInterval));
			if (idx < 0 || idx > battleSceneManager.fieldCapacity - 1)
			{
				return -1;
			}
			return idx;
		}
		return -1;
	}








	public static void ExplanationParse(string category, string description, ref List<string> explanations)
	{
		switch(category)
		{
			case "LightArmor":
				explanations.Add("<b>兵种：轻装</b>\n近战攻击敌人");
				break;
			case "Motorized":
				explanations.Add("<b>兵种：机动</b>\n移动时减少攻击计数器");
				break;
			case "Artillery":
				explanations.Add("<b>兵种：轰击</b>\n随机攻击敌方目标");
				break;
			case "Guardian":
				explanations.Add("<b>兵种：重装</b>\n嘲讽攻击范围内的单位");
				break;
			case "Construction":
				explanations.Add("<b>兵种：建筑</b>\n无法移动");
				break;
			case "Command":
				explanations.Add("<b>指令</b>\n产生即时效果");
				break;
		}
		if (description.Contains("部署"))
		{
			explanations.Add("<b>部署</b>\n自身部署后触发的效果");
		}
		if (description.Contains("招募"))
		{
			explanations.Add("<b>招募</b>\n从牌堆招募指定单位到战场");
		}
		if (description.Contains("突击"))
		{
			explanations.Add("<b>突击</b>\n部署后可以立即行动");
		}
		if (description.Contains("越野"))
		{
			explanations.Add("<b>越野</b>\n可以移动至不相邻的战线");
		}
		if (description.Contains("顺劈"))
		{
			explanations.Add("<b>顺劈</b>\n同时攻击范围内的所有敌人");
		}
		if (description.Contains("反击"))
		{
			explanations.Add("<b>反击</b>\n受击后对攻击者造成等同于自身攻击力的伤害");
		}
		if (description.Contains("阵亡"))
		{
			explanations.Add("<b>阵亡</b>\n自身阵亡后触发的效果");
		}
		if (description.Contains("护甲(x)"))
		{
			explanations.Add("<b>护甲(x)</b>\n减少受到的伤害x点");
		}
		if (description.Contains("格挡"))
		{
			explanations.Add("<b>格挡</b>\n免疫下次受到的伤害");
		}
		if (description.Contains("压制"))
		{
			explanations.Add("<b>压制</b>\n使目标在本回合内无法行动");
		}
		if (description.Contains("召唤"))
		{
			explanations.Add("<b>召唤</b>\n生成特定单位到战场");
		}
	}

}
