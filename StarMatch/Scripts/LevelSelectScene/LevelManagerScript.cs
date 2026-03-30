using System;
using System.Collections;
using TMPro;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private string LEVELNUMBERPATH = "level_number";
    private string LEVELDATAPATH = "level_data";
    private string LOADEDLEVELDATAPATH = "loaded_level_data";
    
    public GameObject AudioManager;
    private AudioScript audioScript;
    
    public TextMeshProUGUI playButtonText;
    public Image starMatchLogoImage;

    public Transform playButtonTransform;
    public RectTransform starMatchLogoRect;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefreshRate rr = Screen.currentResolution.refreshRateRatio;
        float hz = (float)rr.numerator / (float)rr.denominator;
        Application.targetFrameRate = Mathf.RoundToInt(hz);
        
        LEVELNUMBERPATH = Path.Combine(Application.persistentDataPath, "level_number.txt");
        LOADEDLEVELDATAPATH = Path.Combine(Application.persistentDataPath, "loaded_level_data.txt");
        setLevelData();
        
        audioScript = AudioManager.GetComponent<AudioScript>();
        
        playButtonTransform.localScale = new Vector3(0,0,1);
        playButtonTransform.gameObject.SetActive(false);
        
        Color c = starMatchLogoImage.color;
        c.a = 0;
        starMatchLogoImage.color = c;
        
        Invoke("sceneFadeIn", 0.3f);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            AndroidJavaObject activity =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call<bool>("moveTaskToBack", true);
        }
    }

    void sceneFadeIn()
    {
        StartCoroutine(fadeInImage(starMatchLogoImage, 0.5f));
        StartCoroutine(setScaleAppear(playButtonTransform, 0.4f));
    }

    public void playButtonClicked()
    {
        audioScript.playButtonClickedSound();
        
        StartCoroutine(setScaleDisappear(playButtonTransform, 0.2f));
        Invoke("moveLogoOutInvoke", 0.1f);
        
        Invoke("openPlayScene", 1.35f);
    }

    void moveLogoOutInvoke()
    {
        StartCoroutine(moveLogoOut(starMatchLogoRect));
    }

    void openPlayScene()
    {
        SceneManager.LoadScene("PlayScene", LoadSceneMode.Single);
    }

    void setLevelData()
    {
        if (!File.Exists(LEVELNUMBERPATH))
        {
            File.WriteAllText(LEVELNUMBERPATH, "1");
        }
        
        string number = File.ReadAllText(LEVELNUMBERPATH);
        
        int nextLevelNumber = int.Parse(number);
        
        playButtonText.text = "Level: " + nextLevelNumber.ToString();
        
        TextAsset levelDataAsset = Resources.Load<TextAsset>(LEVELDATAPATH);
        string[] levels = levelDataAsset.text.Split(';');

        try
        {
            string level = levels[nextLevelNumber - 1].Trim();
            File.WriteAllText(LOADEDLEVELDATAPATH, level);
        }
        catch (Exception e)
        {
            File.WriteAllText(LEVELNUMBERPATH, "1");
            string level = levels[0].Trim();
            File.WriteAllText(LOADEDLEVELDATAPATH, level);
            playButtonText.text = "Level: 1";
        }
        
    }
    
    IEnumerator fadeInImage(Image image, float fadeInTime)
    {
        float timer = 0;

        Color c = image.color;
        c.a = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            if (timer >= fadeInTime)
            {
                c.a = 1;
                image.color = c;
                yield break;
            }
            
            c.a = (timer / fadeInTime);
            
            image.color = c;
            
            yield return null;
        }
    }
    
    IEnumerator setScaleAppear(Transform transform, float setTime)
    {
        transform.gameObject.SetActive(true);
        
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            if (timer >= setTime)
            {
                transform.localScale = new Vector3(1, 1, 1);
                yield break;
            }
            
            float value = (timer / setTime);
            transform.localScale = new Vector3(value, value, 1);
            
            yield return null;
        }
    }
    
    IEnumerator setScaleDisappear(Transform transform, float setTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            if (timer >= setTime)
            {
                transform.localScale = new Vector3(0, 0, 1);
                yield break;
            }
            
            float value = 1 - (timer / setTime);
            transform.localScale = new Vector3(value, value, 1);
            
            yield return null;
        }
    }

    IEnumerator moveLogoOut(RectTransform rect)
    {
        float moveDownOffset = 150f; // px
        float moveDownSpeed = -50f;
        float moveDownAccel = -3000f;
        float downDistanceTravelled = 0f;
        float moveDownSpeedMinimum = moveDownSpeed;
        
        float moveUpOffset = 900f; // px (excluding moveDownOffset)
        float moveUpSpeed = 200f;
        float moveUpAccel = 10000f;
        float upDistanceTravelled = 0f;
        float moveUpSpeedMinimum = moveUpSpeed;

        moveUpOffset += moveDownOffset;
        int stage = 1;
        while (true)
        {
            switch (stage)
            {
                case 1:
                {
                    float travelDistance = moveDownSpeed * Time.deltaTime;
                    rect.anchoredPosition += new Vector2(0, travelDistance);
                    moveDownSpeed += moveDownAccel * Time.deltaTime;
                    
                    downDistanceTravelled += Math.Abs(travelDistance);

                    if (downDistanceTravelled >= moveDownOffset / 2)
                    {
                        stage = 2;
                    }
                    
                    break;
                }
                case 2:
                {
                    float travelDistance = moveDownSpeed * Time.deltaTime;
                    rect.anchoredPosition += new Vector2(0, travelDistance);
                    moveDownSpeed -= moveDownAccel * Time.deltaTime;
                    
                    if(moveDownSpeed >= moveDownSpeedMinimum) moveDownSpeed = moveDownSpeedMinimum;
                    
                    downDistanceTravelled += Math.Abs(travelDistance);

                    if (downDistanceTravelled >= moveDownOffset)
                    {
                        stage = 3;
                    }
                    
                    break;
                }
                case 3:
                {
                    float travelDistance = moveUpSpeed * Time.deltaTime;
                    rect.anchoredPosition += new Vector2(0, travelDistance);
                    moveUpSpeed += moveUpAccel * Time.deltaTime;
                    
                    upDistanceTravelled += Math.Abs(travelDistance);

                    if (upDistanceTravelled >= moveUpOffset / 2)
                    {
                        stage = 4;
                    }
                    
                    break;
                }
                case 4:
                {
                    float travelDistance = moveUpSpeed * Time.deltaTime;
                    rect.anchoredPosition += new Vector2(0, travelDistance);
                    moveUpSpeed -= moveUpAccel * Time.deltaTime;
                    
                    if(moveUpSpeed <= moveUpSpeedMinimum) moveUpSpeed = moveUpSpeedMinimum;
                    
                    upDistanceTravelled += Math.Abs(travelDistance);

                    if (upDistanceTravelled >= moveUpOffset)
                    {
                        yield break;
                    }
                    
                    break;
                }
            }
            
            
            yield return null;
        }
    }
}
