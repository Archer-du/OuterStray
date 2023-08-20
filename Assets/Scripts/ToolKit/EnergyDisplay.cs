using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnergyDisplay : MonoBehaviour
{
	public GameObject humanEnergy;
	public GameObject[] humanSlots;
	public GameObject[] plantSlots;

	public TMP_Text[] energyText;
	public TMP_Text[] energySupplyText;
	// Start is called before the first frame update
	void Start()
    {
		for (int i = 0; i < 5; i++)
		{
			humanSlots[i].SetActive(false);
			plantSlots[i].SetActive(false);
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
