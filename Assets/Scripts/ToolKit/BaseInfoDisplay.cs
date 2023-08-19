using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BaseInfoDisplay : MonoBehaviour
{
	public float duration = 0.2f;

	public Image baseImage;
	public Image baseFrame;
	public Image baseGround;
	public Image baseIcon;
	public TMP_Text baseHealth;
	public TMP_Text baseDescription;

    public void BaseInfoInitialize(Color color, Image image, Image icon, string description)
    {
		baseFrame.color = color;
		baseGround.color = color;
		baseImage.sprite = image.sprite;
		baseIcon.sprite = icon.sprite;
		baseDescription.text = description;
	}
	public void UpdateBaseHealth(int health, int maxHealth)
	{
		baseHealth.text = health.ToString();
		baseHealth.DOColor(new Color(1, (float)health / maxHealth, (float)health / maxHealth), duration);
	}
}
