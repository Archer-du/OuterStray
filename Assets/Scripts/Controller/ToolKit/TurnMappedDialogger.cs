using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class TurnMappedDialogger : MonoBehaviour
{
	public BattleSceneManager manager;

	public bool disable;

	public GameObject dialogFrame;
	public TMP_Text dialogText;
	public string dialogSource;

	public float time;

	private string dialogs;
	private StreamReader dialogReader;

	private void Awake()
	{
		manager = GetComponent<BattleSceneManager>();
		//TODO
		dialogSource = "Assets\\Tutorial\\TutorialDialog.txt";

		dialogFrame.SetActive(false);

		dialogReader = File.OpenText(dialogSource);
	}
	public void UpdateDialog()
	{
		if (disable) return;
		if (manager.turnNum % 2 == 1)
		{
			DisplayDialog();
		}
		if (manager.turnNum % 2 == 0)
		{
			dialogFrame.SetActive(false);
		}
	}
	private void DisplayDialog()
	{
		dialogs = dialogReader.ReadLine();
		if (dialogs == null) return;
		dialogFrame.SetActive(true);
		DOTween.To(
			() => "",
			value => dialogText.text = value,
			dialogs,
			time
		).SetEase(Ease.Linear);
	}
}
