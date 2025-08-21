using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using Random = UnityEngine.Random;


public class SharkBehaviour : MonoBehaviour
{
    public float speed = 0.25f;
    private bool striking = false;
    private bool retrieval = false;
    private bool collection = false;
    private bool strike_complete = false;
    private bool strike_begun = false;
    private bool strike_ending = false;
    private Vector3 targetVec;
    private int slowDownTime = 0;
    private bool prepareToStrike = true;
    private int hunger;
    private Vector3 fixedPoint;
    private int consumed = 0;
    private float currentAngle;
    private Transform Target;
    private bool missed;
    private float chooseStrike;
    private Vector3 positionOffset;
    private float moveSpeed, radius;
    private float angle2;
    private Vector3 previousDirection;
    private Vector3 targetDir;
    private int stepCounter;
    private Vector3 rot;
    private Quaternion qTo = new Quaternion();
    private double timer = 0.0;
    private double missTimer = 0.0;
    private int missedCounter = 0;
    Animator animator;
    private bool rightSwinging = false;
    private bool leftSwinging = false;
    private bool doingHit = false;
    private bool spun = false;
    private bool doSideways = false;
    private int fishEaten = 0;
    private BoxCollider _col;
    private Vector3 randomTarget;
    private bool hasRandomTarget = false;
    private int randomLeaveTarget = 0;
    private bool manualControl = false;
    float hungerTimer;
    float nextTick = 1f;
    Vector3 velocity;
    Vector3 acceleration;

    public static class SharkTags
    {
        public const string Fish = "Sardine";
        public const string Stunned = "Stunned";
        public const string Eaten = "Eaten";
        public const string Observer = "Observer";
    }

    // Obstacle Avoidance
    [SerializeField] float obstacleCheckRadius = 1f;

    // Around-Obstacle state
    bool alongObstacle = false;
    float alongTimer = 0f;
    Vector3 surfaceTarget;

    static GameObject[] GetFishByTag(string tag) =>
        GameObject.FindGameObjectsWithTag(tag);

    [Header("Hunger Setting")]
    [SerializeField] int burnRatePerSecond = 1;   // Hunger Drop per Second
    [SerializeField] int mustHuntThreshold = 40;  // <40 
    [SerializeField] int satiatedThreshold = 70;  // ≥70 
    [SerializeField] int maxSatiatedThreshold = 90; // ≤90 stop eating
    [SerializeField] float baitDetectRadius = 25f; // Distance bait ball N meters count as nearby
    [SerializeField][Range(0, 1)] float huntProbNear = 0.7f; // Attack probability

    [Header("Hunting Area")]
    [SerializeField] private BoxCollider sceneBounds;

    [Header("Obstacle Avoidance")]
    [SerializeField] float obstacleDetectRadius = 1.2f;
    [SerializeField] float obstacleDetectDistance = 6f;
    [SerializeField] LayerMask obstacleMask;

    [Header("Free-swim")]
    [SerializeField] float wanderInterval = 4f;
    [SerializeField] float wanderWeight = 1.2f;

    Vector3 wanderTarget;
    float wanderTimer;


    [Range(188, 414)]
    [SerializeField] private float _sharkSize;
    public float sharkSize { get { return _sharkSize; } }

    [SerializeField] private Vector3 spawnLocation;

    [Range(2, 10)]
    [SerializeField] private int _strikeDistance;
    public float strikeDistance { get { return _strikeDistance; } }

    [Header("Strike Speed Multipliers")]
    [SerializeField] float lungeForwardMult = 2f;
    [SerializeField] float lungeTurnMult = 2f;

    public event Action<SharkActivity> OnActivityChanged;
    public enum SharkActivity { Swim, Play, HuntSardine }
    public SharkActivity CurrentActivity { get; private set; } = SharkActivity.Swim;

    void Awake()
    {
        _col = GetComponent<BoxCollider>();
    }
    void SetColliderSize(Vector3 newSize) => _col.size = newSize;

    void MoveForward(float multiplier = 1f)
    {
        Vector3 next = transform.position + transform.forward * speed * Time.deltaTime * multiplier;
        transform.position = ClampInsideBounds(next);
    }

