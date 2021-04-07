using UnityEngine;

public class Nodes : MonoBehaviour
{

    public Nodes[] neighbors;
    public Vector3[] validDirections;

    [ContextMenu("Find Neighbors")]
    void FindNeighbors()
    {
        validDirections = new Vector3[neighbors.Length];

        for (int i = 0; i < neighbors.Length; i++)
        {
            Nodes neighbor = neighbors[i];
            Vector3 temp = neighbor.transform.localPosition - transform.localPosition;

            validDirections[i] = temp.normalized;
        }
    }
}
