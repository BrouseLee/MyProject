using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class NewFlock : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private FlockUnit flockUnitPrefab;
    [SerializeField] private int flockSize;
    [SerializeField] private Vector3 spawnBounds;

    [Header("Speed Setup")]                 
    [Range(0, 10)][SerializeField] private float _minSpeed; public float minSpeed { get { return _minSpeed; } }
    [Range(0, 10)][SerializeField] private float _maxSpeed; public float maxSpeed { get { return _maxSpeed; } }

    [Header("Detection Distances")]
    [Range(0, 20)][SerializeField] private float _cohesionDistance; public float cohesionDistance { get { return _cohesionDistance; } }
    [Range(0, 10)][SerializeField] private float _avoidanceDistance; public float avoidanceDistance { get { return _avoidanceDistance; } }
    [Range(0, 10)][SerializeField] private float _aligementDistance; public float aligementDistance { get { return _aligementDistance; } }
    [Range(0, 10)][SerializeField] private float _obstacleDistance; public float obstacleDistance { get { return _obstacleDistance; } }
    [Range(0, 100)][SerializeField] private float _boundsDistance; public float boundsDistance { get { return _boundsDistance; } }
    [Range(0, 20)][SerializeField] private float _sharkAvoidDistance = 15f; public float SharkAvoidDistance => _sharkAvoidDistance;

    [Header("Behaviour Weights")]            
    [Range(0, 10)][SerializeField] private float _cohesionWeight; public float cohesionWeight { get { return _cohesionWeight; } }
    [Range(0, 10)][SerializeField] private float _avoidanceWeight; public float avoidanceWeight { get { return _avoidanceWeight; } }
    [Range(0, 10)][SerializeField] private float _aligementWeight; public float aligementWeight { get { return _aligementWeight; } }
    [Range(0, 10)][SerializeField] private float _boundsWeight; public float boundsWeight { get { return _boundsWeight; } }
    [Range(0, 100)][SerializeField] private float _obstacleWeight; public float obstacleWeight { get { return _obstacleWeight; } }
    [Range(0, 10)] private float _sharkAvoidWeight = 5; public float sharkAvoidWeight => _sharkAvoidWeight;

    public FlockUnit[] allUnits { get; private set; }

    [Header("Peripheral Detection Params")]
    [Tooltip("Search radius for neighbour counting (suggest > alignment distance).")]
    public float neighbourRadius = 4f;
    [Tooltip("If neighbours within radius < this -> considered sparse (edge).")]
    public int minNeighbour = 5;
    [Tooltip("If the opposite‑of‑centre half‑sphere contains fewer than this fraction of neighbours -> edge.")]
    [Range(0f, 1f)] public float asymmetryThreshold = 0.25f;

    private NativeArray<int> neighbourCounts;
    private NativeArray<byte> peripheralFlags;   // 1 = true, 0 = false (NativeArray<bool> not allowed)

    private void Start()
    {
        GenerateUnits();
        neighbourCounts = new NativeArray<int>(flockSize, Allocator.Persistent);
        peripheralFlags = new NativeArray<byte>(flockSize, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (neighbourCounts.IsCreated) neighbourCounts.Dispose();
        if (peripheralFlags.IsCreated) peripheralFlags.Dispose();
    }

    private void Update()
    {
        if (allUnits == null || allUnits.Length == 0) return;

        // 4‑1  Gather positions into NativeArray
        NativeArray<Vector3> positions = new NativeArray<Vector3>(allUnits.Length, Allocator.TempJob);
        NativeArray<Vector3> forwards = new NativeArray<Vector3>(allUnits.Length, Allocator.TempJob);
        for (int i = 0; i < allUnits.Length; i++)
        {
            positions[i] = allUnits[i].transform.position;
            forwards[i] = allUnits[i].transform.forward; // kept for API consistency
        }

        // 4‑2  Calculate swarm centre (required for asymmetry test)
        Vector3 centre = Vector3.zero;
        for (int i = 0; i < positions.Length; i++) centre += positions[i];
        centre /= positions.Length;

        // 4‑3  Schedule job
        PeripheralDetectionJob job = new PeripheralDetectionJob
        {
            unitPositions = positions,
            neighbourCounts = neighbourCounts,
            peripheralFlags = peripheralFlags,
            centre = centre,
            neighbourRadius = neighbourRadius,
            minNeighbour = minNeighbour,
            asymThreshold = asymmetryThreshold
        };

        JobHandle handle = job.Schedule(positions.Length, 32);
        handle.Complete();

        // 4‑4 visual debug – colour edge fish yellow
        //for (int i = 0; i < allUnits.Length; i++)
        //{
        //    bool isEdge = peripheralFlags[i] == 1;
        //    Color c = isEdge ? Color.yellow : Color.white;
        //    allUnits[i].GetComponent<Renderer>().material.color = c;
        //}

        // 4‑5  Dispose temp arrays
        positions.Dispose();
        forwards.Dispose();
    }

    private void GenerateUnits()
    {
        allUnits = new FlockUnit[flockSize];
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 offset = UnityEngine.Random.insideUnitSphere;
            offset = new Vector3(offset.x * spawnBounds.x, offset.y * spawnBounds.y, offset.z * spawnBounds.z);
            Vector3 spawnPos = transform.position + offset;
            Quaternion rot = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);

            allUnits[i] = Instantiate(flockUnitPrefab, spawnPos, rot);
            allUnits[i].tag = "Fish";
        }
    }
}

[BurstCompile]
public struct PeripheralDetectionJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> unitPositions;
    public NativeArray<int> neighbourCounts; // output
    public NativeArray<byte> peripheralFlags; // output (0/1)

    public Vector3 centre;
    public float neighbourRadius;
    public int minNeighbour;
    public float asymThreshold;

    public void Execute(int index)
    {
        Vector3 pos = unitPositions[index];
        Vector3 toCentre = centre - pos;

        int total = 0;
        int front = 0; // neighbours in half‑sphere opposite centre
        float radiusSqr = neighbourRadius * neighbourRadius;

        for (int j = 0; j < unitPositions.Length; j++)
        {
            if (j == index) continue;
            Vector3 delta = unitPositions[j] - pos;
            float distSqr = delta.sqrMagnitude;
            if (distSqr < radiusSqr)
            {
                total++;
                if (Vector3.Dot(toCentre, delta) < 0f) front++; // neighbour is on outer side
            }
        }

        neighbourCounts[index] = total;

        bool sparse = total < minNeighbour;
        bool asymmetric = total > 0 && (front / (float)total) < asymThreshold;
        peripheralFlags[index] = (byte)((sparse || asymmetric) ? 1 : 0);
    }
}
