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
using UnityEngine.EventSystems;
using System;
using System.IO;
using UnityEngine.Rendering.VirtualTexturing;

public class GameManager : MonoBehaviour, IGameManagement,
	IResourceLoader
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

	public EventSystem eventSystem;
	//定义一个公共方法，用于更新游戏状态，并根据不同的状态执行不同的逻辑
	public AsyncOperation UpdateGameState(GameState state)
	{
		gameState = state;
		switch (state)
		{
			case GameState.Start:
				async = SceneManager.LoadSceneAsync("StartScene");
				StandaloneInputModule[] inputModules = FindObjectsOfType<StandaloneInputModule>();

				// 遍历并禁用所有StandaloneInputModule
				foreach (StandaloneInputModule inputModule in inputModules)
				{
					inputModule.enabled = false;
				}
				Destroy(eventSystem.gameObject);
				DOTween.Clear();
				StartCoroutine(LoadingNewScene(async, "StartScene"));
				break;

			case GameState.Cultivate:
				async = SceneManager.LoadSceneAsync("CultivateScene");
				StartCoroutine(LoadingNewScene(async, "CultivateScene"));
				return async;

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
		SceneLoader.blocksRaycasts = false;
		if(scene != "StartScene")
		{
			SceneLoader.DOFade(0f, 0.3f)
				.OnComplete(() =>
				{
					progressText.gameObject.SetActive(false);
				});
		}
		switch (scene)
		{
			case "StartScene":
				OnGameStateChanged = null;
				Destroy(gameObject);
				//Destroy(cultivateSceneManager);
				//Destroy(tacticalSceneManager);

				//DestroyOtherInstancesOfType();
				break;
			case "CultivateScene":
				if (cultivateSceneManager != null)
				{
					cultivateSceneManager.DestroyOtherInstancesOfType();
				}
				else
				{
					cultivateSceneManager = GameObject.Find("CultivateSceneManager").GetComponent<CultivateSceneManager>();
					cultivateSceneManager.EnableAllBuilding();
					cultivateSceneManager.playerDeck.EnableAllDeckTags();
					cultivateSceneManager.cultivateSystem = cultivationSystem;
					cultivationSystem.SetSceneController(cultivateSceneManager);
					if (config.tutorial) cultivationSystem.LoadTutorialHumanDeck();
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


		// 或者 progressBar.gameObject.SetActive(false);
		// 或者 AudioSource.PlayClipAtPoint(loadSound, transform.position);
	}

	public Button start;

	public GlobalConfig config;
	public string configPath;
	private void Start()
	{
		DontDestroyOnLoad(gameObject);

		configPath = "Config\\GlobalConfig.json";
		TextAsset text = Resources.Load<TextAsset>(configPath);
		string jsonContent = text.text;
		config = JsonUtility.FromJson<GlobalConfig>(jsonContent);

		start.onClick.AddListener(StartGame);

		//EXTEND
		pool = new Pool(this);
		pool.LoadCardPool();

		battleSystem = new BattleSystem(pool, battleSceneManager);
		tacticalSystem = new TacticalSystem(pool, tacticalSceneManager, battleSystem as BattleSystem);
		battleSystem.SetTacticalSystem(tacticalSystem);
		cultivationSystem = new CultivationSystem(pool, cultivateSceneManager, tacticalSystem as TacticalSystem, battleSystem as BattleSystem);
		tacticalSystem.SetCultivateSystem(cultivationSystem);

		TacticalBGM.Play();
	}
	private void DestroyOtherInstancesOfType()
	{
		// 查找同类型的所有游戏对象
		GameManager[] otherInstances = UnityEngine.Object.FindObjectsOfType<GameManager>();

		// 遍历并销毁除自身以外的同类型游戏对象
		foreach (GameManager instance in otherInstances)
		{
			if (instance != this)
			{
				Destroy(instance.gameObject);
			}
		}
	}






	//Tutorial
	public void StartGame()
	{
		if (!config.tutorial)
		{
			UpdateGameState(GameState.Cultivate);
		}
		else
		{
			AsyncOperation async = UpdateGameState(GameState.Cultivate);
			StartCoroutine(LateLoadTactical(async));
		}
	}
	IEnumerator LateLoadTactical(AsyncOperation async)
	{
		while (!async.isDone)
		{
			yield return null;
		}
        dialogGround.gameObject.SetActive(true);

		readyText.gameObject.SetActive(false);
		readyText.DOFade(0, 0);
        for (int i = 0; i < dialogTexts.Length; i++)
        {
            dialogTexts[i].gameObject.SetActive(false);
			dialogTexts[i].DOFade(0, 0);
        }
        AsyncOperation asyncNew = UpdateGameState(GameState.Tactical);

        StartCoroutine(TutorialDialogAnimation(asyncNew));

    }
	public void BuildTutorial()
	{
		dialogGround.gameObject.SetActive(false);
		tacticalSceneManager.currentNode.gameObject.SetActive(true);
		tacticalSceneManager.currentNode.CastEvent();
	}




	public Button dialogGround;

	public TMP_Text[] dialogTexts;
	public TMP_Text readyText;
	IEnumerator TutorialDialogAnimation(AsyncOperation async)
	{
		yield return async;

		yield return new WaitForSeconds(1);

		float start = 0.5f;
		float end = 0.5f;
		float duration = 2f;


        dialogGround.enabled = false;
		dialogGround.onClick.AddListener(BuildTutorial);

		Sequence seq = DOTween.Sequence();
		for(int i = 0; i < dialogTexts.Length; i++)
		{
			dialogTexts[i].gameObject.SetActive(true);
			int temp = i;
			seq.Append(dialogTexts[temp].DOFade(1, start));
			seq.AppendInterval(duration);
			seq.Append(dialogTexts[temp].DOFade(0, end));
		}
		readyText.gameObject.SetActive(true);
		seq.Append(readyText.DOFade(1, start));

		seq.OnComplete(() => dialogGround.enabled = true);
	}


	public StreamReader OpenText(string path)
	{
		TextAsset text = Resources.Load<TextAsset>(path);
		MemoryStream memoryStream = new MemoryStream(text.bytes);
		return new StreamReader(memoryStream);
	}

	public string ReadAllText(string path)
	{
		TextAsset text = Resources.Load<TextAsset>(path);
		return text.text;
	}





	[Serializable]
	public class GlobalConfig
	{
		public bool tutorial;
	}
}
