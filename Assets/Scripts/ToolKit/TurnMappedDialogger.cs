using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TurnMappedDialogger : MonoBehaviour
{
	private BattleSceneManager manager;

	private bool running = false;

	private GameObject dialogFrame;
	private TMP_Text dialogText;

	private float time = 3;
	private Dictionary<int, List<string>> dialogDict;
    private Tween currentTween;
    public int TurnNum
	{
		get => manager.turnNum;
	}
    private int lastTurnNum = -1;

    public void StartDialog()
    {
        running = true;
        manager = GetComponent<BattleSceneManager>();
        dialogFrame = transform.Find("Dialog").gameObject;
        dialogText = dialogFrame.transform.Find("DialogText").GetComponent<TMP_Text>();
        dialogFrame.SetActive(false);

        lastTurnNum = TurnNum;
        LoadDialogs();
        StartCoroutine(CheckTurnNum());
    }
    public void EndDialog()
    {
        running = false;
        manager = null;
        dialogFrame.SetActive(false);
    }

    /// <summary>
    /// 将csv表内容存入字典
    /// </summary>
    private void LoadDialogs()
    {
        TextAsset dialogData = Resources.Load<TextAsset>("Tutorial/TutorialDialog.csv");
        dialogDict = new Dictionary<int, List<string>>();
        string[] lines = dialogData.text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var values = line.Split(',');
            int turnNum = int.Parse(values[0]);
            List<string> dialogParts = new(values[1].Split('/'));
            dialogDict[turnNum] = dialogParts;
        }
    }



    private IEnumerator CheckTurnNum()
    {
        while (running)
        {
            if (TurnNum != lastTurnNum)
            {
                lastTurnNum = TurnNum;
                UpdateDialog();
            }
            yield return null;
        }
    }

    private void UpdateDialog()
    {
        if (!running)
        {
            if (currentTween != null)
            {
                currentTween.Kill();
                dialogText.text = "";
            }
            return;
        }

        if (dialogDict.ContainsKey(TurnNum))
        {
            StartCoroutine(DisplayDialog(dialogDict[TurnNum]));
            dialogDict.Remove(TurnNum);
        }
        else
        {
            dialogFrame.SetActive(false);
        }
    }

    private IEnumerator DisplayDialog(List<string> dialogParts)
    {
        if (dialogParts[0] == "")
        {
            yield break;
        }

        yield return new WaitForSeconds(1.5f);
        dialogFrame.SetActive(true);

        foreach (string dialog in dialogParts)
        {
            time = dialog.Length * 0.1f;
            currentTween = DOTween.To(
                () => "",
                value => dialogText.text = value,
                dialog,
                time
            ).SetEase(Ease.Linear);
            yield return currentTween.WaitForCompletion();
            yield return new WaitForSeconds(1f);
        }
        dialogFrame.SetActive(false);
    }
}
