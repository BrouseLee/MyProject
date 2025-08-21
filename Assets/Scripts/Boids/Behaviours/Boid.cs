using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Boid : MonoBehaviour
{

    [Header("Bubble reaction (SO override)")]
    public BubbleProfile bubbleProfile;
    [SerializeField] BubbleProfile.Reaction defaultReaction = BubbleProfile.Reaction.Flee;
    [SerializeField] float defaultDistance = 25f;
    [SerializeField] float defaultBoost = 3f;

    protected Vector3 acceleration;
    protected Vector3 velocity;
    protected float speed;
    protected Vector3 goalPos;
    public List<GameObject> neighbours;
    protected BoidManager manager;
    protected BoidSettings settings;
    [SerializeField, HideInInspector] protected Transform observer;  // XR rig / player

    protected float eventInterval = 1.0f; // seconds
    protected float timeSinceLastEvent = 0.0f;

    BubbleProfile.Reaction GetBubbleReaction() => bubbleProfile ? bubbleProfile.reaction : defaultReaction;
    float GetBubbleDistance() => bubbleProfile ? bubbleProfile.distance : defaultDistance;
    float GetBubbleBoost() => bubbleProfile ? bubbleProfile.boost : defaultBoost;

    #region Initialisation
    public virtual void Initialize(BoidManager newManager)
    {
        manager = newManager;
        settings = newManager.settings;
        GameObject observerObj = GameObject.FindGameObjectWithTag("Observer");
        if (observerObj) observer = observerObj.transform;
    }

    protected virtual void Start()
    {
        speed = Random.Range(settings.minSpeed, settings.maxSpeed);
        velocity = Random.onUnitSphere * speed;
        transform.forward = velocity;
        goalPos = manager.GetRandomPosition();
    }
    #endregion

    #region Update loop – hooks & core logic


    protected virtual void PreUpdateCustom(ref Vector3 extraAcceleration) { }

 
    protected virtual void PostUpdateCustom() { }


    protected virtual void Update()
    {
        acceleration = Vector3.zero;

        // Bubble parameters – available to whole frame 
        var react = GetBubbleReaction();
        float distToPlayer = observer ? Vector3.Distance(transform.position, observer.position) : Mathf.Infinity;
        float bubbleDistance = GetBubbleDistance();
        float bubbleBoost = GetBubbleBoost();

        // Core flocking / bounds / custom extras 
        if (!manager.IsInsideBounds(transform.position))
        {
            acceleration = manager.AvoidBounds(transform.position) * settings.avoidBoundsWeight;
        }
        else
        {
            neighbours = manager.GetNeighbours(gameObject);
            acceleration += ApplyRules();
            acceleration += FindGoal(goalPos) * settings.goalWeight;
            acceleration += AvoidOtherFish() * settings.separationWeight;

            // Child‑defined extra forces
            Vector3 extra = Vector3.zero;
            PreUpdateCustom(ref extra);
            acceleration += extra;

            //Conditional AvoidPlayer – skip when attracting
            bool skipAvoid = (react == BubbleProfile.Reaction.Attract && distToPlayer < bubbleDistance);
            if (!skipAvoid)
            {
                acceleration += manager.AvoidPlayer(transform.position) * settings.avoidPlayerWeight;
            }
        }

        // Bubble reaction forces
        if (Breathing.IsExhaling && react != BubbleProfile.Reaction.Ignore && distToPlayer < bubbleDistance)
        {
            Vector3 bubbleInfluence = Vector3.zero;

            switch (react)
            {
                case BubbleProfile.Reaction.Flee:
                    bubbleInfluence = bubbleBoost * settings.avoidPlayerWeight *
                                       (transform.position - observer.position).normalized;
                    break;

                case BubbleProfile.Reaction.MildAvoid:
                    Vector3 away = transform.position - observer.position;
                    Vector3 side = Vector3.Cross(Vector3.up, away).normalized;
                    bubbleInfluence = 0.7f * settings.avoidBoundsWeight * side;
                    break;

                case BubbleProfile.Reaction.Attract:
                    float h = bubbleProfile ? bubbleProfile.bubbleHeight : 2f;
                    float rad = bubbleProfile ? bubbleProfile.orbitRadius : 3f;
                    float spd = bubbleProfile ? bubbleProfile.orbitSpeed : 1f;

                    Vector3 target = observer.position + Vector3.up * h;
                    Vector3 toTgt = target - transform.position;
                    Vector3 horiz = new(toTgt.x, 0, toTgt.z);

                    if (horiz.magnitude > rad)
                        bubbleInfluence += toTgt.normalized * settings.goalWeight; // approach
                    else
                        bubbleInfluence += new Vector3(-horiz.z, 0, horiz.x).normalized * spd; // orbit
                    break;
            }

            acceleration += bubbleInfluence * 10f;
        }
        CheckGoal();
        RandomiseSpeedAndGoal();
        UpdatePosition();

        
        PostUpdateCustom();
    }
    #endregion

    #region Behaviour helpers (override in child where noted)
    protected abstract Vector3 AvoidOtherFish();

    protected void CheckGoal()
    {
        if (Vector3.Distance(goalPos, transform.position) < 1f)
            goalPos = manager.GetRandomPosition();
    }

    protected void RandomiseSpeedAndGoal()
    {
        timeSinceLastEvent += Time.deltaTime;
        if (timeSinceLastEvent >= eventInterval)
        {
            timeSinceLastEvent = 0f;
            if (Random.value < settings.speedChangeProbability)
            {
                speed = Random.Range(settings.minSpeed, settings.maxSpeed);
                goalPos = manager.GetRandomPosition();
            }
        }
    }

    protected void UpdatePosition()
    {
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * speed;
        transform.position += velocity * Time.deltaTime;

        Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, settings.rotationSpeed * Time.deltaTime);
    }

    protected Vector3 ApplyRules() =>
        Alignment() * settings.alignmentWeight +
        Cohesion() * settings.cohesionWeight +
        Separation() * settings.separationWeight;

    protected Vector3 Alignment()
    {
        if (neighbours.Count == 0) return Vector3.zero;
        Vector3 avg = Vector3.zero;
        foreach (var n in neighbours) avg += n.GetComponent<Boid>().velocity;
        avg /= neighbours.Count;
        return (avg - velocity).normalized;
    }

    protected Vector3 Cohesion()
    {
        if (neighbours.Count == 0) return Vector3.zero;
        Vector3 centre = Vector3.zero;
        foreach (var n in neighbours) centre += n.transform.position;
        centre /= neighbours.Count;
        return (centre - transform.position).normalized;
    }

    protected Vector3 Separation()
    {
        if (neighbours.Count == 0) return Vector3.zero;
        Vector3 res = Vector3.zero;
        foreach (var n in neighbours)
        {
            Vector3 diff = transform.position - n.transform.position;
            if (diff.sqrMagnitude > 0)
                res += diff.normalized / diff.magnitude;
        }
        return res.normalized;
    }

    protected Vector3 FindGoal(Vector3 goalPos)
    {
        if (goalPos == null) return Vector3.zero;
        Vector3 diff = goalPos - transform.position;
        return diff.normalized;
    }
    //Vector3 FindGoal(Vector3 g) => g == Vector3.zero ? Vector3.zero : (g - transform.position).normalized;
    #endregion
}
