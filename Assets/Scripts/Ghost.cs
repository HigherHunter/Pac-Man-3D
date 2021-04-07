using System.Collections;
using UnityEngine;

public class Ghost : MonoBehaviour
{

    public float moveSpeed, levelMoveSpeed;
    public float frightenedMoveSpeed;
    public float regularMoveSpeed = 7f;
    public float consumedMoveSpeed = 10f;
    public float tunnelSpeed;

    public bool canMove = true;

    public int pinkyReleaseTimer = 7;
    public int inkyReleaseTimer = 17;
    public int clydeReleaseTimer = 32;
    public float ghostReleaseTimer = 0;

    public int inkyPelletTimer = 30;
    public int clydePelletTimer = 90;

    public bool isInGhostHouse;

    public Nodes homeNode;
    public Nodes ghostHouse;
    private Nodes startingNode;
    private Vector3 startingPosition;

    public int scatterModeTimer1 = 7;
    public int chaseModeTimer1 = 20;
    public int scatterModeTimer2 = 7;
    public int chaseModeTimer2 = 20;
    public int scatterModeTimer3 = 5;
    public int chaseModeTimer3 = 20;
    public int scatterModeTimer4 = 5;

    private int modeChangeIteration = 1;
    private float modeChangeTimer = 0;

    private int frightenedModeDuration = 6;
    private float frightenedModeTimer = 0;
    private int blinkingStart = 4;
    private float blinkTimer = 0;
    private bool blinkWhite;

    public bool useGlobalTimer = false;

    public enum GhostType
    {
        Red, Pink, Blue, Orange
    }

    public GhostType ghostType = GhostType.Red;

    public enum Mode
    {
        Chase, Scatter, Frightened, Consumed
    }

    Mode currentMode = Mode.Scatter;
    Mode previousMode;

    private GameObject pacMan;

    public Nodes currentNode;

    private Nodes targetNode, previousNode;

    private Vector3 direction, startPlace;

    public Material baseMat, frightenedBlueMat, frightenedRedMat, frightenedWhiteMat;

    // Use this for initialization
    void Start()
    {
        pacMan = GameObject.FindGameObjectWithTag("Pacman");

        baseMat = transform.GetChild(0).GetComponent<MeshRenderer>().material;

        startingPosition = transform.position;
        startingNode = currentNode;

        startPlace = transform.position;

        SetLevelDifficulity();

        if (isInGhostHouse)
        {
            if (transform.name.Equals("Pinky"))
            {
                direction = Vector3.forward;

            }
            else if (transform.name.Equals("Inky"))
            {
                direction = Vector3.right;

            }
            else if (transform.name.Equals("Clyde"))
            {
                direction = Vector3.left;
            }

            targetNode = currentNode.neighbors[0];
        }
        else
        {
            direction = Vector3.left;
            targetNode = ChooseNextNode();
        }

        previousNode = currentNode;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {

            ModeUpdate();

            Move();

            UpdateOrientation();

            ReleaseGhosts();

            CheckCollision();

            CheckIsInGhostHouse();
        }
    }

    void SetLevelDifficulity()
    {
        if (GameBoard.currentLevel == 2)
        {
            inkyPelletTimer = 0;
            clydePelletTimer = 50;
        }
        else if (GameBoard.currentLevel >= 3)
        {
            inkyPelletTimer = 0;
            clydePelletTimer = 0;
        }

        if (GameBoard.currentLevel == 1)
        {
            //move speed 75%
            levelMoveSpeed = regularMoveSpeed * 75 / 100;
            moveSpeed = levelMoveSpeed;
            //frightened move speed 50%
            frightenedMoveSpeed = regularMoveSpeed * 50 / 100;
            //tunnel move speed 40%
            tunnelSpeed = regularMoveSpeed * 40 / 100;
        }
        else if (GameBoard.currentLevel >= 2 && GameBoard.currentLevel <= 4)
        {
            //mode timer
            //rest is the same
            chaseModeTimer3 = 1033;
            scatterModeTimer4 = 1;

            //speed 85%
            levelMoveSpeed = regularMoveSpeed * 85 / 100;
            moveSpeed = levelMoveSpeed;
            //frightened move speed 55%
            frightenedMoveSpeed = regularMoveSpeed * 55 / 100;

            //tunnel move speed 45%
            tunnelSpeed = regularMoveSpeed * 45 / 100;

            //frightened time
            frightenedModeDuration = 5;
            blinkingStart = 3;
        }
        else if (GameBoard.currentLevel >= 5)
        {
            if (GameBoard.currentLevel >= 5 && GameBoard.currentLevel <= 10)
            {
                //frightened time
                frightenedModeDuration = 3;
                blinkingStart = 1;
            }
            else
            {
                //frightened time
                frightenedModeDuration = 1;
            }
            //mode timer
            scatterModeTimer1 = 5;
            scatterModeTimer2 = 5;
            scatterModeTimer3 = 5;
            chaseModeTimer3 = 1037;
            scatterModeTimer4 = 1;

            //speed 95%
            levelMoveSpeed = regularMoveSpeed * 95 / 100;
            moveSpeed = levelMoveSpeed;
            //frightened move speed 60%
            frightenedMoveSpeed = regularMoveSpeed * 60 / 100;

            //tunnel move speed 40%
            tunnelSpeed = regularMoveSpeed * 50 / 100;
        }
    }

