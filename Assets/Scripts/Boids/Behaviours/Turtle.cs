using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class Turtle : Boid
{
    enum TurtleState { Swimming, Resting, Breathing }

    [Header("Breath timing (sec)")]
    public float swimBreathInterval = 90f;   // 90s
    public float restBreathInterval = 180f;
    public float breathingHoldTime = 2f;

    [Header("Energy / Breathing")]
    public float energy = 100f;
    public float maxEnergy = 100f;
    public float lowEnergyThresh = 40f;
    public float depletionRate = 0.1f;
    public float recoveryRate = 0.5f;

    [Header("Water surface")]
    public float surfaceY = 67f;              // Scene water surface height
    public float descendDepth = 10f;            // Depth of dive (from the water surface)

    TurtleState state = TurtleState.Swimming;
    TurtleState prevState;
    float lastBreathTime = 0f;
    bool isBreathing = false;
    Vector3 surfacePos, divePos;
    // Start is called before the first frame update
    protected override void Start()
    {
        lastBreathTime += Time.deltaTime;

        switch (state)
        {
            case TurtleState.Swimming:
                base.Update();
                energy -= speed * depletionRate * Time.deltaTime;

                if (energy <= lowEnergyThresh) ChangeState(TurtleState.Resting);
                else if (NeedsToBreathe()) ChangeState(TurtleState.Breathing);
                break;
            case TurtleState.Resting:
                base.Update();
                energy += recoveryRate * Time.deltaTime;

                if (energy >= maxEnergy * 0.95f) ChangeState(TurtleState.Swimming);
                else if (NeedsToBreathe()) ChangeState(TurtleState.Breathing);
                break;

            case TurtleState.Breathing:
                if (!isBreathing) StartCoroutine(HandleBreathing());
                break;
        }
        energy = Mathf.Clamp(energy, 0f, maxEnergy);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        RandomiseSpeedAndGoal();
        CheckGoal();
    }

    IEnumerator HandleBreathing()
    {
        isBreathing = true;
        prevState = state;
        state = TurtleState.Breathing;

        surfacePos = transform.position;
        surfacePos.y = surfaceY;

        while (Vector3.Distance(transform.position, surfacePos) > 0.3f)
        {
            Vector3 dir = (surfacePos - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(dir),
                                                  settings.rotationSpeed * Time.deltaTime);
            // Ascend a bit faster
            transform.position = Vector3.MoveTowards(transform.position,
                                                     surfacePos,
                                                     speed * 1.5f * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(breathingHoldTime);

        divePos = transform.position + transform.forward * 20f;
        divePos.y = surfaceY - descendDepth;

        while (Vector3.Distance(transform.position, divePos) > 0.3f)
        {
            Vector3 dir = (divePos - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(dir),
                                                  settings.rotationSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position,
                                                     divePos,
                                                     speed * Time.deltaTime);
            yield return null;
        }

        lastBreathTime = 0f;
        isBreathing = false;
        state = prevState;
    }

    // Turtles avoid both WhaleSharks and Mantas
    protected override Vector3 AvoidOtherFish()
    {
        Vector3 result = WhaleSharkManager.Instance.AvoidMe(transform.position);
        result += MantaManager.Instance.AvoidMe(transform.position);
        return result.normalized;
    }

    bool NeedsToBreathe()
    {
        float interval = (state == TurtleState.Resting) ? restBreathInterval
                                                        : swimBreathInterval;
        return lastBreathTime >= interval;
    }

    void ChangeState(TurtleState newState)
    {
        prevState = state;
        state = newState;
    }
}