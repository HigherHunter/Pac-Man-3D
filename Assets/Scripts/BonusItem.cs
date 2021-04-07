using UnityEngine;

public class BonusItem : MonoBehaviour
{

    float randomLifeExpectancy;
    float currentLifeTime;

    // Use this for initialization
    void Start()
    {

        randomLifeExpectancy = Random.Range(9, 10);

        name = "bonusItem";

        GameBoard.instance.board[13, 12] = gameObject;

    }

    // Update is called once per frame
    void Update()
    {

        if (currentLifeTime < randomLifeExpectancy)
        {
            currentLifeTime += Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }

    }
}
