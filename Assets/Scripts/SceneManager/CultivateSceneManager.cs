using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneState;
using UnityEngine.UI;
using InputHandler;
using LogicCore;
using DisplayInterface;

public class CultivateSceneManager : MonoBehaviour,
    ICultivateSceneController
{
    ICultivationSystemInput cultivateSystem;

    private GameManager gameManager;

    public DepartmentController buildingController;

    public Button startExpedition;


    //note: DontDestroyOnLoad的游戏对象，在切换到不存在自身的新场景中时会被保留，但是它不能查找新场景中的物体。
    public void Start()
    {

        gameManager = GameManager.GetInstance();

        GameManager.OnGameStateChanged += OnGameStateChanged;

        cultivateSystem = gameManager.cultivationSystem;

        buildingController = gameManager.GetComponent<DepartmentController>();

        //note: 功能从manager下放到controller
        startExpedition.onClick.AddListener(() => gameManager.UpdateGameState(GameState.Tactical));
    }
    public void OnGameStateChanged(GameState state)
    {
        if(state == GameState.Start)
        {
            Destroy(this);
        }
        if(state == GameState.Cultivate)
        {
            DestroyOtherInstancesOfType();
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

    }

    public void ImportPack()
    {

    }
}
