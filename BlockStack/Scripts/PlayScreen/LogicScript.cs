using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using Random = System.Random;

public class LogicScript : MonoBehaviour
{
    private System.Random random = new System.Random();

    public GameObject UIManager;
    private UIScript uiScript;
    
    public GameObject audioManager;
    public AudioScript audioScript;

    private int playgroundRowCount = 13;
    private int playgroundColumnCount = 7;
    
    private float timer = 0;
    private float oldTime = 0;
    public TextMeshProUGUI clockText;

    private int score;
    public TextMeshProUGUI scoreText;
    
    public Image nextBlockImage;
    
    public Sprite blockT_Sprite;
    public Sprite blockZ_Sprite;
    public Sprite blockSolZ_Sprite;
    public Sprite blockL_Sprite;
    public Sprite blockSolL_Sprite;
    public Sprite blockI_Sprite;
    public Sprite blockO_Sprite;
    public Sprite blockU_Sprite;
    
    private Block nextBlock = null;
    
    private bool isGameOver = false;
    
    private Block currentBlock;
    private int currentRow;
    private int currentColumn;
    
    private bool[][] map;

    private float tickSpeed = 1.5f;

    private float tickTimer = 0;

    private bool rowComboAnimation = false;
    
    private bool gamePaused = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiScript = UIManager.GetComponent<UIScript>();
        audioScript = audioManager.GetComponent<AudioScript>();

        map = new bool[playgroundRowCount][];

        for (int i = 0; i < playgroundRowCount; i++)
        {
            map[i] = new bool[playgroundColumnCount];
        }

        startGame();
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
        
        if(!gamePaused && !rowComboAnimation && !isGameOver) tickTimer += Time.deltaTime;
        
        if (tickTimer >= tickSpeed)
        {
            StartCoroutine(tick());
            tickTimer = 0;
        }
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

    public void togglePause()
    {
        if (gamePaused) gamePaused = false;
        else gamePaused = true;
    }

    public void increaseScore(int value)
    {
        score += value;
        scoreText.text = score.ToString();
    }

    enum BlockType
    {
        T,
        L,
        SolL,
        Z,
        SolZ,
        I,
        O,
        U
    }
    
    class Block
    {
        public BlockType type;
        
        public bool[][] blockMap;

        public int mapLength;

        public Block(BlockType blockType)
        {
            if (blockType == BlockType.T)
            {
                type = BlockType.T;
                blockMap = new bool[3][]
                {
                    new bool[]{false, false, false},
                    new bool[]{true, true, true},
                    new bool[]{false, true, false}
                };
                mapLength = 3;
            }
            else if (blockType == BlockType.L)
            {
                type = BlockType.L;
                blockMap = new bool[3][]
                {
                    new bool[]{false, true, false},
                    new bool[]{false, true, false},
                    new bool[]{false, true, true}
                };
                mapLength = 3;
            }
            else if (blockType == BlockType.SolL)
            {
                type = BlockType.SolL;
                blockMap = new bool[3][]
                {
                    new bool[]{false, true, false},
                    new bool[]{false, true, false},
                    new bool[]{true, true, false}
                };
                mapLength = 3;
            }
            else if (blockType == BlockType.Z)
            {
                type = BlockType.Z;
                blockMap = new bool[3][]
                {
                    new bool[]{true, true, false},
                    new bool[]{false, true, true},
                    new bool[]{false, false, false}
                };
                mapLength = 3;
            }
            else if (blockType == BlockType.SolZ)
            {
                type = BlockType.SolZ;
                blockMap = new bool[3][]
                {
                    new bool[]{false, true, true},
                    new bool[]{true, true, false},
                    new bool[]{false, false, false}
                };
                mapLength = 3;
            }
            else if (blockType == BlockType.I)
            {
                type = BlockType.I;
                blockMap = new bool[4][]
                {
                    new bool[]{false, true, false, false},
                    new bool[]{false, true, false, false},
                    new bool[]{false, true, false, false},
                    new bool[]{false, true, false, false}
                };
                mapLength = 4;
            }
            else if (blockType == BlockType.O)
            {
                type = BlockType.O;
                blockMap = new bool[3][]
                {
                    new bool[]{true, true, false},
                    new bool[]{true, true, false},
                    new bool[]{false, false, false}
                };
                mapLength = 3;
            }
            else if (blockType == BlockType.U)
            {
                type = BlockType.U;
                blockMap = new bool[3][]
                {
                    new bool[]{true, true, true},
                    new bool[]{true, false, true},
                    new bool[]{false, false, false}
                };
                mapLength = 3;
            }
        }

        public void rotateCCW()
        {
            if (type == BlockType.O) return;
            
            bool[][] newMap = new bool[mapLength][];

            for (int i = 0; i < newMap.Length; i++)
            {
                newMap[i] = new bool[mapLength];
            }
            
            for (int i = 0; i<mapLength ; i++)
            {
                for (int j = 0; j<mapLength ; j++)
                {
                    newMap[i][j] = blockMap[j][mapLength-i-1];
                }
            }
            
            blockMap = newMap;
        }
        
