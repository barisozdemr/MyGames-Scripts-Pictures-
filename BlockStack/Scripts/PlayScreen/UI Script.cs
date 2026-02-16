using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TextMeshProUGUI = TMPro.TextMeshProUGUI;

public class UIScript : MonoBehaviour
{
    public GameObject logicManager;
    public LogicScript logicScript;

    public GameObject audioManager;
    public AudioScript audioScript;

    public Image rootImage;

    public Image pauseButton;
    public Sprite pauseSprite;
    public Sprite playSprite;
    private bool gamePaused = false;
    
    //========================================
    
    public Transform playground;
    
    public Sprite tetrisBlockSprite;
    
    private int playgroundRowCount = 13;
    private int playgroundColumnCount = 7;

    private float playgroundLeftAnchor = -420f;
    private float playgroundRightAnchor = 420f;
    
    private float playgroundTopAnchor = 835f;
    private float playgroundBottomAnchor = -725f;

    private Vector2 playgroundCenterAnchor;

    private float cellGap;

    private bool[][] previousMap;

    private Vector2[][] cellPositions;
    
    private GameObject[][] blockObjectsMap;
    
    //========================================
    
    public GameObject gameOverScreen;
    
    private Image[] gameOverScreenImages;
    
    private TextMeshProUGUI[] gameOverScreenTexts;

    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverTimeText;
    

    void Awake()
    {
        logicScript = logicManager.GetComponent<LogicScript>();
        audioScript = audioManager.GetComponent<AudioScript>();

        previousMap = new bool[playgroundRowCount][];
        cellPositions = new Vector2[playgroundRowCount][];
        blockObjectsMap = new GameObject[playgroundRowCount][];
        
        for (int i = 0; i < playgroundRowCount; i++)
        {
            previousMap[i] = new bool[playgroundColumnCount];
            cellPositions[i] = new Vector2[playgroundColumnCount];
            blockObjectsMap[i] = new GameObject[playgroundColumnCount];
        }
        
        float horizontalGap = playgroundRightAnchor - playgroundLeftAnchor;
        float verticalGap = playgroundTopAnchor - playgroundBottomAnchor;
        
        float horizontalGapHalf = horizontalGap * 0.5f;
        float verticalGapHalf = verticalGap * 0.5f;
        
        playgroundCenterAnchor = new Vector2(playgroundLeftAnchor + horizontalGapHalf, playgroundBottomAnchor + verticalGapHalf);
        
        cellGap = (playgroundRightAnchor - playgroundLeftAnchor) / playgroundColumnCount;
        
        for (int i = 0; i < playgroundRowCount; i++)
        {
            for (int j = 0; j < playgroundColumnCount; j++)
            {
                int horizontalGapFactor = -(playgroundColumnCount/2) + j;
                int verticalGapFactor = (playgroundRowCount/2) - i;
                
                float horizontalSlide = horizontalGapFactor * cellGap;
                float verticalSlide = verticalGapFactor * cellGap;
                
                cellPositions[i][j] = new Vector2(playgroundCenterAnchor.x + horizontalSlide, playgroundCenterAnchor.y + verticalSlide);
            }
        }

        gameOverScreenImages = gameOverScreen.GetComponentsInChildren<Image>();
        gameOverScreenTexts = gameOverScreen.GetComponentsInChildren<TextMeshProUGUI>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadeInAndStartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void fadeInAndStartGame()
    {
        StartCoroutine(fadeOutImageAndDisable(rootImage, 1.5f));
        Invoke("startGameViaLogicScript", 1.6f);
    }

    void startGameViaLogicScript()
    {
        logicManager.SetActive(true);
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

    //===================================================================================
    //============================================================== playground
    
    public void drawPlayground(bool[][] map)
    {
        for (int i = 0; i < playgroundRowCount; i++)
        {
            for (int j = 0; j < playgroundColumnCount; j++)
            {
                if (map[i][j] != previousMap[i][j])
                {
                    if (map[i][j] == true)
                    {
                        GameObject block = new GameObject("block_"+i+"_"+j);
                        block.gameObject.transform.SetParent(playground.transform);
                        block.gameObject.transform.localScale = new Vector3(1, 1, 1);
                        
                        RectTransform rect = block.AddComponent<RectTransform>();
                        rect.anchoredPosition = new Vector2(cellPositions[i][j].x, cellPositions[i][j].y);
                        rect.sizeDelta = new Vector2(110, 110);
                        
                        Image image = block.AddComponent<Image>();
                        image.sprite = tetrisBlockSprite;
                        
                        blockObjectsMap[i][j] = block;
                    }
                    else
                    {
                        Destroy(blockObjectsMap[i][j]);
                    }
                }
            }
        }
        
        previousMap = new bool[map.Length][];

        for (int i = 0; i < map.Length; i++)
        {
            previousMap[i] = new bool[map[i].Length];
            Array.Copy(map[i], previousMap[i], map[i].Length);
        }
    }

    public IEnumerator clearComboRows(List<int> comboRows)
    {
        float timer = 0;
        
        int comboCount = comboRows.Count;
        
        float totalTime = 1f;
        float blockFadeOutTime = 0.1f;
        float lastRowStartTime = totalTime - blockFadeOutTime * playgroundColumnCount;
        float timeStep;
        
        if(comboRows.Count == 1) timeStep = lastRowStartTime;
        else timeStep = lastRowStartTime / (comboCount-1);
        
        int index = 0;

        while (true)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < comboCount; i++)
            {
                if (index == i && timer >= timeStep * i)
                {
                    audioScript.playComboSoundClip();
                    
                    int row = comboRows[index];
                    for (int j = 0; j < playgroundColumnCount; j++)
                    {
                        StartCoroutine(fadeOutBlock(row, j, blockFadeOutTime));
                        previousMap[row][j] = false;
                    }
                    index++;
                    logicScript.increaseScore(5 * index);
                }
            }

            if (timer >= totalTime)
            {
                yield return StartCoroutine(moveDownRowsThatAboveComboRows(comboRows));
                yield break;
            }
            
            yield return null;
        }
    }

