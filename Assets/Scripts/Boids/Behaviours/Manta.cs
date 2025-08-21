using UnityEngine;
using System.Collections;

public class Manta : Boid
{

    protected RoamingState state = RoamingState.Roaming;
    private MantaSettings mantaSettings;
    private bool isOnCooldown = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        mantaSettings = settings as MantaSettings;
        goalPos = manager.GetRandomPosition();
    }

    // Update is called once per frame
    protected override void Update()
    {

        // Update the state of the manta. When on cooldown, the manta will not be able to become curious
        if (!isOnCooldown)
        {
            UpdateState();
        }

        // Debug.Log($"State: {state}");

        switch (state)
        {
            case RoamingState.Roaming:
                RandomiseSpeedAndGoal();
                base.Update(); // Normal boid behaviour
                break;

            case RoamingState.Curious:
                // In a curious state, the manta will prioritise swimming towards the player
                neighbours = manager.GetNeighbours(gameObject);

                acceleration = ApplyRules();
                acceleration += SwimTowardsPlayer() * mantaSettings.towardsPlayerWeight;

                UpdatePosition();
                break;
        }
    }

    protected override Vector3 AvoidOtherFish()
    {
        // Avoids whale sharks
        Vector3 result = WhaleSharkManager.Instance.AvoidMe(transform.position) * settings.separationWeight;
        return result.normalized;
    }

    // Updates the state of the manta based on the distance to the player
    protected void UpdateState()
    {
        float distance = manager.DistanceToPlayer(transform.position);
        // Debug.Log($"Distance to player: {distance}");

        if (distance < mantaSettings.curiousRange && distance > mantaSettings.minPlayerDistance)
        {
            state = RoamingState.Curious;
        }
        else
        {
            state = RoamingState.Roaming;
            if (distance < mantaSettings.minPlayerDistance)
            {
                // Debug.Log("Cooldown start");
                StartCoroutine(Cooldown());
            }
        }
    }

    // Cooldown coroutine to prevent the manta from becoming curious too often
    IEnumerator Cooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(mantaSettings.cooldownTime);
        isOnCooldown = false;
    }

    // Swims towards the player
    private Vector3 SwimTowardsPlayer()
    {
        return FindGoal(PlayerTracker.Instance.playerPos);
    }

}
