using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameBoard : MonoBehaviour
{
    public static GameBoard instance;

    private void Awake()
    {
        instance = this;
    }

    private static int width = 28, height = 36;

    public GameObject[,] board = new GameObject[width, height];

    public AudioClip normalGameSound, frightenedGameSound, gameStart, pacManDeath, ghostDeath;

    private AudioSource audioSource;

    public GameObject readyText, scoreText, highScoreText, consumedScoreText, Life2, Life3;

    public static int pacManLives = 3, currentLevel = 1, score = 0;

    public int consumedGhostComboScore;

    private int highScore;

    private GameObject[] ghosts;

    private GameObject bonusItem;

    // Use this for initialization
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        highScore = PlayerPrefs.GetInt("Score", 0);

        highScoreText.GetComponent<TextMeshProUGUI>().text = "HIGH SCORE:\n" + highScore;

        ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        Object[] objects = GameObject.FindGameObjectsWithTag("Pellet");

        foreach (GameObject o in objects)
        {
            Vector3 pos = o.transform.position;
            board[(int)pos.x, (int)pos.z] = o;
        }

        StartGame();
    }

    void Update()
    {
        scoreText.GetComponent<TextMeshProUGUI>().text = "SCORE:\n" + score;

        if (consumedGhostComboScore == 1600)
        {
            PlayNormalMusic();
        }
    }

    public void PlayNormalMusic()
    {
        audioSource.clip = normalGameSound;
        audioSource.Play();
    }

    public void PlayFrightenedMusic()
    {
        audioSource.clip = frightenedGameSound;
        audioSource.Play();
    }

    public void SpawnBonusItem()
    {
        Instantiate(bonusItem);
    }

    public void SpawnDisplayItem()
    {
        GameObject item = null;

        if (currentLevel == 1)
        {
            item = Resources.Load("Prefabs/Cherry", typeof(GameObject)) as GameObject;
        }
        else if (currentLevel == 2)
        {
            item = Resources.Load("Prefabs/Strawberry", typeof(GameObject)) as GameObject;
        }
        else if (currentLevel == 3 || currentLevel == 4)
        {
            item = Resources.Load("Prefabs/Orange", typeof(GameObject)) as GameObject;
        }
        else if (currentLevel == 5 || currentLevel == 6)
        {
            item = Resources.Load("Prefabs/Apple", typeof(GameObject)) as GameObject;
        }
        else if (currentLevel == 7 || currentLevel == 8)
        {
            item = Resources.Load("Prefabs/Grapes", typeof(GameObject)) as GameObject;
        }
        else if (currentLevel == 9 || currentLevel == 10)
        {
            item = Resources.Load("Prefabs/Galaxian", typeof(GameObject)) as GameObject;
        }
        else if (currentLevel == 11 || currentLevel == 12)
        {
            item = Resources.Load("Prefabs/Bell", typeof(GameObject)) as GameObject;
        }
        else
        {
            item = Resources.Load("Prefabs/Key", typeof(GameObject)) as GameObject;
        }

        bonusItem = item;

        GameObject displayItem = Instantiate(item);

        displayItem.transform.position = new Vector3(23.5f, 0, -3);
        displayItem.GetComponent<BonusItem>().enabled = false;
        displayItem.GetComponent<Tile>().enabled = false;
        Instantiate(displayItem);

    }

    public void StartGame()
    {
        foreach (GameObject ghost in ghosts)
        {
            ghost.transform.GetComponent<Ghost>().canMove = false;
        }

        PacmanMovement.instance.canMove = false;

        if (currentLevel == 1)
        {
            audioSource.PlayOneShot(gameStart);
        }

        if (pacManLives == 2)
        {
            Life3.SetActive(false);
        }
        else if (pacManLives == 1)
        {
            Life3.SetActive(false);
            Life2.SetActive(false);
        }

        SpawnDisplayItem();

        StartCoroutine(StartGameAfter(4.2f));
    }

    IEnumerator StartGameAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject ghost in ghosts)
        {
            ghost.transform.GetComponent<Ghost>().canMove = true;
        }

        PacmanMovement.instance.canMove = true;

        readyText.SetActive(false);

        audioSource.clip = normalGameSound;
        audioSource.Play();
    }

    public void StartConsumed(Ghost consumedGhost)
    {
        foreach (GameObject ghost in ghosts)
        {
            ghost.transform.GetComponent<Ghost>().canMove = false;
        }
        PacmanMovement.instance.canMove = false;

        Vector3 pos = consumedGhost.transform.position;

        Vector3 viewPortPoint = Camera.main.WorldToViewportPoint(pos);

        consumedScoreText.SetActive(true);
        consumedScoreText.GetComponent<TextMeshProUGUI>().text = consumedGhostComboScore.ToString();

        consumedScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
        consumedScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

        audioSource.PlayOneShot(ghostDeath);

        StartCoroutine(ProcessConsumedAfter(0.75f, consumedGhost));
    }

    IEnumerator ProcessConsumedAfter(float delay, Ghost consumedGhost)
    {
        yield return new WaitForSeconds(delay);

        consumedScoreText.SetActive(false);

        foreach (GameObject ghost in ghosts)
        {
            ghost.transform.GetComponent<Ghost>().canMove = true;
        }
        PacmanMovement.instance.canMove = true;

        audioSource.Play();
    }

    public void StartConsumedBonusItem(GameObject bonusItem)
    {
        Vector3 pos = bonusItem.transform.position;

        Vector3 viewPortPoint = Camera.main.WorldToViewportPoint(pos);

        consumedScoreText.SetActive(true);
        consumedScoreText.GetComponent<TextMeshProUGUI>().text = bonusItem.GetComponent<Tile>().fruitValue.ToString();

        consumedScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
        consumedScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

        audioSource.PlayOneShot(ghostDeath);

        StartCoroutine(ProcessConsumedBonusItem(0.75f));
    }

    IEnumerator ProcessConsumedBonusItem(float delay)
    {
        yield return new WaitForSeconds(delay);

        consumedScoreText.SetActive(false);
    }

    public void PlayerWin()
    {
        currentLevel++;
        StartCoroutine(ProcessWin(2));
    }

    IEnumerator ProcessWin(float delay)
    {
        PacmanMovement.instance.canMove = false;

        foreach (GameObject ghost in ghosts)
        {
            ghost.transform.GetComponent<Ghost>().canMove = false;
        }

        audioSource.Stop();

        yield return new WaitForSeconds(delay);

        StartCoroutine(ProcessWinAfter(2));
    }

    IEnumerator ProcessWinAfter(float delay)
    {
        PacmanMovement.instance.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        PacmanMovement.instance.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;

        foreach (GameObject ghost in ghosts)
        {
            ghost.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(3).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(4).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(5).GetComponent<MeshRenderer>().enabled = false;
        }

        yield return new WaitForSeconds(delay);

        //restart game at next level
        SceneManager.LoadScene(1);
    }

    public void StartDeath()
    {
        GameObject bonusItem = GameObject.Find("bonusItem");
        if (bonusItem)
            Destroy(bonusItem.gameObject);

        foreach (GameObject ghost in ghosts)
        {
            ghost.transform.GetComponent<Ghost>().canMove = false;
        }
        PacmanMovement.instance.canMove = false;

        PacmanMovement.instance.transform.GetComponent<Animator>().enabled = false;

        audioSource.Stop();

        StartCoroutine(ProcessDeath(2));
    }

    IEnumerator ProcessDeath(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject ghost in ghosts)
        {
            ghost.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(3).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(4).GetComponent<MeshRenderer>().enabled = false;
            ghost.transform.GetChild(5).GetComponent<MeshRenderer>().enabled = false;
        }

        StartCoroutine(ProcessDeathAnimation(2));
    }

    IEnumerator ProcessDeathAnimation(float delay)
    {
        PacmanMovement.instance.transform.GetComponent<Animator>().enabled = true;
        PacmanMovement.instance.transform.GetComponent<Animator>().Play("Dying");

        audioSource.PlayOneShot(pacManDeath);

        yield return new WaitForSeconds(delay);

        StartCoroutine(ProcessRestart(2));
    }

    IEnumerator ProcessRestart(float delay)
    {
        pacManLives -= 1;

        if (pacManLives == 0)
        {
            readyText.GetComponent<TextMeshProUGUI>().text = "GAME OVER";
            readyText.GetComponent<TextMeshProUGUI>().color = Color.red;

            audioSource.Stop();

            if (score > highScore)
            {
                PlayerPrefs.SetInt("Score", score);
            }

            StartCoroutine(ProcessGameOver(2));
        }
        else
        {
            yield return new WaitForSeconds(delay);

            Restart();
        }
    }

    IEnumerator ProcessGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        if (pacManLives == 2)
        {
            Life3.SetActive(false);
        }
        else if (pacManLives == 1)
        {
            Life2.SetActive(false);
        }

        PacmanMovement.instance.Restart();

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().Restart();
        }

        readyText.SetActive(true);

        foreach (GameObject ghost in ghosts)
        {
            ghost.transform.GetComponent<Ghost>().canMove = false;
        }
        PacmanMovement.instance.canMove = false;

        StartCoroutine(StartGameAfter(2));
    }
}
