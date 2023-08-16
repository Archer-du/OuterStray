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

	public AudioSource TacticalBGM;
	public AudioSource BattleBGM;

	public ICultivationSystemInput cultivationSystem;
	public ITacticalSystemInput tacticalSystem;
	public IBattleSystemInput battleSystem;
	public Pool pool;

	public StartSceneManager startSceneManager;
	public CultivateSceneManager cultivateSceneManager;
	public TacticalSceneManager tacticalSceneManager;
	public BattleSceneManager battleSceneManager;

	public AsyncOperation async;
	//定义一个公共方法，用于更新游戏状态，并根据不同的状态执行不同的逻辑
	public AsyncOperation UpdateGameState(GameState state)
	{
		gameState = state;
		switch (state)
		{
			case GameState.Start:
				SceneManager.LoadScene("StartScene");
				break;

			case GameState.Cultivate:
				async = SceneManager.LoadSceneAsync("CultivateScene");
				StartCoroutine(LoadingNewScene(async, "CultivateScene"));
				break;

			case GameState.Tactical:
				async = SceneManager.LoadSceneAsync("TacticalScene");
				StartCoroutine(LoadingNewScene(async, "TacticalScene"));
				return async;

			case GameState.Battle:
				TacticalBGM.Stop();
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
			// 等待一帧
			yield return null;
		}
		switch (scene)
		{
			case "CultivateScene":
				if (cultivateSceneManager != null)
				{
					cultivateSceneManager.DestroyOtherInstancesOfType();
				}
				else
				{
					cultivateSceneManager = GameObject.Find("CultivateSceneManager").GetComponent<CultivateSceneManager>();
				}
				break;
			case "TacticalScene":
				if (tacticalSceneManager != null)
				{
					tacticalSceneManager.DestroyOtherInstancesOfType();
				}
				else
				{
					tacticalSceneManager = GameObject.Find("TacticalSceneManager").GetComponent<TacticalSceneManager>();
					tacticalSystem.SetSceneController(tacticalSceneManager);
				}
				break;
			case "BattleScene":
				BattleBGM.Play();
				battleSceneManager = GameObject.Find("BattleSceneManager").GetComponent<BattleSceneManager>();
				battleSystem.SetSceneController(battleSceneManager);
				break;
		}

		SceneLoader.blocksRaycasts = false;
		SceneLoader.DOFade(0f, 0.3f)
			.OnComplete(() =>
			{
				progressText.gameObject.SetActive(false);
			});

		// 或者 progressBar.gameObject.SetActive(false);
		// 或者 AudioSource.PlayClipAtPoint(loadSound, transform.position);
	}

	public Button start;
	public AudioSource loadAudio;
	public AudioClip loadAudioClip;
	private void Start()
	{
		DontDestroyOnLoad(gameObject);

		loadAudio = gameObject.AddComponent<AudioSource>();
		loadAudio.clip = loadAudioClip;

		start.onClick.AddListener(() =>
		{
			loadAudio.Play();
			UpdateGameState(GameState.Cultivate);
		});

		//EXTEND
		pool = new Pool();
		pool.LoadCardPool();

		battleSystem = new BattleSystem(pool, battleSceneManager);
		tacticalSystem = new TacticalSystem(pool, tacticalSceneManager, battleSystem as BattleSystem);
		battleSystem.SetTacticalSystem(tacticalSystem);
		cultivationSystem = new CultivationSystem(cultivateSceneManager, tacticalSystem as TacticalSystem);

		TacticalBGM.Play();
	}





	//private void OnDisable()
	//{
	//	enabled = true;
	//}

	//private void OnDestroy()
	//{
	//	DontDestroyOnLoad(gameObject);
	//}
}
