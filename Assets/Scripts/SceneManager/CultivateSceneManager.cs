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

    public Button baseChoose;

    public Button testImportButton;

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
        baseChoose.onClick.AddListener(BaseChoose);
        startExpedition.onClick.AddListener(StartExpedition);
        testImportButton.onClick.AddListener(ImportPack);

        selectionIndex = -1;
        startExpedition.interactable = false;
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
		duration = 0.5f;
        GameObject base1 = Instantiate(baseCardPrototype, new Vector3(2500, 0, 0), Quaternion.Euler(new Vector3(0, 90, 0)));
		GameObject base2 = Instantiate(baseCardPrototype, new Vector3(3000, 0, 0), Quaternion.Euler(new Vector3(0, 90, 0)));
		GameObject base3 = Instantiate(baseCardPrototype, new Vector3(3500, 0, 0), Quaternion.Euler(new Vector3(0, 90, 0)));

        base1.GetComponent<BaseSelection>().index = 0;
        base2.GetComponent<BaseSelection>().index = 1;
        base3.GetComponent<BaseSelection>().index = 2;

        base1.transform.DOBlendableMoveBy(new Vector3(-2000, 0, 0), duration);
        base2.transform.DOBlendableMoveBy(new Vector3(-2000, 0, 0), duration);
        base3.transform.DOBlendableMoveBy(new Vector3(-2000, 0, 0), duration);
	}
    public void StartExpedition()
    {
        cultivateSystem.SetBase(selectionIndex);

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
