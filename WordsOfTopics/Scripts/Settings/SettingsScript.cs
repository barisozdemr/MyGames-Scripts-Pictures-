using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    private String SETTINGSDATAPATH;

    private float fadeOutTime = 0.5f;
    
    public RectTransform canvasRect;

    public GameObject settingsAudioManager;
    private SettingsAudioScript settingsAudioScript;
    
    public GameObject fadeManager;
    private SettingsFadeScript settingsFadeScript;
    
    //=============================================================
    
    public Image gameSoundBarFiller;
    public Image keyboardSoundBarFiller;
    
    private bool soundEffectsOn = true;
    public Image soundEffectsTick;
    
    private bool gameSoundOn = true;
    public Image gameSoundTick;
    
    private bool keyboardSoundOn = true;
    public Image keyboardSoundTick;
    
    public Sprite emptyTickBox;
    public Sprite fullTickBox;
    
    //=============================================================

    public RectTransform gameSoundParentRectTransform;
    public Image gameSoundButtonImage;
    public TextMeshProUGUI gameSoundText;
    
    public RectTransform keyboardSoundParentRectTransform;
    public Image keyboardSoundButtonImage;
    public TextMeshProUGUI keyboardSoundText;
    
    //=============================================================

    private int gameSoundValue = 50; // 0 - 100
    private int keyboardSoundValue = 50; // 0 - 100

    private float soundBarLeftAnchor = -344f;
    private float soundBarRightAnchor = 236f;

    private float soundBarGapLength;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SETTINGSDATAPATH = Application.persistentDataPath + "/settings.txt";
        
        settingsAudioScript = settingsAudioManager.GetComponent<SettingsAudioScript>();
        
        settingsFadeScript = fadeManager.GetComponent<SettingsFadeScript>();
        
        soundBarGapLength = Mathf.Abs(soundBarLeftAnchor - soundBarRightAnchor);
        
        readSettings();
        
        setSettings();
        
        settingsAudioScript.setVolume(gameSoundValue, setSoundOnForAudioScript());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void backButtonClicked()
    {
        playButtonSound();

        settingsFadeScript.sceneFadeOut(fadeOutTime);
        
        Invoke("openSceneStartMenu", fadeOutTime + 0.1f);
    }

    void openSceneStartMenu()
    {
        SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
    }
    
    public void playButtonSound()
    {
        settingsAudioScript.playButtonSound();
    }
    
    public void saveSettings()
    {
        if (! File.Exists(SETTINGSDATAPATH))
        {
            File.WriteAllText(SETTINGSDATAPATH, "");
        }

        string text = "";
        
        //========================================== SoundEffects
        text += "soundEffects:";
        text += soundEffectsOn ? "on\n" : "off\n";
        
        //========================================== Game Sound
        text += "gameSound:";
        text += gameSoundOn ? "on\n" : "off\n";
        
        text += "gameSoundValue:";
        text += gameSoundValue+"\n";

        //========================================== Keyboard Sound
        text += "keyboardSound:";
        text += keyboardSoundOn ? "on\n" : "off\n";
        
        text += "keyboardSoundValue:";
        text += keyboardSoundValue+"\n";
        
        File.WriteAllText(SETTINGSDATAPATH,text);
        
        settingsAudioScript.setVolume(gameSoundValue, setSoundOnForAudioScript());
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
                if (data == "on")
                {
                    soundEffectsOn = true;
                }
                else if (data == "off")
                {
                    soundEffectsOn = false;
                }
            }
            else if (i == 1)
            {
                string data = lines[i].Split(":")[1];
                if (data == "on")
                {
                    gameSoundOn = true;
                }
                else if (data == "off")
                {
                    gameSoundOn = false;
                }
            }
            else if (i == 2)
            {
                gameSoundValue = int.Parse(lines[i].Split(":")[1]);
            }
            else if (i == 3)
            {
                string data = lines[i].Split(":")[1];
                if (data == "on")
                {
                    keyboardSoundOn = true;
                }
                else if (data == "off")
                {
                    keyboardSoundOn = false;
                }
            }
            else if (i == 4)
            {
                keyboardSoundValue = int.Parse(lines[i].Split(":")[1]);
            }
        }
    }

    bool setSoundOnForAudioScript()
    {
        bool soundOn = true;

        if (!soundEffectsOn || !gameSoundOn)
        {
            soundOn = false;
        }
        
        return soundOn;
    }

    void setSettings()
    {
        if (soundEffectsOn)
        {
            soundEffectsOn = true;
            soundEffectsTick.sprite = fullTickBox;
        }
        else
        {
            soundEffectsOn = false;
            soundEffectsTick.sprite = emptyTickBox;
        }
        
        if (gameSoundOn)
        {
            gameSoundOn = true;
            gameSoundTick.sprite = fullTickBox;
        }
        else
        {
            gameSoundOn = false;
            gameSoundTick.sprite = emptyTickBox;
        }
        
        if (keyboardSoundOn)
        {
            keyboardSoundOn = true;
            keyboardSoundTick.sprite = fullTickBox;
        }
        else
        {
            keyboardSoundOn = false;
            keyboardSoundTick.sprite = emptyTickBox;
        }

        setGameSoundButton();
        
        setKeyboardSoundButton();
    }
    
    public void soundEffectsTickClicked()
    {
        playButtonSound();
        
        if (soundEffectsOn)
        {
            soundEffectsOn = false;
            soundEffectsTick.sprite = emptyTickBox;
        }
        else
        {
            soundEffectsOn = true;
            soundEffectsTick.sprite = fullTickBox;
        }
        
        saveSettings();
    }

    public void gameSoundTickClicked()
    {
        playButtonSound();
        
        if (gameSoundOn)
        {
            gameSoundOn = false;
            gameSoundTick.sprite = emptyTickBox;
        }
        else
        {
            gameSoundOn = true;
            gameSoundTick.sprite = fullTickBox;
        }
        
        saveSettings();
    }

    public void gameSoundButtonDragged(BaseEventData data)
    {
        PointerEventData eventData = (PointerEventData)data;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 screenPos
        );
        float inputX = screenPos.x;
        inputX = Mathf.Clamp(inputX, soundBarLeftAnchor, soundBarRightAnchor);
        
        float barLength = inputX - soundBarLeftAnchor;
        
        //=============================================================================
        
        Vector2 pos = gameSoundButtonImage.rectTransform.anchoredPosition;
        
        pos.x = inputX;
        
        gameSoundButtonImage.rectTransform.anchoredPosition = pos;
        
        //=============================================================================
        
        gameSoundValue = (int)(barLength / soundBarGapLength * 100);
        
        gameSoundText.text = gameSoundValue.ToString();
        
        //=============================================================================
        
        Vector3 scaleVector = gameSoundBarFiller.rectTransform.localScale;

        scaleVector.x = (barLength / 30);
        
        gameSoundBarFiller.rectTransform.localScale = scaleVector;

        pos = gameSoundBarFiller.rectTransform.anchoredPosition;

        pos.x = soundBarLeftAnchor + (barLength / 2);

        gameSoundBarFiller.rectTransform.anchoredPosition = pos;
        
        //=============================================================================
        
        saveSettings();

    }
    
    public void keyboardSoundTickClicked()
    {
        playButtonSound();
        
        if (keyboardSoundOn)
        {
            keyboardSoundOn = false;
            keyboardSoundTick.sprite = emptyTickBox;
        }
        else
        {
            keyboardSoundOn = true;
            keyboardSoundTick.sprite = fullTickBox;
        }
        
        saveSettings();
    }

    public void keyboardSoundButtonDragged(BaseEventData data)
    {
        PointerEventData eventData = (PointerEventData)data;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 screenPos
        );
        float inputX = screenPos.x;
        inputX = Mathf.Clamp(inputX, soundBarLeftAnchor, soundBarRightAnchor);
        
        float barLength = inputX - soundBarLeftAnchor;
        
        //=============================================================================
        
        Vector2 pos = keyboardSoundButtonImage.rectTransform.anchoredPosition;
        
        pos.x = inputX;
        
        keyboardSoundButtonImage.rectTransform.anchoredPosition = pos;
        
        //=============================================================================
        
        keyboardSoundValue = (int)(barLength / soundBarGapLength * 100);
        
        keyboardSoundText.text = keyboardSoundValue.ToString();
        
        //=============================================================================
        
        Vector3 scaleVector = keyboardSoundBarFiller.rectTransform.localScale;

        scaleVector.x = (barLength / 30);
        
        keyboardSoundBarFiller.rectTransform.localScale = scaleVector;

        pos = keyboardSoundBarFiller.rectTransform.anchoredPosition;

        pos.x = soundBarLeftAnchor + (barLength / 2);

        keyboardSoundBarFiller.rectTransform.anchoredPosition = pos;
        
        //=============================================================================
        
        saveSettings();
    }

    void setGameSoundButton()
    {
        gameSoundText.text = gameSoundValue.ToString();
        
        //=============================================================================
        
        float barFullLength = soundBarRightAnchor - soundBarLeftAnchor;

        float barLength = barFullLength * gameSoundValue / 100;
        
        Vector3 scaleVector = gameSoundBarFiller.rectTransform.localScale;
        
        scaleVector.x = (barLength / 30);
        
        gameSoundBarFiller.rectTransform.localScale = scaleVector;
        
        //==============================================================================
        
        Vector2 pos = gameSoundBarFiller.rectTransform.anchoredPosition;

        pos.x = soundBarLeftAnchor + (barLength / 2);

        gameSoundBarFiller.rectTransform.anchoredPosition = pos;
        
        //==============================================================================
        
        pos = gameSoundButtonImage.rectTransform.anchoredPosition;
        
        pos.x = soundBarLeftAnchor + barLength;
        
        gameSoundButtonImage.rectTransform.anchoredPosition = pos;
    }
    
    void setKeyboardSoundButton()
    {
        keyboardSoundText.text = keyboardSoundValue.ToString();
        
        //=============================================================================
        
        float barFullLength = soundBarRightAnchor - soundBarLeftAnchor;

        float barLength = barFullLength * keyboardSoundValue / 100;
        
        Vector3 scaleVector = keyboardSoundBarFiller.rectTransform.localScale;
        
        scaleVector.x = (barLength / 30);
        
        keyboardSoundBarFiller.rectTransform.localScale = scaleVector;
        
        //==============================================================================
        
        Vector2 pos = keyboardSoundBarFiller.rectTransform.anchoredPosition;

        pos.x = soundBarLeftAnchor + (barLength / 2);

        keyboardSoundBarFiller.rectTransform.anchoredPosition = pos;
        
        //==============================================================================
        
        pos = keyboardSoundButtonImage.rectTransform.anchoredPosition;
        
        pos.x = soundBarLeftAnchor + barLength;
        
        keyboardSoundButtonImage.rectTransform.anchoredPosition = pos;
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}