    void CheckIsInGhostHouse()
    {
        if (currentMode == Mode.Consumed)
        {
            GameObject tile = GetTileAtPosition(transform.position);

            if (tile != null)
            {
                if (tile.transform.GetComponent<Tile>() != null)
                {
                    if (tile.transform.GetComponent<Tile>().isHome)
                    {
                        moveSpeed = levelMoveSpeed;
                        RegularModeChange();

                        Nodes node = GetNodeAtPosition(transform.position);

                        if (node != null)
                        {
                            currentNode = node;

                            direction = Vector3.forward;
                            targetNode = currentNode.neighbors[0];

                            previousNode = currentNode;

                            ChangeMode(Mode.Chase);
                        }
                    }
                }
            }
        }
    }

    public void Restart()
    {
        canMove = true;

        useGlobalTimer = true;

        RegularModeChange();

        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        transform.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
        transform.GetChild(4).GetComponent<MeshRenderer>().enabled = true;
        transform.GetChild(5).GetComponent<MeshRenderer>().enabled = true;

        currentMode = Mode.Scatter;

        transform.position = startingPosition;

        ghostReleaseTimer = 0;
        modeChangeIteration = 1;
        modeChangeTimer = 0;

        currentNode = startingNode;

        if (transform.name != "Blinky")
            isInGhostHouse = true;

        if (isInGhostHouse)
        {
            if (transform.name.Equals("Pinky"))
            {
                direction = Vector3.forward;
                transform.position = startPlace;
            }
            else if (transform.name.Equals("Inky"))
            {
                direction = Vector3.right;

            }
            else if (transform.name.Equals("Clyde"))
            {
                direction = Vector3.left;
            }
            targetNode = currentNode.neighbors[0];
        }
        else
        {
            direction = Vector3.left;
            targetNode = ChooseNextNode();
        }

        previousNode = currentNode;
    }

