using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneState;
using UnityEngine.UI;
using InputHandler;
using LogicCore;
using DisplayInterface;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.IO.Pipes;
using UnityEngine.SceneManagement;
using DataCore.Cards;
using System;

public class CultivateSceneManager : MonoBehaviour,
    ICultivateSceneController
{
    public ICultivationSystemInput cultivateSystem;

    private GameManager gameManager;

    public DeckController playerDeck;
    //public DepartmentController buildingController;

    public CanvasGroup inputMask;

	public bool panelEnabled;

    [Header("Text")]
	public TMP_Text gasMineText;
	public TMP_Text cardNumText;

	public Image baseDashBoard;
	public Image baseComponent;
	public TMP_Text baseHealthText;
	public TMP_Text baseMaxHealthText;

    [Header("Prototype")]
    public GameObject baseCardPrototype;
    public GameObject packPrototype;

	[Header("Interaction")]
    public Button apron;
    public Button govern;

    public Button startExpedition;

    [Header("Components")]
    public BaseSelection[] selections;
	public PanelController[] Panels;

	public int baseSelectionIndex;

	public void Start()
    {
        gameManager = GameManager.GetInstance();

        GameManager.OnGameStateChanged += OnGameStateChanged;

        //note: 功能从manager下放到controller
        apron.onClick.AddListener(BaseChoose);

        startExpedition.onClick.AddListener(StartExpedition);

        buildingsDisabled = false;

        startExpedition.enabled = false;
        startExpedition.image.color = Color.gray;

		//TODO
		List<string> ID = new List<string>();
		for(int i = 0; i < gameManager.pool.humanCardNum; i++)
		{
			ID.Add(gameManager.pool.humanCardPool[i].backendID);
		}
		Panels[0].BuildPanel(PanelType.Base);
		govern.onClick.AddListener(Panels[0].OpenPanel);
		Panels[0].InitializePanel(ID);

		Panels[0].PackChosen += AddDeckTagFromPool;

		PanelController.PanelEnabled += () => panelEnabled = true;
		PanelController.PanelDisabled += () => panelEnabled = false;

		baseHealthText.gameObject.SetActive(false);
		baseMaxHealthText.gameObject.SetActive(false);
		baseComponent.gameObject.SetActive(false);
	}
    public void OnGameStateChanged(GameState state)
    {
        if(state == GameState.Start)
        {
            Destroy(this);
        }
		else
        {
			if(state == GameState.Tactical)
			{
				baseHealthText.gameObject.SetActive(true);
				baseMaxHealthText.gameObject.SetActive(true);
				baseComponent.gameObject.SetActive(true);
			}
			transform.SetParent(gameManager.transform);
        }
    }
	public void DestroyOtherInstancesOfType()
	{
		// 查找同类型的所有游戏对象
		CultivateSceneManager[] otherInstances = UnityEngine.Object.FindObjectsOfType<CultivateSceneManager>();

		// 遍历并销毁除自身以外的同类型游戏对象
		foreach (CultivateSceneManager instance in otherInstances)
		{
			if (instance != this)
			{
				Destroy(instance.gameObject);
			}
		}
	}



	public float finalHeight;
	//TODO remove
	public void EnablePanel()
	{

	}
	public void DisablePanel()
	{
		// 创建一个 Tweener 对象

	}






	public float duration;
    //TEST
	public void BaseChoose()
    {
		if (playerDeck.IsEmpty())
		{
			Debug.LogWarning("你还没有导入卡组！");
			return;
		}
		duration = 0.5f;

        DisableAllBuildings();
        playerDeck.DisableAllDeckTags();
		inputMask.DOFade(0.3f, duration);

        for(int i = 0; i < 3; i++)
        {
            selections[i].transform.SetParent(transform.Find("UI"));
            selections[i].index = i;
            selections[i].transform.DOBlendableMoveBy(new Vector3(-3000 + i * 800, 0, 0), duration);
            selections[i].transform.DOBlendableRotateBy(new Vector3(0, -90, 0), duration);
		}
        startExpedition.transform.DOMove(new Vector3(300, -800, 0), duration);
	}
    public void StartExpedition()
    {
		inputMask.alpha = 0;

		cultivateSystem.SetBase(baseSelectionIndex);

        playerDeck.EnableAllDeckTags();
        startExpedition.transform.position = new Vector3(0, -1200, 0);
		// 查找同类型的所有游戏对象
		BaseSelection[] otherInstances = UnityEngine.Object.FindObjectsOfType<BaseSelection>();

		// 遍历并销毁除自身以外的同类型游戏对象
		foreach (BaseSelection instance in otherInstances)
		{
			if (instance != this)
			{
				Destroy(instance.gameObject);
			}
		}

		gameManager.UpdateGameState(GameState.Tactical);
    }
    public bool buildingsDisabled;
    public void DisableAllBuildings()
    {
        buildingsDisabled = true;
        govern.interactable = false;
        apron.interactable = false;
	}
    public void EnableAllBuilding()
    {
        buildingsDisabled = false;
		govern.interactable = true;
		apron.interactable = true;
	}



	//TODO
	[Obsolete]
	public void ImportPack()
	{
		cultivateSystem.FromPackImportDeck(0, 0);
		govern.interactable = false;
	}


	public void AddDeckTagFromPool(int index)
	{
		CardInspector card = Panels[0].packs[index].inspector;

		playerDeck.AddNewTag(card.ID);
	}



	public IDeckController InstantiateDeck()
	{
		return playerDeck;
	}

	public void UpdateBasicInfo(int gasMine, int cardNum)
	{
		gasMineText.text = gasMine.ToString();
		cardNumText.text = cardNum.ToString();
		//baseHealthText.text = baseHealth.ToString();
		//baseMaxHealthText.text = baseHealth.ToString();
	}
    public void UpdateBaseInfo(List<string> IDs, List<string> names, List<string> categories, List<int> healths, List<string> description)
    {
		selections = new BaseSelection[3];
        for(int i = 0; i < IDs.Count; i++)
		{
            GameObject bases = Instantiate(baseCardPrototype, new Vector3(2500, 0, 0), Quaternion.Euler(new Vector3(0, 90, 0)));
            selections[i] = bases.GetComponent<BaseSelection>();
            selections[i].SetInfo(IDs[i], names[i], categories[i], healths[i], description[i]);
        }

        //baseHealthText.text = baseHealth.ToString();
        //baseMaxHealthText.text = baseHealth.ToString();
    }





	public void ClearOtherSelectionFrame()
    {
        foreach(BaseSelection bases in selections)
        {
            if(bases.index != baseSelectionIndex)
            {
                bases.Frame.SetActive(false);
                bases.disableExit = false;
                bases.transform.DOScale(bases.originScale, bases.duration);
            }
        }
    }

}
