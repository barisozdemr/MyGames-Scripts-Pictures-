using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    private RocketSpriteProvider provider;
    
    private Transform playgroundTransform;
    
    public GameObject LogicManager;
    private LogicScript logicScript;
    
    private Vector2[][] cellUIPositionMatrix;

    private float cellGap;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logicScript = LogicManager.GetComponent<LogicScript>();

        provider = new RocketSpriteProvider();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setUIPositionMatrix(Vector2[][] cellUIPositionMatrix)
    {
        this.cellUIPositionMatrix = cellUIPositionMatrix;
    }

    public void setCellGap(float gap)
    {
        this.cellGap = gap;
    }

    public void setPlaygroundTransform(Transform transform)
    {
        this.playgroundTransform = transform;
    }

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
    
    public IEnumerator shootRocketsHorizontal(int row, int col, float timeStep, bool[] flags, int flagIndex)
    {
        float rocket1TargetX = cellUIPositionMatrix[row][col].x - (col + 1) * cellGap;
        float speed = cellGap / timeStep;
    
        GameObject rocket1 = new GameObject("rocket1");
        rocket1.transform.localScale = new Vector3(1, 1, 1);
        rocket1.transform.SetParent(playgroundTransform);
    
        RectTransform rect1 = rocket1.AddComponent<RectTransform>();
        rect1.anchoredPosition = cellUIPositionMatrix[row][col];
        rect1.sizeDelta = new Vector2(cellGap, cellGap);
        rect1.localScale = new Vector2(1,1);
        
        Image image1 = rocket1.AddComponent<Image>();
        image1.sprite = provider.getLeftRocketSprite();
    
        //==================================================================
        GameObject rocket2 = new GameObject("rocket2");
        rocket2.transform.localScale = new Vector3(1, 1, 1);
        rocket2.transform.SetParent(playgroundTransform);
    
        RectTransform rect2 = rocket2.AddComponent<RectTransform>();
        rect2.anchoredPosition = cellUIPositionMatrix[row][col];
        rect2.sizeDelta = new Vector2(cellGap, cellGap);
        rect2.localScale = new Vector2(1,1);
        
        Image image2 = rocket2.AddComponent<Image>();
        image2.sprite = provider.getRightRocketSprite();
    
        while (true)
        {
            rect1.anchoredPosition -=  speed * Time.deltaTime * new Vector2(1, 0);
            rect2.anchoredPosition +=  speed * Time.deltaTime * new Vector2(1, 0);
        
            if(rect1.anchoredPosition.x <= rocket1TargetX)
            {
                Destroy(rocket1);
                Destroy(rocket2);
                flags[flagIndex] = true;
                yield break;
            }
        
            yield return null;
        }
    }
    
    public IEnumerator shootRocketsVertical(int row, int col, float timeStep, bool[] flags, int flagIndex)
    {
        float rocket1TargetY = cellUIPositionMatrix[row][col].y - (row + 1) * cellGap;
        float speed = cellGap / timeStep;
    
        GameObject rocket1 = new GameObject("rocket1");
        rocket1.transform.localScale = new Vector3(1, 1, 1);
        rocket1.transform.SetParent(playgroundTransform);
    
        RectTransform rect1 = rocket1.AddComponent<RectTransform>();
        rect1.anchoredPosition = cellUIPositionMatrix[row][col];
        rect1.sizeDelta = new Vector2(cellGap, cellGap);
        rect1.localScale = new Vector2(1,1);
        
        Image image1 = rocket1.AddComponent<Image>();
        image1.sprite = provider.getDownRocketSprite();
    
        //==================================================================
        GameObject rocket2 = new GameObject("rocket2");
        rocket2.transform.localScale = new Vector3(1, 1, 1);
        rocket2.transform.SetParent(playgroundTransform);
    
        RectTransform rect2 = rocket2.AddComponent<RectTransform>();
        rect2.anchoredPosition = cellUIPositionMatrix[row][col];
        rect2.sizeDelta = new Vector2(cellGap, cellGap);
        rect2.localScale = new Vector2(1,1);
        
        Image image2 = rocket2.AddComponent<Image>();
        image2.sprite = provider.getUpRocketSprite();
    
        while (true)
        {
            rect1.anchoredPosition -=  speed * Time.deltaTime * new Vector2(0, 1);
            rect2.anchoredPosition +=  speed * Time.deltaTime * new Vector2(0, 1);
        
            if(rect1.anchoredPosition.y <= rocket1TargetY)
            {
                Destroy(rocket1);
                Destroy(rocket2);
                flags[flagIndex] = true;
                yield break;
            }
        
            yield return null;
        }
    }
    
    public void setLevelWonUI()
    {
        
    }

    public void setGameOverUI()
    {
        
    }

    public IEnumerator fadeInImage()
    {
        yield break;
    }
}
