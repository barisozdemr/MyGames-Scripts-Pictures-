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
    
    public TextMeshProUGUI playButtonText;
    public Image playButtonImage;

    public Transform playButtonTransform;
    public RectTransform starMatchLogoRect;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LEVELNUMBERPATH = Path.Combine(Application.persistentDataPath, "level_number.txt");
        LOADEDLEVELDATAPATH = Path.Combine(Application.persistentDataPath, "loaded_level_data.txt");
        setLevelData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playButtonClicked()
    {
        SceneManager.LoadScene("PlayScene", LoadSceneMode.Single);
    }

    void setLevelData()
    {
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
            Debug.Log(e);
            File.WriteAllText(LEVELNUMBERPATH, "1");
            string level = levels[0].Trim();
            File.WriteAllText(LOADEDLEVELDATAPATH, level);
            playButtonText.text = "Level: 1";
        }
        
    }
    
    IEnumerator fadeInImageArray(Image[] images, float fadeInTime)
    {
        float timer = 0;

        Color c = new Color(); // white, zero alpha
        c.r = 255;
        c.g = 255;
        c.b = 255;
        c.a = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            if (timer >= fadeInTime)
            {
                c.a = 1;
                foreach (Image image in images) image.color = c;
                yield break;
            }
            
            c.a = (timer / fadeInTime);
            
            foreach (Image image in images) image.color = c;
            
            yield return null;
        }
    }

    IEnumerator fadeInTextArray(TextMeshProUGUI[] texts, float fadeInTime)
    {
        float timer = 0;

        Color c;

        while (true)
        {
            timer += Time.deltaTime;
            
            if (timer >= fadeInTime)
            {
                foreach (TextMeshProUGUI text in texts)
                {
                    c = text.color;
                    c.a = 1;
                    text.color = c;
                }
                yield break;
            }
            
            foreach (TextMeshProUGUI text in texts)
            {
                c = text.color;
                c.a = (timer / fadeInTime);
                text.color = c;
            }
            
            yield return null;
        }
    }

    IEnumerator moveLogoOut(RectTransform rect, float outTime)
    {
        float timer = 0f;

        float moveDownOffset = 100f; // px
        float moveUpOffset = 850f; // px (excluding moveDownOffset)

        while (true)
        {
            timer += Time.deltaTime;
            
            
            
            yield return null;
        }
    }
}
