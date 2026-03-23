using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public GameObject logicManager;
    private LogicScript logicScript;
    
    private RocketSpriteProvider provider;
    
    private Transform playgroundTransform;
    
    public Image endScreenBackgroundImage;
    
    public Transform congratsTextImageTransform;
    public Image continueButtonImage;

    public Transform gameOverTextImageTransform;
    public Image menuButtonImage;
    
    //========================================================= Level & Playground Settings
    private Vector3[][] cellUIPositionMatrix;
    private int rowCount;
    private int colCount;

    private float cellGap;
    
    //====================================================================== Particles
    public Transform wonParticleTransform;
    public Transform blockParticleTransform;
    
    public GameObject rocketFireParticlePrefab;
    
    public GameObject leftWonParticlePrefab;
    public GameObject rightWonParticlePrefab;
    
    public GameObject highlighterPrefab;
    public GameObject blockClickedParticlePrefab;
    public GameObject rocketCreatedParticlePrefab;
    
    //======================================================================== Fade In Elements
    public Image playgroundBackgroundImage;
    public Image levelDataBackgroundImage;

    public TextMeshProUGUI moveCountText;
    public TextMeshProUGUI moveText;
    
    public TextMeshProUGUI obstacleText;
    
    public TextMeshProUGUI boxObstacleCountText;
    public Image boxObstacleImage;
    public Image boxObstacleBackgroundImage;
    
    public TextMeshProUGUI stoneObstacleCountText;
    public Image stoneObstacleImage;
    public Image stoneObstacleBackgroundImage;
    
    public TextMeshProUGUI vaseObstacleCountText;
    public Image vaseObstacleImage;
    public Image vaseObstacleBackgroundImage;

    public Image[] stage1FadeInImages = new Image[2];
    
    public Image[] stage2FadeInImages = new Image[6];
    public TextMeshProUGUI[] stage2FadeInTexts = new TextMeshProUGUI[6];
    
    //===========================================================================================
    
    private Image[] playgroundBlockImages; //= IEnumerator parameter
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logicScript = logicManager.GetComponent<LogicScript>();

        provider = new RocketSpriteProvider();

        stage1FadeInImages[0] = playgroundBackgroundImage;
        stage1FadeInImages[1] = levelDataBackgroundImage;
        
        stage2FadeInImages[0] = boxObstacleImage;
        stage2FadeInImages[1] = boxObstacleBackgroundImage;
        stage2FadeInImages[2] = stoneObstacleImage;
        stage2FadeInImages[3] = stoneObstacleBackgroundImage;
        stage2FadeInImages[4] = vaseObstacleImage;
        stage2FadeInImages[5] = vaseObstacleBackgroundImage;

        stage2FadeInTexts[0] = moveCountText;
        stage2FadeInTexts[1] = moveText;
        stage2FadeInTexts[2] = obstacleText;
        stage2FadeInTexts[3] = boxObstacleCountText;
        stage2FadeInTexts[4] = stoneObstacleCountText;
        stage2FadeInTexts[5] = vaseObstacleCountText;
        
        setAlphaZero();
        
        fadeInScene();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void highlightBlock(int row, int col)
    {
        GameObject highlighter = Instantiate(highlighterPrefab, blockParticleTransform);
        RectTransform rectTransform = highlighter.GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = cellUIPositionMatrix[row][col];
    }

    public void blockClickedParticles(int row, int col)
    {
        GameObject particle = Instantiate(blockClickedParticlePrefab, blockParticleTransform);
        RectTransform rectTransform = particle.GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = cellUIPositionMatrix[row][col];
    }
    
    public void rocketCreatedParticles(int row, int col)
    {
        GameObject particle = Instantiate(rocketCreatedParticlePrefab, blockParticleTransform);
        RectTransform rectTransform = particle.GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = cellUIPositionMatrix[row][col];
    }

    void setAlphaZero()
    {
        foreach (Image image in stage1FadeInImages)
        {
            image.color = new Color(255, 255, 255, 0);
        }
        
        foreach (Image image in stage2FadeInImages)
        {
            image.color = new Color(255, 255, 255, 0);
        }
        
        foreach (TextMeshProUGUI text in stage2FadeInTexts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        }
    }

    void fadeInScene()
    {
        stage1ElementsFadeIn();
        Invoke("stage2ElementsFadeIn", 0.5f);
    }

    void stage1ElementsFadeIn()
    {
        StartCoroutine(fadeInImageArray(stage1FadeInImages, 0.5f));
    }

    void stage2ElementsFadeIn()
    {
        StartCoroutine(fadeInImageArray(stage2FadeInImages, 0.5f));
        StartCoroutine(fadeInTextArray(stage2FadeInTexts, 0.5f));
    }

    public void playgroundFadeIn(Image[] images)
    {
        playgroundBlockImages = images;
        Invoke("playgroundFadeInInvoke", 0.5f);
    }

    void playgroundFadeInInvoke()
    {
        StartCoroutine(fadeInImageArray(playgroundBlockImages, 0.5f));
    }

    public void setTexts(int moveCount, int boxObstacleCount, int stoneObstacleCount, int vaseObstacleCount)
    {
        moveCountText.text = moveCount.ToString();
        boxObstacleCountText.text = boxObstacleCount.ToString();
        stoneObstacleCountText.text = stoneObstacleCount.ToString();
        vaseObstacleCountText.text = vaseObstacleCount.ToString();
    }

    public void setUIPositionMatrix(Vector3[][] cellUIPositionMatrix)
    {
        this.cellUIPositionMatrix = cellUIPositionMatrix;
        rowCount = cellUIPositionMatrix.Length;
        colCount = cellUIPositionMatrix[0].Length;
    }

    public void setCellGap(float gap)
    {
        this.cellGap = gap;
    }

    public void setPlaygroundTransform(Transform transform)
    {
        this.playgroundTransform = transform;
    }
    
    //==================================================================================================================

    class RocketSpriteProvider
    {
        private Sprite leftRocketSprite;
        private Sprite rightRocketSprite;
        private Sprite upRocketSprite;
        private Sprite downRocketSprite;

        public RocketSpriteProvider()
        {
            leftRocketSprite = Resources.Load<Sprite>("BlockTextures/leftRocket");
            rightRocketSprite = Resources.Load<Sprite>("BlockTextures/rightRocket");
            upRocketSprite = Resources.Load<Sprite>("BlockTextures/upRocket");
            downRocketSprite = Resources.Load<Sprite>("BlockTextures/downRocket");
        }

        public Sprite getLeftRocketSprite()
        {
            return leftRocketSprite;
        }
        
        public Sprite getRightRocketSprite()
        {
            return rightRocketSprite;
        }
        
        public Sprite getUpRocketSprite()
        {
            return upRocketSprite;
        }
        
        public Sprite getDownRocketSprite()
        {
            return downRocketSprite;
        }
    }
    
    public IEnumerator fall(int column, Dictionary<RectTransform, int> fallingBlocksRectAndRow, int count, float cellGap, bool[] flags, int flagIndex)
    {
        var enumerator = fallingBlocksRectAndRow.Keys.GetEnumerator();
        enumerator.MoveNext();
        
        RectTransform referanceRectKey = enumerator.Current;
        
        float targetY = referanceRectKey.anchoredPosition.y - count*cellGap;
        float speed = -2 * cellGap;
        float acceleration = -25 * cellGap;
    
        while(true){
            foreach(RectTransform rect in fallingBlocksRectAndRow.Keys){
                Vector2 pos = rect.anchoredPosition;
                pos.y += Time.deltaTime * speed;
                rect.anchoredPosition = pos;
            }
                
            speed += Time.deltaTime * acceleration;
        
            if(referanceRectKey.anchoredPosition.y <= targetY){
                foreach(var pair in fallingBlocksRectAndRow){
                    pair.Key.anchoredPosition = cellUIPositionMatrix[pair.Value][column];
                }
            
                flags[flagIndex] = true;
                yield break;
            }
        
            yield return null;
        }
    }
    
    public IEnumerator shootRocketsHorizontal(int row, int col, float timeStep, LogicScript.BoolRef flag)
    {
        int maxCellDistance = Math.Max(col + 1, colCount - col);
        
        float rocket1TargetX = cellUIPositionMatrix[row][col].x - maxCellDistance * cellGap;
        float speed = cellGap / timeStep;
    
        GameObject rocket1 = new GameObject("rocket1");
        rocket1.transform.localScale = new Vector3(1, 1, 1);
        rocket1.transform.SetParent(playgroundTransform);
    
        RectTransform rect1 = rocket1.AddComponent<RectTransform>();
        rect1.anchoredPosition3D = cellUIPositionMatrix[row][col];
        Vector3 rect1Pos = new Vector3(rect1.anchoredPosition3D.x, rect1.anchoredPosition3D.y, -1);
        rect1.anchoredPosition3D = rect1Pos;
        rect1.sizeDelta = new Vector2(cellGap, cellGap);
        rect1.localScale = new Vector2(1,1);
        
        Image image1 = rocket1.AddComponent<Image>();
        image1.sprite = provider.getLeftRocketSprite();
        
        GameObject fireParticle1 = Instantiate(rocketFireParticlePrefab, rocket1.transform);
        fireParticle1.transform.localRotation = Quaternion.Euler(0,90,315);
        fireParticle1.transform.localPosition = Vector3.zero;
    
        //==================================================================
        GameObject rocket2 = new GameObject("rocket2");
        rocket2.transform.localScale = new Vector3(1, 1, 1);
        rocket2.transform.SetParent(playgroundTransform);
    
        RectTransform rect2 = rocket2.AddComponent<RectTransform>();
        rect2.anchoredPosition3D = cellUIPositionMatrix[row][col];
        Vector3 rect2Pos = new Vector3(rect2.anchoredPosition3D.x, rect2.anchoredPosition3D.y, -1);
        rect2.anchoredPosition3D = rect2Pos;
        rect2.sizeDelta = new Vector2(cellGap, cellGap);
        rect2.localScale = new Vector2(1,1);
        
        Image image2 = rocket2.AddComponent<Image>();
        image2.sprite = provider.getRightRocketSprite();
        
        GameObject fireParticle2 = Instantiate(rocketFireParticlePrefab, rocket2.transform);
        fireParticle2.transform.localRotation = Quaternion.Euler(180,90,315);
        fireParticle2.transform.localPosition = Vector3.zero;
    
        while (true)
        {
            rect1.anchoredPosition -=  speed * Time.deltaTime * new Vector2(1, 0);
            rect2.anchoredPosition +=  speed * Time.deltaTime * new Vector2(1, 0);
        
            if(rect1.anchoredPosition.x <= rocket1TargetX)
            {
                Destroy(rocket1);
                Destroy(rocket2);
                Debug.Log("row:"+row+", col:"+col+"\nui flag.setTrue()");
                flag.setTrue();
                yield break;
            }
        
            yield return null;
        }
    }
    
    public IEnumerator shootRocketsVertical(int row, int col, float timeStep, LogicScript.BoolRef flag)
    {
        int maxCellDistance = Math.Max(row + 1, rowCount - row);
        
        float rocket1TargetY = cellUIPositionMatrix[row][col].y - maxCellDistance * cellGap;
        float speed = cellGap / timeStep;
    
        GameObject rocket1 = new GameObject("rocket1");
        rocket1.transform.localScale = new Vector3(1, 1, 1);
        rocket1.transform.SetParent(playgroundTransform);
    
        RectTransform rect1 = rocket1.AddComponent<RectTransform>();
        rect1.anchoredPosition3D = cellUIPositionMatrix[row][col];
        Vector3 rect1Pos = new Vector3(rect1.anchoredPosition3D.x, rect1.anchoredPosition3D.y, -1);
        rect1.anchoredPosition3D = rect1Pos;
        rect1.sizeDelta = new Vector2(cellGap, cellGap);
        rect1.localScale = new Vector2(1,1);
        
        Image image1 = rocket1.AddComponent<Image>();
        image1.sprite = provider.getDownRocketSprite();
        
        GameObject fireParticle1 = Instantiate(rocketFireParticlePrefab, rocket1.transform);
        fireParticle1.transform.localRotation = Quaternion.Euler(270,90,315);
        fireParticle1.transform.localPosition = Vector3.zero;
    
        //==================================================================
        GameObject rocket2 = new GameObject("rocket2");
        rocket2.transform.localScale = new Vector3(1, 1, 1);
        rocket2.transform.SetParent(playgroundTransform);
    
        RectTransform rect2 = rocket2.AddComponent<RectTransform>();
        rect2.anchoredPosition3D = cellUIPositionMatrix[row][col];
        Vector3 rect2Pos = new Vector3(rect2.anchoredPosition3D.x, rect2.anchoredPosition3D.y, -1);
        rect2.anchoredPosition3D = rect2Pos;
        rect2.sizeDelta = new Vector2(cellGap, cellGap);
        rect2.localScale = new Vector2(1,1);
        
        Image image2 = rocket2.AddComponent<Image>();
        image2.sprite = provider.getUpRocketSprite();
        
        GameObject fireParticle2 = Instantiate(rocketFireParticlePrefab, rocket2.transform);
        fireParticle2.transform.localRotation = Quaternion.Euler(90,90,315);
        fireParticle2.transform.localPosition = Vector3.zero;
    
        while (true)
        {
            rect1.anchoredPosition -=  speed * Time.deltaTime * new Vector2(0, 1);
            rect2.anchoredPosition +=  speed * Time.deltaTime * new Vector2(0, 1);
        
            if(rect1.anchoredPosition.y <= rocket1TargetY)
            {
                Destroy(rocket1);
                Destroy(rocket2);
                flag.setTrue();
                yield break;
            }
        
            yield return null;
        }
    }
    
    //==================================================================================================================

    public void continueButtonClicked()
    {
        SceneManager.LoadScene("LevelSelectScene", LoadSceneMode.Single);
    }
    
    public void menuButtonClicked()
    {
        SceneManager.LoadScene("LevelSelectScene", LoadSceneMode.Single);
    }
    
    //==================================================================================================================
    void setEndScreenBackground(float fadeInTime)
    {
        endScreenBackgroundImage.transform.gameObject.SetActive(true);
        StartCoroutine(fadeInEndScreenBackgroundImage(fadeInTime));
    }
    
    //======================================================
    public void setGameOverUI()
    {
        setEndScreenBackground(0.3f);
        setGameOverTextImage(0.5f);
        
        Invoke("setMenuButtonInvoke", 0.5f);
    }
    
    void setMenuButtonInvoke()
    {
        setMenuButton(0.3f);
    }
    void setMenuButton(float fadeInTime)
    {
        menuButtonImage.transform.gameObject.SetActive(true);
        StartCoroutine(fadeInImage(menuButtonImage, fadeInTime));
    }
    
    void setGameOverTextImage(float setTime)
    {
        gameOverTextImageTransform.gameObject.SetActive(true);
        StartCoroutine(setTextScale(gameOverTextImageTransform, setTime));
    }

    //======================================================
    public void setLevelWonUI()
    {
        setEndScreenBackground(0.3f);
        setCongratsTextImage(0.5f);
        GameObject leftWonParticle = Instantiate(leftWonParticlePrefab, wonParticleTransform);
        GameObject rightWonParticle = Instantiate(rightWonParticlePrefab, wonParticleTransform);
        
        Invoke("setContinueButtonInvoke", 0.5f);
    }

    void setContinueButtonInvoke()
    {
        setContinueButton(0.3f);
    }
    void setContinueButton(float fadeInTime)
    {
        continueButtonImage.transform.gameObject.SetActive(true);
        StartCoroutine(fadeInImage(continueButtonImage, fadeInTime));
    }

    void setCongratsTextImage(float setTime)
    {
        congratsTextImageTransform.gameObject.SetActive(true);
        StartCoroutine(setTextScale(congratsTextImageTransform, setTime));
    }
    
    //======================================================
    IEnumerator setTextScale(Transform textTransform, float setTime)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            if (timer >= setTime)
            {
                textTransform.localScale = new Vector3(1, 1, 1);
                yield break;
            }
            
            float value = (timer / setTime);
            textTransform.localScale = new Vector3(value, value, 1);
            
            yield return null;
        }
    }

    IEnumerator fadeInEndScreenBackgroundImage(float fadeInTime)
    {
        float timer = 0;
        
        Color c = endScreenBackgroundImage.color;
        c.a = 0;

        while (true)
        {
            timer += Time.deltaTime;
            
            if (timer >= fadeInTime)
            {
                c.a = 220/255f;
                endScreenBackgroundImage.color = c;
                yield break;
            }
            
            c.a = (timer / fadeInTime) * 220/255f;
            endScreenBackgroundImage.color = c;
            
            yield return null;
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
    
    //==================================================================================================================
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
}
