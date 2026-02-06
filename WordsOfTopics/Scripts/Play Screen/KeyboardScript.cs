using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardScript : MonoBehaviour
{
    public GameObject logicManager;
    private LogicScript logicScript;
    
    private char letterPressed;
    
    public GameObject keyboard;
    
    public Image keyboardBackgroundImage;
    
    public List<Image> keyboardImages = new List<Image>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logicScript = logicManager.GetComponent<LogicScript>();
        
        Color color = keyboardBackgroundImage.color;
        color.a = 0;
        keyboardBackgroundImage.color = color;
        
        keyboardImages.Add(keyboardBackgroundImage);
        
        setKeyboardButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setKeyboardButtons()
    {
        foreach (Transform child in keyboard.transform)
        {
            String name = child.name;

            if (name == "Space")
            {
                Button button = child.GetComponent<Button>();
                button.onClick.AddListener(() => { logicScript.keyboardClicked('_'); });
                
                ColorBlock colorBlock = button.colors;
                colorBlock.disabledColor = Color.white;

                Color c = button.image.color;
                c.a = 0;
                button.image.color = c;
                
                keyboardImages.Add(child.GetComponent<Image>());
            }
            else
            {
                Button button = child.GetComponent<Button>();
                button.onClick.AddListener(() => { logicScript.keyboardClicked(name[0]); });
                
                ColorBlock colorBlock = button.colors;
                colorBlock.disabledColor = Color.white;
                
                Color c = button.image.color;
                c.a = 0;
                button.image.color = c;
                
                keyboardImages.Add(child.GetComponent<Image>());
            }
        }
    }

    public IEnumerator fadeInKeyboard(float fadeInTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;

            foreach (Image image in keyboardImages)
            {
                Color c = image.color;
                c.a = timer / fadeInTime;
                image.color = c;
            }

            if (timer >= fadeInTime)
            {
                foreach (Image image in keyboardImages)
                {
                    Color c = image.color;
                    c.a = 1;
                    image.color = c;
                }
                
                yield break;
            }
            
            yield return null;
        }
    }
    
    public IEnumerator fadeOutKeyboard(float fadeOutTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;

            foreach (Image image in keyboardImages)
            {
                Color c = image.color;
                c.a = 1 - (timer / fadeOutTime);
                image.color = c;
            }

            if (timer >= fadeOutTime)
            {
                foreach (Image image in keyboardImages)
                {
                    Color c = image.color;
                    c.a = 0;
                    image.color = c;
                }
                
                yield break;
            }
            
            yield return null;
        }
    }
    
    
    
    
    
}