        public void rotateCW()
        {
            if (type == BlockType.O) return;
            
            bool[][] newMap = new bool[mapLength][];

            for (int i = 0; i < newMap.Length; i++)
            {
                newMap[i] = new bool[mapLength];
            }
            
            for (int i = 0; i<mapLength ; i++)
            {
                for (int j = 0; j<mapLength ; j++)
                {
                    newMap[i][j] = blockMap[mapLength - j - 1][i];
                }
            }
            
            blockMap = newMap;
        }
    }

    void startGame()
    {
        placeNewBlock();
        
        drawBlockToMap();
        
        drawPlayground();
    }

    Block produceNewBlock()
    {
        Debug.Log("new Block=========================");
        int randomBlockType = random.Next(0, 8);
        
        if (randomBlockType == 0) return new Block(BlockType.T);
        else if (randomBlockType == 1) return new Block(BlockType.L);
        else if (randomBlockType == 2) return new Block(BlockType.SolL);
        else if (randomBlockType == 3) return new Block(BlockType.Z);
        else if (randomBlockType == 4) return new Block(BlockType.SolZ);
        else if (randomBlockType == 5) return new Block(BlockType.I);
        else if (randomBlockType == 6) return new Block(BlockType.O);
        else  /*(randomBlockType == 7)*/ return new Block(BlockType.U);
    }

    Block getNextBlock()
    {
        Block block = nextBlock;
        
        nextBlock = produceNewBlock();
        
        setNextBlockImage();

        if (block == null) return produceNewBlock();
        
        return block;
    }

    void setNextBlockImage()
    {
        if (nextBlockImage.color.a != 1)
        {
            Color color = nextBlockImage.color;
            color.a = 1;
            nextBlockImage.color = color;
        }
        
        if (nextBlock.type == BlockType.T) nextBlockImage.sprite = blockT_Sprite;
        else if (nextBlock.type == BlockType.L) nextBlockImage.sprite = blockL_Sprite;
        else if (nextBlock.type == BlockType.SolL) nextBlockImage.sprite = blockSolL_Sprite;
        else if (nextBlock.type == BlockType.Z) nextBlockImage.sprite = blockZ_Sprite;
        else if (nextBlock.type == BlockType.SolZ) nextBlockImage.sprite = blockSolZ_Sprite;
        else if (nextBlock.type == BlockType.I) nextBlockImage.sprite = blockI_Sprite;
        else if (nextBlock.type == BlockType.O) nextBlockImage.sprite = blockO_Sprite;
        else if (nextBlock.type == BlockType.U) nextBlockImage.sprite = blockU_Sprite;
    }

    void gameOver()
    {
        isGameOver = true;
        Debug.Log("gameOver");
        
        uiScript.setGameOverScreen(clockText.text, scoreText.text);
    }

    bool placeNewBlock()
    {
        Block nextBlock = getNextBlock();

        (int, int) newRowColumn = newPlacementRowColumn(nextBlock);
        
        currentBlock = nextBlock;
        currentRow = newRowColumn.Item1;
        currentColumn = newRowColumn.Item2;

        bool canPlace = checkNewBlock();

        if (!canPlace) return false;

        return true;
    }
    
    (int, int) newPlacementRowColumn(Block block)
    {
        int newRow = 0;
        int newColumn = (int)MathF.Floor(playgroundColumnCount/2f); // 4th column
        
        int blockMapLength = block.mapLength;
        int halfBlockMapLength = block.mapLength / 2;

        newRow += halfBlockMapLength;

        if (block.type == BlockType.T) newRow--;

        return (newRow, newColumn);
    }

    IEnumerator tick()
    {
        clearBlockFromMap();
    
        currentRow++;

        bool canTick = checkBlock();

        if (!canTick)
        {
            currentRow--;
            drawBlockToMap(); // draw the current block where it was

            List<int> comboRows = checkRowCombo();
            bool combo = comboRows.Count > 0;

            if (combo)
            {
                rowComboAnimation = true;
                
                yield return uiScript.clearComboRows(comboRows);
                clearComboRowsAndCollapseMap(comboRows);
                
                rowComboAnimation = false;
            }
            
            bool canPlaceNewBlock = placeNewBlock();
    
            if(!canPlaceNewBlock) // ========== gameOver
            {
                audioScript.playGameOverSoundClip();
                gameOver();
            }
            else // ========== newBlock
            {
                audioScript.playTickSoundClip();
                increaseScore(1);
                drawBlockToMap(); // draw new block
                drawPlayground();
            }
        }
        else // ========== tick
        {
            audioScript.playTickSoundClip();
            drawBlockToMap(); // draw the falling block
            drawPlayground();
        }
    }

    public void tickButtonClicked()
    {
        if (!isGameOver && !rowComboAnimation && !gamePaused)
        {
            StartCoroutine(tick());
            tickTimer = 0;
        }
    }

