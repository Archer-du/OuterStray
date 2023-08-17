using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoaderAudio : MonoBehaviour
{
	public Button start;
	private AudioSource loadAudio;
	public AudioClip loadAudioClip;
	// Start is called before the first frame update
	void Start()
    {
		loadAudio = gameObject.AddComponent<AudioSource>();
		loadAudio.clip = loadAudioClip;
		loadAudio.playOnAwake = false;
		loadAudio.loop = false;

		start.onClick.AddListener(() =>
		{
			loadAudio.Play();
		});
	}
}
