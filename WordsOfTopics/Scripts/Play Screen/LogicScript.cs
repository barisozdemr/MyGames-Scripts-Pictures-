using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class LogicScript : MonoBehaviour
{
    private Random random = new Random();
    
    //===================================================

    private float fadeInDelay = 0.2f;
    
    private float backgroundFadeInTime = 0.5f;
    private float fadeInRestAfterBackgroundDelay = 0.5f;
    
    private float keyboardFadeInTime = 0.5f;
    private float buttonFadeInTime = 0.5f;
    private float restFadeInTime = 0.5f;
    
    private float theTopicFadeInDelay = 2f;
    
    private float fadeInTheTopicTime = 1f;
    
    private float fadeOutTime = 1f;
    
    private float fadeInEndScreenTime = 0.5f;
    private float fadeOutEndScreenTime = 0.5f;
    
    //===================================================
    
    private int selectedRow = -1;
    private int selectedColumn = -1;

    private char letterClicked;

    private int combo = 0;
    
    //===================================================

    public GameObject fadeManager;
    private FadeScript fadeScript;
    
    public GameObject backgroundManager;
    private BackgroundScript backgroundScript;
    
    public GameObject audioManager;
    private AudioScript audioScript;
    
    //===================================================

    private String highlightDirection = ""; // "vertical" or "horizontal"
    
    //===================================================
    
    private float oldTime = 0;
    private float timer = 0;
    public TextMeshProUGUI clockText;

    public TextMeshProUGUI topic;
    public RectTransform topicBackgroundRT;
    public RectTransform topicBackground2RT;

    private float hintTimer = 0;
    private int extraHintCount = 1;
    private int hintUsed = 0;
    public TextMeshProUGUI hintCountText;
    public Image hintButtonImage;
    
    public TextMeshProUGUI mistakesCountText;
    public int mistakesCount = 0;

    public GameObject endScreen;
    public TextMeshProUGUI endScreenTimeText;
    public TextMeshProUGUI endScreenMistakeText;
    public TextMeshProUGUI endScreenHintText;
    
    //===================================================
    
    public GameObject playgroundManager;
    private PlaygroundScript playgroundScript;
    
    public GameObject keyboardManager;
    private KeyboardScript keyboardScript;
    
    //===================================================

    private String wordsPath = "Words/words.txt";
    
    //===================================================
    
    private Dictionary<string, List<string>> wordMap = new Dictionary<string, List<string>>();
    
    private List<string> topics = new List<string>();
    
    private List<String> firstLettersRowsAndColumnsAndDirections = new List<String>(); //ex: "1,2,v", "3,5,h" -> 'v' for vertical, 'h' for horizontal
    private List<String> wordDoneFirstLetters = new List<String>();
    
    private char[,] playgroundMatrix;
    
    private char[,] matrix = new char[21,21];
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playgroundScript = playgroundManager.GetComponent<PlaygroundScript>();
        keyboardScript = keyboardManager.GetComponent<KeyboardScript>();
        backgroundScript = backgroundManager.GetComponent<BackgroundScript>();
        fadeScript = fadeManager.GetComponent<FadeScript>();
        audioScript = audioManager.GetComponent<AudioScript>();
        
        getWordsFromTxt();
        
        buildPlaygroundMatrix();
        
        playgroundScript.buildPlaygroundButtons(playgroundMatrix);
        
        endOfGameControl();
        
        fadeInScreen();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer - oldTime > 1)
        {
            setClock();
            oldTime = timer;
        }
        
        hintTimer += Time.deltaTime;

        if (hintTimer >= 60)
        {
            extraHintCount++;
            hintTimer = 0;
            
            StartCoroutine(flashHintButton());
            updateHintCountText();
        }
    }

    void fadeOutScreen()
    {
        fadeScript.sceneFadeOut(fadeOutTime);
        StartCoroutine(keyboardScript.fadeOutKeyboard(fadeOutTime));
        playgroundScript.fadeOutPlayground(fadeOutTime);
    }

    void fadeInScreen()
    {
        Invoke("fadeInBackground", fadeInDelay);
        Invoke("fadeInBackgroundSoundPlay", fadeInDelay/3);
        
        Invoke("fadeInScreenRest", fadeInRestAfterBackgroundDelay + fadeInDelay);
    }

    void fadeInBackground()
    {
        StartCoroutine(fadeScript.fadeInBackground(backgroundFadeInTime));
    }
    void fadeInBackgroundSoundPlay()
    {
        audioScript.playWhooshSoundClip();
    }

    void fadeInScreenRest()
    {
        StartCoroutine(playgroundScript.fadeInPlayground(buttonFadeInTime));
        StartCoroutine(keyboardScript.fadeInKeyboard(keyboardFadeInTime));
        fadeScript.sceneFadeIn(restFadeInTime, theTopicFadeInDelay, fadeInTheTopicTime);
    }

    void fadeInEndScreen()
    {
        fadeScript.fadeInEndScreen(fadeInEndScreenTime);
    }

    void fadeOutEndScreen()
    {
        fadeScript.fadeOutEndScreen(fadeOutEndScreenTime);
    }

    void setClock()
    {
        int seconds = (int)(timer % 60);
        
        int minutes = (int)(timer / 60);

        int hours = 0;

        if (minutes >= 60)
        {
            hours = minutes / 60;
            minutes = minutes % 60;
        }

        String timeString = "";
        if (hours > 0)
        {
            timeString += hours.ToString();
            timeString += ":";
            if(minutes < 10) timeString += "0";
            timeString += minutes.ToString();
            timeString += ":";
            if(seconds < 10) timeString += "0";
            timeString += seconds.ToString();
        }
        else
        {
            timeString += minutes.ToString();
            timeString += ":";
            if(seconds < 10) timeString += "0";
            timeString += seconds.ToString();
        }
        
        clockText.text = timeString;
    }

    public void hintButtonClicked()
    {
        audioScript.playHintButtonSoundClip();
        
        if (extraHintCount != 0)
        {
            audioScript.playHintSoundClip();

            hintUsed++;
            extraHintCount--;
            
            updateHintCountText();

            int rowCount = playgroundMatrix.GetLength(0);
            int colCount = playgroundMatrix.GetLength(1);
            
            int wordCount = firstLettersRowsAndColumnsAndDirections.Count;
            
            List<int> wordIndexPool = Enumerable.Range(0, wordCount).ToList();

            for (int i = 0; i < wordCount; i++)
            {
                int randomIndex = random.Next(wordIndexPool.Count);

                if (wordDoneFirstLetters.Contains(firstLettersRowsAndColumnsAndDirections[wordIndexPool[randomIndex]])) //== word done
                {
                    wordIndexPool.RemoveAt(randomIndex);
                    continue;
                }
                
                String[] rowColumnDirectionArray = firstLettersRowsAndColumnsAndDirections[wordIndexPool[randomIndex]].Split(',');
                
                int row = Int32.Parse(rowColumnDirectionArray[0]);
                int column = Int32.Parse(rowColumnDirectionArray[1]);
                String direction = rowColumnDirectionArray[2];
                
                List<String> emptyLettersRowsAndColumns = new List<String>();

                if (direction == "v") //========== Get empty letters
                {
                    while (row < rowCount)
                    {
                        if (playgroundMatrix[row, column] == ';')
                        {
                            emptyLettersRowsAndColumns.Add(row+","+column);
                        }
                        else if (playgroundMatrix[row, column] == '\0') break;

                        row++;
                    }
                }
                else if (direction == "h") //========== Get empty letters
                {
                    while (column < colCount)
                    {
                        if (playgroundMatrix[row, column] == ';')
                        {
                            emptyLettersRowsAndColumns.Add(row+","+column);
                        }
                        else if (playgroundMatrix[row, column] == '\0') break;

                        column++;
                    }
                }
                
                int randomHintIndex = random.Next(0, emptyLettersRowsAndColumns.Count);

                row = Int32.Parse(emptyLettersRowsAndColumns[randomHintIndex].Split(",")[0]);
                column = Int32.Parse(emptyLettersRowsAndColumns[randomHintIndex].Split(",")[1]);

                char letter = matrix[row, column];
                
                playgroundMatrix[row, column] = letter;
                
                playgroundScript.setButtonText(row, column, letter);

                StartCoroutine(playgroundScript.highlightButtonGreen(row, column));
                
                endOfGameControl();
                
                return;
            }
        }
    }

    void updateHintCountText()
    {
        hintCountText.text = extraHintCount.ToString();
    }

    void endOfGameControl()
    {
        int rowCount = playgroundMatrix.GetLength(0);
        int colCount = playgroundMatrix.GetLength(1);
        
        bool g = true;
        foreach (String rowColumnDirection in firstLettersRowsAndColumnsAndDirections)
        {
            bool c = true;
            
            String[] rowColumnDirectionArray = rowColumnDirection.Split(",");
            
            int row = Int32.Parse(rowColumnDirectionArray[0]);
            int column = Int32.Parse(rowColumnDirectionArray[1]);
            String direction = rowColumnDirectionArray[2];

            while (playgroundMatrix[row, column] != '\0')
            {
                if (playgroundMatrix[row, column] == ';')
                {
                    c = false;
                    g = false;
                    break;
                }
                
                if (direction == "v") row++;
                else if (direction == "h") column++;
                
                if (row == rowCount || column == colCount)
                {
                    break;
                }
            }

            if (c) wordDone(rowColumnDirection);
        }
        
        if(g) endGame();
    }
    
    void wordDone(String rowColumnDirection) //=== highlight true guess
    {
        if (!wordDoneFirstLetters.Contains(rowColumnDirection))
        {
            wordDoneFirstLetters.Add(rowColumnDirection);

            extraHintCount++;
            
            StartCoroutine(flashHintButton());
            updateHintCountText();
            
            audioScript.playWordDoneSoundClip();
            StartCoroutine(higlightWordDone(rowColumnDirection));
        }
    }

    void endGame()
    {
        audioScript.playWinSoundClip();
        
        Invoke("fadeOutScreen", 4f);
        
        Invoke("loadSceneStartMenu", 9f);
        
        endScreen.SetActive(true);
        setEndScreenTexts();
        
        Invoke("fadeInEndScreen", 2.5f);

        Invoke("fadeOutEndScreen", 8f);
    }

    void setEndScreenTexts()
    {
        String time = "Süre: ";
        String mistake = "Hata: ";
        String hint = "İpucu: ";

        time += clockText.text;
        mistake += mistakesCount;
        hint += hintUsed;
        
        endScreenMistakeText.text = mistake;
        endScreenTimeText.text = time;
        endScreenHintText.text = hint;
    }

    void loadSceneStartMenu()
    {
        SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
    }
    
    public void setSelectedRow(int row)
    {
        selectedRow = row;
    }

    public void setSelectedColumn(int column)
    {
        selectedColumn = column;
    }

    public void buttonClicked()
    {
        audioScript.playPlaygroundButtonSoundClip();
        combo = 0;
        
        highlightDirection = "";
        playgroundScript.highlightButton(selectedRow, selectedColumn);
    }
    
    void highlightButton()
    {
        playgroundScript.highlightButton(selectedRow, selectedColumn);
    }

    public void keyboardClicked(char letter)
    {
        audioScript.playKeyboardButtonSoundClip();
        
        letterClicked = letter;
        
        if (selectedRow == -1 || selectedColumn == -1) return;

        if (matrix[selectedRow, selectedColumn] == letter && playgroundMatrix[selectedRow, selectedColumn] == ';') //== true letter
        {
            audioScript.playTrueLetterSoundClip(combo);
            combo++;
            
            playgroundScript.setButtonText(selectedRow, selectedColumn, letter);
            playgroundMatrix[selectedRow, selectedColumn] = letter;
            StartCoroutine(playgroundScript.highlightButtonGreen(selectedRow, selectedColumn));
            
            if(highlightDirection == "") detectHighlightDirection(selectedRow, selectedColumn);

            int rowCount = playgroundMatrix.GetLength(0);
            int colCount = playgroundMatrix.GetLength(1);

            if (highlightDirection == "vertical")
            {
                Debug.Log("vertical");
                Debug.Log("selectedRow++");
                selectedRow++;
                while(selectedRow < rowCount)
                {
                    Debug.Log("while");
                    Debug.Log("playgroundMatrix[selectedRow, selectedColumn] = "+playgroundMatrix[selectedRow, selectedColumn]);
                    if (playgroundMatrix[selectedRow, selectedColumn] != ';' && playgroundMatrix[selectedRow, selectedColumn] != '\0')
                    {
                        Debug.Log("selectedRow++");
                        selectedRow++;
                    }
                    else if (playgroundMatrix[selectedRow, selectedColumn] == '\0')
                    {
                        Debug.Log("selectedRow--\nbreak");
                        selectedRow--;
                        break;
                    }
                    else
                    {
                        Debug.Log("break");
                        break;
                    }
                }

                if (selectedRow == rowCount)
                {
                    Debug.Log("limit - selectedRow--");
                    selectedRow--;
                }
                
                highlightButton();
            }
            else if (highlightDirection == "horizontal")
            {
                Debug.Log("horizontal");
                Debug.Log("selectedColumn++");
                selectedColumn++;
                while(selectedColumn < colCount)
                {
                    Debug.Log("while");
                    Debug.Log("playgroundMatrix[selectedRow, selectedColumn] = "+playgroundMatrix[selectedRow, selectedColumn]);
                    if (playgroundMatrix[selectedRow, selectedColumn] != ';' && playgroundMatrix[selectedRow, selectedColumn] != '\0')
                    {
                        Debug.Log("selectedColumn++");
                        selectedColumn++;
                    }
                    else if (playgroundMatrix[selectedRow, selectedColumn] == '\0')
                    {
                        Debug.Log("selectedColumn--\nbreak");
                        selectedColumn--;
                        break;
                    }
                    else
                    {
                        Debug.Log("break");
                        break;
                    }
                }

                if (selectedColumn == colCount)
                {
                    Debug.Log("limit - selectedColumn--");
                    selectedColumn--;
                }
                
                highlightButton();
            }
            
            endOfGameControl();
        }
        else if (playgroundMatrix[selectedRow, selectedColumn] == ';') //== false letter
        {
            audioScript.playFalseLetterSoundClip();
            combo = 0;
            
            mistakesCount++;
            mistakesCountText.text = mistakesCount.ToString();
            
            StartCoroutine(playgroundScript.highlightButtonRed(selectedRow, selectedColumn));
        }
    }

    void detectHighlightDirection(int row, int column)
    {
        int rowCount = playgroundMatrix.GetLength(0);
        int colCount = playgroundMatrix.GetLength(1);

        if (row + 1 == rowCount && column + 1 == colCount)
        {
            highlightDirection = "";
            return;
        }
        else if (row + 1 == rowCount)
        {
            if (playgroundMatrix[row, column + 1] != '\0')
            {
                highlightDirection = "horizontal";
                return;
            }
            else
            {
                highlightDirection = "";
                return;
            }
        }
        else if (column + 1 == colCount)
        {
            if (playgroundMatrix[row + 1, column] != '\0')
            {
                highlightDirection = "vertical";
                return;
            }
            else
            {
                highlightDirection = "";
                return;
            }
        }
        else
        {
            if (playgroundMatrix[row, column + 1] != '\0' && playgroundMatrix[row + 1, column] == '\0')
            {
                highlightDirection = "horizontal";
                return;
            }
            else if (playgroundMatrix[row, column + 1] == '\0' && playgroundMatrix[row + 1, column] != '\0')
            {
                highlightDirection = "vertical";
                return;
            }
            else if (playgroundMatrix[row, column + 1] == '\0' && playgroundMatrix[row + 1, column] == '\0')
            {
                highlightDirection = "";
                return;
            }
        }
        
        int verticalEmptyLetters = 0;
        int verticalTrueLetters = 0;
        
        int horizontalEmptyLetters = 0;
        int horizontalTrueLetters = 0;

        int row2 = row + 1;
        while (row2 + 1 < rowCount)
        {
            if (playgroundMatrix[row2, column] == ';')
            {
                verticalEmptyLetters++;
            }
            else if (playgroundMatrix[row2, column] == '\0')
            {
                break;
            }
            else
            {
                verticalTrueLetters++;
            }
            row2++;
        }
        
        int column2 = column + 1;
        while (column2 + 1 < colCount)
        {
            if (playgroundMatrix[row, column2] == ';')
            {
                horizontalEmptyLetters++;
            }
            else if (playgroundMatrix[row, column2] == '\0')
            {
                break;
            }
            else
            {
                horizontalTrueLetters++;
            }
            column2++;
        }

        if (verticalEmptyLetters != 0 && horizontalEmptyLetters == 0)
        {
            highlightDirection = "vertical";
            return;
        }

        if (verticalEmptyLetters == 0 && horizontalEmptyLetters != 0)
        {
            highlightDirection = "horizontal";
            return;
        }

        if (verticalTrueLetters >= horizontalTrueLetters)
        {
            highlightDirection = "vertical";
        }
        else
        {
            highlightDirection = "horizontal";
        }
    }

    void getWordsFromTxt()
    {
        wordMap.Clear();
        topics.Clear();
        
        string resourcePath = wordsPath;
        if (resourcePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            resourcePath = resourcePath.Substring(0, resourcePath.Length - 4);
        
        TextAsset ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null)
        {
            Debug.LogError($"WordLoader: Resources.Load couldn't find -> {resourcePath}");
            return;
        }
        
        String[] lines = ta.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (String line in lines)
        {
            String[] topicAndWords = line.Split(" - "); //example line: "topic - word1, word2, word3"
            
            String topic = topicAndWords[0];
            String[] words = topicAndWords[1].Split(", ");

            List<String> wordsList = new List<String>(words);
            
            wordMap.Add(topic, wordsList);
            topics.Add(topic);
        }

        //==============================================================
        //================================================== Debug =====
        String topicString = "";
        foreach (String topic in topics)
        {
            topicString += topic + ", ";
        }
        Debug.Log(topicString);

        String topicWords = "";
        foreach (String topic in wordMap.Keys)
        {
            topicWords += topic + " -> ";
            foreach (String word in wordMap[topic])
            {
                topicWords += word + ", ";
            }

            topicWords += "\n";
        }
        Debug.Log(topicWords);
        //==============================================================
        //==============================================================
    }
    
    List<String> randomSelectTopicAndWords()
    {
        String topic = topics[random.Next(0, topics.Count)];

        List<String> topicWords = new List<String>(wordMap[topic]);
        
        List<String> selectedWords = new List<String>();
        for (int i = 0; i < 5; i++)
        {
            int randomIndex = random.Next(0, topicWords.Count);
            
            selectedWords.Add(topicWords[randomIndex]);
            
            topicWords.RemoveAt(randomIndex);
        }
        
        backgroundScript.setBackground(topic);
        
        this.topic.text = topic;
        if (topic.Length == 6)
        {
            topicBackgroundRT.anchoredPosition -= new Vector2(20f, 0);
            topicBackground2RT.anchoredPosition += new Vector2(20f, 0);
        }
        else if (topic.Length == 7)
        {
            topicBackgroundRT.anchoredPosition -= new Vector2(40f, 0);
            topicBackground2RT.anchoredPosition += new Vector2(40f, 0);
        }
        else if (topic.Length == 8)
        {
            topicBackgroundRT.anchoredPosition -= new Vector2(60f, 0);
            topicBackground2RT.anchoredPosition += new Vector2(60f, 0);
        }
        else if (topic.Length == 9)
        {
            topicBackgroundRT.anchoredPosition -= new Vector2(80f, 0);
            topicBackground2RT.anchoredPosition += new Vector2(80f, 0);
        }
        else if (topic.Length == 10)
        {
            topicBackgroundRT.anchoredPosition -= new Vector2(100f, 0);
            topicBackground2RT.anchoredPosition += new Vector2(100f, 0);
        }
        
        //==============================================================
        //================================================== Debug =====
        
        Debug.Log("Selected topic -> "+topic);
        String selectedWordsString = "";
        foreach (String word in selectedWords)
        {
            selectedWordsString += word + ", ";
        }
        Debug.Log(selectedWordsString);
        
        //==============================================================
        //==============================================================
        
        return selectedWords;
    }

    void putLongestWordInMatrixAndRemoveIt(List<String> selectedWords)
    {
        int longest = 0;
        int longestIndex = 0;
        for (int i = 0; i < selectedWords.Count; i++)
        {
            if (selectedWords[i].Length > longest)
            {
                longest = selectedWords[i].Length;
                longestIndex = i;
            }
        }
        
        //==============================================================
        //================================================== Debug =====
        
        Debug.Log("Longest ("+longest+") word -> "+selectedWords[longestIndex]);
        
        //==============================================================
        //==============================================================
        
        String longestWord = selectedWords[longestIndex];
        
        int maxGap = (21 - longest);
        int randomGap = random.Next(2, maxGap-2);
        Debug.Log("maxGap= "+maxGap+" randomGap= "+randomGap); //===== DEBUG
        
        if (random.Next(0, 2) == 0) //== horizontal
        {
            int firstRow = 10;
            
            firstLettersRowsAndColumnsAndDirections.Add(firstRow+","+randomGap+",h");
            
            for (int i = randomGap; i < longest + randomGap; i++) //== place
            {
                matrix[firstRow, i] = longestWord[i - randomGap];
            }
        }
        else //========================= vertical
        {
            int firstColumn = 10;
            
            firstLettersRowsAndColumnsAndDirections.Add(randomGap+","+firstColumn+",v");
            
            for (int i = randomGap; i < longest + randomGap; i++) //== place
            {
                matrix[i, firstColumn] = longestWord[i - randomGap];
            }
        }
        
        selectedWords.RemoveAt(longestIndex);
    }

    Dictionary<char, List<String>> lettersMapInMatrix()
    {
        Debug.Log("lettersMapInMatrix()"); //===== DEBUG
        Dictionary<char, List<String>> lettersInMatrix = new Dictionary<char, List<String>>();

        for (int i = 0; i < 21; i++)
        {
            for (int j = 0; j < 21; j++)
            {
                if (matrix[i, j] != '\0')
                {
                    if (!lettersInMatrix.ContainsKey(matrix[i, j]))
                    {
                        lettersInMatrix.Add(matrix[i, j], new List<String> {i+","+j});
                    }
                    else
                    {
                        lettersInMatrix[matrix[i, j]].Add(i+","+j);
                    }
                }
            }
        }
        
        return lettersInMatrix;
    }

    List<int> shuffleFisherYates(List<int> list)
    {
        Debug.Log("shuffleFisherYates()"); //===== DEBUG
        int length = list.Count;
        for (int i = length-1; i > 0; i--) //shuffle "Fisher-Yates"
        {
            int randomInt = random.Next(0, i+1);
            
            int temp = list[i];
            list[i] = list[randomInt];
            list[randomInt] = temp;
        }
        
        return list;
    }

    bool tryPuttingWordToMatrixBasedOnOverlap(List<int> remainingIndexesShuffled, 
                                              List<String> selectedWords, 
                                              Dictionary<char, List<String>> lettersInMatrix)
    {
        Debug.Log("tryPuttingWordToMatrixBasedOnOverlap()"); //===== DEBUG
        int totalIndexCount = remainingIndexesShuffled.Count;
        
        if (totalIndexCount == 0)
        {
            return false;
        }
        
        bool c = false;
        
        for (int k = 0 ; k < totalIndexCount ; k++)
        {
            int index = remainingIndexesShuffled[k];
            String word = selectedWords[index];
            
            List<int> wordIndexPool = Enumerable.Range(0, word.Length).ToList();
            for (int y = 0 ; y < word.Length ; y++) // check every letter of word for overlaps
            {
                int i = wordIndexPool[random.Next(0, wordIndexPool.Count)]; // get random letter
                
                Debug.Log("random letter:"+word[i]); //== DEBUG
                c = false;
                if (lettersInMatrix.ContainsKey(word[i])) // there is overlap/s
                {
                    Debug.Log("overlap! \noverlap List:"); //== DEBUG
                    List<String> overlappingLettersRowsAndColumns = new List<String>(lettersInMatrix[word[i]]);

                    int overlapCount = overlappingLettersRowsAndColumns.Count;
                    
                    //======================================== DEBUG
                    String overlapString = "";
                    foreach (String s in overlappingLettersRowsAndColumns)
                    {
                        overlapString += s + " - ";
                    }
                    Debug.Log(overlapString);
                    //==============================================
                    
                    for (int x = 0; x < overlapCount; x++)
                    {
                        int randomRowAndColumnIndex = random.Next(0, overlappingLettersRowsAndColumns.Count);
                        
                        Debug.Log("letter row-column: "+overlappingLettersRowsAndColumns[randomRowAndColumnIndex]); //== DEBUG
                        
                        String[] rowAndColumn = overlappingLettersRowsAndColumns[randomRowAndColumnIndex].Split(','); // get random overlap

                        overlappingLettersRowsAndColumns.RemoveAt(randomRowAndColumnIndex);
                        
                        int overlapRow = Int32.Parse(rowAndColumn[0]);
                        int overlapColumn = Int32.Parse(rowAndColumn[1]);

                        //==================================================================================================
                        //============================================ No Letters In Diagonal Check
                        bool rowZero = false;
                        bool rowMax = false;
                        bool columnZero = false;
                        bool columnMax = false;
                        
                        if (overlapRow == 0) rowZero = true;
                        if (overlapRow == 20) rowMax = true;
                        if (overlapColumn == 0) columnZero = true;
                        if (overlapColumn == 20) columnMax = true;

                        if (rowZero && columnZero)
                        {
                            if (matrix[overlapRow + 1, overlapColumn + 1] != '\0') continue;
                        }
                        else if (rowMax && columnMax)
                        {
                            if (matrix[overlapRow - 1, overlapColumn - 1] != '\0') continue;
                        }
                        else if (rowZero)
                        {
                            if(matrix[overlapRow + 1, overlapColumn - 1] != '\0' || matrix[overlapRow + 1, overlapColumn + 1] != '\0') continue;
                        }
                        else if (rowMax)
                        {
                            if(matrix[overlapRow - 1, overlapColumn - 1] != '\0' || matrix[overlapRow - 1, overlapColumn + 1] != '\0') continue;
                        }
                        else if (columnZero)
                        {
                            if(matrix[overlapRow - 1, overlapColumn + 1] != '\0' || matrix[overlapRow + 1, overlapColumn + 1] != '\0') continue;
                        }
                        else if (columnMax)
                        {
                            if(matrix[overlapRow - 1, overlapColumn - 1] != '\0' || matrix[overlapRow + 1, overlapColumn - 1] != '\0') continue;
                        }
                        else
                        {
                            if(matrix[overlapRow + 1, overlapColumn + 1] != '\0' || matrix[overlapRow - 1, overlapColumn - 1] != '\0' ||
                               matrix[overlapRow + 1, overlapColumn - 1] != '\0' || matrix[overlapRow - 1, overlapColumn + 1] != '\0') continue;
                        }
                        //==================================================================================================
                        
                        //==================================================================================================
                        //============================================ Placement Direction Detection
                        String direction = null; // "vertical" or "horizontal"

                        if (!rowZero && !rowMax)
                        {
                            if (matrix[overlapRow - 1, overlapColumn] == '\0' && matrix[overlapRow + 1, overlapColumn] == '\0')
                            {
                                direction = "vertical";
                            }
                        }
                        else if (rowZero)
                        {
                            if (matrix[overlapRow + 1, overlapColumn] == '\0')
                            {
                                direction = "vertical";
                            }
                        }
                        else if (rowMax)
                        {
                            if (matrix[overlapRow - 1, overlapColumn] == '\0')
                            {
                                direction = "vertical";
                            }
                        }
                        //=========================================================
                        if (direction == null)
                        {
                            if (!columnZero && !columnMax)
                            {
                                if(matrix[overlapRow, overlapColumn - 1] == '\0' && matrix[overlapRow, overlapColumn + 1] == '\0')
                                {
                                    direction = "horizontal";
                                }
                            }
                            else if (columnZero)
                            {
                                if (matrix[overlapRow, overlapColumn + 1] == '\0')
                                {
                                    direction = "horizontal";
                                }
                            }
                            else if (columnMax)
                            {
                                if (matrix[overlapRow, overlapColumn - 1] == '\0')
                                {
                                    direction = "horizontal";
                                }
                            }
                        }
                        
                        if(direction == null) continue;
                        //==================================================================================================
                        
                        //==================================================================================================
                        //================================== No Wrong and Double Overlaps or Collisions In The Way Detection
                        if (direction == "vertical") //=============================================== Vertical
                        {
                            if (overlapRow - i < 0 || overlapRow + (word.Length - 1 - i) > 20) // out of bounds
                            {
                                continue;
                            }

                            if (overlapRow - i - 1 >= 0)
                            {
                                if (matrix[overlapRow - i - 1, overlapColumn] != '\0') // head control
                                {
                                    continue;
                                }
                            }

                            if (overlapRow + (word.Length - i) <= 20)
                            {
                                if (matrix[overlapRow + (word.Length - i), overlapColumn] != '\0') // tail control
                                {
                                    continue;
                                }
                            }

                            bool g = false;
                            bool doubleOverlap = false;
                            for (int j = 0; j < word.Length; j++) // letters on the way control
                            {
                                if (matrix[overlapRow + (j - i), overlapColumn] != '\0')
                                {
                                    if (matrix[overlapRow + (j - i), overlapColumn] != word[j]) // wrong letter
                                    {
                                        g = true;
                                        break;
                                    }
                                    else
                                    {
                                        if (doubleOverlap) // double overlap control
                                        {
                                            g = true;
                                            break;
                                        }
                                        doubleOverlap = true;
                                        continue;
                                    }
                                }

                                //====================================== body surroundings control
                                if (!columnZero && !columnMax)
                                {
                                    if (matrix[overlapRow + (j - i), overlapColumn + 1] != '\0' || matrix[overlapRow + (j - i), overlapColumn - 1] != '\0')
                                    {
                                        g = true;
                                        break;
                                    }
                                }
                                else if (columnZero)
                                {
                                    if (matrix[overlapRow + (j - i), overlapColumn + 1] != '\0')
                                    {
                                        g = true;
                                        break;
                                    }
                                }
                                else if (columnMax)
                                {
                                    if (matrix[overlapRow + (j - i), overlapColumn - 1] != '\0')
                                    {
                                        g = true;
                                        break;
                                    }
                                }

                                doubleOverlap = false;
                            }
                            
                            if(g) continue;
                        }
                        //============================================================================ Horizontal
                        else if (direction == "horizontal")
                        {
                            if (overlapColumn - i < 0 || overlapColumn + (word.Length - 1 - i) > 20) // out of bounds
                            {
                                continue;
                            }

                            if (overlapColumn - i - 1 >= 0)
                            {
                                if (matrix[overlapRow, overlapColumn - i - 1] != '\0') // head control
                                {
                                    continue;
                                }
                            }

                            if (overlapColumn + (word.Length - i) <= 20)
                            {
                                if (matrix[overlapRow, overlapColumn + (word.Length - i)] != '\0') // tail control
                                {
                                    continue;
                                }
                            }
                            
                            bool g = false;
                            bool doubleOverlap = false;
                            for (int j = 0; j < word.Length; j++) // letters on the way control
                            {
                                if (matrix[overlapRow, overlapColumn + (j - i)] != '\0')
                                {
                                    if (matrix[overlapRow, overlapColumn + (j - i)] != word[j]) // wrong letter
                                    {
                                        g = true;
                                        break;
                                    }
                                    else
                                    {
                                        if (doubleOverlap) // double overlap control
                                        {
                                            g = true;
                                            break;
                                        }
                                        doubleOverlap = true;
                                        continue;
                                    }
                                }
                                
                                //====================================== body surroundings control
                                if (!rowZero && !rowMax)
                                {
                                    if (matrix[overlapRow + 1, overlapColumn + (j - i)] != '\0' || matrix[overlapRow - 1, overlapColumn + (j - i)] != '\0')
                                    {
                                        g = true;
                                        break;
                                    }
                                }
                                else if (rowZero)
                                {
                                    if (matrix[overlapRow + 1, overlapColumn + (j - i)] != '\0')
                                    {
                                        g = true;
                                        break;
                                    }
                                }
                                else if (rowMax)
                                {
                                    if (matrix[overlapRow - 1, overlapColumn + (j - i)] != '\0')
                                    {
                                        g = true;
                                        break;
                                    }
                                }

                                doubleOverlap = false;
                            }
                            
                            if(g) continue;
                        }
                        //==================================================================================================
                        
                        //==================================================================================================
                        //============================================ Controls Done, Put The Word to Matrix.
                        if (direction == "vertical")
                        {
                            firstLettersRowsAndColumnsAndDirections.Add((overlapRow - i)+","+overlapColumn+",v");
                                
                            for (int j = 0; j < word.Length; j++) //== place
                            {
                                matrix[overlapRow + (j - i), overlapColumn] = word[j];
                                if (j - i != 0)
                                {
                                    if (!lettersInMatrix.ContainsKey(word[j]))
                                    {
                                        lettersInMatrix.Add(word[j], new List<String> {(overlapRow + (j - i))+","+overlapColumn});
                                    }
                                    else
                                    {
                                        lettersInMatrix[word[j]].Add((overlapRow + (j - i))+","+overlapColumn);
                                    }
                                }
                            }
                        }
                        else if (direction == "horizontal")
                        {
                            firstLettersRowsAndColumnsAndDirections.Add(overlapRow+","+(overlapColumn - i)+",h");
                            
                            for (int j = 0; j < word.Length; j++) //== place
                            {
                                matrix[overlapRow, overlapColumn + (j - i)] = word[j];
                                if (j - i != 0)
                                {
                                    if (!lettersInMatrix.ContainsKey(word[j]))
                                    {
                                        lettersInMatrix.Add(word[j], new List<String> {overlapRow+","+(overlapColumn + (j - i))});
                                    }
                                    else
                                    {
                                        lettersInMatrix[word[j]].Add(overlapRow+","+(overlapColumn + (j - i)));
                                    }
                                }
                            }
                        }
                        
                        remainingIndexesShuffled.RemoveAt(k);
                        c = true;
                        break;
                    }
                }

                wordIndexPool.Remove(i);

                if (c) break;
            }

            if (c) break;
        }

        debugPostMatrix(matrix); //===== DEBUG
        if (c) return true;
        return false;
    }

    void putRandomWordToMatrix(List<int> remainingIndexesShuffled, 
                               List<String> selectedWords, 
                               Dictionary<char, List<String>> lettersInMatrix)
    {
        Debug.Log("putRandomWordToMatrix()"); //===== DEBUG
        int totalIndexCount = remainingIndexesShuffled.Count;
        
        if (totalIndexCount == 0)
        {
            return;
        }

        int index = remainingIndexesShuffled[0];
        String word = selectedWords[index];
        
        Debug.Log("index: "+index+", word: "+word); //== DEBUG
        
        int wordLength = word.Length;
        
        //===================================================================================================================
        //========================================================================================== Get smallest used matrix
        int left = -1;
        int right = -1;

        int top = -1;
        int bottom = -1;
        
        for (int i = 0; i < 21; i++) // check left to right -> find top and bottom border of matrix
        {
            for (int j = 0; j < 21; j++)
            {
                if (top == -1)
                {
                    if (matrix[i, j] != '\0')
                    {
                        top = i;
                    }
                }

                if (bottom == -1)
                {
                    if (matrix[20 - i,j] != '\0')
                    {
                        bottom = 20 - i;
                    }
                }
            }

            if (top != -1 && bottom != -1) break;
        }
        
        for (int i = 0; i < 21; i++) // check top to bottom -> find left and right border of matrix
        {
            for (int j = 0; j < 21; j++)
            {
                if (left == -1)
                {
                    if (matrix[j, i] != '\0')
                    {
                        left = i;
                    }
                }

                if (right == -1)
                {
                    if (matrix[j, 20 - i] != '\0')
                    {
                        right = 20 - i;
                    }
                }
            }

            if (left != -1 && right != -1) break;
        }
        
        Debug.Log("left: "+left+", right: "+right+"top: "+top+", bottom: "+bottom); //== DEBUG
        //===================================================================================================================
        
        //===================================================================================================================
        //=================================================================================== Look for a free place in matrix
        
        for (int i = top; i <= bottom; i++) //======================= LEFT TO RIGHT
        {
            int freeSpaceLength = 0;
            
            for (int j = left; j <= right; j++)
            {
                if(freeSpaceLength < 0) freeSpaceLength = 0;
                
                if (matrix[i, j] == '\0')
                {
                    freeSpaceLength++;
                }
                else{
                    freeSpaceLength = 0;
                }

                if (freeSpaceLength == wordLength)
                {
                    //==================================================== Head and Tail Control - Diagonals Included
                    if (j - wordLength < 0)
                    {
                        if (i == 0)
                        {
                            if (matrix[i, j + 1] != '\0' || matrix[i + 1, j + 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else if (i == 20)
                        {
                            if (matrix[i, j + 1] != '\0' || matrix[i - 1, j + 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else
                        {
                            if (matrix[i - 1, j + 1] != '\0' || matrix[i, j + 1] != '\0' || matrix[i + 1, j + 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        
                    }
                    else if(j + 1 > 20)
                    {
                        if (i == 0)
                        {
                            if (matrix[i, j - wordLength] != '\0' || matrix[i + 1, j - wordLength] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else if (i == 20)
                        {
                            if (matrix[i, j - wordLength] != '\0' || matrix[i - 1, j - wordLength] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else
                        {
                            if (matrix[i - 1, j - wordLength] != '\0' || matrix[i, j - wordLength] != '\0' || matrix[i + 1, j - wordLength] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            if (matrix[i, j + 1] != '\0'          || matrix[i + 1, j + 1] != '\0' ||
                                matrix[i, j - wordLength] != '\0' || matrix[i + 1, j - wordLength] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else if (i == 20)
                        {
                            if (matrix[i - 1, j + 1] != '\0'          || matrix[i, j + 1] != '\0' ||
                                matrix[i - 1, j - wordLength] != '\0' || matrix[i, j - wordLength] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else
                        {
                            if (matrix[i - 1, j + 1] != '\0'          || matrix[i, j + 1] != '\0'          || matrix[i + 1, j + 1] != '\0' ||
                                matrix[i - 1, j - wordLength] != '\0' || matrix[i, j - wordLength] != '\0' || matrix[i + 1, j - wordLength] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                    }

                    //========================================================= Body Surroundings Control
                    for (int k = j; k > j - wordLength; k--)
                    {
                        if (i == 0)
                        {
                            if (matrix[i + 1, k] != '\0')
                            {
                                freeSpaceLength--;
                                break;
                            }
                        }
                        else if (i == 20)
                        {
                            if (matrix[i - 1, k] != '\0')
                            {
                                freeSpaceLength--;
                                break;
                            }
                        }
                        else
                        {
                            if (matrix[i + 1, k] != '\0' || matrix[i - 1, k] != '\0')
                            {
                                freeSpaceLength--;
                                break;
                            }
                        }
                    }
                }
                //======================================================================================================

                //======================================================================================================
                //============================================= Controls done, Place the word if there is suitable place
                if (freeSpaceLength == wordLength)
                {
                    Debug.Log("A"); //== DEBUG
                    firstLettersRowsAndColumnsAndDirections.Add(i+","+(j-wordLength+1)+",h"); // store the first letter
                    Debug.Log("firsLetter add-> ("+i+","+(j-wordLength+1)+","+"h)");
                    Debug.Log("i: "+i+", j: "+j+", wordLength: "+wordLength);
                    
                    for (int k = j; k > j - wordLength; k--) // == place
                    {
                        int wordIndex = wordLength - 1 - (j - k);
                        Debug.Log("wordIndex: "+wordIndex+", k: "+k); //== DEBUG
                        
                        matrix[i, k] = word[wordLength-1-(j-k)];
                        
                        if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                        {
                            lettersInMatrix.Add(word[wordIndex], new List<String> {i+","+k});
                        }
                        else
                        {
                            lettersInMatrix[word[wordIndex]].Add(i+","+k);
                        }
                    }

                    remainingIndexesShuffled.RemoveAt(0);

                    debugPostMatrix(matrix); //===== DEBUG
                    return;
                }
                //======================================================================================================
            }
        }
        
        for (int i = left; i <= right; i++) //======================= TOP TO BOTTOM
        {
            int freeSpaceLength = 0;
            
            for (int j = top; j <= bottom; j++)
            {
                if(freeSpaceLength < 0) freeSpaceLength = 0;
                
                if (matrix[j, i] == '\0')
                {
                    freeSpaceLength++;
                }
                else{
                    freeSpaceLength = 0;
                }

                if (freeSpaceLength == wordLength)
                {
                    //==================================================== Head and Tail Control - Diagonals Included
                    if (j - wordLength < 0)
                    {
                        if (i == 0)
                        {
                            if (matrix[j + 1, i] != '\0' || matrix[j + 1, i + 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else if (i == 20)
                        {
                            if (matrix[j + 1, i] != '\0' || matrix[j + 1, i - 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else
                        {
                            if (matrix[j + 1, i - 1] != '\0' || matrix[j + 1, i] != '\0' || matrix[j + 1, i + 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        
                    }
                    else if(j + 1 > 20)
                    {
                        if (i == 0)
                        {
                            if (matrix[j - wordLength, i] != '\0' || matrix[j - wordLength, i + 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else if (i == 20)
                        {
                            if (matrix[j - wordLength, i] != '\0' || matrix[j - wordLength, i - 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else
                        {
                            if (matrix[j - wordLength, i - 1] != '\0' || matrix[j - wordLength, i] != '\0' || matrix[j - wordLength, i + 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            if (matrix[j + 1, i] != '\0'          || matrix[j + 1, i + 1] != '\0' ||
                                matrix[j - wordLength, i] != '\0' || matrix[j - wordLength, i + 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else if (i == 20)
                        {
                            if (matrix[j + 1, i - 1] != '\0'          || matrix[j + 1, i] != '\0' ||
                                matrix[j - wordLength, i - 1] != '\0' || matrix[j - wordLength, i] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                        else
                        {
                            if (matrix[j + 1, i - 1] != '\0'          || matrix[j + 1, i] != '\0'          || matrix[j + 1, i + 1] != '\0' ||
                                matrix[j - wordLength, i - 1] != '\0' || matrix[j - wordLength, i] != '\0' || matrix[j - wordLength, i + 1] != '\0')
                            {
                                freeSpaceLength--;
                                continue;
                            }
                        }
                    }

                    //========================================================= Body Surroundings Control
                    for (int k = j; k > j - wordLength; k--)
                    {
                        if (i == 0)
                        {
                            if (matrix[k, i + 1] != '\0')
                            {
                                freeSpaceLength--;
                                break;
                            }
                        }
                        else if (i == 20)
                        {
                            if (matrix[k, i - 1] != '\0')
                            {
                                freeSpaceLength--;
                                break;
                            }
                        }
                        else
                        {
                            if (matrix[k, i + 1] != '\0' || matrix[k, i - 1] != '\0')
                            {
                                freeSpaceLength--;
                                break;
                            }
                        }
                    }
                }
                //======================================================================================================

                //======================================================================================================
                //============================================= Controls done, Place the word if there is suitable place
                if (freeSpaceLength == wordLength)
                {
                    Debug.Log("B"); //== DEBUG
                    firstLettersRowsAndColumnsAndDirections.Add((j-wordLength+1)+","+i+",v"); // store the first letter
                    Debug.Log("firsLetter add-> ("+(j-wordLength+1)+","+i+","+"h)");
                    Debug.Log("i: "+i+", j: "+j+", wordLength: "+wordLength);
                    
                    for (int k = j; k > j - wordLength; k--) // == place
                    {
                        int wordIndex = wordLength - 1 - (j - k);
                        Debug.Log("wordIndex: "+wordIndex+", k: "+k); //== DEBUG
                        
                        matrix[k, i] = word[wordIndex];
                        
                        if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                        {
                            lettersInMatrix.Add(word[wordIndex], new List<String> {k+","+i});
                        }
                        else
                        {
                            lettersInMatrix[word[wordIndex]].Add(k+","+i);
                        }
                    }

                    remainingIndexesShuffled.RemoveAt(0);

                    debugPostMatrix(matrix); //===== DEBUG
                    return;
                }
                //======================================================================================================
            }
        }
        //===================================================================================================================

        //===================================================================================================================
        //===================================================================== Not Enough Space, Put outside smallest matrix
        if (left != 0) //===== left of left side
        {
            int freeSpaceLength = 0;
            for (int i = top; i < bottom+1; i++)
            {
                if (matrix[i, left] == '\0') // side check
                {
                    freeSpaceLength++;
                }
                else
                {
                    freeSpaceLength = 0;
                }

                if (freeSpaceLength == wordLength)
                {
                    if (matrix[i + 1, left] == '\0' && matrix[i - wordLength, left] == '\0') // diagonals check
                    {
                        Debug.Log("1-C"); //== DEBUG
                        firstLettersRowsAndColumnsAndDirections.Add((i-wordLength+1)+","+(left-1)+",v"); // store the first letter
                        
                        for (int k = i; k > i - wordLength; k--) // == place
                        {
                            Debug.Log("wordIndex: "+ (wordLength-1-(i-k))+", k: "+k); //== DEBUG
                            int wordIndex = wordLength - 1 - (i - k);
                            
                            matrix[k, left - 1] = word[wordIndex];
                            
                            if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                            {
                                lettersInMatrix.Add(word[wordIndex], new List<String> {k+","+(left - 1)});
                            }
                            else
                            {
                                lettersInMatrix[word[wordIndex]].Add(k+","+(left - 1));
                            }
                        }

                        remainingIndexesShuffled.RemoveAt(0);

                        debugPostMatrix(matrix); //===== DEBUG
                        return;
                    }
                    else
                    {
                        freeSpaceLength = 0;
                    }
                }
            }
        }
        
        if (right != 20) //===== right of right side
        {
            int freeSpaceLength = 0;
            for (int i = top; i < bottom+1; i++)
            {
                if (matrix[i, right] == '\0') // side check
                {
                    freeSpaceLength++;
                }
                else
                {
                    freeSpaceLength = 0;
                }

                if (freeSpaceLength == wordLength)
                {
                    if (matrix[i + 1, right] == '\0' && matrix[i - wordLength, right] == '\0') // diagonals check
                    {
                        Debug.Log("1-D"); //== DEBUG
                        firstLettersRowsAndColumnsAndDirections.Add((i-wordLength+1)+","+(right+1)+",v"); // store the first letter
                        
                        for (int k = i; k > i - wordLength; k--) // == place
                        {
                            Debug.Log("wordIndex: "+ (wordLength-1-(i-k))+", k: "+k); //== DEBUG
                            int wordIndex = wordLength - 1 - (i - k);
                            
                            matrix[k, right + 1] = word[wordIndex];
                            
                            if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                            {
                                lettersInMatrix.Add(word[wordIndex], new List<String> {k+","+(right + 1)});
                            }
                            else
                            {
                                lettersInMatrix[word[wordIndex]].Add(k+","+(right + 1));
                            }
                        }

                        remainingIndexesShuffled.RemoveAt(0);

                        debugPostMatrix(matrix); //===== DEBUG
                        return;
                    }
                    else
                    {
                        freeSpaceLength = 0;
                    }
                }
            }
        }
        
        if (top != 0) //===== top of top
        {
            int freeSpaceLength = 0;
            for (int i = left; i < right+1; i++)
            {
                if (matrix[top, i] == '\0') // side check
                {
                    freeSpaceLength++;
                }
                else
                {
                    freeSpaceLength = 0;
                }

                if (freeSpaceLength == wordLength)
                {
                    if (matrix[top, i + 1] == '\0' && matrix[top, i - wordLength] == '\0') // diagonals check
                    {
                        Debug.Log("1-E"); //== DEBUG
                        firstLettersRowsAndColumnsAndDirections.Add((top-1)+","+(i-wordLength+1)+",h"); // store the first letter
                        
                        for (int k = i; k > i - wordLength; k--) // == place
                        {
                            Debug.Log("wordIndex: "+ (wordLength-1-(i-k))+", k: "+k); //== DEBUG
                            int wordIndex = wordLength - 1 - (i - k);
                            
                            matrix[top - 1, k] = word[wordIndex];
                            
                            if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                            {
                                lettersInMatrix.Add(word[wordIndex], new List<String> {(top - 1)+","+k});
                            }
                            else
                            {
                                lettersInMatrix[word[wordIndex]].Add((top - 1)+","+k);
                            }
                        }

                        remainingIndexesShuffled.RemoveAt(0);

                        debugPostMatrix(matrix); //===== DEBUG
                        return;
                    }
                    else
                    {
                        freeSpaceLength = 0;
                    }
                }
            }
        }
        
        if (bottom != 20) //===== bottom of bottom
        {
            int freeSpaceLength = 0;
            for (int i = left; i < right+1; i++)
            {
                if (matrix[bottom, i] == '\0') // side check
                {
                    freeSpaceLength++;
                }
                else
                {
                    freeSpaceLength = 0;
                }

                if (freeSpaceLength == wordLength)
                {
                    if (matrix[bottom, i + 1] == '\0' && matrix[bottom, i - wordLength] == '\0') // diagonals check
                    {
                        Debug.Log("1-F"); //== DEBUG
                        firstLettersRowsAndColumnsAndDirections.Add((i-wordLength+1)+","+(bottom+1)+",h"); // store the first letter
                        
                        for (int k = i; k > i - wordLength; k--) // == place
                        {
                            Debug.Log("wordIndex: "+ (wordLength-1-(i-k))+", k: "+k); //== DEBUG
                            int wordIndex = wordLength - 1 - (i - k);
                            
                            matrix[bottom + 1, k] = word[wordIndex];
                            
                            if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                            {
                                lettersInMatrix.Add(word[wordIndex], new List<String> {(bottom + 1)+","+k});
                            }
                            else
                            {
                                lettersInMatrix[word[wordIndex]].Add((bottom + 1)+","+k);
                            }
                        }

                        remainingIndexesShuffled.RemoveAt(0);

                        debugPostMatrix(matrix); //===== DEBUG
                        return;
                    }
                    else
                    {
                        freeSpaceLength = 0;
                    }
                }
            }
        }

        //======================================= Edges are full with letters, put 2 blocks further than smallest matrix
        if (bottom - top + 1 >= wordLength)
        {
            if (left - 2 >= 0)
            {
                Debug.Log("2-G"); //== DEBUG
                firstLettersRowsAndColumnsAndDirections.Add(top+","+(left-2)+",v"); // store the first letter
                
                for (int i = top; i < top + wordLength; i++) // == place
                {
                    int wordIndex = i - top;
                    
                    matrix[i, left - 2] = word[wordIndex];
                    
                    if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                    {
                        lettersInMatrix.Add(word[wordIndex], new List<String> {i+","+(left - 2)});
                    }
                    else
                    {
                        lettersInMatrix[word[wordIndex]].Add(i+","+(left - 2));
                    }
                }
                
                remainingIndexesShuffled.RemoveAt(0);
                
                debugPostMatrix(matrix); //===== DEBUG
                return;
            }
            
            if (right + 2 <= 20)
            {
                Debug.Log("2-H"); //== DEBUG
                firstLettersRowsAndColumnsAndDirections.Add(top+","+(right+2)+",v"); // store the first letter
                
                for (int i = top; i < top + wordLength; i++) // == place
                {
                    int wordIndex = i - top;
                    
                    matrix[i, right + 2] = word[wordIndex];
                    
                    if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                    {
                        lettersInMatrix.Add(word[wordIndex], new List<String> {i+","+(right + 2)});
                    }
                    else
                    {
                        lettersInMatrix[word[wordIndex]].Add(i+","+(right + 2));
                    }
                }
                
                remainingIndexesShuffled.RemoveAt(0);
                
                debugPostMatrix(matrix); //===== DEBUG
                return;
            }
        }

        if (top - 2 >= 0)
        {
            int leftSide = left;

            if (left + wordLength > 20)
            {
                leftSide -= (left + wordLength) - 20;
            }
            
            Debug.Log("2-I"); //== DEBUG
            firstLettersRowsAndColumnsAndDirections.Add(top-2+","+leftSide+",h"); // store the first letter
            
            for (int i = leftSide; i < leftSide + wordLength; i++) // == place
            {
                int wordIndex = i - leftSide;
                
                matrix[top - 2, i] = word[wordIndex];
                
                if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                {
                    lettersInMatrix.Add(word[wordIndex], new List<String> {(top - 2)+","+i});
                }
                else
                {
                    lettersInMatrix[word[wordIndex]].Add((top - 2)+","+i);
                }
            }
                
            remainingIndexesShuffled.RemoveAt(0);
            
            debugPostMatrix(matrix); //===== DEBUG
            return;
        }
        else
        {
            int leftSide = left;

            if (left + wordLength > 20)
            {
                leftSide -= (left + wordLength) - 20;
            }
            
            Debug.Log("2-J"); //== DEBUG
            firstLettersRowsAndColumnsAndDirections.Add(bottom+2+","+leftSide+",h"); // store the first letter
            
            for (int i = leftSide; i < leftSide + wordLength; i++) // == place
            {
                int wordIndex = i - leftSide;
                
                matrix[bottom + 2, i] = word[wordIndex];
                
                if (!lettersInMatrix.ContainsKey(word[wordIndex]))
                {
                    lettersInMatrix.Add(word[wordIndex], new List<String> {(bottom + 2)+","+i});
                }
                else
                {
                    lettersInMatrix[word[wordIndex]].Add((bottom + 2)+","+i);
                }
            }
            
            remainingIndexesShuffled.RemoveAt(0);
            
            debugPostMatrix(matrix); //===== DEBUG
            return;
        }
    }

    void convertMatrixToPlaygroundMatrix()
    {
        Debug.Log("convertMatrixToPlaygroundMatrix()"); //===== DEBUG
        Dictionary<char, List<String>> hintLetters = new Dictionary<char, List<String>>();
        
        List<String> firstLettersPool = new List<String>(firstLettersRowsAndColumnsAndDirections);
        
        //==============================================================================================================
        //======================================================================================================== HINTS
        for (int i = 0; i < 3; i++) //================================================== Give 3 first letter hints
        {
            int randomIndex = random.Next(0, firstLettersPool.Count);
            
            String[] rowAndColumn = firstLettersPool[randomIndex].Split(',');
            int row = Int32.Parse(rowAndColumn[0]);
            int column = Int32.Parse(rowAndColumn[1]);

            char letter = matrix[row, column];

            if (!hintLetters.ContainsKey(letter))
            {
                hintLetters.Add(letter, new List<String>{row+","+column});
            }
            else
            {
                hintLetters[letter].Add(row+","+column);
            }
            
            firstLettersPool.RemoveAt(randomIndex);
        }

        foreach (String rowAndColumnAndDirection in firstLettersRowsAndColumnsAndDirections) //=========== Give hint for each word
        {
            int row = Int32.Parse(rowAndColumnAndDirection.Split(",")[0]);
            int column = Int32.Parse(rowAndColumnAndDirection.Split(",")[1]);
            String direction = rowAndColumnAndDirection.Split(",")[2];
            
            Debug.Log("row: "+row+", column: "+column+", direction: "+direction);
            
            

            if (direction == "v") //== vertical
            {
                int length = 1;

                while (row + length < 21)
                {
                    if (matrix[row + length, column] == '\0') break;
                    
                    length++;
                }
                
                int hintCount = 1;
                if (length >= 7) hintCount = 2;
                if (length >= 11) hintCount = 3;
                
                while (hintCount > 0)
                {
                    int randomIndex = random.Next(2, length);
                
                    if (!hintLetters.ContainsKey(matrix[row + randomIndex, column])) //=== give hint
                    {
                        hintLetters.Add(matrix[row + randomIndex, column], new List<String>{(row + randomIndex) + "," + column});
                    }
                    else
                    {
                        if (!hintLetters[matrix[row + randomIndex, column]].Contains((row + randomIndex) + "," + column))
                        {
                            hintLetters[matrix[row + randomIndex, column]].Add((row + randomIndex) + "," + column);
                        }
                    }
                    
                    hintCount--;
                }
            }
            else if (direction == "h") //== horizontal
            {
                int length = 1;

                while (column + length < 21)
                {
                    if (matrix[row, column + length] == '\0') break;
                    
                    length++;
                }
                
                int hintCount = 1;
                if (length >= 7) hintCount = 2;
                if (length >= 11) hintCount = 3;

                while (hintCount > 0)
                {
                    int randomIndex = random.Next(2, length);
                
                    if (!hintLetters.ContainsKey(matrix[row, column + randomIndex]))
                    {
                        hintLetters.Add(matrix[row, column + randomIndex], new List<String>{row + "," + (column + randomIndex)});
                    }
                    else
                    {
                        if (!hintLetters[matrix[row, column + randomIndex]].Contains(row + "," + (column + randomIndex)))
                        {
                            hintLetters[matrix[row, column + randomIndex]].Add(row + "," + (column + randomIndex));
                        }
                    }
                    
                    hintCount--;
                }
            }
        }
        
        //==============================================================================================================
        //==============================================================================================================
        
        playgroundMatrix = (char[,])matrix.Clone();

        
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {
                if (playgroundMatrix[i, j] != '\0')
                {
                    playgroundMatrix[i, j] = ';'; //== ';' for -> hidden letters ,  '\0' for -> empty cells
                }
            }
        }

        foreach (char letter in hintLetters.Keys)
        {
            foreach (String rowAndColumn in hintLetters[letter])
            {
                int row = Int32.Parse(rowAndColumn.Split(",")[0]);
                int column = Int32.Parse(rowAndColumn.Split(",")[1]);
                
                playgroundMatrix[row, column] = letter; //== reveal hint letters
            }
        }
        
        debugPostMatrix(playgroundMatrix); //===== DEBUG
        
        //===================================================================================================================
        //========================================================================================== Get smallest used matrix
        
        int left = -1;
        int right = -1;

        int top = -1;
        int bottom = -1;
        
        for (int i = 0; i < 21; i++) // check left to right -> find top and bottom border of matrix
        {
            for (int j = 0; j < 21; j++)
            {
                if (top == -1)
                {
                    if (matrix[i, j] != '\0')
                    {
                        top = i;
                    }
                }

                if (bottom == -1)
                {
                    if (matrix[20 - i,j] != '\0')
                    {
                        bottom = 20 - i;
                    }
                }
            }

            if (top != -1 && bottom != -1) break;
        }
        
        for (int i = 0; i < 21; i++) // check top to bottom -> find left and right border of matrix
        {
            for (int j = 0; j < 21; j++)
            {
                if (left == -1)
                {
                    if (matrix[j, i] != '\0')
                    {
                        left = i;
                    }
                }

                if (right == -1)
                {
                    if (matrix[j, 20 - i] != '\0')
                    {
                        right = 20 - i;
                    }
                }
            }

            if (left != -1 && right != -1) break;
        }
        
        Debug.Log("left: "+left+" - top: "+top);
        
        updateFirstLettersRowsAndColumns(left, top);
        
        //===================================================================================================================

        int verticalLength = bottom - top + 1;
        int horizontalLength = right - left + 1;
        
        char[, ] smallestMatrix = new char[verticalLength, horizontalLength];

        for (int i = 0; i < verticalLength; i++)
        {
            for (int j = 0; j < horizontalLength; j++)
            {
                smallestMatrix[i, j] = playgroundMatrix[top + i, left + j];
            }
        }
        
        playgroundMatrix = (char[,])smallestMatrix.Clone();
        
        debugPostMatrix(playgroundMatrix); //=====DEBUG
        
        for (int i = 0; i < verticalLength; i++)
        {
            for (int j = 0; j < horizontalLength; j++)
            {
                smallestMatrix[i, j] = matrix[top + i, left + j];
            }
        }

        matrix = smallestMatrix;
        
        Debug.Log("matrix:: ");
        debugPostMatrix(matrix); //=====DEBUG
        
        Debug.Log("playgroundMatrix:: ");
        debugPostMatrix(playgroundMatrix); //=====DEBUG
    }

    void updateFirstLettersRowsAndColumns(int left, int top)
    {
        for (int i=0 ; i < firstLettersRowsAndColumnsAndDirections.Count ; i++)
        {
            String[] rowColumnDirectionArray = firstLettersRowsAndColumnsAndDirections[i].Split(",");
            
            int row = Int32.Parse(rowColumnDirectionArray[0]);
            int column = Int32.Parse(rowColumnDirectionArray[1]);
            String direction = rowColumnDirectionArray[2];

            column -= left;
            row -= top;
            
            String newString = row+","+column+","+direction;
            
            firstLettersRowsAndColumnsAndDirections[i] = newString;
        }
    }

    void debugPostMatrix(char[, ] matrix)
    {
        String matrixString = "      ";

        for (int i = 0; i < matrix.GetLength(1); i++)
        {
            if (i < 10)
            {
                matrixString += " "+i+"-";
            }
            else
            {
                matrixString += i+"-";
            }
        }
        matrixString += "\n";
        
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            if (i < 10)
            {
                matrixString += i+" - ";
            }
            else
            {
                matrixString += i+"- ";
            }
            
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j] == '\0')
                {
                    matrixString += "  .  ";
                }
                else
                {
                    if (matrix[i, j] == ';' || matrix[i, j] == 'İ' || matrix[i, j] == 'I')
                    {
                        matrixString += " " + matrix[i, j] + "   ";
                    }
                    else if(matrix[i, j] == 'M')
                    {
                        matrixString += " " + matrix[i, j] + " ";
                    }
                    else
                    {
                        if (random.Next(0, 2) == 0)
                        {
                            matrixString += " " + matrix[i, j] + "  ";
                        }
                        else
                        {
                            matrixString += " " + matrix[i, j] + " ";
                        }
                    }
                }
            }
            matrixString += "\n";
        }
        
        Debug.Log(matrixString);
    }

    void debugPostLettersInMatrix(Dictionary<char, List<String>> lettersInMatrix)
    {
        String result = "";
        foreach (char c in lettersInMatrix.Keys)
        {
            String line = c+" --> ";
            foreach (String s in lettersInMatrix[c])
            {
                line += s + " - ";
            }
            result += line + "\n";
        }
        
        Debug.Log(result);
    }

    void debugPostFirstLetters()
    {
        String result = "";
        
        for (int i=0 ; i < firstLettersRowsAndColumnsAndDirections.Count ; i++)
        {
            result += firstLettersRowsAndColumnsAndDirections[i] + " - ";
        }
        
        Debug.Log(result);
    }

    void buildPlaygroundMatrix()
    {
        //===================================================================================== Random select 5 words ->

        List<String> selectedWords = randomSelectTopicAndWords();

        //======================================================================== Put longest word into matrix first ->
        
        putLongestWordInMatrixAndRemoveIt(selectedWords);
        
        //============================================================================== HashMap of letters in matrix ->
        
        Dictionary<char, List<String>> lettersInMatrix = lettersMapInMatrix();
        
        //======================================================================= Put rest of words based on overlaps ->

        List<int> remainingIndexesShuffled = shuffleFisherYates(Enumerable.Range(0, selectedWords.Count).ToList());

        do
        {
            //=============== Tries to put new word to matrix based on overlaps
            //=============== Everytime it puts a new word, tries again for new overlaps
            while (tryPuttingWordToMatrixBasedOnOverlap(remainingIndexesShuffled, selectedWords, lettersInMatrix));
            
            //========================== If theres no overlap, puts random world to matrix
            putRandomWordToMatrix(remainingIndexesShuffled, selectedWords, lettersInMatrix);
        }
        while(remainingIndexesShuffled.Count > 0);

        //======================================================================= Convert matrix to playground matrix ->
        
        convertMatrixToPlaygroundMatrix();
    }
    
    public IEnumerator higlightWordDone(String rowColumnDirection)
    {
        float higlightDelay = 0.1f;
        
        int rowCount = matrix.GetLength(0);
        int columnCount = matrix.GetLength(1);
        
        float timer = higlightDelay;
        
        String[] rowColumnDirectionArray = rowColumnDirection.Split(",");
            
        int row = Int32.Parse(rowColumnDirectionArray[0]);
        int column = Int32.Parse(rowColumnDirectionArray[1]);
        String direction = rowColumnDirectionArray[2];

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= higlightDelay)
            {
                if (direction == "v")
                {
                    if(row == rowCount) yield break;
                    
                    if (matrix[row, column] != '\0')
                    {
                        timer = 0;
                        StartCoroutine(playgroundScript.highlightButtonGreen(row, column));
                        row++;
                    }
                    else yield break;
                }
                else if(direction == "h")
                {
                    if(column == columnCount) yield break;
                    
                    if (matrix[row, column] != '\0')
                    {
                        timer = 0;
                        StartCoroutine(playgroundScript.highlightButtonGreen(row, column));
                        column++;
                    }
                    else yield break;
                }
            }
            
            yield return null;
        }
    }

    public IEnumerator flashHintButton()
    {
        float timer = 0;
        
        float flashDuration = 0.3f;

        int stage = 1;
        while (true)
        {
            if (stage == 1)
            {
                timer += Time.deltaTime;
                
                Color c = hintButtonImage.color;
                c.g = 1 - (timer/flashDuration);
                c.b = 1 - (timer/flashDuration);
                hintButtonImage.color = c;

                if (timer >= flashDuration)
                {
                    c = hintButtonImage.color;
                    c.g = 0;
                    c.b = 0;
                    hintButtonImage.color = c;
                    
                    stage = 2;
                    timer = flashDuration;
                    continue;
                }
            }
            else
            {
                timer -= Time.deltaTime;
                
                Color c = hintButtonImage.color;
                c.g = 1 - (timer/flashDuration);
                c.b = 1 - (timer/flashDuration);
                hintButtonImage.color = c;

                if (timer <= 0)
                {
                    c = hintButtonImage.color;
                    c.g = 1;
                    c.b = 1;
                    hintButtonImage.color = c;
                    
                    yield break;
                }
            }
            
            yield return null;
        }
    }
    
    
    
    
    
    
    
    
}
