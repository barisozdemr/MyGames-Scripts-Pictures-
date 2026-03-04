using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class LogicScript : MonoBehaviour
{
    LogicScript logicScriptInstance;
    
    private string LEVELDATAPATH = "loaded_level_data";

    public GameObject UIManager;
    private UIScript uiScript;
    
    private Random random = new Random();

    public Transform playgroundTransform;

    private string levelNumber;
    private string levelGridWidth;
    private string levelGridHeight;
    private string levelMoveCount;
    private string levelGridBlocksData;
    
    private float playgroundLeftAnchor = -450f;
    private float playgroundRightAnchor = 450f;
    private float playgroundTopAnchor = 640f;
    private float playgroundBottomAnchor = -860f;
    
    void Awake()
    {
        logicScriptInstance = this;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiScript = UIManager.GetComponent<UIScript>();
        uiScript.setPlaygroundTransform(playgroundTransform);
        
        buildLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void buildLevel()
    {
        BlockSpriteProvider spriteProvider = new BlockSpriteProvider();
        
        TextAsset textAsset = Resources.Load<TextAsset>(LEVELDATAPATH);
        string[] lines = textAsset.text.Split('\n');

        for (int i=0 ; i<lines.Length ; i++)
        {
            if (i == 0) //=== level number
            {
                levelNumber = lines[i].Split(':')[1];
            }
            else if (i == 1) //=== grid width
            {
                levelGridWidth = lines[i].Split(':')[1];
            }
            else if (i == 2) //=== grid height
            {
                levelGridHeight = lines[i].Split(':')[1];
            }
            else if (i == 3) //=== move count
            {
                levelMoveCount = lines[i].Split(':')[1];
            }
            else if (i == 4) //=== grid blocks data
            {
                levelGridBlocksData = lines[i].Split(':')[1];
            }
        }
        
        int moveCount = int.Parse(levelMoveCount);
        
        int rowCount = int.Parse(levelGridHeight);
        int colCount = int.Parse(levelGridWidth);
        
        //========================================================================================= Create Block Factory
        int blockCount = rowCount * colCount;
        
        Block[][] blocks = new Block[rowCount][];
        for(int i=0; i<rowCount; i++) blocks[i] = new Block[colCount];

        BlockFactory factory = new BlockFactory(levelGridBlocksData, spriteProvider, playgroundTransform, rowCount, colCount, random);

        for (int i=0 ; i<rowCount ; i++)
        {
            for (int j=0 ; j<colCount ; j++)
            {
                blocks[i][j] = factory.getNextBlock();
            }
        }
        
        //======================================================================================= Determine UI Positions
        Vector2[][] cellUIPositionMatrix = new Vector2[rowCount][];
        for (int i = 0; i < rowCount; i++)
        {
            cellUIPositionMatrix[i] = new Vector2[colCount];
        }
            
        float horizontalLength = playgroundRightAnchor - playgroundLeftAnchor;
        float verticalLength = playgroundTopAnchor - playgroundBottomAnchor;
            
        float cellGap = Math.Min(horizontalLength/colCount, verticalLength/rowCount);
            
        Vector2 playgroundCenter = new Vector2(0, playgroundBottomAnchor + (playgroundTopAnchor - playgroundBottomAnchor)/2);
            
        Vector2 topLeftUIPosition = new Vector2(playgroundLeftAnchor + cellGap/2, playgroundTopAnchor - cellGap/2);

        for (int i = 0; i<rowCount; i++)  //=== determine UI Positions
        {
            for (int j = 0; j<colCount; j++)
            {
                cellUIPositionMatrix[i][j] = topLeftUIPosition + new Vector2(cellGap*j,-cellGap*i);
            }
        }
        
        uiScript.setUIPositionMatrix(cellUIPositionMatrix);
        uiScript.setCellGap(cellGap);
        
        //========================================================================================== Create And Set Game

        Game game = new Game(
            uiScript,
            logicScriptInstance,
            cellGap,
            cellUIPositionMatrix,
            rowCount,
            colCount,
            moveCount,
            blocks,
            factory
            );
        
        for (int i=0 ; i<rowCount ; i++)
        {
            for (int j=0 ; j<colCount ; j++)
            {
                blocks[i][j].setGame(game);
            }
        }
        
        factory.setGame(game);
        
        setComboCounts(game);
    }

    void setComboCounts(Game game)
    {
        Debug.Log("**setComboCounts**");
        
        game.debugLogMap();
        
        HashSet<Block> alreadySetBlocks = new HashSet<Block>();
        
        for (int i = 0; i < game.getRowCount(); i++)
        {
            for (int j = 0; j < game.getColCount(); j++)
            {
                string debug = "row: " + i + ", column: " + j + "===================================================\n";
                
                Block block = game.getMap()[i][j];
                
                if(alreadySetBlocks.Contains(block))
                {
                    debug += "block combo is already set\n";
                    Debug.Log(debug);
                    continue;
                }

                block.comboCount = 1;
                block.comboBlocks = new List<Block>();
                
                HashSet<Block> thisLoop = new HashSet<Block>();
                
                thisLoop.Add(block);

                int totalComboCount = 1;
                totalComboCount += getComboCountDFS(block, thisLoop, block.getType(), game, i, j-1);
                totalComboCount += getComboCountDFS(block, thisLoop, block.getType(), game, i, j+1);
                totalComboCount += getComboCountDFS(block, thisLoop, block.getType(), game, i-1, j);
                totalComboCount += getComboCountDFS(block, thisLoop, block.getType(), game, i+1, j);

                debug += "totalComboCount: " + totalComboCount + "\n thisLoop: ";
                
                foreach (Block b in thisLoop)
                {
                    debug += "row:"+b.row+",col:"+b.column+" - ";
                    b.comboCount = totalComboCount;
                    b.comboBlocks = new List<Block>(thisLoop);
                    alreadySetBlocks.Add(b);
                }
                
                Debug.Log(debug);
            }
        }
    }

    int getComboCountDFS(Block block, HashSet<Block> thisLoop, BlockType type, Game game, int row, int col)
    {
        string DFSDebug = "DFS row:"+row + ", col:" + col + "\n";
        
        if (row < 0 || row >= game.getRowCount() || col < 0 || col >= game.getColCount()) return 0;
        
        Block current = game.getMap()[row][col];
        
        if(thisLoop.Contains(current)) return 0;
        
        if(!current.isColoredBlock || current.getType() != type) return 0;
        
        block.addComboBlock(current);
        thisLoop.Add(current);
        int totalCombo = 1;
        
        totalCombo += getComboCountDFS(block, thisLoop, type, game, row, col-1); // left
        totalCombo += getComboCountDFS(block, thisLoop, type, game, row, col+1); // right
        totalCombo += getComboCountDFS(block, thisLoop, type, game, row-1, col); // top
        totalCombo += getComboCountDFS(block, thisLoop, type, game, row+1, col); // bottom
        
        Debug.Log(DFSDebug);
        return totalCombo;
    }
    
    BlockType blockTypeConverter(string type)
    {
        if(type == "r") return BlockType.Red;
        else if(type == "g") return BlockType.Green;
        else if(type == "b") return BlockType.Blue;
        else if(type == "y") return BlockType.Yellow;
        
        else if(type == "bo") return BlockType.Box;
        else if(type == "s") return BlockType.Stone;
        else if(type == "v") return BlockType.Vase;
        
        else if(type == "vro") return BlockType.VerticalRocket;
        else if(type == "hro") return BlockType.HorizontalRocket;
        
        else /* (type == "rand") */
        {
            int random = this.random.Next(0,4);
            
            if (random == 0) return BlockType.Red;
            else if (random == 1) return BlockType.Green;
            else if (random == 2) return BlockType.Blue;
            else /* (random == 3) */ return BlockType.Yellow;
        }
    }

    class Game
    {
        private int rowCount;
        private int colCount;
        
        private Block[][] map;

        private LogicScript logicScript;
        private UIScript uiScript;
        private BlockFactory factory;

        public bool isAnimationPlaying = false;
        public bool isGameDone = false;
        
        private float cellGap;
        public readonly Vector2[][] cellUIPositionMatrix;
        
        private int moveCount;

        private int boxCount = 0;
        private int stoneCount = 0;
        private int vaseCount = 0;

        public List<(int,int)> destroyedBlocksRowAndColumn = new List<(int,int)>();

        public Game(UIScript uiScript, LogicScript logicScript, float cellGap, Vector2[][] cellUIPositionMatrix, 
            int rowCount, int columnCount, int moveCount, 
            Block[][] gridBlocks, BlockFactory factory)
        {
            Debug.Log("Game()\n"+"rowCount:"+rowCount+", columnCount:"+columnCount);
            this.uiScript = uiScript;
            this.logicScript = logicScript;
            this.cellGap = cellGap;
            this.cellUIPositionMatrix = cellUIPositionMatrix;
            this.rowCount = rowCount;
            this.colCount = columnCount;
            this.moveCount = moveCount;
            map = gridBlocks;
            this.factory = factory;
            
            setBlocksRectAndObstacleCount();
        }

        void setBlocksRectAndObstacleCount()
        {
            for (int i = 0; i<rowCount; i++)  //=== set Blocks UI Positions(RectTransform) And Obstacle Counts
            {
                for (int j = 0; j<colCount; j++)
                {
                    map[i][j].rect.anchoredPosition = cellUIPositionMatrix[i][j];
                    map[i][j].rect.sizeDelta = new Vector2(cellGap, cellGap);
                    map[i][j].rect.localScale = new Vector2(1,1);

                    if (map[i][j].isObstacle)
                    {
                        if (map[i][j].getType() == BlockType.Box)
                        {
                            boxCount++;
                        }
                        else if (map[i][j].getType() == BlockType.Stone)
                        {
                            stoneCount++;
                        }
                        else if (map[i][j].getType() == BlockType.Vase)
                        {
                            vaseCount++;
                        }
                    }
                }
            }
        }

        public Block[][] getMap()
        {
            return map;
        }

        public int getRowCount()
        {
            return rowCount;
        }
        
        public int getColCount()
        {
            return colCount;
        }

        public void addDestroyedBlock(int row, int column)
        {
            if (!destroyedBlocksRowAndColumn.Contains((row, column))) destroyedBlocksRowAndColumn.Add((row, column));
        }

        public void decreaseObstacleCount(BlockType type)
        {
            if(type == BlockType.Box) boxCount--;
            else if(type == BlockType.Stone) stoneCount--;
            else if(type == BlockType.Vase) vaseCount--;
        }

        public void decreaseMoveCount()
        {
            moveCount--;
        }
        
        public void checkIfGameIsOver()
        {
            if (boxCount == 0 && stoneCount == 0 && vaseCount == 0)
            {
                isGameDone = true;
                Debug.Log("Level Won!");
            }
            if (moveCount == 0)
            {
                isGameDone = true;
                Debug.Log("Game Over!");
            }
            
            Debug.Log("boxCount: "+boxCount+"stoneCount: "+stoneCount+"vaseCount: "+vaseCount+"\nmoveCount: "+moveCount);
        }

        public void addRocket(int row, int col)
        {
            Block rocketBlock = factory.getRandomRocket(row, col);
            
            rocketBlock.rect.anchoredPosition = cellUIPositionMatrix[row][col];
            rocketBlock.rect.sizeDelta = new Vector2(cellGap, cellGap);
            rocketBlock.rect.localScale = new Vector2(1,1);
            
            map[row][col] = rocketBlock;
        }

        public void shootRockets(string direction, int row, int col)
        {
            logicScript.StartCoroutine(shootRocketsMasterCoroutine(direction, row, col));
        }

        public IEnumerator shootRocketsMasterCoroutine(string direction, int row, int col)
        {
            bool[] flags = new bool[2];

            isAnimationPlaying = true;
            
            float timeStep = 0.2f;
            
            if (direction == "horizontal")
            {
                logicScript.StartCoroutine(shootRocketsHorizontal(row, col, timeStep, flags, 0));
                logicScript.StartCoroutine(uiScript.shootRocketsHorizontal(row, col, timeStep, flags, 1));
            }
            else if (direction == "vertical")
            {
                logicScript.StartCoroutine(shootRocketsVertical(row, col, timeStep, flags, 0));
                logicScript.StartCoroutine(uiScript.shootRocketsVertical(row, col, timeStep, flags, 1));
            }
            
            Debug.Log("waiting coroutines...");
            yield return logicScript.StartCoroutine(WaitForAllCoroutines(flags));
            Debug.Log("coroutines done!");
            
            checkIfGameIsOver();
            fallColumns();
        }
        
        IEnumerator shootRocketsHorizontal(int row, int col, float timeStep, bool[] flags, int flagIndex)
        {
            float timer = 0;
    
            int targetStepCount = Math.Max(col + 1, colCount - col);
    
            while (true)
            {
                timer += Time.deltaTime;
                
                int step = (int)(timer / timeStep);
        
                if(step == targetStepCount)
                {
                    flags[flagIndex] = true;
                    yield break;
                }
        
                if(col - step >= 0)
                {
                    map[row][col-step].decreaseHealth();
                }
        
                if(col + step < colCount)
                {
                    map[row][col+step].decreaseHealth();
                }
        
                yield return null;
            }
        }
        
        IEnumerator shootRocketsVertical(int row, int col, float timeStep, bool[] flags, int flagIndex)
        {
            float timer = 0;
    
            int targetStepCount = Math.Max(row + 1, rowCount - row);
    
            while (true)
            {
                timer += Time.deltaTime;
                
                int step = (int)(timer / timeStep);
        
                if(step == targetStepCount)
                {
                    flags[flagIndex] = true;
                    yield break;
                }
        
                if(row - step >= 0)
                {
                    map[row-step][col].decreaseHealth();
                }
        
                if(row + step < rowCount)
                {
                    map[row+step][col].decreaseHealth();
                }
        
                yield return null;
            }
        }

        public void fallColumns()
        {
            //================================================================= DEBUG
            string debug = "destroyedBlocksRowAndColumn:\n";
            for (int i = 0; i < destroyedBlocksRowAndColumn.Count; i++)
            {
                int row = destroyedBlocksRowAndColumn[i].Item1;
                int col = destroyedBlocksRowAndColumn[i].Item2;
                
                debug += "row:" + row + ", col:" + col + "\n";
            }
            Debug.Log(debug);
            //=======================================================================
            
            Dictionary<int, List<int>> destroyedRowsOfFallingColumns = new Dictionary<int, List<int>>();
                
            for (int i = 0; i < destroyedBlocksRowAndColumn.Count; i++)
            {
                int row = destroyedBlocksRowAndColumn[i].Item1;
                int col = destroyedBlocksRowAndColumn[i].Item2;
                
                map[row][col] = null; // destroy blocks
                
                if(!destroyedRowsOfFallingColumns.ContainsKey(col))
                {
                    destroyedRowsOfFallingColumns.Add(col, new List<int>(){row});
                    continue;
                }

                destroyedRowsOfFallingColumns[col].Add(row);
            }
            
            debug = "destroyedBlocksRowAndColumn:\n";
            foreach (var pair in destroyedRowsOfFallingColumns)
            {
                int column = pair.Key;
                List<int> destroyedRows = pair.Value;
                
                debug += "column:" + column + " - rows:";
                foreach (int row in destroyedRows) debug += row + ", ";
                debug += "\n";
            }
            Debug.Log(debug);

            //==========================================================================================================
            List<(int, List<int>, int)> fallDataOfColumns = new List<(int, List<int>, int)>();
            //===== Column (int),  Falling Rows List (List<int>),  Drop Count (int)
                
            foreach (var pair in destroyedRowsOfFallingColumns)
            {
                int column = pair.Key;
                List<int> destroyedRows = pair.Value;
                
                //=====================================================  shift down and fill top with random blocks
                destroyedRows.Sort((a, b) => b.CompareTo(a));
                
                List<(int,int)> shiftCountAndStartingRow = new List<(int,int)>();
                
                int offset = 0;
                int consecutive;
                for(int i=0 ; i<destroyedRows.Count ; i++)
                {
                    consecutive = 1;
                    offset++;
                    while (i + 1 < destroyedRows.Count && destroyedRows[i+1] == destroyedRows[i] - 1)
                    {
                        i++;
                        consecutive++;
                        offset++;
                    }
                    
                    shiftCountAndStartingRow.Add((consecutive, destroyedRows[i]-1));
                }
                
                shiftCountAndStartingRow.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                int topOffset = 0;
                for(int i = 0; i < shiftCountAndStartingRow.Count; i++)
                {
                    int count = shiftCountAndStartingRow[i].Item1;
                    int startingRow = shiftCountAndStartingRow[i].Item2;
                    
                    addRandomBlocksToColumnAndShift(startingRow, column, count, topOffset);
                    
                    topOffset += count;
                }
                
                //===================================================== determine falling blocks and falling counts
                if (shiftCountAndStartingRow.Count > 1) // => different rows falling different heights
                {
                    int lastRow = destroyedRows[0];
                    int dropCount = 0;

                    debug = "";
                    for (int i = 0; i < destroyedRows.Count; i++)
                    {
                        int row = destroyedRows[i];
                        
                        List<int> fallingRows = new List<int>();

                        if (i + 1 >= destroyedRows.Count)
                        {
                            dropCount++;

                            for (int j = lastRow; j >= 0; j--)
                            {
                                fallingRows.Add(j);
                            }
                        }
                        else if (row-1 != destroyedRows[i+1])
                        {
                            int count = row - destroyedRows[i+1] - 1;
                            dropCount += count;
                            
                            for (int j = lastRow; j > lastRow - count; j--)
                            {
                                fallingRows.Add(j);
                            }

                            lastRow = lastRow - count;
                        }
                        else if (row-1 == destroyedRows[i+1])
                        {
                            i++;
                            dropCount++;
                            row--;
                            
                            while(i + 1 < destroyedRows.Count && row-1 == destroyedRows[i+1])
                            {
                                i++;
                                dropCount++;
                                row--;
                            }

                            if (i + 1 >= destroyedRows.Count)
                            {
                                dropCount++;
                                
                                for (int j = lastRow; j >= 0; j--)
                                {
                                    fallingRows.Add(j);
                                }
                            }
                            else
                            {
                                int count = row - destroyedRows[i+1] - 1;
                                dropCount += count;
                            
                                for (int j = lastRow; j > lastRow - count; j--)
                                {
                                    fallingRows.Add(j);
                                }
                                
                                lastRow = lastRow - count;
                            }
                        }
                        
                        fallingRows.Sort((a,b) => b.CompareTo(a));
                        
                        debug += "column:"+column+", dropCount:"+dropCount+"\nfallingRows = ";
                        foreach (int r in fallingRows) debug += r + ", ";
                        debug += "\n";
                        
                        fallDataOfColumns.Add((column, fallingRows, dropCount));
                    }
                    Debug.Log(debug);
                }
                else // => all rows falling same height
                {
                    int endingRow = -1;
                    int count = 1;
                    int startingRow = destroyedRows[0];

                    int j = 0;
                    while(j + 1 < destroyedRows.Count && destroyedRows[j]-1 == destroyedRows[j+1])
                    {
                        count++;
                        j++;
                    }
                
                    List<int> fallingRows = new List<int>();
                
                    for (int i = startingRow; i > endingRow; i--)
                    {
                        fallingRows.Add(i);
                    }
                
                    fallingRows.Sort((a,b) => b.CompareTo(a));
                    
                    debug = "column:"+column+", count:"+count+"\nfallingRows = ";
                    foreach (int row in fallingRows) debug += row + ", ";
                    Debug.Log(debug);
                    
                    fallDataOfColumns.Add((column, fallingRows, count));
                }
            }
            //==========================================================================================================
            
            debugLogMap();
                
            logicScript.setComboCounts(this);
            
            destroyedBlocksRowAndColumn.Clear();
            
            logicScript.StartCoroutine(masterFallCoroutine(fallDataOfColumns));
        }

        public void debugLogMap()
        {
            string debug = "      0- 1- 2- 3- 4- 5\n";

            for (int i = 0; i < rowCount; i++)
            {
                debug += i + " - ";
                for (int j = 0; j < colCount; j++)
                {
                    BlockType type = map[i][j].getType();

                    if (type == BlockType.Red) debug += "R  ";
                    else if (type == BlockType.Green) debug += "G  ";
                    else if (type == BlockType.Blue) debug += "B  ";
                    else if (type == BlockType.Yellow) debug += "Y  ";
                    else if (type == BlockType.Box) debug += "X  ";
                    else if (type == BlockType.Stone) debug += "S  ";
                    else if (type == BlockType.Vase) debug += "V  ";
                }
                debug += "\n";
            }
            
            Debug.Log(debug);
        }

        void addRandomBlocksToColumnAndShift(int startRow, int column, int count, int topOffset)
        {
            Debug.Log("addRandomBlocksToColumnAndShift\nstartRow:"+startRow+", column:"+column+", count:"+count);
            for (int i = startRow; i >= 0; i--) //== shift
            {
                map[i+count][column] = map[i][column];
                map[i+count][column].updateRowAndColumn(i+count, column);
            }

            for (int i = 0; i < count; i++) //== add random blocks to top
            {
                map[i][column] = factory.getRandomBlock(i,column);

                Vector2 pos = cellUIPositionMatrix[0][column];
                pos.y += cellGap * (count - i + topOffset);
                
                map[i][column].rect.anchoredPosition = pos;
                map[i][column].rect.sizeDelta = new Vector2(cellGap, cellGap);
                map[i][column].rect.localScale = new Vector2(1,1);
            }
        }

        IEnumerator masterFallCoroutine(List<(int, List<int>, int)> fallDataOfColumns)
        {
            isAnimationPlaying = true;
            
            bool[] flags = new bool[fallDataOfColumns.Count];

            int index = 0;
            foreach (var data in fallDataOfColumns)
            {
                int column = data.Item1;
                int dropCount = data.Item3;
                List<int> fallingRows = data.Item2;
                
                Dictionary<RectTransform, int> rectAndRows = new Dictionary<RectTransform, int>();

                foreach (var row in fallingRows)
                {
                    rectAndRows.Add(map[row][column].rect, row);
                }
                
                logicScript.StartCoroutine(uiScript.fall(column, rectAndRows, dropCount, cellGap, flags, index));

                index++;
            }

            yield return logicScript.StartCoroutine(WaitForAllCoroutines(flags));

            isAnimationPlaying = false;
            Debug.Log("Tüm coroutineler bitti, master tamamlandı!");
        }
        
        IEnumerator WaitForAllCoroutines(bool[] flags)
        {
            while (true)
            {
                bool g = true;
                foreach (bool b in flags)
                {
                    if(!b)
                    {
                        g = false;
                        break;
                    }
                }
                
                if(g) yield break;
                yield return null;
            }
        }
    }

    enum BlockType
    {
        Red,
        Green,
        Blue,
        Yellow,
        Box,
        Stone,
        Vase,
        VerticalRocket,
        HorizontalRocket
    }

    class BlockSpriteProvider
    {
        private Sprite redBlockSprite;
        private Sprite greenBlockSprite;
        private Sprite blueBlockSprite;
        private Sprite yellowBlockSprite;
    
        private Sprite boxBlockSprite;
        private Sprite stoneBlockSprite;
        private Sprite vaseBlockSprite;
        
        private Sprite verticalRocketSprite;
        private Sprite horizontalRocketSprite;

        public BlockSpriteProvider()
        {
            redBlockSprite = Resources.Load<Sprite>("BlockTextures/redBlock");
            greenBlockSprite = Resources.Load<Sprite>("BlockTextures/greenBlock");
            blueBlockSprite = Resources.Load<Sprite>("BlockTextures/blueBlock");
            yellowBlockSprite = Resources.Load<Sprite>("BlockTextures/yellowBlock");
            
            boxBlockSprite = Resources.Load<Sprite>("BlockTextures/boxBlock");
            stoneBlockSprite = Resources.Load<Sprite>("BlockTextures/stoneBlock");
            vaseBlockSprite = Resources.Load<Sprite>("BlockTextures/vaseBlock");
            
            verticalRocketSprite = Resources.Load<Sprite>("BlockTextures/verticalRocket");
            horizontalRocketSprite = Resources.Load<Sprite>("BlockTextures/horizontalRocket");
        }
        
        public Sprite GetSprite(BlockType type)
        {
            switch (type)
            {
                case BlockType.Red: return redBlockSprite;
                case BlockType.Green: return greenBlockSprite;
                case BlockType.Blue: return blueBlockSprite;
                case BlockType.Yellow: return yellowBlockSprite;
                case BlockType.Box: return boxBlockSprite;
                case BlockType.Stone: return stoneBlockSprite;
                case BlockType.Vase: return vaseBlockSprite;
                case BlockType.VerticalRocket: return verticalRocketSprite;
                case BlockType.HorizontalRocket: return horizontalRocketSprite;
                default: return null;
            }
        }
    }

    class Block
    {
        private Game game;
        private Transform playgroundTransform;
        private BlockSpriteProvider provider;
        
        private BlockType type;
        public RectTransform rect;
        public Button button;

        public bool isColoredBlock;
        public bool isObstacle;
        
        public int comboCount = 1;
        public List<Block> comboBlocks = new List<Block>();
        
        public int row;
        public int column;
        public int health;

        public Block(BlockType type, BlockSpriteProvider provider, int row, int column, Transform playgroundTransform)
        {
            this.playgroundTransform = playgroundTransform;
            this.provider = provider;
            
            this.row = row;
            this.column = column;
            
            GameObject obj = new GameObject("Block_"+row+"_"+column+"_"+type.ToString());
            obj.transform.localScale = new Vector3(1, 1, 1);
            obj.transform.SetParent(playgroundTransform);
                
            rect = obj.AddComponent<RectTransform>();

            Button button = obj.AddComponent<Button>();
            button.onClick.AddListener(buttonClicked);
                
            Image image = obj.AddComponent<Image>();
            
            setTypeHealthSprite(type, image);
        }

        void setTypeHealthSprite(BlockType type, Image image)
        {
            if (type == BlockType.Red)
            {
                this.type = BlockType.Red;
                isObstacle = false;
                isColoredBlock = true;
                health = 1;
                image.sprite = provider.GetSprite(type);
            }
            else if(type == BlockType.Green)
            {
                this.type = BlockType.Green;
                isObstacle = false;
                isColoredBlock = true;
                health = 1;
                image.sprite = provider.GetSprite(type);
            }
            else if(type == BlockType.Blue)
            {
                this.type = BlockType.Blue;
                isObstacle = false;
                isColoredBlock = true;
                health = 1;
                image.sprite = provider.GetSprite(type);
            }
            else if(type == BlockType.Yellow)
            {
                this.type = BlockType.Yellow;
                isObstacle = false;
                isColoredBlock = true;
                health = 1;
                image.sprite = provider.GetSprite(type);
            }
            else if(type == BlockType.Box)
            {
                this.type = BlockType.Box;
                isObstacle = true;
                isColoredBlock = false;
                health = 1;
                image.sprite = provider.GetSprite(type);
            }
            else if(type == BlockType.Stone)
            {
                this.type = BlockType.Stone;
                isObstacle = true;
                isColoredBlock = false;
                health = 1;
                image.sprite = provider.GetSprite(type);
            }
            else if(type == BlockType.Vase)
            {
                this.type = BlockType.Vase;
                isObstacle = true;
                isColoredBlock = false;
                health = 2;
                image.sprite = provider.GetSprite(type);
            }
            else if(type == BlockType.VerticalRocket)
            {
                this.type = BlockType.VerticalRocket;
                isObstacle = false;
                isColoredBlock = false;
                health = 1;
                image.sprite = provider.GetSprite(type);
            }
            else if(type == BlockType.HorizontalRocket)
            {
                this.type = BlockType.HorizontalRocket;
                isObstacle = false;
                isColoredBlock = false;
                health = 1;
                image.sprite = provider.GetSprite(type);
            }
        }

        public BlockType getType()
        {
            return type;
        }

        public void setGame(Game game)
        {
            this.game = game;
        }

        public void updateRowAndColumn(int row, int col)
        {
            this.row = row;
            this.column = col;
        }

        public void buttonClicked()
        {
            Debug.Log("**buttonClicked** row: "+row+", column: "+column);

            string debug = "comboCount: ";
            debug += comboCount.ToString() + "\n";
            debug = logDebugComboBlocks(debug);
            Debug.Log(debug);

            if (isColoredBlock)
            {
                if (comboCount <= 1 || game.isAnimationPlaying || game.isGameDone) return;
            
                game.decreaseMoveCount();
                
                foreach (Block block in comboBlocks)
                {
                    int blockRow = block.row;
                    int blockColumn = block.column;

                    if (blockColumn-1 >= 0 && game.getMap()[blockRow][blockColumn-1].isObstacle) game.getMap()[blockRow][blockColumn-1].decreaseHealth();
                    if (blockColumn+1 < game.getColCount() && game.getMap()[blockRow][blockColumn+1].isObstacle) game.getMap()[blockRow][blockColumn+1].decreaseHealth();
                    if (blockRow-1 >= 0 && game.getMap()[blockRow-1][blockColumn].isObstacle) game.getMap()[blockRow-1][blockColumn].decreaseHealth();
                    if (blockRow+1 < game.getRowCount() && game.getMap()[blockRow+1][blockColumn].isObstacle) game.getMap()[blockRow+1][blockColumn].decreaseHealth();
                
                    block.destroyBlock();
                }

                if (comboCount >= 5) // create rocket
                {
                    game.destroyedBlocksRowAndColumn.Remove((row, column));
                    
                    game.addRocket(row, column);
                }
            }
            else if(!isObstacle) // is rocket
            {
                Debug.Log("Rocket Clicked");
                if (game.isAnimationPlaying || game.isGameDone) return;
                
                game.decreaseMoveCount();
                destroyBlock();

                if (type == BlockType.HorizontalRocket) game.shootRockets("horizontal", row, column);
                if (type == BlockType.VerticalRocket) game.shootRockets("vertical", row, column);

                return;
            }
            
            game.checkIfGameIsOver();
            game.fallColumns();
        }

        string logDebugComboBlocks(string debug)
        {
            foreach (Block block in comboBlocks)
            {
                debug += "comboBlock - row: " + block.row.ToString() + ", column: " + block.column.ToString() + ", type: " + block.type.ToString() + "\n";
            }

            return debug;
        }

        public void addComboBlock(Block block)
        {
            if (comboBlocks.Contains(block)) return;
            comboCount++;
            comboBlocks.Add(block);
        }

        public bool decreaseHealth() // return true if decreased health is zero
        {
            if (health == 0) return false;
            
            health--;
            if (health <= 0)
            {
                destroyBlock();
                return true;
            }

            return false;
        }

        void destroyBlock()
        {
            if(isObstacle) game.decreaseObstacleCount(type);
            game.addDestroyedBlock(row, column);
            Destroy(rect.gameObject);
        }
    }

    class BlockFactory
    {
        private Random random;
        private Game game;
        
        private BlockType[] types;
        private BlockSpriteProvider provider;
        private Transform playgroundTransform;
        
        private int index = 0;
        private int rowCount;
        private int columnCount;
        
        public BlockFactory(string levelBlocksData, BlockSpriteProvider provider, Transform playgroundTransform, int rowCount, int columnCount, Random random)
        {
            this.provider = provider;
            this.playgroundTransform = playgroundTransform;
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            this.random = random;
            
            string[] data = levelBlocksData.Split(',');
            
            int length = data.Length;
            
            types = new BlockType[length];

            for (int i=0 ; i<length ; i++)
            {
                int row = 1 + (i / columnCount);
                int dataIndex = (i % columnCount) + (length - (columnCount * row));
                
                types[i] = blockTypeConverter(data[dataIndex]);
            }
        }

        public void setGame(Game game)
        {
            this.game = game;
        }

        BlockType blockTypeConverter(string type)
        {
            if(type == "r") return BlockType.Red;
            else if(type == "g") return BlockType.Green;
            else if(type == "b") return BlockType.Blue;
            else if(type == "y") return BlockType.Yellow;
        
            else if(type == "bo") return BlockType.Box;
            else if(type == "s") return BlockType.Stone;
            else if(type == "v") return BlockType.Vase;
        
            else if(type == "vro") return BlockType.VerticalRocket;
            else if(type == "hro") return BlockType.HorizontalRocket;
        
            else /* (type == "rand") */
            {
                int random = this.random.Next(0,4);
            
                if (random == 0) return BlockType.Red;
                else if (random == 1) return BlockType.Green;
                else if (random == 2) return BlockType.Blue;
                else /* (random == 3) */ return BlockType.Yellow;
            }
        }

        public Block getNextBlock()
        {
            int row = index / columnCount;
            int col = index % columnCount;
            
            BlockType type = types[index];
            index++;
            
            return new Block(type, provider, row, col, playgroundTransform);
        }

        public Block getRandomBlock(int row, int col)
        {
            int random = this.random.Next(0,4);

            Block block;
            
            if (random == 0) block = new Block(BlockType.Red, provider, row, col, playgroundTransform);
            else if (random == 1) block = new Block(BlockType.Green, provider, row, col, playgroundTransform);
            else if (random == 2) block = new Block(BlockType.Blue, provider, row, col, playgroundTransform);
            else /* (random == 3) */ block = new Block(BlockType.Yellow, provider, row, col, playgroundTransform);
            
            block.setGame(game);

            return block;
        }
        
        public Block getRandomRocket(int row, int col)
        {
            int random = this.random.Next(0, 2);
            
            Block block;
    
            if (random == 0) block = new Block(BlockType.HorizontalRocket, provider, row, col, playgroundTransform);
            else block = new Block(BlockType.VerticalRocket, provider, row, col, playgroundTransform);
            
            block.setGame(game);

            return block;
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}
