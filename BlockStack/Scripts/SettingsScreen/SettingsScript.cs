using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    private String SETTINGSDATAPATH;
    
    public RectTransform canvasRect;
    
    public GameObject audioManager;
    public AudioScript audioScript;

    public Image rootImage;
    
    private bool soundOn = true;
    public Image soundTick;
    public Sprite emptyTickBox;
    public Sprite fullTickBox;
    
    private float soundBarLeftAnchor = -420f;
    private float soundBarRightAnchor = 290f;

    private float totalBarLength;
    
    private int soundValue = 50;
    public TextMeshProUGUI soundValueText;
    
    public RectTransform soundButton;
    public RectTransform soundBar;

    private float soundBarAndButton_y = -330f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SETTINGSDATAPATH = Application.persistentDataPath + "/settings.txt";
        
        audioScript = audioManager.GetComponent<AudioScript>();
        
        totalBarLength = soundBarRightAnchor - soundBarLeftAnchor;
        
        readSettings();
        
        setSettingsUI();
        
        StartCoroutine(fadeOutImageAndDisable(rootImage, 1f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void saveSettings()
    {
        if (! File.Exists(SETTINGSDATAPATH))
        {
            File.WriteAllText(SETTINGSDATAPATH, "");
        }

        string text = "";
        
        //========================================== SoundEffects
        text += "sound:";
        text += soundOn ? "on\n" : "off\n";
        
        text += "soundValue:";
        text += soundValue+"\n";
        
        File.WriteAllText(SETTINGSDATAPATH,text);
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
                soundValue = int.Parse(lines[i].Split(":")[1]);
            }
        }
    }

    public void backToMenuClicked()
    {
        audioScript.playButtonSoundClip();
        
        saveSettings();
        
        StartCoroutine(enableAndFadeInImage(rootImage, 1f));
        
        Invoke("changeSceneToMenu", 1.1f);
    }

    void changeSceneToMenu()
    {
        SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
    }

    void setSettingsUI()
    {
        if (soundOn == true)
        {
            soundTick.sprite = fullTickBox;
        }
        else
        {
            soundTick.sprite = emptyTickBox;
        }
        
        soundValueText.text = soundValue.ToString();
        
        setSoundBarUI();
    }

    void setSoundBarUI()
    {
        float shiftAmount = totalBarLength * soundValue / 100;

        Vector2 soundButtonAndBarPos = new Vector2(soundBarLeftAnchor + shiftAmount, soundBarAndButton_y);
        
        soundButton.anchoredPosition = soundButtonAndBarPos;
        soundBar.anchoredPosition = soundButtonAndBarPos;
    }
    
    public void soundTickClicked()
    {
        audioScript.playButtonSoundClip();
        
        if (soundOn)
        {
            soundOn = false;
            soundTick.sprite = emptyTickBox;
        }
        else
        {
            soundOn = true;
            soundTick.sprite = fullTickBox;
        }
        
        
        audioScript.setSettings(soundOn, soundValue);
    }

    public void playButtonSoundClip()
    {
        audioScript.playButtonSoundClip();
    }

    public void soundButtonDragged(BaseEventData data)
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

        Vector2 soundBarAndButtonPos = new Vector2(inputX, soundBarAndButton_y);
        
        soundButton.anchoredPosition = soundBarAndButtonPos;
        soundBar.anchoredPosition = soundBarAndButtonPos;
        
        //===============================================================================

        float barLength = inputX - soundBarLeftAnchor;
        
        int soundPercentage = (int)((barLength/totalBarLength) * 100);
        
        soundValue = soundPercentage;
        soundValueText.text = soundValue.ToString();
        
        audioScript.setSettings(soundOn, soundValue);
    }
    
    IEnumerator fadeOutImageAndDisable(Image image, float fadeOutTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = image.color;
            c.a = 1 - (timer / fadeOutTime);
            image.color = c;

            if (timer >= fadeOutTime)
            {
                image.gameObject.SetActive(false);
                yield break;
            }
            
            yield return null;
        }
    }

    IEnumerator enableAndFadeInImage(Image image, float fadeInTime)
    {
        image.gameObject.SetActive(true);
        
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = image.color;
            c.a = (timer / fadeInTime);
            image.color = c;

            if (timer >= fadeInTime)
            {
                yield break;
            }
            
            yield return null;
        }
    }
}
