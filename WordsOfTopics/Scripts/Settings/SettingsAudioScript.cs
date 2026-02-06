using System;
using UnityEngine;

public class SettingsAudioScript : MonoBehaviour
{
    private AudioSource audioSource;
    
    public AudioClip audioClip;
    
    private float volume;
    
    private bool soundOn = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setVolume(int volume, bool soundOn)
    {
        this.volume = volume / 100f;
        audioSource.volume = volume / 100f;
        
        this.soundOn = soundOn;
    }

    public void playButtonSound()
    {
        if(soundOn)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }
}