    public IEnumerator fadeOutBlock(int row, int column, float fadeOutTime)
    {
        float timer = 0;
        
        Image image = blockObjectsMap[row][column].GetComponent<Image>();
        
        while (true)
        {
            timer += Time.deltaTime;
            
            Color color = image.color;
            color.a = 1 - timer/fadeOutTime;
            image.color = color;

            if (timer >= fadeOutTime)
            {
                Destroy(blockObjectsMap[row][column]);
                yield break;
            }
            
            yield return null;
        }
    }
    
    IEnumerator moveDownRowsThatAboveComboRows(List<int> rowCombo)
    {
        float timer = 0;
        
        int comboCount = rowCombo.Count;

        float timeStep = 0.15f;
        float totalTime = timeStep * (comboCount + 2);

        int step = 0;
        while (true)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < comboCount; i++)
            {
                if (step == i && timer >= timeStep * i)
                {
                    audioScript.playRowCollapseSoundClip();
                    
                    shiftDownAboveRowAndDrawPlayground(rowCombo[i]);
                    step++;
                }
            }
            
            if (timer >= totalTime)
            {
                yield break;
            }
            
            yield return null;
        }
    }

    void shiftDownAboveRowAndDrawPlayground(int row)
    {
        bool[][] newMap = new bool[playgroundRowCount][];
        for (int i = 0; i < playgroundRowCount; i++)
        {
            newMap[i] = new bool[playgroundColumnCount];
            Array.Copy(previousMap[i], newMap[i], playgroundColumnCount);
        }
        
        for (int i = row; i > 0; i--)
        {
            for (int j = 0; j < playgroundColumnCount; j++)
            {
                newMap[i][j] = previousMap[i-1][j];
            }
        }

        for (int i = 0; i < playgroundColumnCount; i++)
        {
            newMap[0][i] = false;
        }
        
        drawPlayground(newMap);
    }
    
    //===================================================================================
    //============================================================== game over screen
    
    public void setGameOverScreen(String time, String score)
    {
        gameOverScreen.SetActive(true);
        
        gameOverScoreText.text = score;
        gameOverTimeText.text = time;

        StartCoroutine(fadeInImageArray(gameOverScreenImages, 2f));
        StartCoroutine(fadeInTextArray(gameOverScreenTexts, 2f));
    }
    
    IEnumerator fadeInImageArray(Image[] images, float fadeInTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;

            foreach (Image image in images)
            {
                Color c = image.color;
                c.a = (timer / fadeInTime);
                image.color = c;
            }

            if (timer >= fadeInTime)
            {
                foreach (Image image in images)
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

    IEnumerator fadeInTextArray(TextMeshProUGUI[] texts, float fadeInTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;

            foreach (TextMeshProUGUI text in texts)
            {
                Color c = text.color;
                c.a = (timer / fadeInTime);
                text.color = c;
            }

            if (timer >= fadeInTime)
            {
                foreach (TextMeshProUGUI text in texts)
                {
                    Color c = text.color;
                    c.a = 1;
                    text.color = c;
                }
                yield break;
            }
            
            yield return null;
        }
    }
    
    //===========================================

    public void playAgainButtonClicked()
    {
        audioScript.playButtonSoundClip();
        fadeOutGameOverScreen(); // 1.5 seconds coroutine
        Invoke("changeSceneToPlayScreen", 1.1f);
    }
    
    public void menuButtonClicked()
    {
        audioScript.playButtonSoundClip();
        fadeOutGameOverScreen(); // 1.5 seconds coroutine
        Invoke("changeSceneToMenu", 1.1f);
    }
    
    public void fadeOutGameOverScreen()
    {
        rootImage.gameObject.SetActive(true);

        StartCoroutine(fadeInImage(rootImage, 1f));
    }
    
    IEnumerator fadeInImage(Image image, float fadeInTime)
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
    
    void changeSceneToPlayScreen()
    {
        SceneManager.LoadScene("PlayScreen", LoadSceneMode.Single);
    }

    void changeSceneToMenu()
    {
        SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
    }
    
    //===================================================================================

    public void pauseButtonClicked()
    {
        if(gamePaused)
        {
            gamePaused = false;
            pauseButton.sprite = playSprite;
        }
        else
        {
            gamePaused = true;
            pauseButton.sprite = pauseSprite;
        }
        
        logicScript.togglePause();
    }

    public void leftButtonClicked()
    {
        audioScript.playButtonSoundClip();
        if (!gamePaused) logicScript.left();
    }
    
    public void rightButtonClicked()
    {
        audioScript.playButtonSoundClip();
        if (!gamePaused) logicScript.right();
    }
    
    public void rotateButtonClicked()
    {
        audioScript.playButtonSoundClip();
        if (!gamePaused) logicScript.rotate();
    }
}
