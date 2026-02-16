using System;
using System.IO;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    private String SETTINGSDATAPATH;
    
    private AudioSource audioSource;
    
    public AudioClip buttonSoundClip;
    
    public AudioClip tickSoundClip;
    
    public AudioClip comboSoundClip;
    public AudioClip rowCollapseSoundClip;
    
    public AudioClip gameOverSoundClip;

    private bool soundOn;
    private int volumeLevel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SETTINGSDATAPATH = Application.persistentDataPath + "/settings.txt";
        
        audioSource = GetComponent<AudioSource>();
        
        readSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void readSettings()
    {
        if (!File.Exists(SETTINGSDATAPATH))
        {
            return;
        }
        
        string[] lines = File.ReadAllLines(SETTINGSDATAPATH);

        for (int i = 0; i < 2; i++)
        {
            if (i == 0)
            {
                string data = lines[i].Split(":")[1];
                if (data == "on")
                {
                    soundOn = true;
                }
                else if (data == "off")
                {
                    soundOn = false;
                }
            }
            else if (i == 1)
            {
                volumeLevel = int.Parse(lines[i].Split(":")[1]);
            }
        }
        
        audioSource.volume = volumeLevel / 100f;
    }
    
    public void setSettings(bool soundOn, int soundValue)
    {
        this.soundOn = soundOn;
        this.volumeLevel = soundValue;
        audioSource.volume = volumeLevel / 100f;
    }

    public void playButtonSoundClip()
    {
        if (soundOn)
        {
            audioSource.PlayOneShot(buttonSoundClip);
        }
    }
    
    public void playTickSoundClip()
    {
        if (soundOn)
        {
            audioSource.PlayOneShot(tickSoundClip);
        }
    }
    
    public void playComboSoundClip()
    {
        if (soundOn)
        {
            audioSource.PlayOneShot(comboSoundClip);
        }
    }
    
    public void playRowCollapseSoundClip()
    {
        if (soundOn)
        {
            audioSource.PlayOneShot(rowCollapseSoundClip);
        }
    }
    
    public void playGameOverSoundClip()
    {
        if (soundOn)
        {
            audioSource.PlayOneShot(gameOverSoundClip);
        }
    }
    
}
