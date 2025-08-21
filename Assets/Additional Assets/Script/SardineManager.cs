using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

public class SardineManager : MonoBehaviour
{
    [Header("Spawn")]
    public SardineUnit unitPrefab;
    public BoxCollider boundaryBox;
    public int flockSize;
    public Vector3 spawnBounds = new(25, 25, 25);

    [Header("Speed")]
    [Range(0.1f, 10f)] public float minSpeed = 2;
    [Range(0.1f, 10f)] public float maxSpeed = 5;

    [Header("Distances")]
    public float cohesionDist = 4;
    public float avoidanceDist = 1.5f;
    public float alignmentDist = 3;
    public float boundsRadius = 20;

    [Header("Weights")]
    public float cohesionWt = 1;
    public float avoidanceWt = 3;
    public float alignmentWt = 1;
    public float boundsWt = 2;

    SardineUnit[] units;
    NativeArray<float3> posRead, dirRead;    // (snapshot)
    NativeArray<float3> posWrite, dirWrite;   // (output)
    NativeArray<float3> vel;                  // 只自己写自己，单缓冲即可
    NativeArray<float> speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        posRead = new NativeArray<float3>(flockSize, Allocator.Persistent);
        dirRead = new NativeArray<float3>(flockSize, Allocator.Persistent);
        posWrite = new NativeArray<float3>(flockSize, Allocator.Persistent);
        dirWrite = new NativeArray<float3>(flockSize, Allocator.Persistent);
        vel = new NativeArray<float3>(flockSize, Allocator.Persistent);
        speed = new NativeArray<float>(flockSize, Allocator.Persistent);

        units = new SardineUnit[flockSize];
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 p = transform.position + new Vector3(
                UnityEngine.Random.Range(-spawnBounds.x, spawnBounds.x),
                UnityEngine.Random.Range(-spawnBounds.y, spawnBounds.y),
                UnityEngine.Random.Range(-spawnBounds.z, spawnBounds.z));
            Quaternion r = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            var u = Instantiate(unitPrefab, p, r);
            u.speed = UnityEngine.Random.Range(minSpeed, maxSpeed);

            units[i] = u;
            posRead[i] = posWrite[i] = p;
            dirRead[i] = dirWrite[i] = u.tf.forward;
            vel[i] = float3.zero;
            speed[i] = u.speed;
        }

    }

    // Update is called once per frame
    void Update()
    {
        var job = new MoveJob
        {
            posRead = posRead,
            dirRead = dirRead,
            posWrite = posWrite,
            dirWrite = dirWrite,
            vel = vel,
            speed = speed,

            deltaTime = Time.deltaTime,
            minSpeed = minSpeed,
            maxSpeed = maxSpeed,

            cohDist2 = cohesionDist * cohesionDist,
            avoidDist2 = avoidanceDist * avoidanceDist,
            alignDist2 = alignmentDist * alignmentDist,
            boundsCenter = (float3)transform.position,
            boundsRad2 = boundsRadius * boundsRadius,

            cohWt = cohesionWt,
            avoidWt = avoidanceWt,
            alignWt = alignmentWt,
            boundsWt = boundsWt,

            cosHalfFov = math.cos(math.radians(unitPrefab.FOVAngle * 0.5f)),
            turnSmooth = unitPrefab.turnSmooth
        };

        JobHandle h = job.Schedule(flockSize, 64);
        h.Complete();

        for (int i = 0; i < flockSize; i++)
        {
            units[i].tf.SetPositionAndRotation(
                posWrite[i],
                Quaternion.LookRotation(dirWrite[i]));
        }
        (posRead, posWrite) = (posWrite, posRead);
        (dirRead, dirWrite) = (dirWrite, dirRead);
    }
    void OnDestroy()
    {
        if (posRead.IsCreated) posRead.Dispose();
        if (posWrite.IsCreated) posWrite.Dispose();
        if (dirRead.IsCreated) dirRead.Dispose();
        if (dirWrite.IsCreated) dirWrite.Dispose();
        if (vel.IsCreated) vel.Dispose();
        if (speed.IsCreated) speed.Dispose();
    }

    [BurstCompile]
    struct MoveJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> posRead, dirRead;
        public NativeArray<float3> vel;
        public NativeArray<float> speed;

        public float deltaTime, minSpeed, maxSpeed;
        public float cohDist2, avoidDist2, alignDist2;
        public float3 boundsCenter;
        public float boundsRad2;
        public float cohWt, avoidWt, alignWt, boundsWt;
        public float cosHalfFov, turnSmooth;
        public NativeArray<float3> posWrite, dirWrite;


        public void Execute(int index)
        {
            float3 p = posRead[index];
            float3 f = dirRead[index];

            float3 cohSum = 0, alignSum = 0, avoidSum = 0;
            int cohCnt = 0, alignCnt = 0, avoidCnt = 0;

            for (int j = 0; j < posRead.Length; j++)
            {
                if (j == index) continue;
                float3 diff = posRead[j] - p;
                float d2 = math.lengthsq(diff);
                float3 diffNorm = diff * math.rsqrt(d2 + 1e-6f);

                if (math.dot(f, diffNorm) < cosHalfFov) continue;
                if (d2 < cohDist2)
                {
                    cohSum += posRead[j];
                    cohCnt++;
                }
                if (d2 < avoidDist2)
                {
                    avoidSum += diffNorm; // later *-1
                    avoidCnt++;
                }
                if (d2 < alignDist2)
                {
                    alignSum += dirRead[j];
                    alignCnt++;
                }
            }
            float3 move = float3.zero;

            if (cohCnt > 0) move += math.normalizesafe(cohSum / cohCnt - p) * cohWt;
            if (avoidCnt > 0) move += math.normalizesafe(-avoidSum / avoidCnt) * avoidWt;
            if (alignCnt > 0) move += math.normalizesafe(alignSum / alignCnt) * alignWt;

            float3 toCenter = boundsCenter - p;
            if (math.lengthsq(toCenter) > boundsRad2 * .81f)
                move += math.normalizesafe(toCenter) * boundsWt;

            float3 curVel = vel[index];
            float3 desired = math.normalizesafe(move);
            curVel = math.lerp(curVel, desired, turnSmooth);
            if (math.lengthsq(curVel) < 1e-8f)
                curVel = dirRead[index]; // Return to the previous frame direction
            float s = math.clamp(speed[index], minSpeed, maxSpeed);

            dirWrite[index] = math.normalizesafe(curVel);
            posWrite[index] = p + dirWrite[index] * s * deltaTime;
            vel[index] = curVel;
            speed[index] = s;
        }
    }
}
