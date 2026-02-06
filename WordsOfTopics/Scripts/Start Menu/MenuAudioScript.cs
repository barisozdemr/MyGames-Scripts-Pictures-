using System;
using System.IO;
using UnityEngine;

public class MenuAudioScript : MonoBehaviour
{
    private String SETTINGSDATAPATH;
    
    private AudioSource audioSource;
    
    public AudioClip audioClip;
    
    private bool soundOn = true;

    private float volume;
    
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

        for (int i = 0; i < 5; i++)
        {
            if (i == 0)
            {
                string data = lines[i].Split(":")[1];
                if (data == "off")
                {
                    soundOn = false;
                }
            }
            else if (i == 1)
            {
                string data = lines[i].Split(":")[1];
                if (data == "off")
                {
                    soundOn = false;
                }
            }
            else if (i == 2)
            {
                volume = int.Parse(lines[i].Split(":")[1]) / 100f;
            }
        }
        
        setVolume();
    }

    void setVolume()
    {
        audioSource.volume = volume;
    }
    
    public void playButtonSound()
    {
        if (soundOn)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }
}
