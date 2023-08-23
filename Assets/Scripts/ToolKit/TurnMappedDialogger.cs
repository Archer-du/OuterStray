using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TurnMappedDialogger : MonoBehaviour
{
	private BattleSceneManager manager;

	private bool running = false;

    private GameObject guide;
    private Dictionary<int, List<GameObject>> TurnGuide;
    private int GuideNum = 0;

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

    public void StartTutorial()
    {
        running = true;
        manager = GetComponent<BattleSceneManager>();
        dialogFrame = transform.Find("Dialog").gameObject;
        guide = transform.Find("Guide").gameObject;
        dialogText = dialogFrame.transform.Find("DialogText").GetComponent<TMP_Text>();

        dialogFrame.SetActive(false);
        guide.SetActive(false);

        lastTurnNum = TurnNum;
        LoadDialogs();

        TurnGuide = GetAllChildren(guide);
        StartCoroutine(UpdateGuide());
        StartCoroutine(CheckTurnNum());
    }
    public void EndTutorial()
    {
        running = false;
        manager = null;
        dialogFrame.SetActive(false);
        guide.SetActive(false);
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

    private Dictionary<int, List<GameObject>> GetAllChildren(GameObject parent)
    {
        Dictionary<int, List<GameObject>> TurnGuide = new Dictionary<int, List<GameObject>>();
        Regex regex = new Regex(@"Turn(\d+)Guide(\d+)");

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject child = parent.transform.GetChild(i).gameObject;
            Match match = regex.Match(child.name);

            if (match.Success)
            {
                int turn = int.Parse(match.Groups[1].Value);
                if (!TurnGuide.ContainsKey(turn))
                {
                    TurnGuide[turn] = new List<GameObject>();
                }
                TurnGuide[turn].Add(child);
            }
        }
        return TurnGuide;
    }



    private IEnumerator UpdateGuide()
    {
        guide.SetActive(true);
        foreach (var guideList in TurnGuide.Values)
        {
            foreach (var guide in guideList)
            {
                guide.SetActive(false);
            }
        }

        while (running)
        {
            if (TurnGuide.ContainsKey(TurnNum))
            {
                TurnGuide[TurnNum][GuideNum].SetActive(true);
            }
        }

        yield return null;
    }

}
