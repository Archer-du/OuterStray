//Author@Archer
using DisplayInterface;
using InputHandler;
using LogicCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneState;


public class GameManager : MonoBehaviour, IGameManagement
{
	private static GameManager instance;

	public static GameManager GetInstance()
	{
		if (instance == null)
		{
			instance = FindObjectOfType<GameManager>();
			if (instance == null)
			{
				GameObject obj = new GameObject("GameManager");
				instance = obj.AddComponent<GameManager>();
			}
		}
		return instance;
	}

	//override
	private GameManager() { }


	public static event System.Action<GameState> OnGameStateChanged;
	private GameState privateGameState;

	public GameState gameState
	{
		get { return privateGameState; }
		set
		{
			privateGameState = value;
			if (OnGameStateChanged != null)
			{
				OnGameStateChanged(privateGameState);
			}
		}
	}

	public ICultivationSystemInput cultivationSystem;
	public ITacticalSystemInput tacticalSystem;
	public IBattleSystemInput battleSystem;

	public StartSceneManager startSceneManager;
	public CultivateSceneManager cultivateSceneManager;
	public TacticalSceneManager tacticalSceneManager;
	public BattleSceneManager battleSceneManager;

	//定义一个公共方法，用于更新游戏状态，并根据不同的状态执行不同的逻辑
	public void UpdateGameState(GameState state)
	{
		switch (state)
		{
			case GameState.Start:
				SceneManager.LoadScene("StartScene");
				break;

			case GameState.Cultivate:
				SceneManager.LoadScene("CultivateScene");
				break;

			case GameState.Tactical:
				SceneManager.LoadScene("TacticalScene");
				tacticalSceneManager = GameObject.Find("TacticalSceneManager").GetComponent<TacticalSceneManager>();
				break;

			case GameState.Battle:
				SceneManager.LoadScene("BattleScene");
				battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();
				break;

			case GameState.End://TODO
				break;
		}
		// Unload all unused assets
		//Resources.UnloadUnusedAssets();

		gameState = state;
	}

	private void Start()
	{
		DontDestroyOnLoad(gameObject);

		gameState = GameState.Battle;


		battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();
		//EXTEND

		battleSystem = new BattleSystem(battleSceneManager);
		tacticalSystem = new TacticalSystem(tacticalSceneManager, battleSystem as BattleSystem);
		cultivationSystem = new CultivationSystem(cultivateSceneManager, tacticalSystem as TacticalSystem);

	}

	private void OnDisable()
	{
		enabled = true; // 取消禁用组件
	}

	private void OnDestroy()
	{
		DontDestroyOnLoad(gameObject); // 取消销毁游戏对象
	}
}
