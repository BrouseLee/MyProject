using UnityEngine;

public class ThresherShark : Boid
{
    [Header("Observer Interaction")]
    [SerializeField] float approachDistance;
    [SerializeField] float circleDistance;
    [SerializeField] float observerWeight;
    [SerializeField] float outerRadius;
    [SerializeField] int maxSharksNearPlayer;
    [SerializeField] float zigPeriod;  // Left and right return cycle (seconds)
    [SerializeField] float zigSideStrength;  // Side vector weight during return

    private readonly int maxSharksInner = 1;
    private float innerRadius = 13f;
    private float minDistance = 5f;  // The closest distance to the player
    private float verticalAmp = 1f;  // Floating amount

    enum InnerStage { None, Circling, ZigZag, Leaving }
    InnerStage stage = InnerStage.None;

    private int orbitSign; // Control clockwise and counterclockwise
    private float angAccum = 0f;
    private bool zigLeft = true;
    private float zigTimer = 0f;

    public override void Initialize(BoidManager newManager)
    {
        base.Initialize(newManager);     
    }

    protected override void PreUpdateCustom(ref Vector3 extraAcc)
    {
        if (!manager.IsInsideBounds(transform.position))
        {
            extraAcc += manager.AvoidBounds(transform.position) * settings.avoidBoundsWeight;
            return;
        }
        neighbours = manager.GetNeighbours(gameObject);
        extraAcc += ApplyRules();
        extraAcc += FindGoal(goalPos) * settings.goalWeight;
        extraAcc += manager.AvoidPlayer(transform.position) * settings.avoidPlayerWeight;
        extraAcc += AvoidOtherFish() * settings.separationWeight;

        if (observer == null) return;

        float d = Vector3.Distance(transform.position, observer.position);

        bool innerFull = SharkLimitNearPlayer(innerRadius) > maxSharksInner;
        bool outerFull = SharkLimitNearPlayer(outerRadius) > maxSharksNearPlayer;

        // Full ¡ú Escape
        if (innerFull || outerFull)
        {
            stage = InnerStage.Leaving;
            extraAcc += FleeDir() * observerWeight;
        }
        else
        {
            // Enter the inner circle ¡ú start circling
            if (stage == InnerStage.None && d < innerRadius)
            {
                stage = InnerStage.Circling;
                orbitSign = Random.value < 0.5f ? 1 : -1;
                angAccum = 0f;
            }
            // Long distance approach
            else if (d < approachDistance)
            {
                extraAcc += SeekDir() * observerWeight;
            }
        }

        // Staged behavior
        switch (stage)
        {
            case InnerStage.Circling:
                extraAcc += OrbitDir() * observerWeight;
                angAccum += velocity.magnitude * Time.deltaTime /
                            Mathf.Max(innerRadius, 0.01f) * Mathf.Rad2Deg;
                if (angAccum >= 330f)
                {
                    stage = InnerStage.ZigZag;
                    zigTimer = Time.time;
                }
                break;

            case InnerStage.ZigZag:
                extraAcc += ZigZagApproachDir() * observerWeight;
                if (d < minDistance) stage = InnerStage.Leaving;
                break;

            case InnerStage.Leaving:
                extraAcc += FleeDir() * observerWeight;
                if (d > innerRadius * 1.2f) stage = InnerStage.None;
                break;
        }
    }

    Vector3 SeekDir() => (observer.position - transform.position).normalized;
    Vector3 FleeDir() => (transform.position - observer.position).normalized;
    Vector3 OrbitDir()
    {
        Vector3 toObs = transform.position - observer.position;
        Vector3 horiz = new Vector3(toObs.x, 0, toObs.z).normalized;
        if (horiz.sqrMagnitude < 0.001f) return Vector3.zero;

        Vector3 tangential = orbitSign * Vector3.Cross(Vector3.up, horiz);
        float v = Mathf.Sin(Time.time * Mathf.PI) * verticalAmp;
        return (tangential + v * Vector3.up).normalized;
    }

    protected override Vector3 AvoidOtherFish()
    {
        return ThresherSharkManager.Instance.AvoidMe(transform.position);
    }

    int SharkLimitNearPlayer(float radius)
    {
        if (observer == null) return 0;
        int nearby = 0;
        foreach (GameObject b in BoidManager.AllBoids)
        {
            if (b && b.GetComponent<ThresherShark>() != null && Vector3.Distance(b.transform.position, observer.position) < radius)
            {
                nearby++;
            }
        }
        return nearby;
    }

    Vector3 ZigZagApproachDir()
    {
        Vector3 toObs = (observer.position - transform.position).normalized;

        if (Time.time - zigTimer > zigPeriod)
        {
            zigLeft = !zigLeft;
            zigTimer = Time.time;
        }

        Vector3 horiz = new Vector3(toObs.x, 0, toObs.z).normalized;
        Vector3 sideDir = orbitSign * Vector3.Cross(Vector3.up, horiz) * (zigLeft ? 1 : -1) * zigSideStrength;
        float v = Mathf.Sin(Time.time * Mathf.PI) * verticalAmp;

        return (toObs + sideDir + v * Vector3.up).normalized;
    }
}