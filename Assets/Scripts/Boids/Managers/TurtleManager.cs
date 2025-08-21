using UnityEngine;
using System.Collections.Generic;
using MantaSim.Utilities;

public class TurtleManager : BoidManager
{
    public static TurtleManager Instance;
    
    // Start is called before the first frame update
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