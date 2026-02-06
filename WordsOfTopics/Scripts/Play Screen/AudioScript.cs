using System;
using System.IO;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    private String SETTINGSDATAPATH;
    
    private AudioSource audioSource;
    private AudioSource audioSource2;
    
    public AudioClip whooshSoundClip;
    
    public AudioClip buttonBuildSoundClip;
    
    public AudioClip hintButtonSoundClip;
    
    public AudioClip hintSoundClip;
    
    public AudioClip keyboardButtonSoundClip;
    
    public AudioClip playgroundButtonSoundClip;
    
    public AudioClip wordDoneSoundClip;
    
    public AudioClip winSoundClip;
    
    public AudioClip trueLetterSoundClip;
    public AudioClip falseLetterSoundClip;
    
    private bool sfxOn = true;
    private bool generalSoundOn = true;
    private bool keyboardSoundOn = true;
    
    private float generalVolume = 0.5f;
    private float keyboardVolume = 0.5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SETTINGSDATAPATH = Application.persistentDataPath + "/settings.txt";
        
        audioSource = GetComponents<AudioSource>()[0];
        audioSource2 = GetComponents<AudioSource>()[1];
        
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
                    sfxOn = false;
                }
            }
            else if (i == 1)
            {
                string data = lines[i].Split(":")[1];
                if (data == "off")
                {
                    generalSoundOn = false;
                }
            }
            else if (i == 2)
            {
                generalVolume = int.Parse(lines[i].Split(":")[1]) / 100f;
            }
            else if (i == 3)
            {
                string data = lines[i].Split(":")[1];
                if (data == "off")
                {
                    keyboardSoundOn = false;
                }
            }
            else if (i == 4)
            {
                keyboardVolume = int.Parse(lines[i].Split(":")[1]) / 100f;
            }
        }
    }

    public void playWhooshSoundClip()
    {
        if (sfxOn && generalSoundOn)
        {
            audioSource.PlayOneShot(whooshSoundClip, generalVolume);
        }
    }
    
    public void playButtonBuildSoundClip()
    {
        if (sfxOn && generalSoundOn)
        {
            audioSource.PlayOneShot(buttonBuildSoundClip, generalVolume);
        }
    }
    
    public void playHintButtonSoundClip()
    {
        if (sfxOn && generalSoundOn)
        {
            audioSource.PlayOneShot(hintButtonSoundClip, generalVolume);
        }
    }
    
    public void playHintSoundClip()
    {
        if (sfxOn && generalSoundOn)
        {
            audioSource.PlayOneShot(hintSoundClip, generalVolume);
        }
    }
    
    public void playKeyboardButtonSoundClip() //==================== keyboard
    {
        if (sfxOn && keyboardSoundOn)
        {
            audioSource.PlayOneShot(keyboardButtonSoundClip, keyboardVolume);
        }
    }
    
    public void playPlaygroundButtonSoundClip()
    {
        if (sfxOn && generalSoundOn)
        {
            audioSource.PlayOneShot(playgroundButtonSoundClip, generalVolume);
        }
    }
    
    public void playWordDoneSoundClip()
    {
        Invoke("wordDonePlayAfterDelay", 0.15f);
    }
    void wordDonePlayAfterDelay()
    {
        if (sfxOn && generalSoundOn)
        {
            audioSource.PlayOneShot(wordDoneSoundClip, generalVolume);
        }
    }
    
    public void playWinSoundClip()
    {
        if (sfxOn && generalSoundOn)
        {
            audioSource.PlayOneShot(winSoundClip, generalVolume);
        }
    }
    
    public void playTrueLetterSoundClip(int pitch)
    {
        if (sfxOn && generalSoundOn)
        {
            audioSource2.pitch = 1 + pitch / 10f;
            audioSource2.PlayOneShot(trueLetterSoundClip, generalVolume);
        }
    }
    
    public void playFalseLetterSoundClip()
    {
        if (sfxOn && generalSoundOn)
        {
            audioSource.PlayOneShot(falseLetterSoundClip, generalVolume);
        }
    }
}
