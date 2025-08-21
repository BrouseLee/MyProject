using UnityEngine;

public class Fish : Boid
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    // Fish avoid everything
    protected override Vector3 AvoidOtherFish()
    {
        Vector3 result = WhaleSharkManager.Instance.AvoidMe(transform.position);
        result += MantaManager.Instance.AvoidMe(transform.position);
        result += TurtleManager.Instance.AvoidMe(transform.position);
        return result.normalized;
    }
}