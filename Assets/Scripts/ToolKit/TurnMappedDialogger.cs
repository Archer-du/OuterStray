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

    public GameObject guide;
    private Dictionary<int, List<GameObject>> TurnGuide;
    private GameObject currentActiveGuide;
    private int GuideNum = 0;

	private GameObject dialogFrame;
	private TMP_Text dialogText;

	private float time = 3;
	private Dictionary<int, List<string>> dialogDict;
    private Tween currentTween;

    private AudioSource audioSource;
    [SerializeField]public List<AudioClip> tutorialAudioClips;
    int audioIdx = 0;

    private Coroutine coroutine;
    public int TurnNum
	{
		get => manager.turnNum;
	}

    // private int lastTurnNum = -1;

/*    Queue<Action> guideEventQueue = new Queue<Action>();
    void EnqueueGuideEvent()
    {
        guideEventQueue.Enqueue(() => OnRetreat("human_02"));
    }*/

    public void StartTutorial()
    {
        running = true;
        manager = GetComponent<BattleSceneManager>();
        dialogFrame = transform.Find("Dialog").gameObject;
        guide = transform.Find("Guide").gameObject;
        dialogText = dialogFrame.transform.Find("DialogText").GetComponent<TMP_Text>();

        dialogFrame.SetActive(false);
        guide.SetActive(false);

        // lastTurnNum = TurnNum;
        LoadDialogs();

        audioSource = GetComponent<AudioSource>();

        manager.Retreat += OnRetreat;
        manager.Deploy += OnDeploy;
        manager.Cast += OnCast;
        manager.Move += OnMove;
        manager.TurnChanged += OnTurnChanged;

        guide.SetActive(true);
        TurnGuide = GetAllChildren(guide);
        foreach (var guideList in TurnGuide.Values)
        {
            foreach (var guide in guideList)
            {
                guide.SetActive(false);
            }
        }
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
            coroutine = StartCoroutine(DisplayDialog(dialogDict[TurnNum]));
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

        PlayDialogAudio(tutorialAudioClips[audioIdx]);
        audioIdx++;
        foreach (string dialog in dialogParts)
        {
            time = dialog.Length * 0.15f;
            currentTween = DOTween.To(
                () => "",
                value => dialogText.text = value,
                dialog,
                time
            ).SetEase(Ease.Linear);
            yield return currentTween.WaitForCompletion();
            yield return new WaitForSeconds(3f);
        }
        dialogFrame.SetActive(false);
    }



    private void StopGuide()
    {
        manager.btBattleNode.EndGuideRunning();
        StopCoroutine(coroutine);
        dialogFrame.SetActive(false);
        StartCoroutine(EndDialog());

        guide.SetActive(false);
        guide = null;
        dialogFrame.SetActive(false);
        running = false;
    }

    private IEnumerator EndDialog()
    {
        Debug.Log("EndDialog Coroutine started");
        yield return new WaitForSeconds(0.5f);
        dialogFrame.SetActive(true);
        string dialog = "看来您很熟悉战斗方式，不需要我来提醒，请您自己战斗吧";

        time = dialog.Length * 0.1f;
        currentTween = DOTween.To(
            () => "",
            value => dialogText.text = value,
            dialog,
            time
        ).SetEase(Ease.Linear);
        yield return currentTween.WaitForCompletion();
        yield return new WaitForSeconds(5f);
        dialogFrame.SetActive(false);
    }

    public void OnRetreat(string cardID)
    {
        if (!guide)
        {
            return;
        }

        if (cardID == "human_02" && TurnNum == 4)
        {
            StartCoroutine(UpdateGuide());
        }
        else
        {
            StopGuide();
        }

    }
    public void OnDeploy(string cardID)
    {
        if (!guide)
        {
            return;
        }

        if (cardID == "tutorial_21" && TurnNum == 6)
        {
            StartCoroutine(UpdateGuide());
        }
        else if (cardID == "tutorial_05" && TurnNum == 14)
        {
            StartCoroutine(UpdateGuide());
        }
        else
        {
            StopGuide();
        }
    }
    public void OnCast(string cardID)
    {
        if (!guide)
        {
            return;
        }

        if (cardID == "comm_human_01" && TurnNum == 10)
        {
            StartCoroutine(UpdateGuide());
        }
        else if (cardID == "comm_human_03" && TurnNum == 12)
        {
            StartCoroutine(UpdateGuide());
        }
        else
        {
            StopGuide();
        }
    }
    public void OnMove(string cardID)
    {
        if (!guide)
        {
            return;
        }

        if (cardID == "tutorial_21" && TurnNum == 8 && GuideNum == 0)
        {
            StartCoroutine(UpdateGuide());
        }
        else if (cardID == "tutorial_21" && TurnNum == 16 && GuideNum == 0)
        {
             StartCoroutine(UpdateGuide());
        }
        else if (cardID == "tutorial_27" && TurnNum == 16 && GuideNum == 1)
        {
            StartCoroutine(UpdateGuide());
        }
        else if (cardID == "tutorial_05" && TurnNum == 16 && GuideNum == 2)
        {
            StartCoroutine(UpdateGuide());
        }
        else
        {
            StopGuide();
        }
    }

    public void OnTurnChanged(int turnNum)
    {
        if (!guide)
        {
            return;
        }

        UpdateDialog();
        StartCoroutine(UpdateTurnGuide());
    }

    private IEnumerator UpdateTurnGuide()
    {
        if (!guide)
        {
            yield break;
        }

        switch(TurnNum)
        {
            case 5:
                if (GuideNum != 1)
                {
                    StopGuide();
                }
                break;
            case 7:
                if (GuideNum != 1)
                {
                    StopGuide();
                }
                break;
            case 9:
                if (GuideNum != 1)
                {
                    StopGuide();
                }
                break;
            case 11:
                if (GuideNum != 2)
                {
                    StopGuide();
                }
                break;
            case 13:
                if (GuideNum != 1)
                {
                    StopGuide();
                }
                break;
            case 15:
                if (GuideNum != 2)
                {
                    StopGuide();
                }
                break;
            case 17:
                if (GuideNum != 3)
                {
                    StopGuide();
                }
                break;
            default:
                break;
        }

        GuideNum = 0;
        if (currentActiveGuide != null)
        {
            currentActiveGuide.SetActive(false);
        }
        if(TurnGuide.ContainsKey(TurnNum))
        {
            if (TurnNum == 1 || TurnNum == 9)
            {
                yield return new WaitForSeconds(2);
            }
            else if (TurnNum == 12)
            {
                yield return new WaitForSeconds(3.5f);
            }
            else
            {
                yield return new WaitForSeconds(3.5f);
            }

            currentActiveGuide = TurnGuide[TurnNum][GuideNum];
            currentActiveGuide.SetActive(true);

            if (TurnNum == 14)
            {
                yield return new WaitForSeconds(5);
                StartCoroutine(UpdateGuide());
                yield break;
            }
            if (TurnNum == 18)
            {
                yield return new WaitForSeconds(3);
                StartCoroutine(UpdateGuide());
                yield break;
            }
        }
    }
    private IEnumerator UpdateGuide()
    {
        currentActiveGuide.SetActive(false);
        GuideNum++;
        if (TurnGuide.ContainsKey(TurnNum) && TurnGuide[TurnNum][GuideNum])
        {
            currentActiveGuide = TurnGuide[TurnNum][GuideNum];
            if (TurnNum == 10 && GuideNum == 1)
            {
                yield return new WaitForSeconds(2f);
            }
            currentActiveGuide.SetActive(true);
        }
        yield break;
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

    private void PlayDialogAudio(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.volume = 1.0f;
        audioSource.Play();
    }
}
