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




	public bool awaked = false;
	public void Init()
	{
		ownership = 1 - transform.GetSiblingIndex();
		awaked = true;
	}
	/// <summary>
	/// 重载的方法，用于初始化Fill
	/// </summary>
	/// <param name="turn"></param>
	/// <returns></returns>
	public IUnitElementController InstantiateUnitElementInBattle()
	{
		if (awaked == false) Init();

		Transform stacks = GameObject.Find("UI/Stacks").transform;
		Quaternion initRotation = Quaternion.Euler(new Vector3(0, 0, -90));

		GameObject unit = Instantiate(unitPrototype, transform.position, initRotation);
		unit.transform.SetParent(transform);

		unit.SetActive(false);

		return unit.GetComponent<UnitElementController>();
	}
	public ICommandElementController InstantiateCommandElementInBattle()
	{
		if (awaked == false) Init();

		Transform stacks = GameObject.Find("UI/Stacks").transform;
		Quaternion initRotation = Quaternion.Euler(new Vector3(0, 0, -90));

		GameObject comm = Instantiate(commandPrototype, transform.position, initRotation);
		comm.transform.SetParent(transform);

		comm.SetActive(false);

		return comm.GetComponent<CommandElementController>();
	}
	/// <summary>
	/// 用于pop
	/// </summary>
	/// <returns></returns>
	/// <exception cref="System.NotImplementedException"></exception>
	public IUnitElementController InstantiateUnitElement()
	{
		throw new System.NotImplementedException();
	}

	public ICommandElementController InstantiateCommandElement()
	{
		throw new System.NotImplementedException();
	}

}