    void UpdateOrientation()
    {
        if (direction == Vector3.left)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);

        }
        else if (direction == Vector3.right)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);

        }
        else if (direction == Vector3.forward)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);

        }
        else if (direction == Vector3.back)
        {
            transform.rotation = Quaternion.Euler(0, 270, 0);
        }
    }

    void Move()
    {
        if (targetNode != currentNode && targetNode != null && !isInGhostHouse)
        {
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

                targetNode = ChooseNextNode();

                if (targetNode.GetComponent<Tile>().isPortal)
                {
                    moveSpeed = tunnelSpeed;
                    StartCoroutine("TunnelMovement", 2);
                }

                previousNode = currentNode;

                currentNode = null;
            }
            else
            {
                transform.localPosition += direction * moveSpeed * Time.deltaTime;
            }
        }
    }

    void ModeUpdate()
    {
        if (currentMode != Mode.Frightened)
        {
            moveSpeed = levelMoveSpeed;
            modeChangeTimer += Time.deltaTime;

            if (modeChangeIteration == 1)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer1)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                else if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer1)
                {
                    modeChangeIteration = 2;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            }
            else if (modeChangeIteration == 2)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer2)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                else if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer2)
                {
                    modeChangeIteration = 3;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }

            }
            else if (modeChangeIteration == 3)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer3)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                else if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer3)
                {
                    modeChangeIteration = 4;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }

            }
            else if (modeChangeIteration == 4)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer4)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
            }
        }
        else if (currentMode == Mode.Frightened)
        {
            moveSpeed = frightenedMoveSpeed;

            FrightenedModeChange();

            frightenedModeTimer += Time.deltaTime;

            if (frightenedModeTimer >= frightenedModeDuration)
            {
                GameBoard.instance.PlayNormalMusic();
                frightenedModeTimer = 0;
                ChangeMode(previousMode);
                RegularModeChange();
                PacmanMovement.instance.changeMoveSpeed();
            }

            if (frightenedModeTimer >= blinkingStart)
            {
                blinkTimer += Time.deltaTime;

                if (blinkTimer >= 0.1f)
                {
                    blinkTimer = 0f;

                    if (blinkWhite)
                    {
                        FrightenedModeChange();
                        blinkWhite = false;
                    }
                    else
                    {
                        EndingFrightenedChange();
                        blinkWhite = true;
                    }
                }
            }
        }
    }

    void ChangeMode(Mode m)
    {
        if (currentMode != Mode.Frightened && currentMode != Mode.Consumed)
        {
            previousMode = currentMode;
        }
        currentMode = m;
    }

    public void StartFrightenedMode()
    {
        if (currentMode != Mode.Consumed)
        {
            GameBoard.instance.PlayFrightenedMusic();
            GameBoard.instance.consumedGhostComboScore = 0;

            frightenedModeTimer = 0;

            ChangeMode(Mode.Frightened);
        }
    }

    void StartConsumedMode()
    {
        currentMode = Mode.Consumed;
        moveSpeed = consumedMoveSpeed;
        ConsumedModeChange();

        if (GameBoard.instance.consumedGhostComboScore == 0)
        {
            GameBoard.instance.consumedGhostComboScore = 200;
        }
        else
        {
            GameBoard.instance.consumedGhostComboScore *= 2;
        }

        GameBoard.score += GameBoard.instance.consumedGhostComboScore;

        GameBoard.instance.StartConsumed(this.GetComponent<Ghost>());
    }

    void CheckCollision()
    {
        if (transform.GetComponent<CapsuleCollider>().bounds.Intersects(pacMan.transform.GetComponent<CapsuleCollider>().bounds))
        {
            if (currentMode == Mode.Frightened)
            {
                StartConsumedMode();
            }
            else if (currentMode != Mode.Consumed)
            {
                //pacman is dead
                GameBoard.instance.StartDeath();
            }
        }
    }

    void FrightenedModeChange()
    {
        //body to blue
        transform.GetChild(0).GetComponent<MeshRenderer>().material = frightenedBlueMat;
        //color left eyeball white
        transform.GetChild(1).GetComponent<MeshRenderer>().material = frightenedWhiteMat;
        //disable left iris
        transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
        //enable mouth
        transform.GetChild(3).GetComponent<MeshRenderer>().enabled = true;
        //color mouth white
        transform.GetChild(3).GetComponent<MeshRenderer>().material = frightenedWhiteMat;
        //color right eyeball white
        transform.GetChild(4).GetComponent<MeshRenderer>().material = frightenedWhiteMat;
        //disable right iris
        transform.GetChild(5).GetComponent<MeshRenderer>().enabled = false;
    }

    void RegularModeChange()
    {
        //enable body
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        //body to base material
        transform.GetChild(0).GetComponent<MeshRenderer>().material = baseMat;
        //color left eyeball white
        transform.GetChild(1).GetComponent<MeshRenderer>().material = frightenedWhiteMat;
        //enable left iris
        transform.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
        //disable mouth
        transform.GetChild(3).GetComponent<MeshRenderer>().enabled = false;
        //color right eyeball white
        transform.GetChild(4).GetComponent<MeshRenderer>().material = frightenedWhiteMat;
        //enable right iris
        transform.GetChild(5).GetComponent<MeshRenderer>().enabled = true;
    }

    void EndingFrightenedChange()
    {
        //body to white
        transform.GetChild(0).GetComponent<MeshRenderer>().material = frightenedWhiteMat;
        //color left eyeball to red
        transform.GetChild(1).GetComponent<MeshRenderer>().material = frightenedRedMat;
        //disable left iris
        transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
        //enable mouth
        transform.GetChild(3).GetComponent<MeshRenderer>().enabled = true;
        //color mouth to red
        transform.GetChild(3).GetComponent<MeshRenderer>().material = frightenedRedMat;
        //color rigth eyeball to red
        transform.GetChild(4).GetComponent<MeshRenderer>().material = frightenedRedMat;
        //disable right iris
        transform.GetChild(5).GetComponent<MeshRenderer>().enabled = false;
    }

    void ConsumedModeChange()
    {
        //disable body
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        //disable mouth
        transform.GetChild(3).GetComponent<MeshRenderer>().enabled = false;
        //color left eyeball to red
        transform.GetChild(1).GetComponent<MeshRenderer>().material = frightenedWhiteMat;
        //color rigth eyeball to red
        transform.GetChild(4).GetComponent<MeshRenderer>().material = frightenedWhiteMat;
    }

    Vector3 GetRedGhostTargetTile()
    {
        Vector3 targetTile = new Vector3(
            Mathf.RoundToInt(pacMan.transform.position.x),
            Mathf.RoundToInt(pacMan.transform.position.y),
            Mathf.RoundToInt(pacMan.transform.position.z));

        return targetTile;
    }

    Vector3 GetPinkGhostTargetTile()
    {
        //four tiles ahead of pacman
        //taking in account position and orientation
        Vector3 pacManOrientation = pacMan.GetComponent<PacmanMovement>().orientation;

        Vector3 pacManTile = new Vector3(
            Mathf.RoundToInt(pacMan.transform.position.x),
            Mathf.RoundToInt(pacMan.transform.position.y),
            Mathf.RoundToInt(pacMan.transform.position.z));

        Vector3 targetTile = pacManTile + (4 * pacManOrientation);

        return targetTile;
    }

    Vector3 GetBlueGhostTargetTile()
    {
        //select two tiles in front of pacman
        //draw vector from blinky to that pos
        //double the length of that vector
        Vector3 pacManOrientation = pacMan.GetComponent<PacmanMovement>().orientation;

        Vector3 pacManTile = new Vector3(
            Mathf.RoundToInt(pacMan.transform.position.x),
            Mathf.RoundToInt(pacMan.transform.position.y),
            Mathf.RoundToInt(pacMan.transform.position.z));

        Vector3 targetTile = pacManTile + (2 * pacManOrientation);

        //blinky pos
        Vector3 blinkyPos = GameObject.Find("Blinky").transform.position;
        blinkyPos = new Vector3(
            Mathf.RoundToInt(blinkyPos.x),
            Mathf.RoundToInt(blinkyPos.y),
            Mathf.RoundToInt(blinkyPos.z));

        float distance = GetDistance(blinkyPos, targetTile);
        distance *= 2;

        targetTile = new Vector3(blinkyPos.x + distance, blinkyPos.y + distance, blinkyPos.z + distance);

        return targetTile;
    }

    Vector3 GetOrangeGhostTargetTile()
    {
        //if distance from pacman > 8 behave as blinky
        //if distance from pacman < 8 go home

        float distance = GetDistance(transform.localPosition, pacMan.transform.position);
        Vector3 targetTile = Vector3.zero;

        if (distance > 8)
        {
            targetTile = new Vector3(
            Mathf.RoundToInt(pacMan.transform.position.x),
            Mathf.RoundToInt(pacMan.transform.position.y),
            Mathf.RoundToInt(pacMan.transform.position.z));
        }
        else if (distance < 8)
        {
            targetTile = homeNode.transform.position;
        }

        return targetTile;
    }

    Vector3 GetTargetTile()
    {
        Vector3 targetTile = Vector3.zero;

        if (ghostType == GhostType.Red)
        {
            targetTile = GetRedGhostTargetTile();

        }
        else if (ghostType == GhostType.Pink)
        {
            targetTile = GetPinkGhostTargetTile();

        }
        else if (ghostType == GhostType.Blue)
        {
            targetTile = GetBlueGhostTargetTile();

        }
        else if (ghostType == GhostType.Orange)
        {
            targetTile = GetOrangeGhostTargetTile();
        }

        return targetTile;
    }

    void ReleaseGhost()
    {
        isInGhostHouse = false;
    }

    void ReleaseGhosts()
    {
        if (isInGhostHouse)
        {
            if (ghostType == GhostType.Pink)
            {
                if (useGlobalTimer)
                {
                    if (pinkyReleaseTimer == PacmanMovement.instance.pelletCounter)
                    {
                        ReleaseGhost();
                        PacmanMovement.instance.ghostPriority = 2;
                    }
                }
                else
                {
                    ReleaseGhost();
                    PacmanMovement.instance.ghostPriority = 2;
                }
                if (PacmanMovement.instance.eatingTimer >= 4 && PacmanMovement.instance.ghostPriority == 1)
                {
                    ReleaseGhost();
                    PacmanMovement.instance.eatingTimer = 0;
                    PacmanMovement.instance.ghostPriority = 2;
                }
            }
            else if (ghostType == GhostType.Blue && isInGhostHouse)
            {
                if (useGlobalTimer)
                {
                    if (inkyReleaseTimer == PacmanMovement.instance.pelletCounter)
                    {
                        ReleaseGhost();
                        PacmanMovement.instance.ghostPriority = 3;
                    }
                }
                else
                {
                    if (inkyPelletTimer == PacmanMovement.instance.pelletCounter)
                    {
                        ReleaseGhost();
                        PacmanMovement.instance.ghostPriority = 3;
                    }
                }
                if (PacmanMovement.instance.eatingTimer >= 4 && PacmanMovement.instance.ghostPriority == 2)
                {
                    ReleaseGhost();
                    PacmanMovement.instance.eatingTimer = 0;
                    PacmanMovement.instance.ghostPriority = 3;
                }
            }
            else if (ghostType == GhostType.Orange && isInGhostHouse)
            {
                if (useGlobalTimer)
                {
                    if (clydeReleaseTimer == PacmanMovement.instance.pelletCounter)
                    {
                        ReleaseGhost();
                        PacmanMovement.instance.ghostPriority = 1;
                        useGlobalTimer = false;
                    }
                }
                else
                {
                    if (clydePelletTimer == PacmanMovement.instance.pelletCounter)
                    {
                        ReleaseGhost();
                        PacmanMovement.instance.ghostPriority = 1;
                    }
                }
                if (PacmanMovement.instance.eatingTimer >= 4 && PacmanMovement.instance.ghostPriority == 3)
                {
                    ReleaseGhost();
                    PacmanMovement.instance.eatingTimer = 0;
                    PacmanMovement.instance.ghostPriority = 1;
                }
            }
        }
    }

    Nodes ChooseNextNode()
    {

        Vector3 targetTile = Vector3.zero;

        if (currentMode == Mode.Chase)
        {
            targetTile = GetTargetTile();

        }
        else if (currentMode == Mode.Scatter)
        {
            targetTile = homeNode.transform.position;

        }
        else if (currentMode == Mode.Frightened)
        {
            targetTile = GetRandomTile();

        }
        else if (currentMode == Mode.Consumed)
        {
            targetTile = ghostHouse.transform.position;
        }

        Nodes moveToNode = null;

        Nodes[] foundNodes = new Nodes[4];
        Vector3[] foundDirection = new Vector3[4];

        int nodeCounter = 0;

        for (int i = 0; i < currentNode.neighbors.Length; i++)
        {
            if (currentNode.validDirections[i] != direction * -1)
            {
                if (currentMode != Mode.Consumed)
                {
                    GameObject tile = GetTileAtPosition(currentNode.transform.position);

                    if (tile.transform.GetComponent<Tile>().isHomeEntrance)
                    {
                        //its ghost house, no moving 
                        if (currentNode.validDirections[i] != Vector3.back)
                        {
                            foundNodes[nodeCounter] = currentNode.neighbors[i];
                            foundDirection[nodeCounter] = currentNode.validDirections[i];
                            nodeCounter++;
                        }
                    }
                    else
                    {
                        foundNodes[nodeCounter] = currentNode.neighbors[i];
                        foundDirection[nodeCounter] = currentNode.validDirections[i];
                        nodeCounter++;
                    }
                }
                else
                {
                    foundNodes[nodeCounter] = currentNode.neighbors[i];
                    foundDirection[nodeCounter] = currentNode.validDirections[i];
                    nodeCounter++;
                }
            }
        }

        if (foundNodes.Length == 1)
        {
            moveToNode = foundNodes[0];
            direction = foundDirection[0];
        }
        else if (foundNodes.Length > 1)
        {
            float leastDistance = 10000f;

            for (int i = 0; i < foundNodes.Length; i++)
            {
                if (foundDirection[i] != Vector3.zero)
                {
                    float distance = GetDistance(foundNodes[i].transform.position, targetTile);

                    if (distance < leastDistance)
                    {
                        leastDistance = distance;
                        moveToNode = foundNodes[i];
                        direction = foundDirection[i];
                    }
                }
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

    float GetDistance(Vector3 posA, Vector3 posB)
    {
        float dx = posA.x - posB.x;
        float dz = posA.z - posB.z;

        float distance = Mathf.Sqrt(dx * dx + dz * dz);

        return distance;
    }

    GameObject GetTileAtPosition(Vector3 pos)
    {
        GameObject tile = GameBoard.instance.board[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z)];

        if (tile != null)
            return tile;

        return null;
    }

    Nodes GetNodeAtPosition(Vector3 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)pos.x, (int)pos.z];

        if (tile != null)
        {
            if (tile.GetComponent<Nodes>() != null)
            {
                return tile.GetComponent<Nodes>();
            }
        }

        return null;
    }

    Vector3 GetRandomTile()
    {
        int x = Random.Range(0, 28);
        float y = transform.position.y;
        int z = Random.Range(0, 36);

        return new Vector3(x, y, z);
    }

    IEnumerator TunnelMovement(float delay)
    {
        yield return new WaitForSeconds(delay);
        moveSpeed = levelMoveSpeed;
    }
}
