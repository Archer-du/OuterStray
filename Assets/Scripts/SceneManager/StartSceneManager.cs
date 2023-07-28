using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SceneState;

public class StartSceneManager : MonoBehaviour
{
    private GameManager gameManager;

    public Button start;
    
    // Start is called before the first frame update
    public void Start()
    {
        gameManager = GameManager.GetInstance();
        start = GameObject.Find("Canvas/Button").GetComponent<Button>();

        start.onClick.AddListener(StartGame);
    }


    public void StartGame()
    {
        gameManager.UpdateGameState(GameState.Cultivate);
    }
}
