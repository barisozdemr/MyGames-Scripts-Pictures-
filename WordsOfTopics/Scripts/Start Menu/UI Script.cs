using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    private float fadeInDelay = 0.2f;
    
    private float fadeOutTime = 0.5f;
    private float fadeInTime = 0.5f;
    
    //=================================================================

    public GameObject audioManager;
    private MenuAudioScript audioScript;
    
    //=================================================================
    
    public TextMeshProUGUI wordsOfTopicsText;
    public TextMeshProUGUI playRandomButtonText;
    public TextMeshProUGUI settingsButtonText;
    
    public Image playRandomButtonImage;
    public Image settingsButtonImage;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioScript = audioManager.GetComponent<MenuAudioScript>();
        
        Invoke("sceneFadeIn", fadeInDelay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playRandomButtonClicked()
    {
        audioScript.playButtonSound();
        
        sceneFadeOut();
        
        Invoke("loadScenePlayScreen", fadeOutTime + 0.05f);
    }
    
    void loadScenePlayScreen()
    {
        SceneManager.LoadScene("PlayScreen", LoadSceneMode.Single);
    }

    public void settingsClicked()
    {
        audioScript.playButtonSound();
        
        sceneFadeOut();
        
        Invoke("loadSceneSettings", fadeOutTime + 0.05f);
    }

    void loadSceneSettings()
    {
        SceneManager.LoadScene("SettingsScene", LoadSceneMode.Single);
    }

    void sceneFadeOut()
    {
        StartCoroutine(fadeOutText(wordsOfTopicsText));
        StartCoroutine(fadeOutText(playRandomButtonText));
        StartCoroutine(fadeOutText(settingsButtonText));
        
        StartCoroutine(fadeOutImage(playRandomButtonImage));
        StartCoroutine(fadeOutImage(settingsButtonImage));
    }

    IEnumerator fadeOutText(TextMeshProUGUI text)
    {
        float timer = 0;
        
        while (true)
        {
            timer += Time.deltaTime;

            Color c = text.color;
            
            c.a = 1 - (timer / fadeOutTime);
            
            text.color = c;

            if (timer >= fadeOutTime)
            {
                c = text.color;
            
                c.a = 0;
                
                text.color = c;
                
                yield break;
            }
            
            yield return null;
        }
    }

    IEnumerator fadeOutImage(Image image)
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
                c = image.color;
            
                c.a = 0;
                
                image.color = c;
                
                yield break;
            }
            
            yield return null;
        }
    }
    
    void sceneFadeIn()
    {
        StartCoroutine(fadeInText(wordsOfTopicsText));
        StartCoroutine(fadeInText(playRandomButtonText));
        StartCoroutine(fadeInText(settingsButtonText));
        
        StartCoroutine(fadeInImage(playRandomButtonImage));
        StartCoroutine(fadeInImage(settingsButtonImage));
    }
    
    IEnumerator fadeInText(TextMeshProUGUI text)
    {
        float timer = 0;
        
        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = text.color;
            c.a = (timer / fadeInTime);
            text.color = c;

            if (timer >= fadeInTime)
            {
                c = text.color;
                c.a = 1;
                text.color = c;
                
                yield break;
            }
            
            yield return null;
        }
    }

    IEnumerator fadeInImage(Image image)
    {
        float timer = 0;
        
        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = image.color;
            c.a = (timer / fadeInTime);
            image.color = c;
            
            if (timer >= fadeInTime)
            {
                c = image.color;
                c.a = 1;
                image.color = c;
                
                yield break;
            }
            
            yield return null;
        }
    }
    
    
    
    
    
}
