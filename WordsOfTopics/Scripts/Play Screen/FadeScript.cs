using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeScript : MonoBehaviour
{
    public Image backgroundImage;
    
    public TextMeshProUGUI clockText;
    public Image clockImage;
    
    public TextMeshProUGUI mistakesText;
    public TextMeshProUGUI mistakesCountText;

    public Image hintButtonImage;
    public Image hintCountBackgroundImage;
    public TextMeshProUGUI hintCountText;
    
    public TextMeshProUGUI topicText;
    public Image topicTextImage;
    
    public TextMeshProUGUI theTopicText;
    public Image theTopicTextImage1;
    public Image theTopicTextImage2;

    public Image endScreenBackgroundImage;
    public Image endScreenCongratsImage;
    public TextMeshProUGUI endScreenTimeText;
    public TextMeshProUGUI endScreenMistakesText;
    public TextMeshProUGUI endScreenHintsText;
    
    //=========================================================

    private float fadeInTheTopicTime;
    
    private float endScreenFadeInTime;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        makeAlphaZeroImage(backgroundImage);
        
        makeAlphaZeroText(clockText);
        makeAlphaZeroImage(clockImage);
        
        makeAlphaZeroText(mistakesText);
        makeAlphaZeroText(mistakesCountText);
        
        makeAlphaZeroImage(hintButtonImage);
        makeAlphaZeroImage(hintCountBackgroundImage);
        makeAlphaZeroText(hintCountText);
        
        makeAlphaZeroText(topicText);
        makeAlphaZeroImage(topicTextImage);
        
        makeAlphaZeroText(theTopicText);
        makeAlphaZeroImage(theTopicTextImage1);
        makeAlphaZeroImage(theTopicTextImage2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void sceneFadeIn(float fadeInTime, float theTopicFadeInDelay, float fadeInTheTopicTime)
    {
        StartCoroutine(fadeInText(clockText, fadeInTime));
        StartCoroutine(fadeInImage(clockImage, fadeInTime));
        
        StartCoroutine(fadeInText(mistakesText, fadeInTime));
        StartCoroutine(fadeInText(mistakesCountText, fadeInTime));
        
        StartCoroutine(fadeInImage(hintButtonImage, fadeInTime));
        StartCoroutine(fadeInImage(hintCountBackgroundImage, fadeInTime));
        StartCoroutine(fadeInText(hintCountText, fadeInTime));

        StartCoroutine(fadeInText(topicText, fadeInTime));
        StartCoroutine(fadeInImage(topicTextImage, fadeInTime));
        
        StartCoroutine(fadeInImage(theTopicTextImage1, fadeInTime));
        StartCoroutine(fadeInImage(theTopicTextImage2, fadeInTime));
        
        this.fadeInTheTopicTime = fadeInTheTopicTime;
        
        Invoke("fadeInTheTopicCall", theTopicFadeInDelay);
    }

    void fadeInTheTopicCall()
    {
        StartCoroutine(fadeInTheTopic(fadeInTheTopicTime));
    }
    
    public void sceneFadeOut(float fadeOutTime)
    {
        StartCoroutine(fadeOutImage(backgroundImage, fadeOutTime));
        
        StartCoroutine(fadeOutText(clockText, fadeOutTime));
        StartCoroutine(fadeOutImage(clockImage, fadeOutTime));
        
        StartCoroutine(fadeOutText(mistakesText, fadeOutTime));
        StartCoroutine(fadeOutText(mistakesCountText, fadeOutTime));
        
        StartCoroutine(fadeOutImage(hintButtonImage, fadeOutTime));
        StartCoroutine(fadeOutImage(hintCountBackgroundImage, fadeOutTime));
        StartCoroutine(fadeOutText(hintCountText, fadeOutTime));

        StartCoroutine(fadeOutText(topicText, fadeOutTime));
        StartCoroutine(fadeOutImage(topicTextImage, fadeOutTime));

        StartCoroutine(fadeOutText(theTopicText, fadeOutTime));
        StartCoroutine(fadeOutImage(theTopicTextImage1, fadeOutTime));
        StartCoroutine(fadeOutImage(theTopicTextImage2, fadeOutTime));
    }
    
    public void fadeInEndScreen(float fadeInTime)
    {
        StartCoroutine(fadeInEndScreenBackground(endScreenBackgroundImage, fadeInTime));
        StartCoroutine(fadeInImage(endScreenCongratsImage, fadeInTime));
        
        endScreenFadeInTime = fadeInTime;
        Invoke("fadeInEndScreenTexts", fadeInTime*2);
    }
    
    public void fadeInEndScreenTexts()
    {
        StartCoroutine(fadeInEndScreenText(endScreenTimeText, endScreenFadeInTime/2));
        StartCoroutine(fadeInEndScreenText(endScreenMistakesText, endScreenFadeInTime/2));
        StartCoroutine(fadeInEndScreenText(endScreenHintsText, endScreenFadeInTime/2));
    }
   
    public void fadeOutEndScreen(float fadeOutTime)
    {
        StartCoroutine(fadeOutEndScreenBackground(endScreenBackgroundImage, fadeOutTime));
        StartCoroutine(fadeOutImage(endScreenCongratsImage, fadeOutTime));
        
        StartCoroutine(fadeOutText(endScreenTimeText, fadeOutTime));
        StartCoroutine(fadeOutText(endScreenMistakesText, fadeOutTime));
        StartCoroutine(fadeOutText(endScreenHintsText, fadeOutTime));
    }

    void makeAlphaZeroImage(Image image)
    {
        Color color = image.color;
        color.a = 0;
        image.color = color;
    }
    
    void makeAlphaZeroText(TextMeshProUGUI text)
    {
        Color color = text.color;
        color.a = 0;
        text.color = color;
    }

    public IEnumerator fadeInBackground(float fadeInTime)
    {
        float timer = 0;
        
        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = backgroundImage.color;
            c.a = timer / fadeInTime;
            backgroundImage.color = c;
            
            if (timer >= fadeInTime)
            {
                c = backgroundImage.color;
                c.a = 1;
                backgroundImage.color = c;
                yield break;
            }
            
            yield return null;
        }
    }
    
    public IEnumerator fadeInTheTopic(float fadeInTime)
    {
        float timer = 0;
        
        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = theTopicText.color;
            c.a = timer / fadeInTime;
            theTopicText.color = c;

            if (timer >= fadeInTime)
            {
                c = theTopicText.color;
                c.a = 1;
                theTopicText.color = c;
                yield break;
            }
            
            yield return null;
        }
    }

    public IEnumerator fadeInImage(Image image, float fadeInTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = image.color;
            c.a = timer / fadeInTime;
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
    
    public IEnumerator fadeInText(TextMeshProUGUI text, float fadeInTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = text.color;
            c.a = timer / fadeInTime;
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
    
    public IEnumerator fadeOutImage(Image image, float fadeOutTime)
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
    
    public IEnumerator fadeOutText(TextMeshProUGUI text, float fadeOutTime)
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
    
    public IEnumerator fadeInEndScreenText(TextMeshProUGUI text, float fadeInTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            float value = timer / fadeInTime;
            
            Color c = text.color;
            c.a = value;
            text.color = c;
            
            Vector3 scaleVector = new Vector3(0, 0, 1);
            scaleVector = new Vector3(value, value, 1);
            text.transform.localScale = scaleVector;

            if (timer >= fadeInTime)
            {
                c = text.color;
                c.a = 1;
                text.color = c;
                
                text.transform.localScale =new Vector3(1, 1, 1);
                
                yield break;
            }
            
            yield return null;
        }
    }

    public IEnumerator fadeInEndScreenBackground(Image image, float fadeInTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = image.color;
            c.a = (timer / fadeInTime) * (200 / 255f);
            image.color = c;

            if (timer >= fadeInTime)
            {
                c = image.color;
                c.a = (200 / 255f);
                image.color = c;
                yield break;
            }
            
            yield return null;
        }
    }
    
    public IEnumerator fadeOutEndScreenBackground(Image image, float fadeInTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = image.color;
            c.a = (200/255) - (timer / fadeInTime)*(200/255);
            image.color = c;

            if (timer >= fadeInTime)
            {
                c = image.color;
                c.a = 0;
                image.color = c;
                yield break;
            }
            
            yield return null;
        }
    }
    
}
