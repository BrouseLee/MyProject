using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CycloneManager : MonoBehaviour
{
    public static CycloneManager CM;

    public GameObject mantaPrefab;
    public int numManta = 20;
    public List<GameObject> allManta;
    public float maxDepth = 5f;
    public float minDistance = 0.5f;

    [Header ("Manta Settings")]
    [Range(0.0f, 1.0f)]
    public float speed;
    [Range(5, 30)]
    public float minRadius;
    [Range(5, 30)]
    public float maxRadius;

    private readonly int maxCounter = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allManta = new List<GameObject>();
        for (int i = 0; i < numManta; i++)
        {
            float yPos = Random.Range(-maxDepth, maxDepth);
            bool isValid = CheckDistance(yPos);
            int counter = 0;
            while (!isValid || counter < maxCounter)
            {
                yPos = Random.Range(-maxDepth, maxDepth);
                isValid = CheckDistance(yPos);
                counter += 1;
            }

            if (!isValid)
            {
                Debug.Log("Manta spawned too close!");
            }

            allManta.Add(Instantiate(mantaPrefab, new Vector3(0, transform.position.y + yPos, 0), Quaternion.identity));
        }
        CM = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Checks if manta is too close to another
    bool CheckDistance(float yPos)
    {
        if (allManta.Count > 0)
        {
            foreach (GameObject manta in allManta)
            {
                float diff = Mathf.Abs(yPos - manta.transform.position.y);
                if (diff < minDistance)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
