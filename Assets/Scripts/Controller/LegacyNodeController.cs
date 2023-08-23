using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LegacyNodeController : NodeController
{
	public int legacy;
	public GameObject gasMinePrototype;

	//public ParticleSystem particleSystem;
	public override void Init()
	{
		base.Init();
		LoadResource();

		gasMinePrototype = transform.Find("GasMineToken").gameObject;
		//particleSystem.Stop();
	}
	public override void LoadResource()
	{
		Icon.sprite = Resources.LoadAll<Sprite>("Map-icon")[3];
		descriptionText.text = "气矿节点";
	}
	public override void CastEvent()
	{
		base.CastEvent();

		List<GameObject> list = new List<GameObject>();

		float duration = 0.3f;
		for(int i = 0; i < 10; i++)
		{
			GameObject gasMine = Instantiate(gasMinePrototype, transform);
			gasMine.SetActive(true);
			Image gasMineImage = gasMine.GetComponent<Image>();
			list.Add(gasMine);
			gasMine.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-60, 60));

			Sequence seq = DOTween.Sequence();
			int temp = i;
			if(temp != 0)
			{
				seq.AppendInterval((temp + 1) * duration);
			}
			seq.Append(gasMine.transform.DOBlendableLocalMoveBy(gasMine.transform.up * 250, duration));
			seq.Join(gasMine.transform.DOBlendableScaleBy(new Vector3(2, 2, 2), duration));
			seq.InsertCallback(duration / 2, () =>
			{
				gasMineImage.DOFade(0, duration).OnComplete(() => Destroy(list[temp].gameObject));
			});
		}
		//particleSystem.Play();
	}
	public override void SetBasicInfo(int legacy, int medicalPrice)
	{
		this.legacy = legacy;
	}
}
