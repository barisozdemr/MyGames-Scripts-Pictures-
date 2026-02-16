using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject audioManager;
    public AudioScript audioScript;
    
    public Image rootImage;
    
    public RectTransform blockStackTextImage;

    private float textMovementTop_y = 1500f;
    private float textMovementBottom_y = 200f;
    
    private float textMovementStage1StartSpeed = -60f;
    private float textMovementStage2StartSpeed = 700f;
    
    public RectTransform playButton;
    
    private float playButtonMovementLeft_y = -100f;
    private float playButtonMovementRight_y = 1500f;
    
    private float playButtonMovementStage1StartSpeed = -60f;
    private float playButtonMovementStage2StartSpeed = 700f;
    
    public RectTransform settingsButton;
    
    private float settingsButtonMovementRight_y = 100f;
    private float settingsButtonMovementLeft_y = -1500f;
    
    private float settingsButtonMovementStage1StartSpeed = 60f;
    private float settingsButtonMovementStage2StartSpeed = -700f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioScript = audioManager.GetComponent<AudioScript>();
        
        StartCoroutine(fadeOutImageAndDisable(rootImage, 1f));
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void playButtonClicked()
    {
        audioScript.playButtonSoundClip();
        
        StartCoroutine(moveButton(playButton, 
            playButtonMovementLeft_y, playButtonMovementRight_y, 
            playButtonMovementStage1StartSpeed, playButtonMovementStage2StartSpeed));
        StartCoroutine(moveButton(settingsButton,
            settingsButtonMovementRight_y, settingsButtonMovementLeft_y,
            settingsButtonMovementStage1StartSpeed, settingsButtonMovementStage2StartSpeed));
        StartCoroutine(moveBlockStackTextImageUp(blockStackTextImage,
            textMovementBottom_y, textMovementTop_y,
            textMovementStage1StartSpeed, textMovementStage2StartSpeed));
        
        Invoke("changeSceneToPlayScreen", 1f);
    }

    void changeSceneToPlayScreen()
    {
        SceneManager.LoadScene("PlayScreen", LoadSceneMode.Single);
    }

    public void settingsButtonClicked()
    {
        audioScript.playButtonSoundClip();
        
        StartCoroutine(moveButton(playButton, 
            playButtonMovementLeft_y, playButtonMovementRight_y, 
            playButtonMovementStage1StartSpeed, playButtonMovementStage2StartSpeed));
        StartCoroutine(moveButton(settingsButton,
            settingsButtonMovementRight_y, settingsButtonMovementLeft_y,
            settingsButtonMovementStage1StartSpeed, settingsButtonMovementStage2StartSpeed));
        StartCoroutine(moveBlockStackTextImageUp(blockStackTextImage,
            textMovementBottom_y, textMovementTop_y,
            textMovementStage1StartSpeed, textMovementStage2StartSpeed));
        
        Invoke("changeSceneToSettingsScreen", 1f);
    }
    
    void changeSceneToSettingsScreen()
    {
        SceneManager.LoadScene("SettingsScreen", LoadSceneMode.Single);
    }

    IEnumerator moveBlockStackTextImageUp(RectTransform rectTransform, float firstGoalPointY, float secondGoalPointY, float firstSpeed, float secondSpeed)
    { 
        float timer = 0;
        
        float stage1Speed = firstSpeed;
        float stage1Length = Math.Abs(firstGoalPointY - rectTransform.anchoredPosition.y);
        float stage1HalfLength = stage1Length / 2;
        float stage1Travelled = 0;
        
        float stage2Speed = secondSpeed;
        float stage2Length = Math.Abs(secondGoalPointY - firstGoalPointY);
        float stage2HalfLength = stage2Length / 2;
        float stage2Travelled = 0;
        
        bool s = true;
        
        int stage = 1;
        while (true)
        {
            timer += Time.deltaTime;

            if (stage == 1)
            {
                float yPos = rectTransform.anchoredPosition.y;

                float travel = stage1Speed * Time.deltaTime;
                
                float newYPos = yPos + travel;
                
                Vector2 newPos = new Vector2(rectTransform.anchoredPosition.x, newYPos);
                
                rectTransform.anchoredPosition = newPos;
                
                stage1Travelled += Math.Abs(travel);
                
                if (stage1Travelled >= stage1HalfLength) // slower
                {
                    if (s)
                    {
                        float speedMultiplyFactor = (stage1Length + stage1HalfLength - stage1Travelled) / stage1Length * 100;

                        stage1Speed -= speedMultiplyFactor * Time.deltaTime * firstSpeed;
                    }
                    
                    if (stage1Speed > firstSpeed)
                    {
                        stage1Speed = firstSpeed;
                        s = false;
                    }
                }
                else // faster
                {
                    float speedMultiplyFactor = (stage1Length - stage1Travelled) / stage1Length * 100;
                    
                    stage1Speed += speedMultiplyFactor * Time.deltaTime * firstSpeed;
                }

                if(newYPos <= firstGoalPointY) stage = 2;
            }
            else if (stage == 2)
            {
                float yPos = rectTransform.anchoredPosition.y;

                float travel = stage2Speed * Time.deltaTime;
                
                float newYPos = yPos + travel;
                
                Vector2 newPos = new Vector2(rectTransform.anchoredPosition.x, newYPos);
                
                rectTransform.anchoredPosition = newPos;
                
                stage2Travelled += travel;
                
                if (stage2Travelled >= stage2HalfLength) // slower
                {
                    if (s)
                    {
                        float speedMultiplyFactor = (stage2Length - Math.Abs(stage2Travelled)) / stage2Length * 75;
                        speedMultiplyFactor = Math.Abs(speedMultiplyFactor);
                    
                        stage2Speed -= speedMultiplyFactor * Time.deltaTime * secondSpeed;
                    }

                    if (stage2Speed < secondSpeed)
                    {
                        stage2Speed = secondSpeed;
                        s = false;
                    }
                }
                else // faster
                {
                    float speedMultiplyFactor = (stage2Length - Math.Abs(stage2Travelled)) / stage2Length * 75;
                    speedMultiplyFactor = Math.Abs(speedMultiplyFactor);
                    
                    stage2Speed += speedMultiplyFactor * Time.deltaTime * secondSpeed;
                }
                
                if(Math.Abs(newYPos) >= Math.Abs(secondGoalPointY)) stage = 3;
            }
            else if(stage == 3) yield break;
            
            yield return null;
        }
    }
    
    IEnumerator moveButton(RectTransform rectTransform, float firstGoalPointX, float secondGoalPointX, float firstSpeed, float secondSpeed)
    {
        float timer = 0;
        
        float stage1Speed = firstSpeed;
        float stage1Length = Math.Abs(firstGoalPointX - rectTransform.anchoredPosition.x);
        float stage1HalfLength = stage1Length / 2;
        float stage1Travelled = 0;
        
        float stage2Speed = secondSpeed;
        float stage2Length = Math.Abs(secondGoalPointX - firstGoalPointX);
        float stage2HalfLength = stage2Length / 2;
        float stage2Travelled = 0;
        
        bool s = true;
        
        int stage = 1;
        while (true)
        {
            timer += Time.deltaTime;

            if (stage == 1)
            {
                float xPos = rectTransform.anchoredPosition.x;

                float travel = stage1Speed * Time.deltaTime;
                
                float newXPos = xPos + travel;
                
                Vector2 newPos = new Vector2(newXPos, rectTransform.anchoredPosition.y);
                
                rectTransform.anchoredPosition = newPos;
                
                stage1Travelled += Math.Abs(travel);

                if (stage1Travelled >= stage1HalfLength)
                {
                    float speedMultiplyFactor = (stage1Length + stage1HalfLength - stage1Travelled) / stage1Length * 100;
                    speedMultiplyFactor = Math.Abs(speedMultiplyFactor);
                    
                    stage1Speed -= speedMultiplyFactor * Time.deltaTime * firstSpeed;
                }
                else
                {
                    float speedMultiplyFactor = (stage1Length - stage1Travelled) / stage1Length * 100;
                    speedMultiplyFactor = Math.Abs(speedMultiplyFactor);
                    
                    stage1Speed += speedMultiplyFactor * Time.deltaTime * firstSpeed;
                }

                if(Math.Abs(newXPos) >= Math.Abs(firstGoalPointX)) stage = 2;
            }
            else if (stage == 2)
            {
                float xPos = rectTransform.anchoredPosition.x;

                float travel = stage2Speed * Time.deltaTime;
                
                float newXPos = xPos + travel;
                
                Vector2 newPos = new Vector2(newXPos, rectTransform.anchoredPosition.y);
                
                rectTransform.anchoredPosition = newPos;
                
                stage2Travelled += travel;
                
                if (stage2Travelled >= stage2HalfLength) // slower
                {
                    if (s)
                    {
                        float speedMultiplyFactor = (stage2Length - Math.Abs(stage2Travelled)) / stage2Length * 90;
                        speedMultiplyFactor = Math.Abs(speedMultiplyFactor);
                    
                        stage2Speed -= speedMultiplyFactor * Time.deltaTime * secondSpeed;
                    }

                    if (stage2Speed < secondSpeed)
                    {
                        stage2Speed = secondSpeed;
                        s = false;
                    }
                }
                else // faster
                {
                    float speedMultiplyFactor = (stage2Length - Math.Abs(stage2Travelled)) / stage2Length * 90;
                    speedMultiplyFactor = Math.Abs(speedMultiplyFactor);
                    
                    stage2Speed += speedMultiplyFactor * Time.deltaTime * secondSpeed;
                }
                
                if(Math.Abs(newXPos) >= Math.Abs(secondGoalPointX)) stage = 3;
            }
            else if(stage == 3) yield break;
            
            yield return null;
        }
    }
    
    
}
