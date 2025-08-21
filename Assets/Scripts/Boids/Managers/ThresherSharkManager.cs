using UnityEngine;
public class ThresherSharkManager : BoidManager
{
    public static ThresherSharkManager Instance;
    protected void Start()
    {
        SpawnBoids();
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
