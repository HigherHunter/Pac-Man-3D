using UnityEngine;

public class PacmanMovement : MonoBehaviour
{

    public static PacmanMovement instance;

    private void Awake()
    {
        instance = this;
    }

    public float movementSpeed, frightenedMoveSpeed = 6f, regularMoveSpeed = 7f;

    public bool canMove = true;

    public Vector3 orientation;

    private Vector3 direction, nextDirection;

    public Nodes currentNode;

    private Nodes previousNode, targetNode, startingNode;

    private Vector3 startingPosition;

    private GameBoard board;

    public AudioClip nom1, nom2, nomBonus;

    private AudioSource audioSource;

    private Animator animator;

    private bool playerNom1 = false, spawnedBonusItem1 = false, spawnedBonusItem2 = false;

    public int totalPellets = 0, maximumPellets = 244, pelletCounter = 0, ghostPriority = 1;

    public float eatingTimer = 0;

    // Use this for initialization
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        animator = GetComponent<Animator>();

        board = GameObject.Find("Game").GetComponent<GameBoard>();

        startingNode = currentNode;

        startingPosition = transform.position;

        SetLevelDifficulity();

        orientation = Vector3.left;
        ChangePosition(Vector3.left);

    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            CheckInput();

            Move();

            UpdateOrientation();

            ConsumePellet();

            UpdateAnimation();

