using UnityEngine;
using System.Collections.Generic;
using MantaSim.Utilities;
using Unity.XR.CoreUtils;

public abstract class BoidManager : MonoBehaviour
{

    public static BoidManager BM;

    // Shared list across ALL species
    public static List<GameObject> AllBoids = new List<GameObject>();
    // Per-species list
    protected List<GameObject> localBoids = new List<GameObject>();

    public BoxCollider bounds;
    public BoidSettings settings;
    public GameObject prefab;
    protected Renderer rend;

    void Awake()
    {
        BM = this;
        rend = prefab.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log($"Bounds center: {bounds.transform.position}");
    }

    // Spawns the boids in the scene
    protected virtual void SpawnBoids()
    {
        for (int i = 0; i < settings.numBoids; i++)
        {
            GameObject boid = Instantiate(prefab, GetRandomPosition(), Random.rotation);
            boid.GetComponent<Boid>().Initialize(this);
            RegisterBoid(boid);
        }
    }

    // Registers the boid locally and globally
    protected virtual void RegisterBoid(GameObject boid)
    {
        AllBoids.Add(boid);
        localBoids.Add(boid);
    }

    // Avoid other fish of the same species
    public Vector3 AvoidMe(Vector3 pos)
    {
        List<GameObject> neighbours = new List<GameObject>();
        float distance;

        if (rend == null) return Vector3.zero;

        float radius = rend.bounds.extents.MaxComponent();

        foreach (GameObject boid in localBoids)
        {
            distance = Vector3.Distance(boid.transform.position, pos);
            if (distance < radius)
            {
                neighbours.Add(boid);
            }
        }

        Vector3 result = Vector3.zero;
        Vector3 nDist;

        if (neighbours.Count == 0) return Vector3.zero;

        foreach (GameObject neighbour in neighbours)
        {
            nDist = pos - neighbour.transform.position;

            if (nDist.magnitude > 0)
                result += nDist.normalized / nDist.magnitude;
        }

        return result.normalized;

    }

    public List<GameObject> GetNeighbours(GameObject self)
    {
        List<GameObject> neighbours = new List<GameObject>();
        float distance;

        foreach (GameObject boid in localBoids)
        {
            if (boid != self)
            {
                distance = Vector3.Distance(boid.transform.position, self.transform.position);
                if (distance < settings.neighbourDistance)
                {
                    neighbours.Add(boid);
                }
            }
        }

        return neighbours;
    }

    // Checks if boid is within bounds
    public virtual bool IsInsideBounds(Vector3 pos)
    {
        
        if (bounds == null)
        {
            // Debug.Log("Bounds not set!");
            return true;
        }

        // Debug.Log($"Checking if {pos} is inside bounds {bounds.transform.position}");

        Vector3 localPoint = bounds.transform.InverseTransformPoint(pos);

        Vector3 centre = bounds.center;
        Vector3 size = bounds.size / 2;

        return Mathf.Abs(localPoint.x - centre.x) <= size.x &&
               Mathf.Abs(localPoint.y - centre.y) <= size.y &&
               Mathf.Abs(localPoint.z - centre.z) <= size.z;
    }

    // Get a random position inside the bounds
    public virtual Vector3 GetRandomPosition()
    {
        return bounds.GetRandomPointInsideCollider();
    }

    // Avoids the bounds by returning a vector towards the center of the bounds
    public virtual Vector3 AvoidBounds(Vector3 pos)
    {
        Vector3 diff = bounds.transform.position - pos;
        return diff.normalized;
    }

    // Gets the distance to the player
    public float DistanceToPlayer(Vector3 pos)
    {
        if (PlayerTracker.Instance == null) return 0;
        
        Vector3 nDist = PlayerTracker.Instance.playerPos - pos;
        return nDist.magnitude;
    }

    // Avoids the player if close
    public Vector3 AvoidPlayer(Vector3 pos)
    {
        if (PlayerTracker.Instance == null) return Vector3.zero;
        
        Vector3 nDist = pos - PlayerTracker.Instance.playerPos;
        if (nDist.magnitude < settings.minPlayerDistance)
            return nDist.normalized / nDist.magnitude;
        return Vector3.zero;
    }
}
