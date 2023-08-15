//Author@Archer
using DisplayInterface;
using InputHandler;
using LogicCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneState;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using DataCore.CultivateItems;

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

	private GameState GameState;
	public GameState gameState
	{
		get { return GameState; }
		set
		{
			GameState = value;
			OnGameStateChanged?.Invoke(GameState);
		}
	}

	public ICultivationSystemInput cultivationSystem;
	public ITacticalSystemInput tacticalSystem;
	public IBattleSystemInput battleSystem;
	public Pool pool;

	public StartSceneManager startSceneManager;
	public CultivateSceneManager cultivateSceneManager;
	public TacticalSceneManager tacticalSceneManager;
	public BattleSceneManager battleSceneManager;


	//定义一个公共方法，用于更新游戏状态，并根据不同的状态执行不同的逻辑
	public AsyncOperation UpdateGameState(GameState state)
	{
		gameState = state;
		AsyncOperation async;
		switch (state)
		{
			case GameState.Start:
				SceneManager.LoadScene("StartScene");
				break;

			case GameState.Cultivate:
				SceneManager.LoadScene("CultivateScene");
				break;

			case GameState.Tactical:
				async = SceneManager.LoadSceneAsync("TacticalScene");
				StartCoroutine(LoadingNewScene(async, "TacticalScene"));
				return async;

			case GameState.Battle:
				async = SceneManager.LoadSceneAsync("BattleScene");
				StartCoroutine(LoadingNewScene(async, "BattleScene"));
				return async;

			case GameState.End://TODO
				break;
		}
		return null;
	}


	public CanvasGroup SceneLoader;
	public TMP_Text progressText;
	IEnumerator LoadingNewScene(AsyncOperation async, string scene)
	{
		progressText.gameObject.SetActive(true);
		SceneLoader.alpha = 1.0f;
		SceneLoader.blocksRaycasts = true;
		// 获取加载进度并更新 UI 文本或滑动条的值
		while (!async.isDone)
		{
			float progress = async.progress;
			progressText.text = "Loading... " + (progress * 100) + "%";
			// 或者 progressBar.value = progress;
			// 等待一帧
			yield return null;
		}
		if(scene == "BattleScene")
		{
			battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();
			battleSystem.SetSceneController(battleSceneManager);
		}

		SceneLoader.blocksRaycasts = false;
		SceneLoader.DOFade(0f, 0.4f)
			.OnComplete(() =>
			{
				progressText.gameObject.SetActive(false);
			});
		//TODO
		//yield return new WaitForSeconds(0.6f);

		// 或者 progressBar.gameObject.SetActive(false);
		// 或者 AudioSource.PlayClipAtPoint(loadSound, transform.position);
	}

	private void Start()
	{
		DontDestroyOnLoad(gameObject);

		gameState = GameState.Battle;


		tacticalSceneManager = GameObject.Find("TacticalSceneManager").GetComponent<TacticalSceneManager>();
		//EXTEND
		pool = new Pool();
		pool.LoadCardPool();

		battleSystem = new BattleSystem(pool, battleSceneManager);
		tacticalSystem = new TacticalSystem(pool, tacticalSceneManager, battleSystem as BattleSystem);
		battleSystem.SetTacticalSystem(tacticalSystem);
		cultivationSystem = new CultivationSystem(cultivateSceneManager, tacticalSystem as TacticalSystem);

	}





	private void OnDisable()
	{
		enabled = true;
	}

	private void OnDestroy()
	{
		DontDestroyOnLoad(gameObject);
	}
}
