using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class LogicScript : MonoBehaviour
{
    LogicScript logicScriptInstance;

    public GameObject highlighterPrefab;

    private Game game;
    
    private static string LEVELNUMBERPATH = "level_number";
    private static string LOADEDLEVELDATAPATH = "loaded_level_data";

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
        LEVELNUMBERPATH = Path.Combine(Application.persistentDataPath, "level_number.txt");
        LOADEDLEVELDATAPATH = Path.Combine(Application.persistentDataPath, "loaded_level_data.txt");
        
        uiScript = UIManager.GetComponent<UIScript>();
        uiScript.setPlaygroundTransform(playgroundTransform);
        
        buildLevel();
        
        playgroundFadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openLevelSelectScene()
    {
        SceneManager.LoadScene("LevelSelectScene", LoadSceneMode.Single);
    }

    void playgroundFadeIn()
    {
        int rowCount = game.getMap().GetLength(0);
        int colCount = game.getMap()[0].GetLength(0);
        
        Image[] images = new Image[rowCount*colCount];

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                images[j+(i*colCount)] = game.getMap()[i][j].image;
            }
        }
        
        uiScript.playgroundFadeIn(images);
    }

    void buildLevel()
    {
        BlockSpriteProvider spriteProvider = new BlockSpriteProvider();
        
        string[] lines = File.ReadAllText(LOADEDLEVELDATAPATH).Split("\n");

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

        BlockFactory factory = new BlockFactory(levelGridBlocksData, logicScriptInstance, uiScript, 
            spriteProvider, playgroundTransform, rowCount, colCount, random);

        //========================================================================================= Create Blocks
        for (int i=0 ; i<rowCount ; i++)
        {
            for (int j=0 ; j<colCount ; j++)
            {
                blocks[i][j] = factory.getNextBlock();
            }
        }
        
        //======================================================================================= Determine UI Positions
        Vector3[][] cellUIPositionMatrix = new Vector3[rowCount][];
        for (int i = 0; i < rowCount; i++)
        {
            cellUIPositionMatrix[i] = new Vector3[colCount];
        }
            
        float horizontalLength = playgroundRightAnchor - playgroundLeftAnchor;
        float verticalLength = playgroundTopAnchor - playgroundBottomAnchor;
            
        float cellGap = Math.Min(horizontalLength/colCount, verticalLength/rowCount);
            
        Vector3 topLeftUIPosition = new Vector3(playgroundLeftAnchor + cellGap/2, playgroundTopAnchor - cellGap/2, 0);

        Vector2 playgroundAnchoredPos = playgroundTransform.GetComponent<RectTransform>().anchoredPosition;
        Vector3 playgroundRectPos = new Vector3(playgroundAnchoredPos.x, playgroundAnchoredPos.y, 0);
        
        topLeftUIPosition -= playgroundRectPos;

        for (int i = 0; i<rowCount; i++)  //=== determine UI Positions
        {
            for (int j = 0; j<colCount; j++)
            {
                cellUIPositionMatrix[i][j] = topLeftUIPosition + new Vector3(cellGap*j, -cellGap*i, 0);
            }
        }
        
        uiScript.setUIPositionMatrix(cellUIPositionMatrix);
        uiScript.setCellGap(cellGap);
        
        //========================================================================================== Create And Set Game

        game = new Game(
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
        HashSet<Block> alreadySetBlocks = new HashSet<Block>();
        
        for (int i = 0; i < game.getRowCount(); i++)
        {
            for (int j = 0; j < game.getColCount(); j++)
            {
                Block block = game.getMap()[i][j];
                
                if(block == null || alreadySetBlocks.Contains(block))
                {
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
                
                foreach (Block b in thisLoop)
                {
                    b.comboCount = totalComboCount;
                    b.comboBlocks = new List<Block>(thisLoop);
                    
                    if(thisLoop.Count >= 5) b.setSpriteRocketHint();
                    else
                    {
                        if(b.isRocketHinted) b.setSpriteDefault();
                    }
                    
                    alreadySetBlocks.Add(b);
                }
            }
        }
    }

    int getComboCountDFS(Block block, HashSet<Block> thisLoop, BlockType type, Game game, int row, int col)
    {
        if (row < 0 || row >= game.getRowCount() || col < 0 || col >= game.getColCount()) return 0;
        
        Block current = game.getMap()[row][col];

        if (current == null) return 0;
        
        if(thisLoop.Contains(current)) return 0;
        
        if(!current.isColoredBlock || current.getType() != type) return 0;
        
        block.addComboBlock(current);
        thisLoop.Add(current);
        int totalCombo = 1;
        
        totalCombo += getComboCountDFS(block, thisLoop, type, game, row, col-1); // left
        totalCombo += getComboCountDFS(block, thisLoop, type, game, row, col+1); // right
        totalCombo += getComboCountDFS(block, thisLoop, type, game, row-1, col); // top
        totalCombo += getComboCountDFS(block, thisLoop, type, game, row+1, col); // bottom
        
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
    
    public class BoolRef
    {
        private bool value;
    
        public BoolRef()
        {
            value = false;
        }
    
        public BoolRef(bool value)
        {
            this.value = value;
        }
    
        public void setTrue()
        {
            value = true;
        }
    
        public void setFalse()
        {
            value = false;
        }
    
        public bool getValue()
        {
            return value;
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
        public readonly Vector3[][] cellUIPositionMatrix;
        
        private int moveCount;

        private int boxCount = 0;
        private int stoneCount = 0;
        private int vaseCount = 0;

        public List<(int,int)> destroyedBlocksRowAndColumn = new List<(int,int)>();

        public Game(UIScript uiScript, LogicScript logicScript, float cellGap, Vector3[][] cellUIPositionMatrix, 
            int rowCount, int columnCount, int moveCount, 
            Block[][] gridBlocks, BlockFactory factory)
        {
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
            
            uiScript.setTexts(moveCount, boxCount, stoneCount, vaseCount);
        }

        void setBlocksRectAndObstacleCount()
        {
            for (int i = 0; i<rowCount; i++)  //=== set Blocks UI Positions(RectTransform) And Obstacle Counts
            {
                for (int j = 0; j<colCount; j++)
                {
                    map[i][j].rect.anchoredPosition3D = cellUIPositionMatrix[i][j];
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
            if (boxCount == 0 && stoneCount == 0 && vaseCount == 0) // level won!
            {
                File.WriteAllText(LEVELNUMBERPATH, (int.Parse(logicScript.levelNumber) + 1).ToString());
                isGameDone = true;
                Debug.Log("Level Won!");

                uiScript.Invoke("setLevelWonUI", 1);
            }
            if (moveCount == 0) // game over!
            {
                isGameDone = true;
                Debug.Log("Game Over!");
                
                uiScript.Invoke("setGameOverUI", 1);
            }
            
            Debug.Log("boxCount: "+boxCount+"stoneCount: "+stoneCount+"vaseCount: "+vaseCount+"\nmoveCount: "+moveCount);
        }

        public void addRocket(int row, int col)
        {
            Block rocketBlock = factory.getRandomRocket(row, col);
            
            rocketBlock.rect.anchoredPosition3D = cellUIPositionMatrix[row][col];
            rocketBlock.rect.sizeDelta = new Vector2(cellGap, cellGap);
            rocketBlock.rect.localScale = new Vector2(1,1);
            
            map[row][col] = rocketBlock;
        }

        public void rocketClicked(string direction, int row, int col)
        {
            //particle
            logicScript.StartCoroutine(shootRocketsMasterCoroutine(direction, row, col));
        }

        IEnumerator shootRocketsMasterCoroutine(string direction, int row, int col)
        {
            BoolRef[] flags = new BoolRef[2];
            flags[0] = new BoolRef();
            flags[1] = new BoolRef();

            isAnimationPlaying = true;
            
            float timeStep = 0.1f; //===================== time step
            
            if (direction == "horizontal")
            {
                logicScript.StartCoroutine(shootRocketsHorizontal(row, col, timeStep, flags[0]));
                logicScript.StartCoroutine(uiScript.shootRocketsHorizontal(row, col, timeStep, flags[1]));
            }
            else if (direction == "vertical")
            {
                logicScript.StartCoroutine(shootRocketsVertical(row, col, timeStep, flags[0]));
                logicScript.StartCoroutine(uiScript.shootRocketsVertical(row, col, timeStep, flags[1]));
            }
            
            yield return logicScript.StartCoroutine(WaitForAllCoroutinesBoolRefArray(flags));
            
            next();
        }
        
        IEnumerator shootRocketsHorizontal(int row, int col, float timeStep, BoolRef flag)
        {
            List<BoolRef> newFlags = new List<BoolRef>();
            
            HashSet<Block> damagedBlocks = new HashSet<Block>(); // check for double damage
            
            float timer = 0;
    
            int targetStepCount = Math.Max(col + 1, colCount - col);
    
            while (true)
            {
                timer += Time.deltaTime;
                
                int step = (int)(timer / timeStep);
        
                if(step >= targetStepCount)
                {
                    bool g = true;
                    for(int i=0 ; i<newFlags.Count ; i++)
                    {
                        if(!newFlags[i].getValue())
                        {
                            g = false;
                            break;
                        }
                    }
                    
                    if(g)
                    {
                        flag.setTrue();
                        yield break;
                    }
                }
        
                if(col - step >= 0)
                {
                    if (map[row][col - step] != null && !damagedBlocks.Contains(map[row][col - step]))
                    {
                        damagedBlocks.Add(map[row][col - step]);
                    
                        map[row][col - step].decreaseHealth();
                    
                        if (map[row][col - step].isRocket && !map[row][col - step].isRocketShooted)
                        {
                            map[row][col - step].isRocketShooted = true;
                        
                            (BoolRef flag1, BoolRef flag2) = shootNewRocketsAndReturnFlags(map[row][col - step].getType(), row, col - step, timeStep);
                    
                            newFlags.Add(flag1);
                            newFlags.Add(flag2);
                        }
                    }
                }
        
                if(col + step < colCount)
                {
                    if (map[row][col + step] != null && !damagedBlocks.Contains(map[row][col + step]))
                    {
                        damagedBlocks.Add(map[row][col + step]);
                    
                        map[row][col + step].decreaseHealth();
                    
                        if (map[row][col + step].isRocket && !map[row][col + step].isRocketShooted)
                        {
                            map[row][col + step].isRocketShooted = true;
                        
                            (BoolRef flag1, BoolRef flag2) = shootNewRocketsAndReturnFlags(map[row][col + step].getType(), row, col + step, timeStep);
                    
                            newFlags.Add(flag1);
                            newFlags.Add(flag2);
                        }
                    }
                }
        
                yield return null;
            }
        }
        
        IEnumerator shootRocketsVertical(int row, int col, float timeStep, BoolRef flag)
        {
            List<BoolRef> newFlags = new List<BoolRef>();
            
            HashSet<Block> damagedBlocks = new HashSet<Block>();
            
            float timer = 0;
    
            int targetStepCount = Math.Max(row + 1, rowCount - row);
    
            while (true)
            {
                timer += Time.deltaTime;
                
                int step = (int)(timer / timeStep);
        
                if(step >= targetStepCount)
                {
                    bool g = true;
                    for(int i=0 ; i<newFlags.Count ; i++)
                    {
                        if(!newFlags[i].getValue())
                        {
                            g = false;
                            break;
                        }
                    }
                    
                    if(g)
                    {
                        flag.setTrue();
                        yield break;
                    }
                }
        
                if(row - step >= 0)
                {
                    if (map[row - step][col] != null && !damagedBlocks.Contains(map[row - step][col]))
                    {
                        damagedBlocks.Add(map[row - step][col]);
                    
                        map[row - step][col].decreaseHealth();
                    
                        if (map[row - step][col].isRocket && !map[row - step][col].isRocketShooted)
                        {
                            map[row - step][col].isRocketShooted = true;
                        
                            (BoolRef flag1, BoolRef flag2) = shootNewRocketsAndReturnFlags(map[row - step][col].getType(), row - step, col, timeStep);
                    
                            newFlags.Add(flag1);
                            newFlags.Add(flag2);
                        }
                    }
                }
        
                if(row + step < rowCount)
                {
                    if (map[row + step][col] != null && !damagedBlocks.Contains(map[row + step][col]))
                    {
                        damagedBlocks.Add(map[row + step][col]);
                    
                        map[row + step][col].decreaseHealth();

                        if (map[row + step][col].isRocket && !map[row + step][col].isRocketShooted)
                        {
                            map[row + step][col].isRocketShooted = true;
                        
                            (BoolRef flag1, BoolRef flag2) = shootNewRocketsAndReturnFlags(map[row + step][col].getType(), row + step, col, timeStep);
                    
                            newFlags.Add(flag1);
                            newFlags.Add(flag2);
                        }
                    }
                }
        
                yield return null;
            }
        }

        (BoolRef, BoolRef) shootNewRocketsAndReturnFlags(BlockType type, int row, int col, float timeStep)
        {
            BoolRef newFlag = new BoolRef();
            BoolRef newFlag2 = new BoolRef();

            if (type == BlockType.HorizontalRocket)
            {
                logicScript.StartCoroutine(shootRocketsHorizontal(row, col, timeStep, newFlag));
                logicScript.StartCoroutine(uiScript.shootRocketsHorizontal(row, col, timeStep, newFlag2));
            }
            else if (type == BlockType.VerticalRocket)
            {
                logicScript.StartCoroutine(shootRocketsVertical(row, col, timeStep, newFlag));
                logicScript.StartCoroutine(uiScript.shootRocketsVertical(row, col, timeStep, newFlag2));
            }

            return (newFlag, newFlag2);
        }

        public void next()
        {
            uiScript.setTexts(moveCount, boxCount, stoneCount, vaseCount);
            
            checkIfGameIsOver();
            fallColumnsAndSetCombo();
        }

        public void blockClicked(int row, int col)
        {
            //particle
            next();
        }

        public void fallColumnsAndSetCombo()
        {
            Dictionary<int, List<int>> destroyedRowsOfFallingColumns = new Dictionary<int, List<int>>();

            for (int i = 0; i < destroyedBlocksRowAndColumn.Count; i++) // add null rows from previous move as destroyed row
            {
                int col = destroyedBlocksRowAndColumn[i].Item2;
                
                if(!destroyedRowsOfFallingColumns.ContainsKey(col))
                {
                    List<int> nullRows = new List<int>();
                    for (int j = 0; j < rowCount; j++)
                    {
                        if(map[j][col] == null) nullRows.Add(j);
                    }
                    
                    if(nullRows.Count > 0) destroyedRowsOfFallingColumns.Add(col, nullRows);
                }
            }
            
            for (int i = 0; i < destroyedBlocksRowAndColumn.Count; i++) // set destroyedRowsOfFallingColumns
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

            //==========================================================================================================
            List<(int, List<int>, int)> fallDataOfColumns = new List<(int, List<int>, int)>();
            //===== Column (int),  Falling Rows List (List<int>),  Drop Count (int)
            
            foreach (var pair in destroyedRowsOfFallingColumns)
            {
                int column = pair.Key;
                Debug.Log("COLUMN:"+column);
                List<int> destroyedRows = pair.Value;
            
                destroyedRows.Sort((a, b) => b.CompareTo(a));

                string debug = "destroyedRows: ";
                foreach (var row in destroyedRows)
                {
                    debug += row + ",";
                }
                Debug.Log(debug);
                
                //================================================================== shift columns and add new blocks
                
                Dictionary<int, List<int>> dropCountOfRows = new Dictionary<int, List<int>>(); //== dropCount => List(rows) "for ui fall"
                
                List<(int, int)> shiftCountOfRowSorted = new List<(int, int)>(); //== (row, shiftCount) sorted based on rows descending "for shifting map"

                int dropCount = 0;
                
                for (int i = rowCount - 1; i >= 0; i--)
                {
                    if (destroyedRows.Contains(i))
                    {
                        dropCount++;
                    }
                    else
                    {
                        if (map[i][column].getType() == BlockType.Stone)
                        {
                            dropCount = 0;
                        }
                        else
                        {
                            if (dropCount > 0)
                            {
                                if(!dropCountOfRows.ContainsKey(dropCount)) dropCountOfRows.Add(dropCount, new List<int>());
                                
                                dropCountOfRows[dropCount].Add(i);
                                
                                shiftCountOfRowSorted.Add((i, dropCount));
                            }
                        }
                    }
                }
                
                shiftCountOfRowSorted.Sort((a, b) => b.Item1.CompareTo(a.Item1));
                
                addRandomBlocksToColumnAndShift(column, shiftCountOfRowSorted, dropCount);
                
                //================================== DEBUG
                debug = "shiftCountOfRowSorted:\n";
                foreach (var pair2 in shiftCountOfRowSorted)
                {
                    debug += "(row:" + pair2.Item1 + ", shift:" + pair2.Item2 + ")\n";
                }
                Debug.Log(debug);
                //================================== DEBUG
                
                //================================================================== fall columns ui
                
                debugLogDropCountOfRows(dropCountOfRows);
                
                foreach (var pair2 in dropCountOfRows) // set shifted indexes of rows and sort
                {
                    int dropCount2 = pair2.Key;
                    List<int> dropRows = pair2.Value;
                    
                    dropRows.Sort((a, b) => b.CompareTo(a));

                    for (int i=0 ; i<dropRows.Count; i++)
                    {
                        dropRows[i] += dropCount2;
                    }
                }
                
                debugLogDropCountOfRows(dropCountOfRows);

                if (dropCount > 0)
                {
                    if(!dropCountOfRows.ContainsKey(dropCount)) // add new blocks
                    {
                        List<int> rows = new List<int>();
                        for (int j = dropCount-1; j >= 0; j--)
                        {
                            rows.Add(j);
                        }
                        dropCountOfRows.Add(dropCount, rows);
                    }
                    else
                    {
                        for (int j = dropCount-1; j >= 0; j--)
                        {
                            dropCountOfRows[dropCount].Add(j);
                        }
                    }
                }
                
                debugLogDropCountOfRows(dropCountOfRows);

                foreach (var pair2 in dropCountOfRows) // set fall data of column
                {
                    int dropCount2 = pair2.Key;
                    List<int> dropRows = pair2.Value;
                    
                    fallDataOfColumns.Add((column, dropRows, dropCount2));
                }
                
                //================================== DEBUG
                debug = "fallDataOfColumns:\n";
                foreach (var pair2 in fallDataOfColumns)
                {
                    debug += "(col:" + pair2.Item1 + ", dropCount:" + pair2.Item3 + ", rows:";
                    foreach (int row in pair2.Item2)
                    {
                        debug += row + ",";
                    }
                    debug += "\n";
                }
                Debug.Log(debug);
                //================================== DEBUG
            }
            
            logicScript.setComboCounts(this);
                
            destroyedBlocksRowAndColumn.Clear();
                
            logicScript.StartCoroutine(masterFallCoroutine(fallDataOfColumns));
        }

        void debugLogDropCountOfRows(Dictionary<int, List<int>> dropCountOfRows)
        {
            string debug = "dropCountOfRows:\n";
            foreach (var pair in dropCountOfRows)
            {
                debug += "dropCount:" + pair.Key + ", rows:";
                foreach (var row in pair.Value)
                {
                    debug += row + ",";
                }
            }
            Debug.Log(debug);
        }

        void addRandomBlocksToColumnAndShift(int column, List<(int, int)> shiftCountOfRowSorted, int newBlockCount)
        {
            foreach (var tuple in shiftCountOfRowSorted) //== shift
            {
                int row = tuple.Item1;
                int shiftCount = tuple.Item2;
                
                map[row+shiftCount][column] = map[row][column];
                map[row][column] = null;
                map[row+shiftCount][column].updateRowAndColumn(row+shiftCount, column);
            }

            for (int i = 0; i < newBlockCount; i++) //== add new blocks
            {
                map[i][column] = factory.getRandomBlock(i,column);

                Vector3 pos = cellUIPositionMatrix[0][column];
                pos.y += cellGap * (newBlockCount - i);
                
                map[i][column].rect.anchoredPosition3D = pos;
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

            yield return logicScript.StartCoroutine(WaitForAllCoroutinesArray(flags));

            isAnimationPlaying = false;
        }
        
        IEnumerator WaitForAllCoroutinesArray(bool[] flags)
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
        
        IEnumerator WaitForAllCoroutinesBoolRefArray(BoolRef[] flags)
        {
            while (true)
            {
                bool g = true;
                for (int i=0 ; i<flags.Length ; i++)
                {
                    if(!flags[i].getValue())
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
        
        private Sprite redBlockRocketHintSprite;
        private Sprite greenBlockRocketHintSprite;
        private Sprite blueBlockRocketHintSprite;
        private Sprite yellowBlockRocketHintSprite;
    
        private Sprite boxBlockSprite;
        private Sprite stoneBlockSprite;
        private Sprite vaseBlockSprite;
        private Sprite vaseBlockCrackedSprite;
        
        private Sprite verticalRocketSprite;
        private Sprite horizontalRocketSprite;

        public BlockSpriteProvider()
        {
            redBlockSprite = Resources.Load<Sprite>("BlockTextures/redBlock");
            greenBlockSprite = Resources.Load<Sprite>("BlockTextures/greenBlock");
            blueBlockSprite = Resources.Load<Sprite>("BlockTextures/blueBlock");
            yellowBlockSprite = Resources.Load<Sprite>("BlockTextures/yellowBlock");
            
            redBlockRocketHintSprite = Resources.Load<Sprite>("BlockTextures/redBlockRocketHint");
            greenBlockRocketHintSprite = Resources.Load<Sprite>("BlockTextures/greenBlockRocketHint");
            blueBlockRocketHintSprite = Resources.Load<Sprite>("BlockTextures/blueBlockRocketHint");
            yellowBlockRocketHintSprite = Resources.Load<Sprite>("BlockTextures/yellowBlockRocketHint");
            
            boxBlockSprite = Resources.Load<Sprite>("BlockTextures/boxBlock");
            stoneBlockSprite = Resources.Load<Sprite>("BlockTextures/stoneBlock");
            vaseBlockSprite = Resources.Load<Sprite>("BlockTextures/vaseBlock");
            vaseBlockCrackedSprite = Resources.Load<Sprite>("BlockTextures/vaseBlockCracked");
            
            verticalRocketSprite = Resources.Load<Sprite>("BlockTextures/verticalRocket");
            horizontalRocketSprite = Resources.Load<Sprite>("BlockTextures/horizontalRocket");
        }
        
        public Sprite getSprite(BlockType type)
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
        
        public Sprite getRocketHintBlockSprite(BlockType type)
        {
            switch (type)
            {
                case BlockType.Red: return redBlockRocketHintSprite;
                case BlockType.Green: return greenBlockRocketHintSprite;
                case BlockType.Blue: return blueBlockRocketHintSprite;
                case BlockType.Yellow: return yellowBlockRocketHintSprite;
                default: return null;
            }
        }

        public Sprite getCrackedVaseSprite()
        {
            return vaseBlockCrackedSprite;
        }
    }

    class Block
    {
        private LogicScript logicScript;
        private UIScript uiScript;
        private Game game;
        private Transform playgroundTransform;
        private BlockSpriteProvider spriteProvider;
        
        private BlockType type;
        public RectTransform rect;
        public Image image;
        public Button button;

        public bool isRocketHinted = false;

        public bool isColoredBlock;
        public bool isObstacle;
        public bool isRocket;
        public bool isRocketShooted;
        
        public int comboCount = 1;
        public List<Block> comboBlocks = new List<Block>();
        
        public int row;
        public int column;
        public int health;

        public Block(BlockType type, BlockSpriteProvider provider, int row, int column, Transform playgroundTransform, 
            LogicScript logicScript, UIScript uiScript)
        {
            this.logicScript = logicScript;
            this.uiScript = uiScript;
            this.playgroundTransform = playgroundTransform;
            this.spriteProvider = provider;
            
            this.row = row;
            this.column = column;
            
            GameObject obj = new GameObject("Block_"+row+"_"+column+"_"+type.ToString());
            obj.transform.localScale = new Vector3(1, 1, 1);
            obj.transform.SetParent(playgroundTransform);
                
            rect = obj.AddComponent<RectTransform>();

            button = obj.AddComponent<Button>();
            button.onClick.AddListener(buttonClicked);
                
            image = obj.AddComponent<Image>();
            
            setTypeHealthSprite(type, image);
        }

        void setTypeHealthSprite(BlockType type, Image image)
        {
            if (type == BlockType.Red)
            {
                this.type = BlockType.Red;
                isObstacle = false;
                isColoredBlock = true;
                isRocket = false;
                health = 1;
                image.sprite = spriteProvider.getSprite(type);
            }
            else if(type == BlockType.Green)
            {
                this.type = BlockType.Green;
                isObstacle = false;
                isColoredBlock = true;
                isRocket = false;
                health = 1;
                image.sprite = spriteProvider.getSprite(type);
            }
            else if(type == BlockType.Blue)
            {
                this.type = BlockType.Blue;
                isObstacle = false;
                isColoredBlock = true;
                isRocket = false;
                health = 1;
                image.sprite = spriteProvider.getSprite(type);
            }
            else if(type == BlockType.Yellow)
            {
                this.type = BlockType.Yellow;
                isObstacle = false;
                isColoredBlock = true;
                isRocket = false;
                health = 1;
                image.sprite = spriteProvider.getSprite(type);
            }
            else if(type == BlockType.Box)
            {
                this.type = BlockType.Box;
                isObstacle = true;
                isColoredBlock = false;
                isRocket = false;
                health = 1;
                image.sprite = spriteProvider.getSprite(type);
            }
            else if(type == BlockType.Stone)
            {
                this.type = BlockType.Stone;
                isObstacle = true;
                isColoredBlock = false;
                isRocket = false;
                health = 1;
                image.sprite = spriteProvider.getSprite(type);
            }
            else if(type == BlockType.Vase)
            {
                this.type = BlockType.Vase;
                isObstacle = true;
                isColoredBlock = false;
                isRocket = false;
                health = 2;
                image.sprite = spriteProvider.getSprite(type);
            }
            else if(type == BlockType.VerticalRocket)
            {
                this.type = BlockType.VerticalRocket;
                isObstacle = false;
                isColoredBlock = false;
                isRocket = true;
                isRocketShooted = false;
                health = 1;
                image.sprite = spriteProvider.getSprite(type);
            }
            else if(type == BlockType.HorizontalRocket)
            {
                this.type = BlockType.HorizontalRocket;
                isObstacle = false;
                isColoredBlock = false;
                isRocket = true;
                isRocketShooted = false;
                health = 1;
                image.sprite = spriteProvider.getSprite(type);
            }
        }

        public void setSpriteRocketHint()
        {
            image.sprite = spriteProvider.getRocketHintBlockSprite(type);
            isRocketHinted = true;
        }
        
        public void setSpriteDefault()
        {
            image.sprite = spriteProvider.getSprite(type);
            isRocketHinted = false;
        }

        public void setSpriteCrackedVase()
        {
            image.sprite = spriteProvider.getCrackedVaseSprite();
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
            if (isColoredBlock)
            {
                if (comboCount <= 1 || game.isAnimationPlaying || game.isGameDone) return;
            
                game.decreaseMoveCount();
                uiScript.blockClickedParticles(row, column);
                
                foreach (Block block in comboBlocks)
                {
                    int blockRow = block.row;
                    int blockColumn = block.column;

                    if (blockColumn-1 >= 0 && game.getMap()[blockRow][blockColumn-1] != null && game.getMap()[blockRow][blockColumn-1].isObstacle) 
                        game.getMap()[blockRow][blockColumn-1].decreaseHealth();
                    if (blockColumn+1 < game.getColCount() && game.getMap()[blockRow][blockColumn+1] != null && game.getMap()[blockRow][blockColumn+1].isObstacle) 
                        game.getMap()[blockRow][blockColumn+1].decreaseHealth();
                    if (blockRow-1 >= 0 && game.getMap()[blockRow-1][blockColumn] != null && game.getMap()[blockRow-1][blockColumn].isObstacle) 
                        game.getMap()[blockRow-1][blockColumn].decreaseHealth();
                    if (blockRow+1 < game.getRowCount() && game.getMap()[blockRow+1][blockColumn] != null && game.getMap()[blockRow+1][blockColumn].isObstacle) 
                        game.getMap()[blockRow+1][blockColumn].decreaseHealth();
                
                    uiScript.highlightBlock(blockRow, blockColumn);
                    block.destroyBlock();
                }

                if (comboCount >= 5) // create rocket
                {
                    game.destroyedBlocksRowAndColumn.Remove((row, column));
                    game.addRocket(row, column);
                    
                    uiScript.rocketCreatedParticles(row, column);
                }
                
                game.next();
            }
            else if(isRocket) // is rocket
            {
                if (game.isAnimationPlaying || game.isGameDone) return;
                
                game.decreaseMoveCount();
                destroyBlock();

                isRocketShooted = true;

                if (type == BlockType.HorizontalRocket) game.rocketClicked("horizontal", row, column);
                if (type == BlockType.VerticalRocket) game.rocketClicked("vertical", row, column);

                return;
            }
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
            else
            {
                if (type == BlockType.Vase)
                {
                    setSpriteCrackedVase();
                }
            }

            return false;
        }

        void destroyBlock()
        {
            if(isObstacle) game.decreaseObstacleCount(type);
            game.addDestroyedBlock(row, column);
            
            logicScript.StartCoroutine(collapseDestroy(0.1f));
        }

        IEnumerator collapseDestroy(float timeStep)
        {
            float timer = 0f;

            while (true)
            {
                timer += Time.deltaTime;
                
                float value = 1 - (timer / timeStep);
                
                if (value <= 0)
                {
                    Destroy(rect.gameObject);
                    yield break;
                }
                
                rect.transform.localScale = new Vector3(value, value, 1);
                yield return null;
            }
        }
    }

    class BlockFactory
    {
        private LogicScript logicScript;
        private UIScript uiScript;
        private Random random;
        private Game game;
        
        private BlockType[] types;
        private BlockSpriteProvider provider;
        private Transform playgroundTransform;
        
        private int index = 0;
        private int rowCount;
        private int columnCount;
        
        public BlockFactory(string levelBlocksData, LogicScript logicScript, UIScript uiScript, 
            BlockSpriteProvider provider, Transform playgroundTransform, int rowCount, int columnCount, Random random)
        {
            this.logicScript = logicScript;
            this.uiScript = uiScript;
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
            
            Block block = new Block(type, provider, row, col, playgroundTransform, logicScript, uiScript);
            
            Color c = block.image.color;
            c.a = 0;
            block.image.color = c;
            
            return block;
        }

        public Block getRandomBlock(int row, int col)
        {
            int randNum = random.Next(0,4);

            Block block;
            
            if (randNum == 0) block = new Block(BlockType.Red, provider, row, col, playgroundTransform, logicScript, uiScript);
            else if (randNum == 1) block = new Block(BlockType.Green, provider, row, col, playgroundTransform, logicScript, uiScript);
            else if (randNum == 2) block = new Block(BlockType.Blue, provider, row, col, playgroundTransform, logicScript, uiScript);
            else /* (randNum == 3) */ block = new Block(BlockType.Yellow, provider, row, col, playgroundTransform, logicScript, uiScript);
            
            block.setGame(game);

            return block;
        }
        
        public Block getRandomRocket(int row, int col)
        {
            int randNum = random.Next(0, 2);
            
            Block block;
    
            if (randNum == 0) block = new Block(BlockType.HorizontalRocket, provider, row, col, playgroundTransform, logicScript, uiScript);
            else block = new Block(BlockType.VerticalRocket, provider, row, col, playgroundTransform, logicScript, uiScript);
            
            block.setGame(game);

            return block;
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}
