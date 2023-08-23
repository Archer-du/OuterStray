using DataCore.Cards;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PanelType
{
	Base,
	OutPost,
	Supply,
	Medical
}
public class PanelController : MonoBehaviour,
	IPointerClickHandler
{
	public event Action<int> PackChosen;
	public event Action<int> MainConfirm;
	public event Action<int> SubConfirm;

	public static event Action PanelEnabled;
	public static event Action PanelDisabled;

	public GameObject packPrototype;

	public Button ExitButton;

	public RectTransform mask;

	public Transform GridGroup;
	public List<PackController> packs;

	public TextAsset randomDialog;

	[Header("DetailedInfo")]
	public bool detailShowing;

	public CanvasGroup detailedCard;
	public CanvasGroup detailedInfo;

	public Image detailedImage;

	public Image detailedMask;

	public TMP_Text attackText;
	public TMP_Text healthText;
	public TMP_Text counterText;


	[Header("Config")]
	public float finalHeight;
	public float duration;

	public Button mainConfirmButton;
	public Button subConfirmButton;

	public PanelType type;

	public ScrollRect scrollRect;

	public RectTransform content;

	[Header("Animation")]
	public Transform OperateBar;
	public Image BackGround;
	public Image NPCImage;
	public DialogController dialogger;

	public void Start()
	{
		gameObject.SetActive(false);
		transform.position = new Vector3(300, 0, 0);

		mainConfirmButton?.onClick.AddListener(() =>
		{
			MainConfirm?.Invoke(selection.deckID);
		});
		subConfirmButton?.onClick.AddListener(() =>
		{
			SubConfirm?.Invoke(selection.deckID);
		});

		ExitButton.onClick.AddListener(ClosePanel);
	}
	public void BuildPanel(PanelType type)
	{
		packs = new List<PackController>();
		this.type = type;
		switch (type)
		{
			//TODO 拆分
			case PanelType.Base:
				BackGround.sprite = Resources.Load<Sprite>("Ilustrate/back/admin");
				NPCImage.sprite = Resources.Load<Sprite>("Ilustrate/chara/admin");
				scrollRect.enabled = true;
				break;
			case PanelType.OutPost:
				BackGround.sprite = Resources.Load<Sprite>("Ilustrate/back/outpost");
				NPCImage.sprite = Resources.Load<Sprite>("Ilustrate/chara/merchant");
				scrollRect.enabled = true;
				break;
			case PanelType.Supply:
				BackGround.sprite = Resources.Load<Sprite>("Ilustrate/back/supply");
				NPCImage.enabled = false;
				dialogger.gameObject.SetActive(false);
				scrollBar.SetActive(false);
				GridGroup.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
				//scrollRect.enabled = false;
				break;
			case PanelType.Medical:
				BackGround.sprite = Resources.Load<Sprite>("Ilustrate/back/medical");
				NPCImage.sprite = Resources.Load<Sprite>("Ilustrate/chara/healer");
				scrollRect.enabled = false;
				scrollBar.SetActive(false);

				selectionCanvas.gameObject.SetActive(true);
				selection.gameObject.SetActive(false);
				mainConfirmButton.gameObject.SetActive(true);
				subConfirmButton.gameObject.SetActive(true);
				break;
		}
	}
	public void InitializePanel(List<string> IDs)
	{
		for (int i = 0; i < IDs.Count; i++)
		{
			GameObject pack = Instantiate(packPrototype, GridGroup);
			PackController controller = pack.GetComponent<PackController>();
			controller.Init();
			packs.Add(controller);

			controller.RenderInspector(IDs[i]);
		}
		for (int i = 0; i < packs.Count; i++)
		{
			int temp = i;
			packs[i].SelectButton.onClick.AddListener(() =>
			{
				Debug.Log(temp);
				PackChosen?.Invoke(temp);
			});
			packs[i].detailInfoButton.onClick.AddListener(() =>
			{
				detailShowing = true;
				EnableDetailedInfo(temp);
			});
		}
		content.sizeDelta = new Vector2(670 * IDs.Count, content.sizeDelta.y);
	}
	public void OpenPanel()
	{
		gameObject.SetActive(true);
		OperateBar.localPosition = new Vector3(0, -1300f, 0);
		BackGround.DOFade(0, 0.01f);
		NPCImage.DOFade(0, 0.01f);
		dialogger.canvas.DOFade(0, 0.01f);
		dialogger.text.gameObject.SetActive(false);

		float moveUpTime = 0.4f;
		float fadeTime = 0.3f;
		float textTime = 0.4f;

		Sequence seq = DOTween.Sequence();
		seq.AppendInterval(0.2f);
		seq.Append(OperateBar.DOLocalMove(new Vector3(0, -466, 0), moveUpTime));
		seq.Append(BackGround.DOFade(1f, fadeTime));
		seq.Append(NPCImage.DOFade(1f, fadeTime));
		seq.AppendInterval(fadeTime);
		seq.Append(dialogger.canvas.DOFade(1, 0.2f));
		//TODO
		if(type != PanelType.Supply)
		{
			seq.AppendCallback(() =>
			{
				dialogger.text.gameObject.SetActive(true);
				DOTween.To(
					() => "",
					value => dialogger.text.text = value,
					"Hello World!",
					textTime
				).SetEase(Ease.Linear);
			});
		}

		PanelEnabled?.Invoke();
		// 创建一个 Tweener 对象
		Tweener tweener = DOTween.To(
			// 获取初始值
			() => 0,
			// 设置当前值
			y => mask.sizeDelta = new Vector2(mask.sizeDelta.x, y),
			// 指定最终值
			finalHeight,
			// 指定持续时间
			duration
		);
	}
	public void ClosePanel()
	{
		PanelDisabled?.Invoke();
		// 创建一个 Tweener 对象
		Tweener tweener = DOTween.To(
			// 获取初始值
			() => finalHeight,
			// 设置当前值
			y => mask.sizeDelta = new Vector2(mask.sizeDelta.x, y),
			// 指定最终值
			0,
			// 指定持续时间
			duration
		).OnComplete(() =>
		{
			gameObject.SetActive(false);
		});
	}
	public void DisablePackButton(int index)
	{
		packs[index].SelectButton.interactable = false;
	}






	private float leftBound = 1940;
	private float rightBound = 2500;
	private float topBound = 950;
	private float bottomBound = 150;

	public CanvasGroup selectionCanvas;
	public CardInspector selection;

	public GameObject scrollBar;

	public void AddNewTag(Vector3 localPosition, string ID, int dynInfo, int deckID)
	{
		if ((localPosition.y > bottomBound && localPosition.y < topBound) && (localPosition.x > leftBound && localPosition.x < rightBound))
		{
			mainConfirmButton.enabled = false;
			subConfirmButton.enabled = false;

			selection.RenderInspector(ID, dynInfo);
			selection.deckID = deckID;
			selection.gameObject.SetActive(true);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if(detailShowing)
		{
			detailShowing = false;
			DisableDetailInfo();
		}
	}
	public void EnableDetailedInfo(int index)
	{
		float duration = 0.3f;

		detailedCard.gameObject.SetActive(true);
		detailedInfo.gameObject.SetActive(true);
		detailedMask.gameObject.SetActive(true);

		detailedMask.DOFade(0.5f, duration);

		detailedCard.alpha = 0f;
		detailedInfo.alpha = 0f;

		CardInspector card = packs[index].inspector;
		detailedImage.sprite = card.cardImage.sprite;
		attackText.enabled = card.category != "Command";
		healthText.enabled = card.category != "Command";
		if(card.category != "Command")
		{
			attackText.text = card.attackText.text;
			healthText.text = card.healthText.text;
		}
		else
		{
			detailedImage.rectTransform.sizeDelta = new Vector2(18, 24);
		}
		counterText.text = card.counterText.text;

		Sequence seq = DOTween.Sequence();

		seq.Append(detailedCard.DOFade(1, duration));
		seq.Append(detailedInfo.DOFade(1, duration));
	}
	public void DisableDetailInfo()
	{
		float duration = 0.3f;

		Sequence seq = DOTween.Sequence();

		seq.Append(detailedCard.DOFade(0, duration));
		seq.Join(detailedMask.DOFade(0, duration));
		seq.Join(detailedInfo.DOFade(0, duration))
			.OnComplete(() =>
			{
				detailedCard.gameObject.SetActive(false);
				detailedInfo.gameObject.SetActive(false);
				detailedMask.gameObject.SetActive(false);
			});
	}
}
