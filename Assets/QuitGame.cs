using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
	public void Start()
	{
		GetComponent<Button>().onClick.AddListener(ExitGame);
	}
	public void ExitGame()
	{
		Application.Quit(); // 退出游戏
	}
}
