using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Sprite bodySprite;
    public Color snakeBodyColor;
    public Sprite foodImage;
    public Color foodColor;

    public TMPro_Text scoreText;
    
    public float updateTime = 1f;
    public float minBodySize = .2f, maxBodySize = .7f, HeadSize = 1f;
    
    public Vector2Int mapSize = new Vector2Int(16, 16); 

    int playerMovementDir;
    int currentScore;
    float currentUpdateTimer;
    float timer;
    bool gameover = false;

    List<GameObject> snakeBodyObjects = new List<GameObject>();
    GameObject foodObject;
    Vector2 EndPosition, lastHeadPos;

    private void Start()
    {
        Init();

        Camera.main.orthographicSize = ((float)mapSize.x / 2) + 1;
        Camera.main.transform.position = new Vector3((float)mapSize.x / 2, (float)mapSize.y / 2, -1);
    }

    void Init()
    {
        playerMovementDir = 0;
        currentUpdateTimer = updateTime;
        currentScore = 0;
        scoreText.text = currentScore.ToString().PadLeft(5, '0');
        
        //Spawn player
        Vector2 mapMid = new Vector2(Mathf.Floor(mapSize.x / 2), Mathf.Floor(mapSize.y / 2));
        SpawnBodyPart(mapMid.x, mapMid.y);
        EndPosition = new Vector2(mapMid.x + 1, mapMid.y);

        //Setup food
        if (foodObject == null)
        {
            foodObject = new GameObject();
            foodObject.name = "Food";
            SpriteRenderer foodRen = foodObject.AddComponent<SpriteRenderer>();
            foodRen.sprite = foodImage;
            foodRen.color = foodColor;
        }
        
        UpdateFood();
    }

    private void Update()
    {
        if (gameover)
        {
            if (Input.anyKeyDown)
            {
                ReStart();
                gameover = false;
            }
            return;
        }

        //Independent on timer
        CheckInput();

        //Timer, essentially characters movement speed
        timer -= Time.deltaTime;
        if (timer > 0) return; //Return out of the code.
        
        //reset timer if timer is below 0 
        timer = currentUpdateTimer;

        //Move objects into next position
        UpdateBodyParts();

        switch (CheckPlayerPosition())
        {
            case 0: //nothing
                break;

            case 1: //food
                UpdateFood();
                EatFood();
                break;

            case 2: //Self, gameover
                GameOver();
                break;
        }
    }

    void UpdateBodySize()
    {
        for (int i = snakeBodyObjects.Count; i-- > 0;)
        {
            if (i == 0) //Set head size
            { 
                snakeBodyObjects[i].transform.localScale = new Vector3(headSize, headSize, 0);
            }
            else //Set Body Size
            {
                float scaleSize = ((maxBodySize - minBodySize) / ((float)snakeBodyObjects.Count - 1)) + minBodySize;
                snakeBodyObjects[i].transform.localScale = new Vector3(scaleSize, scaleSize, 0);
            }
        }
    }

    void UpdateBodyParts()
    {
        EndPosition = snakeBodyObjects[snakeBodyObjects.Count - 1].transform.position;
        lastHeadPos = snakeBodyObjects[0].transform.position;

        UpdateBodySize();

        for (int i = snakeBodyObjects.Count; i-- > 0;)
        {
            //Body            
            if (i > 1)
            {
                //Set position to next in List
                snakeBodyObjects[i].transform.position = snakeBodyObjects[i - 1].transform.position;
            }
            else if (i == 1)
            {
                snakeBodyObjects[i].transform.position = lastHeadPos;
            }
            else
            {
                //Move head and "neck"
                switch (playerMovementDir)
                {
                    case 0:
                        snakeBodyObjects[i].transform.position += new Vector3(-1, 0);
                        break;
                    case 1:
                        snakeBodyObjects[i].transform.position += new Vector3(0, 1, 0);
                        break;
                    case 2:
                        snakeBodyObjects[i].transform.position += new Vector3(1, 0, 0);
                        break;
                    case 3:
                        snakeBodyObjects[i].transform.position += new Vector3(0, -1, 0);
                        break;
                }
            }

            //Repeating of the map
            #region MapRepeating
            //Teleport to other side when outside of mapSize
            if (snakeBodyObjects[i].transform.position.x > mapSize.x) //If more than size, on x
            {
                snakeBodyObjects[i].transform.position = new Vector3(0, snakeBodyObjects[i].transform.position.y, 0); ;
            }
            else if (snakeBodyObjects[i].transform.position.x < 0) //If less than 0, on x
            {
                snakeBodyObjects[i].transform.position = new Vector3(mapSize.x, snakeBodyObjects[i].transform.position.y, 0); ;
            }

            if (snakeBodyObjects[i].transform.position.y > mapSize.y) //If more than size, on y
            {
                snakeBodyObjects[i].transform.position = new Vector3(snakeBodyObjects[i].transform.position.x, 0, 0); ;
            }
            else if (snakeBodyObjects[i].transform.position.y < 0) //If less than 0, on y
            {
                snakeBodyObjects[i].transform.position = new Vector3(snakeBodyObjects[i].transform.position.x, mapSize.y, 0); ;
            }
            #endregion
        }
    }

    void CheckInput()
    {
        //I did this this way to not allow diagonal movement, and it should "remember" last input so to speak
        //Also cant move in opposite direction
        if (Input.GetKeyDown(KeyCode.LeftArrow) && playerMovementDir != 2) playerMovementDir = 0;
        if (Input.GetKeyDown(KeyCode.UpArrow) && playerMovementDir != 3) playerMovementDir = 1;
        if (Input.GetKeyDown(KeyCode.RightArrow) && playerMovementDir != 0) playerMovementDir = 2;
        if (Input.GetKeyDown(KeyCode.DownArrow) && playerMovementDir != 1) playerMovementDir = 3;
    }

    int CheckPlayerPosition()
    {
        //Check food
        if (snakeBodyObjects[0].transform.position == foodObject.transform.position)
        {
            print("Hit food!");
            return 1;
        }

        //Check if hit self
        if (snakeBodyObjects.Count > 2)
        {
            for (int i = 2; i < snakeBodyObjects.Count; i++)
            {
                if (snakeBodyObjects[i].transform.position == snakeBodyObjects[0].transform.position)
                {
                    print("Hit Self! :C");
                    return 2;
                }
            }
        }
        //return 0 if nothing
        return 0;
    }

    void UpdateFood()
    {
        //Move food to new location, Random, can spawn inside player
        foodObject.transform.position = new Vector2((int)Random.Range(0, mapSize.x), (int)Random.Range(0, mapSize.y));
    }

    void EatFood()
    {
        SpawnBodyPart(EndPosition);
        UpdateBodySize();
        
        currentScore += 10;
        scoreText.text = currentScore.ToString().PadLeft(5, '0')


        //Man i do NOT rember what this whole section is for, i will figure it out!
        if (currentUpdateTimer > updateTime * .66f)
        {
            currentUpdateTimer = currentUpdateTimer * .9f;
        }
        else if (currentUpdateTimer <= 0.1f)
            return;
        else
        {
            currentUpdateTimer = currentUpdateTimer * .95f;
        }
    }

    void SpawnBodyPart(Vector2 position)
    {
        GameObject playerObj = new GameObject();
        playerObj.transform.position = position;
        playerObj.name = "Snake Part " + snakeBodyObjects.count + 1;

        SpriteRenderer spriteRen = playerObj.AddComponent<SpriteRenderer>();
        spriteRen.sprite = bodySprite;
        spriteRen.color = snakeBodyColor;

        snakeBodyObjects.Add(playerObj);
    }

    void ReStart()
    {
        for (int i = snakeBodyObjects.Count; i-- > 0;)
        {
            Destroy(snakeBodyObjects[i]);
            snakeBodyObjects.Remove(snakeBodyObjects[i]);
        }

        Init();
    }

    void GameOver()
    {
        gameover = true;
    }
}
