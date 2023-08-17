using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneState;
using UnityEngine.UI;
using InputHandler;
using LogicCore;
using DisplayInterface;
using TMPro;

public class CultivateSceneManager : MonoBehaviour,
    ICultivateSceneController
{
    public ICultivationSystemInput cultivateSystem;

    private GameManager gameManager;

    public DeckController playerDeck;

    public DepartmentController buildingController;

    public Button startExpedition;

    public Button testImportButton;

    [Header("Text")]
	public TMP_Text gasMineText;
	public TMP_Text cardNumText;
	public TMP_Text baseHealthText;
	public TMP_Text baseMaxHealthText;


	public void Start()
    {
        gameManager = GameManager.GetInstance();

        GameManager.OnGameStateChanged += OnGameStateChanged;

        //buildingController = gameManager.GetComponent<DepartmentController>();

        //note: 功能从manager下放到controller
        startExpedition.onClick.AddListener(StartExpedition);
        testImportButton.onClick.AddListener(ImportPack);
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

	public void StartExpedition()
    {
        if (playerDeck.IsEmpty())
        {
            Debug.LogWarning("你还没有导入卡组！");
            return;
        }
        gameManager.UpdateGameState(GameState.Tactical);
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

	public void UpdateBasicInfo(int gasMine, int cardNum, int baseHealth)
	{
        gasMineText.text = gasMine.ToString();
        cardNumText.text = cardNum.ToString();
        baseHealthText.text = baseHealth.ToString();
        baseMaxHealthText.text = baseHealth.ToString();
	}
}
