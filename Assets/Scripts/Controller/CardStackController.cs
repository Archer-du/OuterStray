using DisplayInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardStackController : MonoBehaviour,
    ICardStackController
{

	private int ownership;

	public GameObject unitPrototype;

	public GameObject commandPrototype;




	public void Init(int ownership)
	{
		this.ownership = ownership;
	}
	/// <summary>
	/// 重载的方法，用于初始化Fill
	/// </summary>
	/// <param name="turn"></param>
	/// <returns></returns>
	public IUnitElementController InstantiateUnitElementInBattle()
	{
		Transform stacks = GameObject.Find("UI/Stacks").transform;
		Quaternion initRotation = Quaternion.Euler(new Vector3(0, 0, -90));

		GameObject unit = Instantiate(unitPrototype, transform.position, initRotation);
		unit.transform.SetParent(transform);

		unit.SetActive(false);

		return unit.GetComponent<UnitElementController>();
	}
	public ICommandElementController InstantiateCommandElementInBattle()
	{
		Transform stacks = GameObject.Find("UI/Stacks").transform;
		Quaternion initRotation = Quaternion.Euler(new Vector3(0, 0, -90));

		GameObject comm = Instantiate(commandPrototype, transform.position, initRotation);
		comm.transform.SetParent(transform);

		comm.SetActive(false);

		return comm.GetComponent<CommandElementController>();
	}
}