    public void left()
    {
        if (isGameOver) return;
    
        clearBlockFromMap();
    
        currentColumn--;

        bool canGoLeft = checkBlock();

        if (!canGoLeft)
        {
            currentColumn++;
        
            drawBlockToMap();
        }
        else
        {
            drawBlockToMap();
            drawPlayground();
        }
    }

    public void right()
    {
        if (isGameOver) return;
    
        clearBlockFromMap();
    
        currentColumn++;

        bool canGoRight = checkBlock();

        if (!canGoRight)
        {
            currentColumn--;
        
            drawBlockToMap();
        }
        else
        {
            drawBlockToMap();
            drawPlayground();
        }
    }

    public void rotate()
    {
        if (isGameOver) return;
    
        clearBlockFromMap();
    
        currentBlock.rotateCCW();
    
        bool canRotate = checkBlock();
    
        if (!canRotate)
        {
            currentBlock.rotateCW();
        
            drawBlockToMap();
        }
        else
        {
            drawBlockToMap();
            drawPlayground();
        }
    }

    void drawBlockToMap()
    {
        int blockMapLength = currentBlock.mapLength;
        int halfBlockMapLength = currentBlock.mapLength / 2;
        
        for (int i = 0; i<blockMapLength ; i++)
        {
            int mapRow = currentRow + i - halfBlockMapLength;
            for (int j = 0; j < blockMapLength; j++)
            {
                int mapColumn = currentColumn + j - halfBlockMapLength;

                try
                {
                    if(currentBlock.blockMap[i][j]) map[mapRow][mapColumn] = true;
                }
                catch (Exception){}
            }
        }
        
        drawMapDebug();
    }

    void drawPlayground()
    {
        uiScript.drawPlayground(map);
    }

    void clearBlockFromMap()
    {
        int blockMapLength = currentBlock.mapLength;
        int halfBlockMapLength = currentBlock.mapLength / 2;
        
        for (int i = 0; i<blockMapLength ; i++)
        {
            int mapRow = currentRow + i - halfBlockMapLength;
            for (int j = 0; j < blockMapLength; j++)
            {
                int mapColumn = currentColumn + j - halfBlockMapLength;

                try
                {
                    if(currentBlock.blockMap[i][j]) map[mapRow][mapColumn] = false;
                }
                catch (Exception){}
            }
        }
    }
    
    bool checkBlock()
    {
        int blockMapLength = currentBlock.mapLength;
        int halfBlockMapLength = currentBlock.mapLength / 2;
        
        for (int i = 0; i<blockMapLength ; i++)
        {
            int mapRow = currentRow + i - halfBlockMapLength;
            for (int j = 0; j < blockMapLength; j++)
            {
                int mapColumn = currentColumn + j - halfBlockMapLength;

                if (mapRow < 0 || mapRow >= playgroundRowCount || mapColumn < 0 || mapColumn >= playgroundColumnCount)
                {
                    if (currentBlock.blockMap[i][j]) return false;
                }
                else
                {
                    if (map[mapRow][mapColumn] && currentBlock.blockMap[i][j]) return false;
                }
            }
        }

        return true;
    }
    
    bool checkNewBlock()
    {
        int blockMapLength = currentBlock.mapLength;
        int halfBlockMapLength = currentBlock.mapLength / 2;
        
        for (int i = 0; i<blockMapLength ; i++)
        {
            int mapRow = currentRow + i - halfBlockMapLength;
            for (int j = 0; j < blockMapLength; j++)
            {
                int mapColumn = currentColumn + j - halfBlockMapLength;

                try
                {
                    if (map[mapRow][mapColumn] && currentBlock.blockMap[i][j]) return false;
                }
                catch (Exception){}
            }
        }

        return true;
    }

    void drawMapDebug()
    {
        String print = "";

        foreach (var array in map)
        {
            foreach (bool b in array)
            {
                if(b) print += "X";
                else print += " . ";
            }
            print += "\n";
        }
        
        Debug.Log(print);
    }

    List<int> checkRowCombo()
    {
        List<int> rowCombo = new List<int>();
        
        for (int i=0 ; i<map.Length ; i++)
        {
            bool combo = true;
            foreach (bool b in map[i])
            {
                if (!b)
                {
                    combo = false;
                    break;
                }
            }
            if (combo) rowCombo.Add(i);
        }
        
        return rowCombo;
    }

    public void clearComboRowsAndCollapseMap(List<int> rowCombo)
    {
        foreach (int row in rowCombo)
        {
            for (int j = 0; j < playgroundColumnCount ; j++) // clear combo row
            {
                map[row][j] = false;
            }
        
            for (int i = row; i > 0; i--) // move rows down that above the combo row
            {
                for (int j = 0; j < playgroundColumnCount; j++)
                {
                    map[i][j] = map[i-1][j];
                }
            }
        }
    }

    
    
    
    
    
    
}
