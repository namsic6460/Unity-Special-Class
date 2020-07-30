using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stage : MonoBehaviour
{
    [Header("Source")]
    public GameObject tilePrefab;
    public GameObject GameOverCanvas;
    public GameObject UICanvas;

    [Header("Setting")]
    [Range(4, 40)]
    public int boardWidth = 10;
    [Range(5, 30)]
    public int boardHeight = 20;
    public float fallCycle = 0.0f;

    [Range(-5, 5)]
    public float offset_x = 0.0f;
    [Range(-5, 5)]
    public float offset_y = -1.0f;
    [Range(10, 500)]
    public float life = 50.0f;

    private Transform BackgroundNode;
    private Transform BoardNode;
    private Transform TetrominoNode;
    private Transform NextTetrominoNode;
    private Text EndScore;
    private Button RestartButton;
    private Text ScoreText;
    private Text NextBlockText;

    public int score = 0;

    private int halfWidth;
    private int halfHeight;

    private int nextIndex;
    private float curLife;
    private int prevKeyType = 0;
    private int keyType = 0;
    private int keyTime = 100;

    private bool isOver = false;

    public ArrayList colors = new ArrayList();
    public ArrayList vectors = new ArrayList();
    private Hashtable blocks = new Hashtable(); //y, x, object

    public void Start()
    {
        BackgroundNode = transform.Find("BackGround");
        BoardNode = transform.Find("Board");
        TetrominoNode = transform.Find("Tetromino");
        NextTetrominoNode = transform.Find("NextTetromino");

        NextTetrominoNode.position = new Vector3(halfWidth - 12.5f, halfHeight - 3);

        GameOverCanvas.SetActive(false);
        EndScore = GameOverCanvas.transform.Find("EndScore").GetComponent<Text>();
        RestartButton = GameOverCanvas.transform.Find("Restart").GetComponent<Button>();

        ScoreText = UICanvas.transform.Find("ScoreText").GetComponent<Text>();
        NextBlockText = UICanvas.transform.Find("NextBlockText").GetComponent<Text>();

        ScoreText.transform.position = new Vector3(390, 670);
        NextBlockText.transform.position = new Vector3(390, 560);
        
        RestartButton.onClick.AddListener(restart);
        RestartButton.GetComponentInChildren<Text>().text = "Restart";
        RestartButton.GetComponentInChildren<Text>().color = Color.white;

        ScoreText.GetComponent<Text>().text = "Score : 0";

        initVariable();

        halfWidth = Mathf.RoundToInt(boardWidth * 0.5f);
        halfHeight = Mathf.RoundToInt(boardHeight * 0.5f);

        CreateBackground();

        nextIndex = Random.Range(0, 7);
        Color32 color = (Color32)colors[nextIndex];
        for (int i = 0; i < 4; i++)
            CreateTile(NextTetrominoNode, ((Vector3[])vectors[nextIndex])[i], color);

        CreateTetromino();
    }

    public void Update()
    {
        if (!isOver)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                score += 10;
                ScoreText.text = "Score : " + score;

                if (score >= 1000)
                {
                    ScoreText.color = new Color(1, 0.12f, 0.16f, 1);
                    ScoreText.fontSize = 35;
                }

                else if (score >= 500)
                {
                    ScoreText.color = new Color(1, 0.32f, 0.16f, 1);
                    ScoreText.fontStyle = FontStyle.BoldAndItalic;
                    ScoreText.fontSize = 30;
                }

                else if (score >= 300)
                {
                    ScoreText.color = new Color(1, 0.52f, 0.16f, 1);
                    ScoreText.fontStyle = FontStyle.Bold;
                    ScoreText.fontSize = 25;
                }

                else if (score >= 100)
                {
                    ScoreText.color = new Color(1, 0.72f, 0.16f, 1);
                    ScoreText.fontSize = 22;
                }

                else if (score >= 50)
                    ScoreText.color = new Color(1, 1, 0.16f, 1);
            }

            curLife -= fallCycle;

            if (curLife <= 0.0f)
            {
                if (!moveDown())
                {
                    ArrayList deleteList = new ArrayList();
                    Transform child;
                    GameObject gameObject = null;
                    float x, y;

                    for (int i = 0; i < 4; i++)
                    {
                        child = TetrominoNode.GetChild(0);
                        gameObject = child.gameObject;
                        x = Mathf.Round(child.position.x);
                        y = Mathf.Round(child.position.y);

                        if (y >= halfHeight)
                        {
                            isOver = true;
                            EndScore.text = "Score : " + score;
                            GameOverCanvas.SetActive(true);
                            return;
                        }

                        child.parent = BoardNode;

                        if (!blocks.ContainsKey(y))
                            blocks.Add(y, new Hashtable());
                        ((Hashtable)blocks[y]).Add(x, gameObject);

                        if (((Hashtable)blocks[y]).Count == boardWidth)
                            deleteList.Add(y);
                    }

                    int downCount;

                    deleteList.Sort();
                    if (deleteList.Count != 0)
                    {
                        int height = blocks.Count;
                        score += (int)Mathf.Pow(2, deleteList.Count);
                        fallCycle += 0.1f * deleteList.Count;
                        ScoreText.text = "Score : " + score;

                        if (score >= 1000)
                        {
                            ScoreText.color = new Color(1, 0.12f, 0.16f, 1);
                            ScoreText.fontSize = 35;
                        }

                        else if (score >= 500)
                        {
                            ScoreText.color = new Color(1, 0.32f, 0.16f, 1);
                            ScoreText.fontStyle = FontStyle.BoldAndItalic;
                            ScoreText.fontSize = 30;
                        }

                        else if (score >= 300)
                        {
                            ScoreText.color = new Color(1, 0.52f, 0.16f, 1);
                            ScoreText.fontStyle = FontStyle.Bold;
                            ScoreText.fontSize = 25;
                        }

                        else if (score >= 100)
                        {
                            ScoreText.color = new Color(1, 0.72f, 0.16f, 1);
                            ScoreText.fontSize = 22;
                        }

                        else if (score >= 50)
                            ScoreText.color = new Color(1, 1, 0.16f, 1);

                        foreach (float i in deleteList)
                        {
                            y = i;

                            for (x = -halfWidth; x < halfWidth; ++x)
                                Destroy((GameObject)((Hashtable)blocks[y])[x]);
                            
                            blocks.Remove(y);
                        }


                        for (y = (float)deleteList[0] + 1; y < -halfHeight + height; y++)
                        {
                            downCount = 0;

                            if (blocks.ContainsKey(y))
                            {
                                foreach(float j in deleteList)
                                {
                                    if (y > j)
                                        downCount++;
                                }

                                foreach (float j in ((Hashtable)blocks[y]).Keys)
                                {
                                    x = j;

                                    gameObject = (GameObject)((Hashtable)blocks[y])[x];
                                    gameObject.transform.position += new Vector3(0, -downCount);

                                    if (!blocks.ContainsKey(y - downCount))
                                        blocks[y - downCount] = new Hashtable();
                                    ((Hashtable)blocks[y - downCount])[x] = gameObject;
                                }
                                
                                blocks.Remove(y);
                            }
                        }
                    }

                    CreateTetromino();
                }

                curLife = life;
            }

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
                {
                    if (keyType != 0)
                        keyType = 0;
                }

                else
                {
                    if (Input.GetKey(KeyCode.DownArrow))
                        keyType = -2;
                    else
                        keyType = Input.GetKey(KeyCode.LeftArrow) ? -1 : 1;
                }

                if (keyType != 0)
                {
                    if (prevKeyType == keyType)
                    {
                        if (keyTime == 0)
                        {
                            if (keyType != -2)
                                move(keyType);

                            else
                            {
                                if (moveDown())
                                    curLife = life;
                                else
                                    curLife = 0;
                            }

                            keyTime = 6;
                        }
                    }

                    else
                    {
                        prevKeyType = keyType;

                        if (keyType != -2)
                            move(keyType);

                        else
                        {
                            if (moveDown())
                                curLife = life;
                            else
                                curLife = 0;
                        }
                        keyTime = 30;
                    }

                    if (keyTime > 0)
                        keyTime--;
                }
            }

            else
            {
                if (prevKeyType != 0)
                {
                    prevKeyType = 0;
                    keyTime = 30;
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
                rotate();

            else if (Input.GetKeyDown(KeyCode.Space))
            {
                while (moveDown()) { };
                curLife = 0;
            }
        }

        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
                restart();
        }
    }

    private void initVariable()
    {
        //Colors Initializing
        colors.Add(new Color32(115, 251, 253, 255));    //Light Blue
        colors.Add(new Color32(0, 33, 245, 255));       //Blue
        colors.Add(new Color32(243, 168, 59, 255));     //Orange
        colors.Add(new Color32(255, 253, 84, 255));     //Yellow
        colors.Add(new Color32(117, 250, 76, 255));     //Green
        colors.Add(new Color32(155, 47, 246, 255));     //Purple
        colors.Add(new Color32(235, 51, 35, 255));      //Red

        //Vectors Initializing
        for (int i = 0; i < 7; i++)
            vectors.Add(new Vector3[4]);

        //Line Block
        ((Vector3[])vectors[0])[0] = new Vector3(-2.0f, 0.0f);
        ((Vector3[])vectors[0])[1] = new Vector3(-1.0f, 0.0f);
        ((Vector3[])vectors[0])[2] = new Vector3(0.0f, 0.0f);
        ((Vector3[])vectors[0])[3] = new Vector3(1.0f, 0.0f);

        //Left Chair Block
        ((Vector3[])vectors[1])[0] = new Vector3(-1.0f, 1.0f);
        ((Vector3[])vectors[1])[1] = new Vector3(-1.0f, 0.0f);
        ((Vector3[])vectors[1])[2] = new Vector3(0.0f, 0.0f);
        ((Vector3[])vectors[1])[3] = new Vector3(1.0f, 0.0f);

        //Right Chair Block
        ((Vector3[])vectors[2])[0] = new Vector3(1.0f, 1.0f);
        ((Vector3[])vectors[2])[1] = new Vector3(-1.0f, 0.0f);
        ((Vector3[])vectors[2])[2] = new Vector3(0.0f, 0.0f);
        ((Vector3[])vectors[2])[3] = new Vector3(1.0f, 0.0f);

        //Square Block
        ((Vector3[])vectors[3])[0] = new Vector3(-1.0f, 1.0f);
        ((Vector3[])vectors[3])[1] = new Vector3(0.0f, 1.0f);
        ((Vector3[])vectors[3])[2] = new Vector3(-1.0f, 0.0f);
        ((Vector3[])vectors[3])[3] = new Vector3(0.0f, 0.0f);

        //Right Stair Block
        ((Vector3[])vectors[4])[0] = new Vector3(-1.0f, 0.0f);
        ((Vector3[])vectors[4])[1] = new Vector3(0.0f, 0.0f);
        ((Vector3[])vectors[4])[2] = new Vector3(0.0f, 1.0f);
        ((Vector3[])vectors[4])[3] = new Vector3(1.0f, 1.0f);

        //Center Chair Block
        ((Vector3[])vectors[5])[0] = new Vector3(-1.0f, 0.0f);
        ((Vector3[])vectors[5])[1] = new Vector3(0.0f, 0.0f);
        ((Vector3[])vectors[5])[2] = new Vector3(0.0f, 1.0f);
        ((Vector3[])vectors[5])[3] = new Vector3(1.0f, 0.0f);

        //Left Stair Block
        ((Vector3[])vectors[6])[0] = new Vector3(-1.0f, 1.0f);
        ((Vector3[])vectors[6])[1] = new Vector3(0.0f, 0.0f);
        ((Vector3[])vectors[6])[2] = new Vector3(0.0f, 1.0f);
        ((Vector3[])vectors[6])[3] = new Vector3(1.0f, 0.0f);
    }

    Tile CreateTile(Transform parent, Vector3 position, Color32 Color32, int order = 1)
    {
        GameObject gameObject = Instantiate(tilePrefab);
        gameObject.transform.parent = parent;
        gameObject.transform.localPosition = position;

        Tile tile = gameObject.GetComponent<Tile>();
        tile.color = Color32;
        tile.sortingOrder = order;

        return tile;
    }

    private void CreateBackground()
    {
        Color color = Color.gray;
        color.a = 0.5f;

        for (int x = -halfWidth; x < halfWidth; ++x)
        {
            for (int y = -halfHeight; y < halfHeight; ++y)
                CreateTile(BackgroundNode, new Vector3(x, y), color, 0);
        }

        CreateOutline();
    }

    private void CreateOutline()
    {
        Color color = new Color(0.7f, 0.7f, 0.7f);
        color.a = 0.5f;

        for (int y = -halfHeight - 1; y < halfHeight; ++y)
        {
            CreateTile(BackgroundNode, new Vector3(-halfWidth - 1, y), color, 0);
            CreateTile(BackgroundNode, new Vector3(halfWidth, y), color, 0);
        }

        for (int x = -halfWidth; x < halfWidth; ++x)
            CreateTile(BackgroundNode, new Vector3(x, -halfHeight - 1), color, 0);
    }

    private void CreateTetromino()
    {
        TetrominoNode.rotation = Quaternion.identity;
        TetrominoNode.position = new Vector3(offset_x, halfHeight + offset_y);

        Color32 color = (Color32)colors[nextIndex];
        for (int i = 0; i < 4; i++)
            CreateTile(TetrominoNode, ((Vector3[])vectors[nextIndex])[i], color);

        CreateNextTetromino();
        curLife = life;
    }

    private void CreateNextTetromino()
    {
        for (int i = 0; i < 4; i++)
            Destroy(NextTetrominoNode.GetChild(i).gameObject);

        nextIndex = Random.Range(0, 7);

        Color32 color = (Color32)colors[nextIndex];
        for (int i = 0; i < 4; i++)
            CreateTile(NextTetrominoNode, ((Vector3[])vectors[nextIndex])[i], color);
    }

    private void move(int xMove)
    {
        if (canMove(xMove))
            TetrominoNode.position += new Vector3(xMove, 0);
    }

    private bool moveDown()
    {
        if (canMoveDown())
        {
            TetrominoNode.position += new Vector3(0, -1);
            return true;
        }

        return false;
    }

    private void rotate()
    {
        if(canRotate())
            TetrominoNode.Rotate(new Vector3(0, 0, 90));
    }

    private bool canMove(int xMove)
    {
        Transform child;
        float x, y;

        for (int i = 0; i < 4; i++)
        {
            child = TetrominoNode.GetChild(i);
            x = Mathf.Round(child.position.x);
            y = Mathf.Round(child.position.y);

            if (blocks.ContainsKey(y) && ((Hashtable)blocks[y]).ContainsKey(x + xMove))
                return false;

            else if (xMove == -1)
            {
                if (x < -halfWidth + 1)
                    return false;
            }

            else
            {
                if (x >= halfWidth - 1)
                    return false;
            }
        }

        return true;
    }

    private bool canMoveDown()
    {
        Transform child;
        float x, y;

        for (int i = 0; i < 4; i++)
        {
            child = TetrominoNode.GetChild(i);
            x = Mathf.Round(child.position.x);
            y = Mathf.Round(child.position.y);

            if (y <= -halfHeight || (blocks.ContainsKey(y - 1) && ((Hashtable)blocks[y - 1]).ContainsKey(x)))
                return false;
        }

        return true;
    }

    private bool canRotate()
    {
        bool flag = true;
        GameObject[] objects = new GameObject[4];
        Vector3[] prevLoc = new Vector3[4];

        for (int i = 0; i < 4; i++)
        {
            objects[i] = TetrominoNode.GetChild(i).gameObject;
            prevLoc[i] = objects[i].transform.position;
        }

        TetrominoNode.Rotate(new Vector3(0, 0, 90));

        float x, y;
        float xChange, yChange;
        int increase;
        for(int i = 0; i < 4; i++)
        {
            xChange = Mathf.Round(objects[i].transform.position.x) - Mathf.Round(prevLoc[i].x);
            yChange = Mathf.Round(objects[i].transform.position.y) - Mathf.Round(prevLoc[i].y);

            increase = yChange > 0 ? 1 : -1;
            if (yChange != 0)
            {
                for(int j = 1; j <= Mathf.Abs(yChange); j += Mathf.Abs(increase))
                {
                    prevLoc[i] = new Vector3(Mathf.Round(prevLoc[i].x), Mathf.Round(prevLoc[i].y) + increase);
                    x = prevLoc[i].x;
                    y = prevLoc[i].y;

                    if (y < -halfHeight || (blocks.ContainsKey(y) && ((Hashtable)blocks[y]).ContainsKey(x)))
                    {
                        flag = false;
                        break;
                    }
                }

                if (!flag)
                    break;
            }

            increase = xChange > 0 ? 1 : -1;
            if (xChange != 0)
            {
                for (int j = 1; j <= Mathf.Abs(xChange); j += Mathf.Abs(increase))
                {
                    prevLoc[i] = new Vector3(Mathf.Round(prevLoc[i].x) + increase, Mathf.Round(prevLoc[i].y));
                    x = prevLoc[i].x;
                    y = prevLoc[i].y;

                    if (x < -halfWidth || x >= halfWidth || (blocks.ContainsKey(y) && ((Hashtable)blocks[y]).ContainsKey(x)))
                    {
                        flag = false;
                        break;
                    }
                }

                if (!flag)
                    break;
            }
        }

        TetrominoNode.Rotate(new Vector3(0, 0, -90));
        return flag;
    }

    private void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
