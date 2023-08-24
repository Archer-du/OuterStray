using DataCore.Cards;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PanelType
{
	Govern,
	CloningLab,
	WorkShop,
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

	public GameObject cardPackPrototype;
	public GameObject tagsPackPrototype;

	public Button FinalConfirmButton;
	public Button ExitButton;

	public RectTransform mask;

	public Transform GridGroup;
	public List<CardPackController> cardPacks;
	public List<TagsPackController> tagsPacks;

	public TextAsset randomDialog;

	[Header("DetailedInfo")]
	public bool detailShowing;

	public CanvasGroup detailedCard;
	public CanvasGroup detailedInfo;

	public Image detailedImage;

	public string backGroundStory;
	public TMP_Text backGroundStoryText;

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

	private int PackSelectionIndex;
	public int packSelectionIndex
	{
		get => PackSelectionIndex;
		set
		{
			PackSelectionIndex = value;
			foreach(TagsPackController pack in tagsPacks)
			{
				if(pack.index != value)
				{
					pack.frameCanvas.DOFade(0, 0.2f);
				}
				else
				{
					pack.frameCanvas.DOFade(1, 0.2f);
				}
			}
		}
	}

	public void Start()
	{
		packSelectionIndex = -1;
		FinalConfirmButton.interactable = false;
		FinalConfirmButton.onClick.AddListener(DisableAllTagsPackButtons);

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

		TagsPackController.TagClicked += ClearAllOtherInspector;
	}
	public void ClearAllOtherInspector(int index)
	{
		foreach (var tagsPack in tagsPacks)
		{
			if (tagsPack.index != index)
			{
				tagsPack.inspectorCanvas.DOFade(0, 0.2f);
			}
		}
	}




	public List<AudioSource> voices;
	public List<string> dialogs;
	public void BuildPanel(PanelType type)
	{

		this.type = type;
		switch (type)
		{
			//TODO 拆分
			case PanelType.Govern:
				tagsPacks = new List<TagsPackController>();
				BackGround.sprite = Resources.Load<Sprite>("Ilustrate/back/admin");
				NPCImage.sprite = Resources.Load<Sprite>("Ilustrate/chara/admin");
				scrollRect.enabled = true;
				scrollBar.SetActive(false);
				FinalConfirmButton.gameObject.SetActive(false);

				LoadDialogResource("Config\\Dialogs\\admin");
				LoadAudioResource("Config\\Voices\\admin\\", ".wav");
				break;
			case PanelType.CloningLab:
				tagsPacks = new List<TagsPackController>();
				BackGround.sprite = Resources.Load<Sprite>("Ilustrate/back/clone");
				NPCImage.sprite = Resources.Load<Sprite>("Ilustrate/chara/clone");
				scrollRect.enabled = true;
				scrollBar.SetActive(false);
				FinalConfirmButton.gameObject.SetActive(true);

				LoadDialogResource("Config\\Dialogs\\clone");
				LoadAudioResource("Config\\Voices\\clone\\", ".mp3");
				break;
			case PanelType.WorkShop:
				tagsPacks = new List<TagsPackController>();
				BackGround.sprite = Resources.Load<Sprite>("Ilustrate/back/fac");
				NPCImage.sprite = Resources.Load<Sprite>("Ilustrate/chara/fac");
				scrollRect.enabled = true;
				scrollBar.SetActive(false);
				FinalConfirmButton.gameObject.SetActive(true);

				LoadDialogResource("Config\\Dialogs\\fac");
				LoadAudioResource("Config\\Voices\\fac\\", ".mp3");
				break;
			case PanelType.OutPost:
				cardPacks = new List<CardPackController>();
				BackGround.sprite = Resources.Load<Sprite>("Ilustrate/back/outpost");
				NPCImage.sprite = Resources.Load<Sprite>("Ilustrate/chara/merchant");
				scrollRect.enabled = true;

				LoadDialogResource("Config\\Dialogs\\merchant");
				LoadAudioResource("Config\\Voices\\merchant\\", ".wav");
				break;
			case PanelType.Supply:
				cardPacks = new List<CardPackController>();
				BackGround.sprite = Resources.Load<Sprite>("Ilustrate/back/supply");
				BackGround.color = Color.white;
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


				LoadDialogResource("Config\\Dialogs\\healer");
				LoadAudioResource("Config\\Voices\\healer\\", ".wav");
				break;
		}
	}
	public void LoadDialogResource(string path)
	{
		dialogs = new List<string>();
		StreamReader reader = GameManager.GetInstance().OpenText(path);
		string dialog = reader.ReadLine();
		while (dialog != null)
		{
			dialogs.Add(dialog);
			dialog = reader.ReadLine();
		}
	}
	public void LoadAudioResource(string path, string format)
	{
		voices = new List<AudioSource>();
		int i = 0;

		string audioPath = path + i.ToString();

		AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
		while (audioClip != null)
		{
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();

			audioSource.clip = audioClip;
			audioSource.playOnAwake = false;
			audioSource.loop = false;

			voices.Add(audioSource);
			i++;
			audioClip = Resources.Load<AudioClip>(path + i.ToString());
		}
	}





	public void FillTagPack(List<List<string>> IDPacks)
	{
		for(int i = 0; i < IDPacks.Count; i++)
		{
			GameObject pack = Instantiate(tagsPackPrototype, GridGroup);
			TagsPackController controller = pack.GetComponent<TagsPackController>();
			controller.Init(this, i);
			controller.FillPack(IDPacks[i]);
			tagsPacks.Add(controller);
		}
		content.sizeDelta = new Vector2(670 * IDPacks.Count, content.sizeDelta.y);
	}
	public void FillCardPack(List<string> IDs)
	{
		for (int i = 0; i < IDs.Count; i++)
		{
			GameObject pack = Instantiate(cardPackPrototype, GridGroup);
			CardPackController controller = pack.GetComponent<CardPackController>();
			controller.Init(this);
			cardPacks.Add(controller);

			controller.RenderInspector(IDs[i]);

			if(type == PanelType.OutPost)
			{
				controller.DisplayGasMineCost();
			}
		}
		for (int i = 0; i < cardPacks.Count; i++)
		{
			int temp = i;
			cardPacks[i].SelectButton.onClick.AddListener(() =>
			{
				PackChosen?.Invoke(temp);
				if(type == PanelType.Govern)
				{
					DisablePackButton(temp);
                }
			});
			cardPacks[i].detailInfoButton.onClick.AddListener(() =>
			{
				detailShowing = true;
				EnableDetailedInfo(temp);
			});
		}
		content.sizeDelta = new Vector2(660 * IDs.Count, content.sizeDelta.y);
	}








	float textTime = 0.4f;

	public void OpenPanel()
	{
		gameObject.SetActive(true);

		FinalConfirmButton.interactable = false;

		transform.position = new Vector3(300, 0, 0);
		OperateBar.localPosition = new Vector3(0, -1300f, 0);
		BackGround.DOFade(0, 0f);
		NPCImage.DOFade(0, 0f);
		dialogger.canvas.DOFade(0, 0f);
		dialogger.text.gameObject.SetActive(false);

		float moveUpTime = 0.4f;
		float fadeTime = 0.3f;

		Sequence seq = DOTween.Sequence();
		seq.AppendInterval(0.2f);
		seq.Append(OperateBar.DOLocalMove(new Vector3(0, -466, 0), moveUpTime));
		seq.Append(BackGround.DOFade(1f, fadeTime));
		seq.Append(NPCImage.DOFade(1f, fadeTime));
		seq.Append(dialogger.canvas.DOFade(0.6f, fadeTime));
		//TODO
		if(type != PanelType.Supply)
		{
			seq.AppendCallback(() => voices[0].Play());
			seq.AppendCallback(() =>
			{
				dialogger.text.gameObject.SetActive(true);
				DOTween.To(
					() => "",
					value => dialogger.text.text = value,
					dialogs[0],
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
		foreach (var tagsPack in tagsPacks)
		{
			tagsPack.inspectorCanvas.alpha = 0;
		}
	}
	public void DisablePackButton(int index)
	{
		cardPacks[index].SelectButton.interactable = false;
	}
	public void DisableAllCardPackButtons()
	{
		foreach(var cardPack in cardPacks)
		{
			if (cardPack.SelectButton.interactable)
			{
				cardPack.SelectButton.image.color = Color.gray;
				cardPack.SelectButton.enabled = false;
			}
		}
	}
	public void DisableAllTagsPackButtons()
	{
		foreach (var tagsPack in tagsPacks)
		{
			tagsPack.frameCanvas.alpha = 0;
		}
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
			mainConfirmButton.enabled = true;
			subConfirmButton.enabled = true;

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
		if(type != PanelType.Supply)
		{
			foreach(AudioSource source in voices)
			{
				source.Stop();
			}

			if(eventData.position.y > Screen.height / 2)
			{
				dialogger.text.gameObject.SetActive(true);

				int index = UnityEngine.Random.Range(1, voices.Count);
				voices[index].Play();

				string dialog = dialogs[index];

				DOTween.To(
					() => "",
					value => dialogger.text.text = value,
					dialog,
					textTime
				).SetEase(Ease.Linear);
			}
		}
		foreach(CardPackController pack in cardPacks)
		{
			pack.explainCanvas.DOFade(0, 0.2f);
		}
		ClearAllOtherInspector(-1);
	}





	public void EnableDetailedInfo(int index)
	{
		float duration = 0.3f;

		foreach (CardPackController pack in cardPacks)
		{
			pack.explainCanvas.DOFade(0, 0.2f);
		}

		detailedCard.gameObject.SetActive(true);
		detailedInfo.gameObject.SetActive(true);
		detailedMask.gameObject.SetActive(true);

		detailedMask.DOFade(0.75f, duration);

		detailedCard.alpha = 0f;
		detailedInfo.alpha = 0f;

		CardInspector card = cardPacks[index].inspector;
		detailedImage.sprite = card.cardImage.sprite;
		attackText.enabled = card.category != "Command";
		healthText.enabled = card.category != "Command";
		if(card.category != "Command")
		{
			attackText.text = card.attackText.text;
			healthText.text = card.healthText.text;
			detailedImage.rectTransform.sizeDelta = new Vector2(18, 19);
		}
		else
		{
			detailedImage.rectTransform.sizeDelta = new Vector2(18, 24);
		}
		counterText.text = card.counterText.text;

		Sequence seq = DOTween.Sequence();

		seq.Append(detailedCard.DOFade(1, duration));
		seq.Append(detailedInfo.DOFade(1, duration));
		backGroundStoryText.gameObject.SetActive(true);
		DOTween.To(
			() => "",
			value => backGroundStoryText.text = value,
			"背景故事：\n",
			duration
		).SetEase(Ease.Linear);
	}
	public void DisableDetailInfo()
	{
		float duration = 0.3f;

		Sequence seq = DOTween.Sequence();

		seq.Append(detailedCard.DOFade(0, duration));
		seq.Join(detailedMask.DOFade(0, duration));
		seq.Join(backGroundStoryText.DOFade(0, duration));
		seq.Join(detailedInfo.DOFade(0, duration))
			.OnComplete(() =>
			{
				detailedCard.gameObject.SetActive(false);
				detailedInfo.gameObject.SetActive(false);
				detailedMask.gameObject.SetActive(false);
				backGroundStoryText.gameObject .SetActive(false);
			});
	}
}
