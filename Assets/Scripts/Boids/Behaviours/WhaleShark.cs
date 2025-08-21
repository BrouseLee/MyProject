using UnityEngine;

public class WhaleShark : Boid
{

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        goalPos = manager.GetRandomPosition();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        Vector3 playerPos = PlayerTracker.Instance.playerPos;
        float distanceToPlayer = Vector3.Distance(transform.position, playerPos);

        if (distanceToPlayer <= 7f && playerPos.y < transform.position.y)
        {
            Vector3 awayFromPlayer = (transform.position - playerPos).normalized;
            Vector3 upward = Vector3.up * 0.5f;

            Vector3 fleeDirection = (awayFromPlayer + upward).normalized;

            acceleration += fleeDirection * settings.avoidPlayerWeight * 3f; 
        }

        RandomiseSpeedAndGoal();
        CheckGoal();
    }


    protected override Vector3 AvoidOtherFish()
    {
        Vector3 result = MantaManager.Instance.AvoidMe(transform.position);
        return result.normalized;
    }

}