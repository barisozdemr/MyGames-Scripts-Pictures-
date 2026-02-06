using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsFadeScript : MonoBehaviour
{
    private float fadeInTime = 0.5f;
    private float fadeOutTime;
    
    private float fadeInTimeDelay = 0.2f;
    
    public Image backButtonImage;
    
    public TextMeshProUGUI settingsText;
    
    public Image soundEffectsTickImage;
    public TextMeshProUGUI soundEffectsText;
    
    public TextMeshProUGUI gameSoundText;
    public Image gameSoundTickImage;
    public TextMeshProUGUI gameSoundValueText;
    public Image gameSoundButtonImage;
    public Image gameSoundBarImage;
    public Image gameSoundBarFillerImage;
    
    public TextMeshProUGUI keyboardSoundText;
    public Image keyboardSoundTickImage;
    public TextMeshProUGUI keyboardSoundValueText;
    public Image keyboardSoundButtonImage;
    public Image keyboardSoundBarImage;
    public Image keyboardSoundBarFillerImage;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("sceneFadeIn", fadeInTimeDelay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void sceneFadeOut(float fadeOutTime)
    {
        this.fadeOutTime = fadeOutTime;
        
        StartCoroutine(fadeOutImage(backButtonImage));
        
        StartCoroutine(fadeOutText(settingsText));
        
        StartCoroutine(fadeOutImage(soundEffectsTickImage));
        StartCoroutine(fadeOutText(soundEffectsText));
        
        StartCoroutine(fadeOutText(gameSoundText));
        StartCoroutine(fadeOutImage(gameSoundTickImage));
        StartCoroutine(fadeOutText(gameSoundValueText));
        StartCoroutine(fadeOutImage(gameSoundButtonImage));
        StartCoroutine(fadeOutImage(gameSoundBarImage));
        StartCoroutine(fadeOutImage(gameSoundBarFillerImage));
        
        StartCoroutine(fadeOutText(keyboardSoundText));
        StartCoroutine(fadeOutImage(keyboardSoundTickImage));
        StartCoroutine(fadeOutText(keyboardSoundValueText));
        StartCoroutine(fadeOutImage(keyboardSoundButtonImage));
        StartCoroutine(fadeOutImage(keyboardSoundBarImage));
        StartCoroutine(fadeOutImage(keyboardSoundBarFillerImage));
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
        StartCoroutine(fadeInImage(backButtonImage));
        
        StartCoroutine(fadeInText(settingsText));
        
        StartCoroutine(fadeInImage(soundEffectsTickImage));
        StartCoroutine(fadeInText(soundEffectsText));
        
        StartCoroutine(fadeInText(gameSoundText));
        StartCoroutine(fadeInImage(gameSoundTickImage));
        StartCoroutine(fadeInText(gameSoundValueText));
        StartCoroutine(fadeInImage(gameSoundButtonImage));
        StartCoroutine(fadeInImage(gameSoundBarImage));
        StartCoroutine(fadeInImage(gameSoundBarFillerImage));
        
        StartCoroutine(fadeInText(keyboardSoundText));
        StartCoroutine(fadeInImage(keyboardSoundTickImage));
        StartCoroutine(fadeInText(keyboardSoundValueText));
        StartCoroutine(fadeInImage(keyboardSoundButtonImage));
        StartCoroutine(fadeInImage(keyboardSoundBarImage));
        StartCoroutine(fadeInImage(keyboardSoundBarFillerImage));
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
