using UnityEngine;

public class MantaManager : BoidManager
{
    public static MantaManager Instance;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        SpawnBoids();
        Instance = this;
    }

}
