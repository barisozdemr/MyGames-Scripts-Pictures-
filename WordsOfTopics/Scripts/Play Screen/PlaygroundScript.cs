using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlaygroundScript : MonoBehaviour
{
    public GameObject logicManager;
    private LogicScript logicScript;

    public GameObject audioManager;
    private AudioScript audioScript;
    
    public Transform playground;

    private Button[,] buttons;
    public Image buttonHighlightImage; // 110x110 px
    public Sprite buttonHighlightSprite;
    
    public Sprite buttonHighlightRedSprite;
    public Sprite buttonHighlightGreenSprite;

    public Transform buttonGreenHighlighterParent;
    public Transform buttonRedHighlighterParent;

    private float highlighterScaler;

    public Sprite buttonSprite;

    private float playgroundLeftSide = -530f;
    private float playgroundRightSide = 530f;
    private float playgroundButtonsXGap;
    
    private float playgroundTop = 850f;
    private float playgroundBottom = -330f;
    private float playgroundButtonsYGap;
    private float playgroundButtonsYCenterCorrection;

    private float minimumGap;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logicScript = logicManager.GetComponent<LogicScript>();
        audioScript = audioManager.GetComponent<AudioScript>();
        
        playgroundButtonsXGap = Mathf.Abs(playgroundLeftSide - playgroundRightSide);
        playgroundButtonsYGap = playgroundTop - playgroundBottom;
        
        playgroundButtonsYCenterCorrection = (playgroundTop+playgroundBottom)/2;
        
        buttonHighlightImage.sprite = buttonHighlightSprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void buildPlaygroundButtons(char[,] matrix)
    {
        int rowCount = matrix.GetLength(0);
        int colCount = matrix.GetLength(1);
        
        playgroundButtonsXGap /= colCount;
        playgroundButtonsYGap /= rowCount;
        
        if (Mathf.Abs(playgroundButtonsXGap) > Mathf.Abs(playgroundButtonsYGap))
        {
            minimumGap = Mathf.Abs(playgroundButtonsYGap);
        }
        else
        {
            minimumGap = Mathf.Abs(playgroundButtonsXGap);
        }
        
        Button[,] buttons = new Button[rowCount, colCount];
        
        for(int i=0 ; i<rowCount ; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                char letter = matrix[i, j];
                if (letter != '\0')
                {
                    int row = i;
                    int col = j;
                    // Button objesi
                    GameObject buttonObj = new GameObject("Button_" + i + '_' + j);
                    buttonObj.transform.SetParent(playground, false);

                    // RectTransform
                    RectTransform rect = buttonObj.AddComponent<RectTransform>();
                    float size = Mathf.Abs(minimumGap) * 0.9f;
                    rect.sizeDelta = new Vector2(size, size);
                    
                    float x;
                    x = minimumGap * (j - ((colCount-1) / 2f));
                    float y;
                    y = minimumGap * -(i - ((rowCount-1) / 2f));
                    y += playgroundButtonsYCenterCorrection;
                    
                    rect.anchoredPosition = new Vector2(x,y);

                    // Image (Button arkaplanı)
                    Image img = buttonObj.AddComponent<Image>();
                    Color c1 = Color.white;
                    c1.a = 0;
                    img.color = c1;
                    img.sprite = buttonSprite;

                    // Button component
                    Button button = buttonObj.AddComponent<Button>();
                    button.onClick.AddListener(() =>
                    {
                        logicScript.setSelectedRow(row);
                        logicScript.setSelectedColumn(col);
                        logicScript.buttonClicked();
                    });
                    
                    ColorBlock colorBlock = button.colors;
                    colorBlock.disabledColor = Color.white;
                    colorBlock.highlightedColor = Color.white;
                    button.colors = colorBlock;

                    // TextMeshPro child
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(buttonObj.transform, false);

                    RectTransform textRect = textObj.AddComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;

                    TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.fontSize = Mathf.Abs(minimumGap)/100f * 65;
                    tmp.fontStyle = FontStyles.Bold;
                    Color c2;
                    UnityEngine.ColorUtility.TryParseHtmlString("#7A0080", out c2);
                    c2.a = 0;
                    tmp.color = c2;

                    if(letter == ';')
                    {
                        tmp.text = "";
                    }
                    else
                    {
                        tmp.text = letter.ToString();
                    }
                    
                    buttons[i,j] = button;
                }
            }
        }
        
        setHighlighterScaler();
        
        this.buttons = buttons;
    }

    public void setButtonText(int row, int column, char letter)
    {
        Button button = buttons[row,column];
        
        TextMeshProUGUI text = button.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        
        text.text = letter.ToString();
    }

    public void setHighlighterScaler()
    {
        highlighterScaler = minimumGap/100;
        
        buttonHighlightImage.transform.localScale = highlighterScaler * Vector3.one;
    }

    public void highlightButton(int row, int column)
    {
        Button button = buttons[row,column];
        
        Vector3 pos = button.transform.position;

        buttonHighlightImage.transform.position = pos;
    }

    public IEnumerator highlightButtonGreen(int row, int column)
    {
        float timer = 0f;
        
        float fadeInTime = 0.5f;
        float stayTime = 1f;
        float fadeOutTime = 1f;
        
        GameObject gameObject = new GameObject(row + "_" + column + "greenHighlight");
        
        gameObject.transform.SetParent(buttonGreenHighlighterParent, false);
        
        Image image = gameObject.AddComponent<Image>();
        
        image.sprite = buttonHighlightGreenSprite;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        image.transform.localScale = highlighterScaler * Vector3.one;
        
        Vector3 pos = buttons[row,column].transform.position;
        
        gameObject.transform.position = pos;

        int stage = 1;
        while (true)
        {
            if (stage == 1) //== fade in
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, timer/fadeInTime);
                
                timer += Time.deltaTime;

                if (timer >= fadeInTime)
                {
                    stage = 2;
                    timer = 0;
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
                }
            }
            else if (stage == 2) //== stay
            {
                timer += Time.deltaTime;

                if (timer >= stayTime)
                {
                    stage = 3;
                    timer = 0;
                }
            }
            else if (stage == 3) //== fade out
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1 - ( timer/fadeOutTime ));
                
                timer += Time.deltaTime;
                
                if(timer >= fadeOutTime)
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
                    Destroy(gameObject);
                    yield break;
                }
            }
            
            yield return null;
        }
    }
    
    public IEnumerator highlightButtonRed(int row, int column)
    {
        float timer = 0f;
        
        float fadeInTime = 0.5f;
        float stayTime = 1f;
        float fadeOutTime = 1f;
        
        GameObject gameObject = new GameObject(row + "_" + column + "redHighlight");
        
        gameObject.transform.SetParent(buttonRedHighlighterParent, false);
        
        Image image = gameObject.AddComponent<Image>();
        
        image.sprite = buttonHighlightRedSprite;
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        image.transform.localScale = highlighterScaler * Vector3.one;
        
        Vector3 pos = buttons[row,column].transform.position;
        
        gameObject.transform.position = pos;

        int stage = 1;
        while (true)
        {
            if (stage == 1) //== fade in
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, timer/fadeInTime);
                
                timer += Time.deltaTime;

                if (timer >= fadeInTime)
                {
                    stage = 2;
                    timer = 0;
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
                }
            }
            else if (stage == 2) //== stay
            {
                timer += Time.deltaTime;

                if (timer >= stayTime)
                {
                    stage = 3;
                    timer = 0;
                }
            }
            else if (stage == 3) //== fade out
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1 - ( timer/fadeOutTime ));
                
                timer += Time.deltaTime;
                
                if(timer >= fadeOutTime)
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
                    Destroy(gameObject);
                    yield break;
                }
            }
            
            yield return null;
        }
    }

    public IEnumerator fadeInPlayground(float buttonFadeInTime)
    {
        float partsTimeDiff = 0.2f;

        int rowCount = buttons.GetLength(0);
        int colCount = buttons.GetLength(1);

        int smallestDimension;
        int longestDimension;
        if (rowCount < colCount)
        {
            smallestDimension = rowCount;
            longestDimension = colCount;
        }
        else
        {
            smallestDimension = colCount;
            longestDimension = rowCount;
        }
        
        int part = rowCount + colCount - 1;
        
        Debug.Log("part: "+part);
        
        int addRetractCount = smallestDimension - 1;

        for (int i = 0; i < part; i++)
        {
            Debug.Log("i: "+i);
            int partLength = smallestDimension;
         
            int diff;
            
            if (i < addRetractCount)
            {
                Debug.Log("A");
                diff = addRetractCount - i;
            }
            else if (i >= longestDimension)
            {
                Debug.Log("B");
                diff = Mathf.Abs(longestDimension - i) + 1;
            }
            else
            {
                Debug.Log("C");
                diff = 0;
            }
            
            Debug.Log(diff);

            partLength -= diff;

            int row = 0;
            int column = i;

            if (i >= colCount)
            {
                column = colCount - 1;
                row += Mathf.Abs(colCount - i) + 1;
            }
            
            Debug.Log("StartCoroutine(fadeInPart("+row+", "+column+", "+partLength+", "+buttonFadeInTime+"));");
            StartCoroutine(fadeInPart(row, column, partLength, buttonFadeInTime));
            
            yield return new WaitForSeconds(partsTimeDiff);
        }
    }

    public IEnumerator fadeInPart(int row, int column, int partLength, float fadeInTime)
    {
        Debug.Log("row: "+row+", column: "+column+", partLength: "+partLength+", fadeInTime: "+fadeInTime);
        float buttonsTimeDiff = 0.1f;
        
        int rowCount = buttons.GetLength(0);
        int colCount = buttons.GetLength(1);
        
        int rowOrg = row;
        int columnOrg = column;
        
        for (int j = 0; j < partLength; j++)
        {
            //Debug.Log("=========================================================\nrowOrg: "+rowOrg+", columnOrg: "+columnOrg+", row: "+row+", column: "+column);
                
            if (buttons[row, column] == null)
            {
                column--;
                row++;
                continue;
            }
                
            Image image = buttons[row, column].GetComponent<Image>();
            TextMeshProUGUI text = buttons[row, column].transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            StartCoroutine(fadeInImage(image, fadeInTime));
            StartCoroutine(fadeInText(text, fadeInTime));

            column--;
            row++;
            
            audioScript.playButtonBuildSoundClip();
            
            yield return new WaitForSeconds(buttonsTimeDiff);
        }
    }

    public IEnumerator fadeInImage(Image image, float fadeInTime)
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
    
    public IEnumerator fadeInText(TextMeshProUGUI text, float fadeInTime)
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

    public void fadeOutPlayground(float fadeOutTime)
    {
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                Image image = button.GetComponent<Image>();
                TextMeshProUGUI text = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                
                StartCoroutine(fadeOutImage(image, fadeOutTime));
                StartCoroutine(fadeOutText(text, fadeOutTime));
            }
        }

        StartCoroutine(fadeOutImage(buttonHighlightImage, fadeOutTime));
    }
    
    public IEnumerator fadeOutImage(Image image, float fadeInTime)
    {
        float timer = 0;
        
        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = image.color;
            c.a = 1 - (timer / fadeInTime);
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
    
    public IEnumerator fadeOutText(TextMeshProUGUI text, float fadeInTime)
    {
        float timer = 0;
        
        while (true)
        {
            timer += Time.deltaTime;
            
            Color c = text.color;
            c.a = 1 - (timer / fadeInTime);
            text.color = c;
            
            if (timer >= fadeInTime)
            {
                c = text.color;
                c.a = 0;
                text.color = c;
                
                yield break;
            }
            
            yield return null;
        }
    }
    
    
    
    
    
    
    
}
