using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAudio : MonoBehaviour
{
    public AudioClip audioClip;
    public bool loadingAudio;

    private Button button;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        if(loadingAudio)
        {
            audioSource = GameManager.GetInstance().gameObject.AddComponent<AudioSource>();
        }
        audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = audioClip;
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        button.onClick.AddListener(() => audioSource.Play());
    }
}