            eatingTimer += Time.deltaTime;
            Mathf.RoundToInt(eatingTimer);
        }
    }

    void SetLevelDifficulity()
    {
        if (GameBoard.currentLevel == 1)
        {
            // move speed 80%
            movementSpeed = regularMoveSpeed * 80 / 100;

            //frightened move speed 90%
            frightenedMoveSpeed = regularMoveSpeed * 90 / 100;
        }
        else if (GameBoard.currentLevel >= 2 && GameBoard.currentLevel <= 4)
        {
            // move speed 90%
            movementSpeed = regularMoveSpeed * 90 / 100;

            //frightened move speed 95%
            frightenedMoveSpeed = regularMoveSpeed * 95 / 100;
        }
        else if (GameBoard.currentLevel >= 5 && GameBoard.currentLevel <= 20)
        {
            // move speed 100%
        }
        else if (GameBoard.currentLevel >= 21)
        {
            // move speed 90%
            movementSpeed = regularMoveSpeed * 90 / 100;
        }
    }

    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangePosition(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangePosition(Vector3.right);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangePosition(Vector3.forward);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangePosition(Vector3.back);
        }
    }

    void UpdateOrientation()
    {
        if (direction == Vector3.left)
        {
            orientation = Vector3.left;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction == Vector3.right)
        {
            orientation = Vector3.right;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (direction == Vector3.forward)
        {
            orientation = Vector3.forward;
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (direction == Vector3.back)
        {
            orientation = Vector3.back;
            transform.rotation = Quaternion.Euler(0, 270, 0);
        }
    }

    void UpdateAnimation()
    {
        if (direction == Vector3.zero)
        {
            animator.enabled = false;
        }
        else
        {
            animator.enabled = true;
        }
    }

    void ChangePosition(Vector3 dir)
    {
        if (dir != direction)
        {
            nextDirection = dir;
        }

        if (currentNode != null)
        {
            Nodes moveToNode = CanMove(dir);

            if (moveToNode != null)
            {
                direction = dir;
                targetNode = moveToNode;
                previousNode = currentNode;
                currentNode = null;
            }
        }
    }

    void Move()
    {
        if (targetNode != currentNode && targetNode != null)
        {
            if (nextDirection == direction * -1)
            {
                direction *= -1;

                Nodes tempNode = targetNode;

                targetNode = previousNode;

                previousNode = tempNode;
            }

            if (OverShotTarget())
            {
                currentNode = targetNode;

                transform.localPosition = currentNode.transform.position;

                GameObject otherPortal = GetPortal();

                if (otherPortal != null)
                {
                    transform.localPosition = otherPortal.transform.position;
                    currentNode = otherPortal.GetComponent<Nodes>();
                }

                Nodes moveToNode = CanMove(nextDirection);

                if (moveToNode != null)
                    direction = nextDirection;

                if (moveToNode == null)
                    moveToNode = CanMove(direction);

                if (moveToNode != null)
                {
                    targetNode = moveToNode;
                    previousNode = currentNode;
                    currentNode = null;
                }
                else
                {
                    direction = Vector3.zero;
                }
            }
            else
            {
                transform.localPosition += direction * movementSpeed * Time.deltaTime;
            }
        }
    }

    void ConsumePellet()
    {
        GameObject o = GetTileAtPosition(transform.position);

        if (o != null)
        {
            Tile tile = o.GetComponent<Tile>();

            if (!tile.didConsume && (tile.isPellet || tile.isSuperPellet))
            {
                o.GetComponent<MeshRenderer>().enabled = false;
                tile.didConsume = true;

                if (tile.isSuperPellet)
                    GameBoard.score += 50;
                else
                    GameBoard.score += 10;

                PlayNomSound();

                eatingTimer = 0;

                totalPellets++;
                pelletCounter++;

                if (totalPellets >= 70 && totalPellets < 170 && !spawnedBonusItem1)
                {
                    GameBoard.instance.SpawnBonusItem();
                    spawnedBonusItem1 = true;
                }
                else if (totalPellets >= 170 && !spawnedBonusItem2)
                {
                    GameBoard.instance.SpawnBonusItem();
                    spawnedBonusItem2 = true;
                }

                if (totalPellets == maximumPellets)
                {
                    GameBoard.instance.PlayerWin();
                }

                if (tile.isSuperPellet)
                {
                    //after level 19 there is no frightened mode
                    if (GameBoard.currentLevel < 19)
                    {
                        movementSpeed = frightenedMoveSpeed;

                        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

                        foreach (GameObject go in ghosts)
                        {
                            go.GetComponent<Ghost>().StartFrightenedMode();
                        }
                    }
                }

            }
            else if (tile.isBonusItem)
            {
                audioSource.PlayOneShot(nomBonus);
                GameBoard.score += tile.fruitValue;
                GameBoard.instance.StartConsumedBonusItem(tile.gameObject);
                Destroy(tile.gameObject);
            }
        }
    }

    public void Restart()
    {
        canMove = true;

        pelletCounter = 0;

        eatingTimer = 0;

        transform.GetComponent<Animator>().Play("Eating");

        transform.position = startingPosition;

        currentNode = startingNode;

        transform.localRotation = Quaternion.Euler(0, 0, 0);

        orientation = Vector3.left;

        ChangePosition(Vector3.left);
    }

    void PlayNomSound()
    {
        if (playerNom1)
        {
            audioSource.PlayOneShot(nom2);
            playerNom1 = false;
        }
        else
        {
            audioSource.PlayOneShot(nom1);
            playerNom1 = true;
        }
    }

    GameObject GetTileAtPosition(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);

        GameObject tile = board.board[x, z];

        if (tile != null)
            return tile;

        return null;
    }

    Nodes CanMove(Vector3 pos)
    {
        Nodes moveToNode = null;

        for (int i = 0; i < currentNode.neighbors.Length; i++)
        {
            if (currentNode.validDirections[i] == pos)
            {
                moveToNode = currentNode.neighbors[i];
                break;
            }
        }
        return moveToNode;
    }

    bool OverShotTarget()
    {
        float nodeToTarget = LengthFromNode(targetNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.localPosition);

        return nodeToSelf > nodeToTarget;
    }

    float LengthFromNode(Vector3 targetPosition)
    {
        Vector3 vec = targetPosition - previousNode.transform.position;
        return vec.sqrMagnitude;
    }

    GameObject GetPortal()
    {
        if (currentNode.GetComponent<Tile>() != null)
        {
            if (currentNode.GetComponent<Tile>().isPortal)
            {
                GameObject otherPortal = currentNode.GetComponent<Tile>().portalReceiver;
                return otherPortal;
            }
        }
        return null;
    }

    public void changeMoveSpeed()
    {
        SetLevelDifficulity();
    }
}
