using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepartmentController : MonoBehaviour
{
	public CultivateSceneManager manager;

	//TODO CONFIG: building num
	private static int DepartmentNum = 3;


	public Button[] departmentButtons;

	public GameObject panel;
	public Button[] panelButtons;

	public GameObject closePanelButton;

	public int departmentIdx;
	public int packIdx;


	void Start()
    {
		manager = GameObject.Find("CultivateSceneManager").GetComponent<CultivateSceneManager>();

		departmentIdx = -1;
		packIdx = -1;

		departmentButtons = new Button[DepartmentNum];
		for(int i = 0; i < DepartmentNum; i++)
		{
			//note: lambda表达式延迟绑定变量，而且绑的是i的引用
			departmentButtons[i] = transform.GetChild(i).GetComponent<Button>();
			int temp = i;
			departmentButtons[i].onClick.AddListener(() => OpenPanel(temp));
		}

		panelButtons = new Button[3];//TODO
		panel = transform.Find("Panel").gameObject;
		for(int i = 0; i < 3; i++)
		{
			Debug.Log(panel.transform.GetChild(i));
			panelButtons[i] = panel.transform.GetChild(i).GetChild(0).GetComponent<Button>();
			int temp = i;
			panelButtons[i].onClick.AddListener(() => ImportPack(temp));
		}


		closePanelButton = transform.Find("ClosePanelButton").gameObject;
		closePanelButton.GetComponent<Button>().onClick.AddListener(ClosePanel);


		panel.SetActive(false);
		closePanelButton.SetActive(false);
	}


	public void ImportPack(int packIdx)
	{
		this.packIdx = packIdx;

		if(this.departmentIdx > 0)
		{
			Debug.Log(departmentIdx);
			Debug.Log(packIdx);
		}

		this.packIdx = -1;
	}

	public void OpenPanel(int departmentIdx)
	{
		this.departmentIdx = departmentIdx;

		panel.SetActive(true);
		closePanelButton.SetActive(true);
	}
	public void ClosePanel()
	{
		this.departmentIdx = -1;

		panel.SetActive(false);
		closePanelButton.SetActive(false);
	}
}
