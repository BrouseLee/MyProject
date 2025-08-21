using UnityEngine;

[CreateAssetMenu(fileName = "BoidSettings", menuName = "Scriptable Objects/BoidSettings")]
public class BoidSettings : ScriptableObject
{
    [Tooltip("The number of entities to spawn.")]
    public int numBoids;

    [Range(0.0f, 5.0f)]
    public float rotationSpeed = 2f;
    [Range(0.0f, 5.0f)]
    public float minSpeed;
    [Range(0.0f, 5.0f)]
    public float maxSpeed;
    [Range(1.0f, 50.0f)]
    public float neighbourDistance;
    [Range(0.0f, 100.0f)]
    public float avoidBoundsWeight;
    [Range(0.1f, 100.0f)]
    public float avoidPlayerWeight;
    [Range(0.0f, 100.0f)]
    public float minPlayerDistance;
    [Range(0.0f, 10.0f)]
    public float goalWeight;
    [Range(0.0f, 1.0f)]
    public float speedChangeProbability = 0.1f;

    [Header("Boids Parameters")]
    [Range(0.0f, 10.0f)]
    public float alignmentWeight;
    [Range(0.0f, 10.0f)]
    public float separationWeight;
    [Range(0.0f, 10.0f)]
    public float cohesionWeight;
}
