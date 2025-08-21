using UnityEngine;
using System.Collections;

public class FeedingManta : Boid
{

    protected FeedingState state = FeedingState.Straight;
    protected FeedingMantaSettings feedingMantaSettings;
    protected float timeSinceLastSomersault = 0.0f;
    protected Vector3 somersaultCentre;
    protected FeedingMantaManager feedingMantaManager;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        feedingMantaSettings = settings as FeedingMantaSettings;
        feedingMantaManager = manager as FeedingMantaManager;
    }

    // Update is called once per frame
    protected override void Update()
    {
        switch (state)
        {
            case FeedingState.Straight:
                base.Update(); // Normal boid behaviour
                UpdateState();
                break;

            case FeedingState.Somersault:
                Somersault();
                break;
        }
    }

    // There are only mantas in the feeding area, so we don't need to avoid other fish
    protected override Vector3 AvoidOtherFish()
    {
        return Vector3.zero;
    }

    protected void Somersault()
    {
        Vector3 centreDirection = (somersaultCentre - transform.position).normalized;
        acceleration = Mathf.Pow(velocity.magnitude, 2) / feedingMantaSettings.somersaultRadius * centreDirection;

        UpdatePosition(acceleration.normalized);
    }

    protected void UpdatePosition(Vector3 up)
    {
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * speed;
        transform.position += velocity * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, settings.rotationSpeed * Time.deltaTime);
    }

    // Updates the state of the manta
    protected void UpdateState()
    {
        timeSinceLastSomersault += Time.deltaTime;

        if (timeSinceLastSomersault >= eventInterval)
        {
            timeSinceLastSomersault = 0.0f;
            if (Random.value < feedingMantaSettings.somersaultProbability)
            {
                Vector3? canSomersault = feedingMantaManager.CanSomersault(transform);
                if (canSomersault != null)
                {
                    somersaultCentre = (Vector3)canSomersault;
                    StartCoroutine(StartSomersault());
                }
            }
        }
    }

    IEnumerator StartSomersault()
    {
        float period = 2 * Mathf.PI * feedingMantaSettings.somersaultRadius / velocity.magnitude;
        float numRevolutions = Random.Range(1, 3);

        state = FeedingState.Somersault;

        yield return new WaitForSeconds(period * numRevolutions);

        state = FeedingState.Straight;
    }
}