    void MoveAndRotateTowards(Vector3 targetPos, float moveMult = 1f, float rotLerp = 1f)
    {
        float step = speed * Time.deltaTime * moveMult;
        Vector3 next = Vector3.MoveTowards(transform.position, targetPos, step);
        transform.position = ClampInsideBounds(next);

        Vector3 dir = (targetPos - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * rotLerp);
        }
    }

    Vector3 ClampInsideBounds(Vector3 pos)
    {
        if (sceneBounds == null) return pos;

        Vector3 centre = sceneBounds.transform.position + sceneBounds.center;
        Vector3 half = sceneBounds.size * 0.5f;

        return new Vector3(
            Mathf.Clamp(pos.x, centre.x - half.x, centre.x + half.x),
            Mathf.Clamp(pos.y, centre.y - half.y, centre.y + half.y),
            Mathf.Clamp(pos.z, centre.z - half.z, centre.z + half.z));
    }

    bool IsPointInsideObstacle(Vector3 point)
    {
        // Adding a small radius OverlapSphere is more efficient than Collider.Contains
        return Physics.CheckSphere(point, 0.1f, obstacleMask, QueryTriggerInteraction.Ignore);
    }

    public void StrikeFish()
    {
        strike_begun = false;
        strike_ending = true;
        GameObject[] arr = GetFishByTag(SharkTags.Fish);
        var tailEnd = transform.Find("Shark/Root/Center/Spine1/Spine2/Spine3/Spine4/Spine5/Spine6/Spine7/Tail1/Tail2/Tail3/Tail4/Tail5/Tail6/Tail7/Tail8/Tail9/Tail10/Tail11/Tail11_end/");
        foreach (var fish in arr)
        {
            if ((Vector3.Distance(tailEnd.transform.position, fish.transform.position) < 2f) & (consumed <= 7))
            {
                consumed += 1;

                fish.tag = "Stunned";

                //fish.GetComponent<Renderer>().material.color = Color.red;

                GameObject[] stunned_arr = GetFishByTag(SharkTags.Stunned);
                if (stunned_arr.Length > 0)
                {
                    collection = true;
                    chooseStrike = Random.Range(0, 1f);
                    if (chooseStrike >= 0.7)
                    {
                        doSideways = true;
                    }
                    else
                    {
                        doSideways = false;
                    }

                }
                else
                {
                }
            }
        }
        animator.SetBool("overheadAttack", false);
        strike_complete = true;
        chooseStrike = Random.Range(0, 1f);

    }

    public void StrikeDone()
    {
        //print("WIND DOWN COMPLETE AT "+ Time.time);
        // stub to allow collection of wind-down recovery time data.
    }


    public void BeginStrike()
    {
        strike_begun = true;
        targetVec = transform.localRotation.eulerAngles;
        var rotationSpeed = 150;
        transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f, Space.Self);


    }
    public void EndRightStrike()
    {
        rightSwinging = true;
        missedCounter += 80;
        animator.SetBool("rightHit", false);
        doingHit = false;
        chooseStrike = UnityEngine.Random.Range(0, 1f);


    }

    public void EndLeftStrike()
    {
        leftSwinging = true;
        missedCounter += 80;
        animator.SetBool("leftHit", false);
        doingHit = false;
        chooseStrike = UnityEngine.Random.Range(0, 1f);
    }

    public Tuple<Vector3, Quaternion> FindCentreOfBall()
    {
        GameObject[] arr = GetFishByTag(SharkTags.Fish);
        var totalX = 0f;
        var totalY = 0f;
        var totalZ = 0f;

        var count = 0;
        foreach (var fish in arr)
        {
            if (count > 0)
            {
                totalX += fish.transform.position.x;
                totalY += fish.transform.position.y;
                totalZ += fish.transform.position.z;
            }
            count += 1;

        }
        count -= 1;
        var centreX = totalX / count;
        var centreY = (totalY / count);
        var centreZ = totalZ / count;


        var myVector = new Vector3(centreX, centreY, centreZ);

        // Calculte distance to closest Fish

        var lookPos = myVector - transform.position;
        Quaternion currentRotation = Quaternion.LookRotation(lookPos);
        return new Tuple<Vector3, Quaternion>(myVector, currentRotation);
    }
    public GameObject FindClosestTaggedObject()
    {
        GameObject[] fishList = GetFishByTag(SharkTags.Fish);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject fish in fishList)
        {
            Vector3 diff = fish.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = fish;
                distance = curDistance;
            }
        }
        return closest;
    }
    private void OverheadPreparation(float step)
    {
        slowDownTime = 0;
        SetColliderSize(new Vector3(1f, 1f, 3f));

        //Approach Centre of Bait Ball
        var locationAndRotation = FindCentreOfBall();
        Vector3 myVector = locationAndRotation.Item1;
        Quaternion rotation = locationAndRotation.Item2;
        MoveForward(lungeForwardMult);
        float dist = Vector3.Distance(myVector, transform.position);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime);

        if (striking == false)
        {
            if (strike_complete == false)
            {
                MoveAndRotateTowards(myVector, 1f, 1f);
                Transform objectToFollow = FindClosestTaggedObject().transform;
                float distToClosest = Vector3.Distance(objectToFollow.position, transform.position);

                if ((distToClosest <= strikeDistance) & (dist <= 10f))
                {
                    prepareToStrike = false;
                    striking = true;
                    //find closest group of fish in eyeline
                    //var positionToApproach = objectToFollow.position;

                    MoveAndRotateTowards(objectToFollow.position, 3f, 1f);

                }
            }
        }
        if (striking)
        {
            float distToCentre = Vector3.Distance(myVector, transform.position);
            if (distToCentre >= 2.5f)
            {
                MoveAndRotateTowards(myVector, lungeForwardMult, lungeTurnMult);
            }
            else
            {
                MoveForward(lungeForwardMult); ;
            }

            animator.SetBool("overheadAttack", true);
            //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime); 
            //
            //transform.position = Vector3.MoveTowards(transform.position, myVector, step*2);
            retrieval = true;
        }

    }
    private void SidewaysStrike()
    {

        var locationAndRotation = FindCentreOfBall();

        Vector3 myVector = locationAndRotation.Item1;
        Quaternion rotation = locationAndRotation.Item2;


        GameObject[] arr = GetFishByTag(SharkTags.Fish);
        IEnumerable<GameObject> sortedThings = arr.OrderBy(t => Vector3.Distance(t.transform.position, transform.position));
        var e = sortedThings.LastOrDefault();

        float distance = Vector3.Distance(transform.position, myVector);
        if (distance <= 3f)
        {
            GameObject chosenFish = e;
            MoveForward(lungeForwardMult);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime);

            var lookPos = myVector - transform.position;
            Vector3 distanceBetween = myVector - e.transform.position;
            Quaternion currentRotation = Quaternion.LookRotation(lookPos);
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 toOther = Vector3.Normalize(distanceBetween);
            float dotProduct = (Vector3.Dot(forward, toOther));

            if ((dotProduct > 0) & (!doingHit))
            {

                animator.SetBool("leftHit", true);
                doingHit = true;
                SetColliderSize(new Vector3(2.5f, 2.5f, 3f));
                doSideways = false;

            }
            else if (!doingHit)
            {
                animator.SetBool("rightHit", true);
                doingHit = true;
                SetColliderSize(new Vector3(2.5f, 2.5f, 3f));
                doSideways = false;
            }

        }
        else
        {
            MoveForward(lungeForwardMult);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime);

        }

    }
    private void DuringStrike(bool s_b, bool s_e)
    {
        if (s_b)
        {
            // flip over to represent abduction of caudal fins

            var rotationSpeed = 150;
            transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f, Space.Self);

        }
        if (s_e)
        {
            // flip back after strike occurs
            var rotationSpeed = -40;
            transform.Rotate(rotationSpeed * Time.deltaTime, 0f, 0f, Space.Self);
            if (Math.Abs(transform.eulerAngles.x - targetVec[0]) <= 10)
            {
                strike_ending = false;
            }
        }
    }

    private float orbitAngle = 0f;

    private void MoveInRandomDirection()
    {
        if (hunger >= 50 && hunger < satiatedThreshold)
        {
            //WanderRandomly();
            return;
        }
        var locationAndRotation = FindCentreOfBall();
        Vector3 baitBallCenter = locationAndRotation.Item1;

        float desiredRadius = 30f; // <<< Increase this for a larger orbit (default 20–30 good)

        // Calculate orbiting movement
        orbitAngle += 50f * Time.deltaTime; // <<< Increase this for faster circling (degrees per second)

        float radians = orbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * desiredRadius;
        Vector3 targetPosition = baitBallCenter + offset;

        // Move smoothly along circle
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime * 6f);

        // Face forward along orbit path
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        if (moveDirection != Vector3.zero)
        {
            Quaternion moveRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, Time.deltaTime * 5f);
        }
    }



    private void SlowDown()
    {

        // move any stunned fish down as if they are sinking in water
        GameObject[] stunnedFishList1 = GetFishByTag(SharkTags.Stunned);
        foreach (GameObject fish in stunnedFishList1)
        {
            if (fish)
            {
                fish.transform.position += Vector3.down * Time.deltaTime * 0.04f;
            }
        }

        //slow down until shark momentum is lost

        //increase the collider size as fish are 'scared' of
        SetColliderSize(new Vector3(2.5f, 2.5f, 3f));
    }
    private void OverheadStrike()
    {

        // move any stunned fish down as if they are sinking in water
        GameObject[] stunnedFishList1 = GetFishByTag(SharkTags.Stunned);
        foreach (GameObject fish in stunnedFishList1)
        {
            if (fish)
            {
                fish.transform.position += Vector3.down * Time.deltaTime * 0.04f;
            }
        }

        //slow down until shark momentum is lost

        //increase the collider size as fish are 'scared' of
        SetColliderSize(new Vector3(2.5f, 2.5f, 3f));
    }
    private void MissedStrikeTimer()
    {
        var rotateSpeed = 0.5f;
        missedCounter -= 1;
        if (missedCounter == 0)
        {
            rightSwinging = false;
            leftSwinging = false;

            missed = false;
        }
        if (missTimer > 2)
        { // missTimer resets at 2, allowing .5 s to do the rotating
            qTo = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(-180f, 180f), 0));
            missTimer = 0.0;
        }
        missTimer += Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, qTo, Time.deltaTime * rotateSpeed);
        MoveForward(lungeForwardMult);

        missTimer += Time.deltaTime;
    }

    private void Collection()
    {

        var locationAndRotation = FindCentreOfBall();
        Vector3 myVector = locationAndRotation.Item1;
        Quaternion rotation = locationAndRotation.Item2;
        // SPIN AROUND FIRST !!!!!!
        if (!spun)
        {
            animator.SetBool("rightTurn", true);

            float dot = Vector3.Dot(transform.forward, (myVector - transform.position).normalized);
            if (dot > 0.2f) { spun = true; }
            else
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime * 1f);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime);


            }

        }
        else
        {
            animator.SetBool("rightTurn", false);

            consumed = 0;
            Transform tMin = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = transform.position;
            var teeth = transform.Find("Shark/Root/Center/Head1/Head2/Teethup/");

            GameObject[] stunnedFishList = GetFishByTag(SharkTags.Stunned);

            if (stunnedFishList.Length > 0)
            {

                foreach (GameObject fish in stunnedFishList)
                {

                    if (fish)
                    {
                        float distanceToClosest = Vector3.Distance(fish.transform.position, teeth.position);
                        if (distanceToClosest <= 0.5f)
                        {
                            int hungerGain = Random.Range(3, 6);
                            hunger = Mathf.Min(hunger + hungerGain, maxSatiatedThreshold);
                            fish.tag = "Eaten";
                            fishEaten += 1;
                            Destroy(fish);
                        }
                        else if (distanceToClosest < minDist)
                        {
                            tMin = fish.transform;
                            minDist = distanceToClosest;
                        }
                    }
                    else
                    {
                        if (hunger < mustHuntThreshold)
                        {
                            missed = true;
                            missedCounter += 70;
                        }
                        spun = false;

                        prepareToStrike = false;
                        strike_complete = false;
                        collection = false;
                        striking = false;
                        retrieval = false;

                    }
                }
            }
            else
            {
                if (hunger <= 50)
                {
                    missed = true;
                    missedCounter += 80;
                }
                collection = false;
                //col.size = new Vector3(1f, 1f, 3f);
                SetColliderSize(new Vector3(2.5f, 2.5f, 3f));

                strike_complete = false;
            }




            GameObject[] stunnedFishList2 = GetFishByTag(SharkTags.Stunned);
            if (stunnedFishList2.Length > 0)
            {


                GameObject[] eatenFishList = GetFishByTag(SharkTags.Eaten);
                foreach (var objects in eatenFishList)
                {
                    Destroy(objects);
                }
                Vector3 targetDirection = tMin.position - (transform.position);
                float singleStep = speed * Time.deltaTime * 5;
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirection);
                transform.position += transform.forward * Time.deltaTime * speed * (2 / 3);
            }
        }
    }


    private void EvaluateBehaviour()
    {
        Vector3 baitPos = FindCentreOfBall().Item1;
        bool nearBait = Vector3.Distance(transform.position, baitPos) < baitDetectRadius;

        if (hunger < mustHuntThreshold)
        {
            if (nearBait)
            {
                BeginHunt();
            }
            else
            {
                MoveTowards(baitPos);
            }
            return;
        }
        if (hunger >= satiatedThreshold)
        {
            if (randomLeaveTarget == 0)
            {
                randomLeaveTarget = Random.Range(70, 91);
            }
            if (hunger >= randomLeaveTarget)
            {
                LeaveScene();
                return;
            }
            if (nearBait)
            {
                BeginHunt();
            }
            else
            {
                MoveTowards(baitPos);
            }
            return;
        }
        MoveInRandomDirection();
    }
    void BeginHunt() => OverheadPreparation(speed * Time.deltaTime);
    void LeaveScene() => MoveInRandomDirection();


    private void CircleBaitBall()
    {
        var centreAndRot = FindCentreOfBall();
        Vector3 centre = centreAndRot.Item1;

        // ① 保持半径不变 —— 先算出在水平方向的切线
        Vector3 toCentre = centre - transform.position;
        Vector3 tangent = Vector3.Cross(Vector3.up, toCentre).normalized;

        // ② 让鲨鱼沿切线移动
        float circSpeed = speed * 0.75f;                    // 稍慢于直线游速
        transform.position += tangent * circSpeed * Time.deltaTime;

        // ③ 让它朝前方（切线方向）看
        if (tangent != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(tangent),
                Time.deltaTime * 3f);
    }

    void MoveTowards(Vector3 pos)
    {
        float step = speed * Time.deltaTime * 2f;
        transform.position = Vector3.MoveTowards(transform.position, pos, step);
        transform.position = ClampInsideBounds(transform.position);
        transform.LookAt(pos);
    }

    private void SetNewRandomTarget()
    {
        randomTarget = GetRandomPositionInBounds();
        hasRandomTarget = true;

        // Check for obstacles
        if (IsPointInsideObstacle(randomTarget))
        {
            int axis = Random.Range(0, 3);
            surfaceTarget = transform.position;
            if (axis == 0) surfaceTarget.x = randomTarget.x;
            else if (axis == 1) surfaceTarget.y = randomTarget.y;
            else surfaceTarget.z = randomTarget.z;

            surfaceTarget = ClampInsideBounds(surfaceTarget);
            alongObstacle = true;
            alongTimer = Random.Range(10f, 20f);
        }
        else
        {
            alongObstacle = false;
        }
        wanderTimer = 0f;
    }

    private Vector3 GetRandomPositionInBounds()
    {
        Vector3 center = sceneBounds.transform.position + sceneBounds.center;
        Vector3 size = sceneBounds.size;
        Vector3 randomPos = new Vector3(
            Random.Range(center.x - size.x / 2, center.x + size.x / 2),
            Random.Range(center.y - size.y / 2, center.y + size.y / 2),
            Random.Range(center.z - size.z / 2, center.z + size.z / 2));
        return randomPos;
    }

    bool IsInsideBounds(Vector3 pos)
    {
        Vector3 local = sceneBounds.transform.InverseTransformPoint(pos) - sceneBounds.center;
        Vector3 ext = sceneBounds.size * 0.5f;
        return Mathf.Abs(local.x) < ext.x &&
               Mathf.Abs(local.y) < ext.y &&
               Mathf.Abs(local.z) < ext.z;
    }

    Vector3 AvoidBounds(Vector3 pos)
    {
        return (sceneBounds.center - pos).normalized;
    }


    // Start is called before the first frame update
    void Start()
    {

        //hunger = startingHungerValue;
        transform.position = spawnLocation;
        //= new Vector3(500,20,200); 
        hunger = Random.Range(40, 100);
        animator = GetComponent<Animator>();
        //Here the GameObject's Collider is not a trigger
        chooseStrike = Random.Range(0, 1f);
        float[] sizeRange = { 188f, 414f };
        float[] scaleRange = { 0.78f, 1.725f };

        float mappedSize = (sharkSize - sizeRange[0]) * (scaleRange[1] - scaleRange[0]) / (sizeRange[1] - sizeRange[0]) + scaleRange[0];
        transform.localScale = new Vector3(mappedSize, mappedSize, mappedSize);

        SetNewRandomTarget();
        randomLeaveTarget = 0;

        wanderTarget = GetRandomPositionInBounds();
    }

    // Update is called once per frame
    void Update()
    {
        // ① 计时：到点就刷新目标
        wanderTimer += Time.deltaTime;
        if (wanderTimer > wanderInterval)
        {
            wanderTarget = GetRandomPositionInBounds();
            wanderTimer = 0f;
        }

        // ② 产生 Seek 加速度（经典 steering）
        Vector3 desired = (wanderTarget - transform.position).normalized * speed;
        Vector3 steer = (desired - velocity).normalized * wanderWeight;

        acceleration += steer;

        if (Time.time >= nextTick)
        {
            hunger = Mathf.Max(hunger - burnRatePerSecond, 0);
            nextTick = Time.time + 1f;
        }

        // Input control when using computer
        // TODO - add input for when using VR controllers

        // Legacy code - only runs when not in manual control
        //if (hunger > 1) { hunger -= 1; }

        var step = speed * Time.deltaTime; // calculate distance to move
        stepCounter += 1;
        if (rightSwinging)
        {
            SetColliderSize(new Vector3(2f, 2f, 3f));
            var rotationSpeed = 1;
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));

        }
        if (leftSwinging)
        {
            SetColliderSize(new Vector3(2f, 2f, 3f));

            var rotationSpeed = -1;
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));

        }

        DuringStrike(strike_begun, strike_ending);

        if (missed)
        { // Allow a time gap between a missed strike and another strike
            MissedStrikeTimer();
            return;
        }
        else if (strike_complete)
        {
            striking = false;
            //collect stunned fish
            float singleStep = speed * Time.deltaTime * 2;
            var locationAndRotation = FindCentreOfBall();
            Vector3 myVector = locationAndRotation.Item1;
            Quaternion rotation = locationAndRotation.Item2;

            transform.position += (transform.forward) * Time.deltaTime * speed * 4;
            slowDownTime += 1;
            //continue swimming until momentum lost
            SetColliderSize(new Vector3(2.5f, 2.5f, 3f));


            if (slowDownTime >= 20)
            {
                //if collecting fish find closest stunned fish
                if (doSideways)
                {
                    SidewaysStrike();
                }
                else if (collection)
                {
                    Collection();
                }
                else
                {
                    prepareToStrike = false;
                    strike_complete = false;
                    collection = false;
                    SetColliderSize(new Vector3(1f, 1f, 3f));
                    striking = false;
                    retrieval = false;
                    missed = true;
                    missedCounter = 80;
                }
            }
            return;
        }

        // Main behavior system - only runs when not in special states
        EvaluateBehaviour();

        //if (hunger > 1) { hunger -= 1; }
        if (strike_complete)
        {
            striking = false;
            //collect stunned fish
            float singlestep = speed * Time.deltaTime * 2;
            var locationAndRotation = FindCentreOfBall();
            Vector3 myVector = locationAndRotation.Item1;
            Quaternion rotation = locationAndRotation.Item2;

            transform.position += (transform.forward) * Time.deltaTime * speed * 4;
            slowDownTime += 1;
            // continue swimming until moment lost
            SetColliderSize(new Vector3(2.5f, 2.5f, 3f));

            if (slowDownTime >= 15)
            {
                if (doSideways)
                {
                    SidewaysStrike();
                }
                else if (collection)
                {
                    Collection();
                }
                else
                {
                    prepareToStrike = false;
                    strike_complete = false;
                    collection = false;
                    SetColliderSize(new Vector3(1f, 1f, 3f));

                    striking = false;
                    retrieval = false;
                    missed = true;
                    missedCounter = 60;
                }
            }
        }
        else if (hunger < mustHuntThreshold)
        {
            fishEaten = 0;
            OverheadPreparation(step);
        }
        else
        {
            MoveInRandomDirection();
        }
    }

    void LateUpdate()
    {
        transform.position = ClampInsideBounds(transform.position);
    }

}