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

        Game game = new Game(logicScriptInstance,
            playgroundLeftAnchor,
            playgroundRightAnchor,
            playgroundTopAnchor,
            playgroundBottomAnchor,
            rowCount,
            colCount,
            moveCount,
            blocks,
            factory);
        
        for (int i=0 ; i<rowCount ; i++)
        {
            for (int j=0 ; j<colCount ; j++)
            {
                blocks[i][j].setGame(game);
            }
        }
        
        setComboCounts(game);
    }

    void setComboCounts(Game game)
    {
        Debug.Log("**setComboCounts**");
        
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
                    continue;
                }
                
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
        
        if(current.getType() != type) return 0;
        
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
    
    BlockType blockTypeConverter(String type)
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
        private Block[][] map;

        private LogicScript logicScript;
        private BlockFactory factory;

        private bool isBlocksFalling = false;
        
        private float cellGap;
        private Vector2[][] cellUIPositionMatrix;

        private int rowCount;
        private int colCount;
        private int moveCount;

        public List<(int,int)> destroyedBlocksRowAndColumn = new List<(int,int)>();

        public Game(LogicScript logicScript, float left, float right, float top, float bottom, 
            int rowCount, int columnCount, int moveCount, 
            Block[][] gridBlocks, BlockFactory factory)
        {
            Debug.Log("Game()\n"+"left:"+left+", right:"+right+", top:"+top+", bottom:"+bottom+", rowCount:"+rowCount+", columnCount:"+columnCount);
            this.factory = factory;
            this.rowCount = rowCount;
            this.colCount = columnCount;
            this.moveCount = moveCount;
            
            cellUIPositionMatrix = new Vector2[rowCount][];
            for (int i = 0; i < rowCount; i++)
            {
                cellUIPositionMatrix[i] = new Vector2[columnCount];
            }
            
            map = gridBlocks;
            
            float horizontalLength = right - left;
            float verticalLength = top - bottom;
            
            cellGap = Math.Min(horizontalLength/columnCount, verticalLength/rowCount);
            
            Vector2 playgroundCenter = new Vector2(0, bottom + (top - bottom)/2);
            
            Vector2 topLeftUIPosition = new Vector2(left + cellGap/2, top - cellGap/2);

            for (int i = 0; i<rowCount; i++)  //=== store UI Positions
            {
                for (int j = 0; j<columnCount; j++)
                {
                    cellUIPositionMatrix[i][j] = topLeftUIPosition + new Vector2(cellGap*j,-cellGap*i);
                }
            }
            
            for (int i = 0; i<rowCount; i++)  //=== set Blocks UI Positions
            {
                for (int j = 0; j<columnCount; j++)
                {
                    map[i][j].rect.anchoredPosition = cellUIPositionMatrix[i][j];
                    map[i][j].rect.sizeDelta = new Vector2(cellGap, cellGap);
                    map[i][j].rect.localScale = new Vector2(1,1);
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

        public void fallColumns()
        {
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

            foreach (var pair in destroyedRowsOfFallingColumns)
            {
                int column = pair.Key;
                List<int> destroyedRows = pair.Value;
                
                //======================================================================================================
                //=====================================================  shift down and fill top with random blocks
                
                destroyedRows.Sort((a, b) => b.CompareTo(a));

                List<(int,int)> fallingCountAndStartingRow = new List<(int,int)>();
                
                int offset = 1;
                int consecutive;
                for(int i=0 ; i<destroyedRows.Count ; i++)
                {
                    consecutive = 1;
                    offset++;
                    while (i + 1 < destroyedRows.Count && destroyedRows[i+1] == destroyedRows[i] + 1)
                    {
                        i++;
                        consecutive++;
                    }
                    
                    fallingCountAndStartingRow.Add((offset, destroyedRows[i]-consecutive+1));
                    
                    addRandomBlocksToColumnAndShift(destroyedRows[i]-1, column, offset);
                }
                
                //======================================================================================================
                //================================================================  fall ui rectTransform of blocks
                
                fallingCountAndStartingRow.Sort((a, b) => b.Item1.CompareTo(a.Item1));

                int previousStartingRow = -1;
                for (int i = 0; i < fallingCountAndStartingRow.Count; i++)
                {
                    int count = fallingCountAndStartingRow[i].Item1;
                    int startingRow = fallingCountAndStartingRow[i].Item2;
                    
                    List<int> fallingRows = new List<int>();
                    
                    for (int j = startingRow; j > previousStartingRow; j--)
                    {
                        fallingRows.Add(j);
                    }
                    previousStartingRow = startingRow;
                    
                    fallingRows.Sort((a,b) => b.CompareTo(a));

                    logicScript.StartCoroutine(fall(column, fallingRows, count));
                }
            }
        }

        void addRandomBlocksToColumnAndShift(int startRow, int column, int count)
        {
            Debug.Log("addRandomBlocksToColumnAndShift\nstartRow:"+startRow+", column:"+column+", count:"+count);
            for (int i = startRow; i >= 0; i--) //== shift
            {
                map[i-count][column] = map[i][column];
            }

            for (int i = 0; i < count; i++) //== add random blocks to top
            {
                map[i][column] = factory.getRandomBlock(i,column);

                Vector2 pos = cellUIPositionMatrix[0][column];
                pos.y -= cellGap * (count - i);
                
                map[i][column].rect.anchoredPosition = pos;
                map[i][column].rect.sizeDelta = new Vector2(cellGap, cellGap);
                map[i][column].rect.localScale = new Vector2(1,1);
            }
            
        }

        IEnumerator fall(int column, List<int> fallingRows, int count)
        {
            float targetY = map[fallingRows[0]][column].rect.anchoredPosition.y - count*cellGap;
            float speed = -2 * cellGap;
            float acceleration = -2 * cellGap;
    
            while(true){
                foreach(int row in fallingRows){
                    Vector2 pos = map[row][column].rect.anchoredPosition;
                    pos.y += Time.deltaTime * speed;
                    map[row][column].rect.anchoredPosition = pos;
                }
                
                speed += Time.deltaTime * acceleration;
        
                if(map[fallingRows[0]][column].rect.anchoredPosition.y <= targetY){
                    foreach(int row in fallingRows){
                        Vector2 pos = map[row][column].rect.anchoredPosition;
                        pos.y = targetY;
                        map[row][column].rect.anchoredPosition = pos;
                    }
            
                    yield break;
                }
        
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
        public Sprite redBlockSprite;
        public Sprite greenBlockSprite;
        public Sprite blueBlockSprite;
        public Sprite yellowBlockSprite;
    
        public Sprite boxBlockSprite;
        public Sprite stoneBlockSprite;
        public Sprite vaseBlockSprite;
        
        public Sprite verticalRocketSprite;
        public Sprite horizontalRocketSprite;

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
        
        private BlockType type;
        public RectTransform rect;
        public Button button;

        private bool isColoredBlock;
        private bool isObstacle;
        
        public int comboCount = 1;
        public List<Block> comboBlocks = new List<Block>();
        
        public int row;
        public int column;
        public int health;

        public Block(BlockType type, BlockSpriteProvider provider, int row, int column, Transform playgroundTransform)
        {
            this.row = row;
            this.column = column;
            
            GameObject obj = new GameObject("Block_"+row+"_"+column+"_"+type.ToString());
            obj.transform.localScale = new Vector3(1, 1, 1);
            obj.transform.SetParent(playgroundTransform);
                
            rect = obj.AddComponent<RectTransform>();

            Button button = obj.AddComponent<Button>();
            button.onClick.AddListener(buttonClicked);
                
            Image image = obj.AddComponent<Image>();
            
            setTypeHealthSprite(type, image, provider);
        }

        void setTypeHealthSprite(BlockType type, Image image, BlockSpriteProvider provider)
        {
            if (type == BlockType.Red)
            {
                this.type = BlockType.Red;
                isObstacle = false;
                isColoredBlock = true;
                health = 1;
                image.sprite = provider.redBlockSprite;
            }
            else if(type == BlockType.Green)
            {
                this.type = BlockType.Green;
                isObstacle = false;
                isColoredBlock = true;
                health = 1;
                image.sprite = provider.greenBlockSprite;
            }
            else if(type == BlockType.Blue)
            {
                this.type = BlockType.Blue;
                isObstacle = false;
                isColoredBlock = true;
                health = 1;
                image.sprite = provider.blueBlockSprite;
            }
            else if(type == BlockType.Yellow)
            {
                this.type = BlockType.Yellow;
                isObstacle = false;
                isColoredBlock = true;
                health = 1;
                image.sprite = provider.yellowBlockSprite;
            }
            else if(type == BlockType.Box)
            {
                this.type = BlockType.Box;
                isObstacle = true;
                isColoredBlock = false;
                health = 1;
                image.sprite = provider.boxBlockSprite;
            }
            else if(type == BlockType.Stone)
            {
                this.type = BlockType.Stone;
                isObstacle = true;
                isColoredBlock = false;
                health = 1;
                image.sprite = provider.stoneBlockSprite;
            }
            else if(type == BlockType.Vase)
            {
                this.type = BlockType.Vase;
                isObstacle = true;
                isColoredBlock = false;
                health = 2;
                image.sprite = provider.vaseBlockSprite;
            }
            else if(type == BlockType.VerticalRocket)
            {
                this.type = BlockType.VerticalRocket;
                isObstacle = false;
                isColoredBlock = false;
                health = 1;
                image.sprite = provider.verticalRocketSprite;
            }
            else if(type == BlockType.HorizontalRocket)
            {
                this.type = BlockType.HorizontalRocket;
                isObstacle = false;
                isColoredBlock = false;
                health = 1;
                image.sprite = provider.horizontalRocketSprite;
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

        public void buttonClicked()
        {
            Debug.Log("**buttonClicked** row: "+row+", column: "+column);

            string debug = "comboCount: ";
            debug += comboCount.ToString() + "\n";
            debug = logDebugComboBlocks(debug);
            Debug.Log(debug);
            
            if (comboCount <= 1)
            {
                return;
            }
            
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

        bool decreaseHealth() // return true if decreased health is zero
        {
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
            game.addDestroyedBlock(row, column);
            Destroy(rect.gameObject);
        }
    }

    class BlockFactory
    {
        private Random random;
        
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
            
            if (random == 0) return new Block(BlockType.Red, provider, row, col, playgroundTransform);
            else if (random == 1) return new Block(BlockType.Green, provider, row, col, playgroundTransform);
            else if (random == 2) return new Block(BlockType.Blue, provider, row, col, playgroundTransform);
            else /* (random == 3) */ return new Block(BlockType.Yellow, provider, row, col, playgroundTransform);
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}
