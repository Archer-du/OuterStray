using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneState;
using UnityEngine.UI;
using InputHandler;
using LogicCore;

public class CultivateSceneManager : MonoBehaviour
{
    ICultivationSystemInput cultivateSystem;

    private GameManager gameManager;

    public DepartmentController buildingController;


    //note: DontDestroyOnLoad的游戏对象，在切换到不存在自身的新场景中时会被保留，但是它不能查找新场景中的物体。
    public void Start()
    {

        gameManager = GameManager.GetInstance();
        cultivateSystem = gameManager.cultivationSystem;

        buildingController = gameManager.GetComponent<DepartmentController>();

        //note: 功能从manager下放到controller
    }


    public void StartExpedition()
    {

    }

    public void ImportPack()
    {

    }
}
