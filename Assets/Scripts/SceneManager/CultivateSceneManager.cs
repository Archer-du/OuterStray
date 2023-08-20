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
using UnityEditor.Search;
using System.IO.Pipes;

public class CultivateSceneManager : MonoBehaviour,
    ICultivateSceneController
{
    public ICultivationSystemInput cultivateSystem;

    private GameManager gameManager;

    public DeckController playerDeck;

    public DepartmentController buildingController;


    public Button baseChooseButton;
    public Button testImportButton;

    public CanvasGroup inputMask;

    [Header("Text")]
	public TMP_Text gasMineText;
	public TMP_Text cardNumText;
	public TMP_Text baseHealthText;
	public TMP_Text baseMaxHealthText;

    [Header("Prototype")]
    public GameObject baseCardPrototype;

    [Header("Interaction")]
    public Button startExpedition;

    public int selectionIndex;

    [Header("Components")]
    public BaseSelection[] selections;

	public void Start()
    {
        gameManager = GameManager.GetInstance();

        GameManager.OnGameStateChanged += OnGameStateChanged;

        //note: 功能从manager下放到controller
        baseChooseButton.onClick.AddListener(BaseChoose);
        startExpedition.onClick.AddListener(StartExpedition);
        testImportButton.onClick.AddListener(ImportPack);

        buildingsDisabled = false;

        selectionIndex = -1;
        startExpedition.enabled = false;
        startExpedition.image.color = Color.gray;

		selections = new BaseSelection[3];
	}
    public void OnGameStateChanged(GameState state)
    {
        if(state == GameState.Start)
        {
            Destroy(this);
        }

		else
        {
            transform.SetParent(gameManager.transform);
        }
    }
	public void DestroyOtherInstancesOfType()
	{
		// 查找同类型的所有游戏对象
		CultivateSceneManager[] otherInstances = Object.FindObjectsOfType<CultivateSceneManager>();

		// 遍历并销毁除自身以外的同类型游戏对象
		foreach (CultivateSceneManager instance in otherInstances)
		{
			if (instance != this)
			{
				Destroy(instance.gameObject);
			}
		}
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
        Debug.Log("test");
		duration = 0.5f;

        DisableAllBuildings();
        playerDeck.DisableAllDeckTags();
		inputMask.DOFade(0.3f, duration);

        for(int i = 0; i < 3; i++)
        {
            GameObject bases = Instantiate(baseCardPrototype, new Vector3(2500, 0, 0), Quaternion.Euler(new Vector3(0, 90, 0)));
			selections[i] = bases.GetComponent<BaseSelection>();
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

		cultivateSystem.SetBase(selectionIndex);
        startExpedition.transform.position = new Vector3(0, -1200, 0);
		// 查找同类型的所有游戏对象
		BaseSelection[] otherInstances = Object.FindObjectsOfType<BaseSelection>();

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
        testImportButton.interactable = false;
        baseChooseButton.interactable = false;
	}
    public void EnableAllBuilding()
    {
        buildingsDisabled = false;
		testImportButton.interactable = true;
		baseChooseButton.interactable = true;
	}




	public void ImportPack()
	{
		cultivateSystem.FromPackImportDeck(0, 0);
		testImportButton.interactable = false;
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
    public void UpdateBaseInfo(int baseHealth)
    {
        baseHealthText.text = baseHealth.ToString();
        baseMaxHealthText.text = baseHealth.ToString();
    }





	public void ClearOtherSelectionFrame()
    {
        foreach(BaseSelection bases in selections)
        {
            if(bases.index != selectionIndex)
            {
                bases.Frame.SetActive(false);
                bases.disableExit = false;
                bases.transform.DOScale(bases.originScale, bases.duration);
            }
        }
    }

}
